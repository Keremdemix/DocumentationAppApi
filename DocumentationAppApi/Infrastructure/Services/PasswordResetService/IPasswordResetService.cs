using DocumentationAppApi.API.Models.Requests.Auth;
using DocumentationAppApi.API.Models.Responses.Auth;
using System.Threading.Tasks;

public interface IPasswordResetService
{
    /// <summary>
/// Initiates a password reset token request for the account identified in the request.
/// </summary>
/// <param name="request">Information required to request a reset token (for example, the user's identifier and delivery details).</param>
/// <returns>Completion of the token request operation.</returns>
Task RequestTokenAsync(ResetTokenRequest request);
    /// <summary>
/// Verifies a password reset token and evaluates its validity.
/// </summary>
/// <param name="request">The verification request containing the token and any required context for validation.</param>
/// <returns>A <see cref="VerifyPasswordResetTokenResponse"/> indicating whether the token is valid and containing related verification metadata.</returns>
VerifyPasswordResetTokenResponse VerifyToken(VerifyPasswordResetTokenRequest request);
    /// <summary>
/// Resets a user's password using the information in the request (reset token and new password).
/// </summary>
/// <param name="request">Request containing the reset token and the new password to apply.</param>
Task ResetPasswordAsync(ResetPasswordRequest request);
}