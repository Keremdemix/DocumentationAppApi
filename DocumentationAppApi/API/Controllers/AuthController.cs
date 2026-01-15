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

    /// <summary>
    /// Initializes a new instance of AuthController with required services for authentication and password reset.
    /// </summary>
    /// <param name="context">Database context for accessing users and password reset tokens.</param>
    /// <param name="tokenService">Service that generates authentication tokens.</param>
    /// <param name="passwordResetService">Service that manages password reset tokens and workflows.</param>
    public AuthController(
        AppDbContext context,
        ITokenService tokenService,
        IPasswordResetService passwordResetService)
    {

        _context = context;
        _tokenService = tokenService;
        _passwordResetService = passwordResetService;
    }

    /// <summary>
    /// Authenticate a user by username and password and issue a JWT plus user metadata.
    /// </summary>
    /// <param name="request">The login credentials (username and password).</param>
    /// <returns>An IActionResult that produces 200 OK with an object containing `token`, `userType`, and `isPasswordCreated` on successful authentication; 401 Unauthorized with "Invalid credentials" if authentication fails.</returns>
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



    /// <summary>
    /// Get the authenticated user's ID and role.
    /// </summary>
    /// <returns>
    /// An OK response containing a MeResponse with `UserId` (int) and `Role` (string), or an Unauthorized response if the user identifier claim is missing.
    /// </returns>
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


    /// <summary>
    /// Initiates a password reset by sending a reset token to the user's email address.
    /// </summary>
    /// <param name="request">The request containing the user's email address (or identifier) to receive the reset token.</param>
    /// <returns>200 OK with an object containing a message that the password reset code has been sent to the email.</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ResetTokenRequest request)
    {
        await _passwordResetService.RequestTokenAsync(request);
        return Ok(new { message = "Şifre sıfırlama kodu mail adresinize gönderildi." });
    }

    /// <summary>
    /// Verifies a password-reset token and returns the verification result.
    /// </summary>
    /// <param name="request">The verification request containing the token and any required identifying data.</param>
    /// <returns>An object that indicates whether the token is valid and contains related details.</returns>
    [HttpPost("verify-reset-token")]
    public IActionResult VerifyResetToken([FromBody] VerifyPasswordResetTokenRequest request)
    {
        var result = _passwordResetService.VerifyToken(request);
        return Ok(result);
    }

    /// <summary>
    /// Updates the authenticated user's password after validating the provided confirmation.
    /// </summary>
    /// <param name="request">Request containing NewPassword and NewPasswordControl; both values must match.</param>
    /// <returns>200 OK with a success message when the password is changed; 400 Bad Request if the provided passwords do not match; 404 Not Found if the authenticated user cannot be found.</returns>
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


    /// <summary>
    /// Resets a user's password using a password reset token and returns a new authentication token.
    /// </summary>
    /// <param name="request">Reset token and new password data used to validate and apply the password reset.</param>
    /// <returns>HTTP 200 with a JSON object containing `token`, `tokenExpire`, and `userType` on success; HTTP 400 if the token is invalid, expired, or the user is not found.</returns>
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
