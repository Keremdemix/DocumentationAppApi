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

    public PasswordResetService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    private string GenerateSecureToken()
    {
        // 6 haneli güvenli token
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

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

    public VerifyPasswordResetTokenResponse VerifyToken(VerifyPasswordResetTokenRequest request)
    {
        var tokenEntry = _db.PasswordResetTokens
            .FirstOrDefault(t => t.Token == request.Token && t.User.Email == request.Mail);

        bool isValid = tokenEntry != null && tokenEntry.ExpireAt > DateTime.UtcNow;
        return new VerifyPasswordResetTokenResponse { IsValid = isValid };
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenEntry = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token);
            
        if (tokenEntry == null || tokenEntry.ExpireAt < DateTime.UtcNow)
            throw new Exception("Token geçersiz veya süresi dolmuş.");

        var user = tokenEntry.User;
        if (user == null || !user.Email.Equals(request.Mail, StringComparison.OrdinalIgnoreCase))
            throw new Exception("Token ile kullanıcı eşleşmiyor.");

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
