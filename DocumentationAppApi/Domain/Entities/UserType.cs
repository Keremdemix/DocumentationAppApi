using System.ComponentModel.DataAnnotations;

namespace DocumentationApp.Domain.Entities;

public class UserType 
{
    [Key]
    public int UserTypeId { get; set; }
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
