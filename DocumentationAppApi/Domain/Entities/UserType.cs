namespace DocumentationApp.Domain.Entities;

public class UserType 
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
