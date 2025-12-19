namespace DocumentationAppApi.Responses.Auth;

public class MeResponse
{
    public int UserId { get; set; }
    public string Role { get; set; } = null!;
}