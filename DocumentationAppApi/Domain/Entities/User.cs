namespace DocumentationApp.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public int UserTypeId { get; set; }

    // Navigation
    public UserType UserType { get; set; } = null!;
}
