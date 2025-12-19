namespace DocumentationAppApi.Requests.Auth;

public record LoginRequest(
    string Username,
    string Password
);
