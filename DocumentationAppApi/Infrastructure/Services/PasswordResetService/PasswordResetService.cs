using DocumentationAppApi.API.Models.Requests.Auth;
using DocumentationAppApi.API.Models.Responses.Auth;
using DocumentationAppApi.Domain.Entities;
using DocumentationAppApi.Infrastructure.Persistence;
using DocumentationAppApi.Infrastructure.Services.EmailService;
using System.Security.Cryptography;
using System;
using System.Linq;
using System.Threading.Tasks;

public class PasswordResetService : IPasswordResetService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new PasswordResetService with the provided database context and email service.
    /// </summary>
    public PasswordResetService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    /// <summary>
    /// Generates a 6-digit numeric token for password reset operations.
    /// </summary>
    /// <returns>A 6-digit numeric string to be used as a token.</returns>
    private string GenerateSecureToken()
    {
        // 6 haneli güvenli token
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    /// <summary>
    /// Sends a password reset token to the user identified by the email in the request.
    /// </summary>
    /// <param name="request">Request containing the target user's email (Request.Mail).</param>
    /// <exception cref="Exception">Thrown with message "Kullanıcı bulunamadı." if no user exists with the provided email.</exception>
    /// <exception cref="Exception">Thrown with message starting "Token DB kaydı sırasında hata oluştu: " if saving the new token to the database fails.</exception>
    public async Task RequestTokenAsync(ResetTokenRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == request.Mail);
        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        // Daha önce var olan ve geçerli token
        var existingToken = _db.PasswordResetTokens
            .FirstOrDefault(t => t.UserId == user.UserId && t.ExpireAt > DateTime.UtcNow);

        string token;
        if (existingToken != null)
        {
            token = existingToken.Token;
        }
        else
        {
            token = GenerateSecureToken();
            var newToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = token,
                ExpireAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow
            };

            _db.PasswordResetTokens.Add(newToken);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Token DB kaydı sırasında hata oluştu: " + ex.Message, ex);
            }
        }

        await _emailService.SendEmailAsync(user.Email, "Şifre Sıfırlama Kodu", $"Kodunuz: {token}");
    }

    /// <summary>
    /// Checks whether the provided token corresponds to the specified email and has not expired.
    /// </summary>
    /// <param name="request">Request containing the token string and the email address to validate.</param>
    /// <returns>`VerifyPasswordResetTokenResponse` with `IsValid` set to `true` if a matching token exists for the email and its expiration is later than the current UTC time, `false` otherwise.</returns>
    public VerifyPasswordResetTokenResponse VerifyToken(VerifyPasswordResetTokenRequest request)
    {
        var tokenEntry = _db.PasswordResetTokens
            .FirstOrDefault(t => t.Token == request.Token && t.User.Email == request.Mail);

        bool isValid = tokenEntry != null && tokenEntry.ExpireAt > DateTime.UtcNow;
        return new VerifyPasswordResetTokenResponse { IsValid = isValid };
    }

    /// <summary>
    /// Resets a user's password using a valid password reset token.
    /// </summary>
    /// <param name="request">Contains the reset `Token`, the new password `NewPassword`, and its confirmation `NewPasswordControl`.</param>
    /// <exception cref="System.Exception">Thrown when the provided token is missing or expired with message "Token geçersiz veya süresi dolmuş."</exception>
    /// <exception cref="System.Exception">Thrown when the user associated with the token cannot be found with message "Kullanıcı bulunamadı."</exception>
    /// <exception cref="System.Exception">Thrown when `NewPassword` and `NewPasswordControl` do not match with message "Şifreler uyuşmuyor."</exception>
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenEntry = _db.PasswordResetTokens.FirstOrDefault(t => t.Token == request.Token);
        if (tokenEntry == null || tokenEntry.ExpireAt < DateTime.UtcNow)
            throw new Exception("Token geçersiz veya süresi dolmuş.");

        var user = _db.Users.FirstOrDefault(u => u.UserId == tokenEntry.UserId);
        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        if (request.NewPassword != request.NewPasswordControl)
            throw new Exception("Şifreler uyuşmuyor.");

        // Şifreyi hashle ve IsPasswordCreated = true yap
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.IsPasswordCreated = true;

        // Token’ı sil
        _db.PasswordResetTokens.Remove(tokenEntry);
        await _db.SaveChangesAsync();
    }
}