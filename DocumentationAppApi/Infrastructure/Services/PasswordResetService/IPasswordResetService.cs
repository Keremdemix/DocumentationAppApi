using DocumentationAppApi.API.Models.Requests.Auth;
using DocumentationAppApi.API.Models.Responses.Auth;
using System.Threading.Tasks;

using DocumentationAppApi.API.Models.Requests.Auth;
using DocumentationAppApi.API.Models.Responses.Auth;
using System.Threading.Tasks;

namespace DocumentationAppApi.Infrastructure.Services.PasswordResetService
{
    public interface IPasswordResetService
    {
        Task RequestTokenAsync(ResetTokenRequest request);
        VerifyPasswordResetTokenResponse VerifyToken(VerifyPasswordResetTokenRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
