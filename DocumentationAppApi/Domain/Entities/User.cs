using System.ComponentModel.DataAnnotations;

namespace DocumentationApp.Domain.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int UserTypeId { get; set; }
    public bool IsPasswordCreated { get; set; }
    public string Status { get; set; } = "A";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserType UserType { get; set; } = null!;
}

