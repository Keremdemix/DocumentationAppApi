using DocumentationApp.Domain.Entities;
using DocumentationAppApi.Responses.Auth;

public interface ITokenService
{
    LoginResponse GenerateToken(User user);
}
