using DocumentationApp.Domain.Entities;
using DocumentationAppApi.API.Models.Requests.Auth;
using DocumentationAppApi.Infrastructure.Persistence;
using DocumentationAppApi.Requests.Auth;
using DocumentationAppApi.Responses.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(
        AppDbContext context,
        ITokenService tokenService,
        IPasswordResetService passwordResetService)
    {

        _context = context;
        _tokenService = tokenService;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _context.Users
            .Include(u => u.UserType)
            .FirstOrDefault(x => x.Username == request.Username);

        if (user == null)
            return Unauthorized("Invalid credentials");

        bool passwordValid;

        if (user.IsPasswordCreated)
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        else
        {
            passwordValid = request.Password == user.PasswordHash;
        }

        if (!passwordValid)
            return Unauthorized("Invalid credentials");

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            userType = user.UserType.Name,
            isPasswordCreated = user.IsPasswordCreated
        });
    }



    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (userId == null)
            return Unauthorized();

        return Ok(new MeResponse
        {
            UserId = int.Parse(userId),
            Role = role!
        });
    }


    /// Şifre sıfırlama maili gönder
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ResetTokenRequest request)
    {
        await _passwordResetService.RequestTokenAsync(request);
        return Ok(new { message = "Şifre sıfırlama kodu mail adresinize gönderildi." });
    }

    /// Token kontrolü
    [HttpPost("verify-reset-token")]
    public IActionResult VerifyResetToken([FromBody] VerifyPasswordResetTokenRequest request)
    {
        var result = _passwordResetService.VerifyToken(request);
        return Ok(result);
    }

    // 
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found");

        if (request.NewPassword != request.NewPasswordControl)
            return BadRequest("Passwords do not match");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.IsPasswordCreated = true;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Password changed successfully" });
    }

    public record ChangePasswordRequest(string NewPassword, string NewPasswordControl);


    /// Şifre sıfırlama
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var tokenEntry = _context.PasswordResetTokens
            .FirstOrDefault(t => t.Token == request.Token);

        if (tokenEntry == null || tokenEntry.ExpireAt < DateTime.UtcNow)
            return BadRequest("Token geçersiz.");

        var user = _context.Users
            .Include(u => u.UserType)
            .FirstOrDefault(u => u.UserId == tokenEntry.UserId);

        if (user == null)
            return BadRequest("Kullanıcı bulunamadı.");

        await _passwordResetService.ResetPasswordAsync(request);

        var jwt = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token = jwt.Token,
            tokenExpire = jwt.Expiration,
            userType = jwt.UserType
        });
    }
}

