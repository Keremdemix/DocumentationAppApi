using DocumentationApp.Domain.Entities;
using DocumentationAppApi.Infrastructure.Persistence;
using DocumentationAppApi.Requests.Auth;
using DocumentationAppApi.Responses.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _context.Users
            .FirstOrDefault(x => x.Username == request.Username);

        if (user == null)
            return Unauthorized("Invalid credentials");

        if (user.PasswordHash != request.Password)
            return Unauthorized("Invalid credentials");

        var token = GenerateToken(user);

        return Ok(token);
    }

    private LoginResponse GenerateToken(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.UserTypeId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(jwt["ExpireMinutes"]!)
            ),
            signingCredentials: creds
        );

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            token.ValidTo,
            user.UserTypeId.ToString()
        );
    }
}
