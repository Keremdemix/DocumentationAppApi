namespace DocumentationAppApi.Responses.Auth;

public record LoginResponse(
    string Token,
    DateTime Expiration,
    string UserType
);
