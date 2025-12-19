namespace DocumentationApp.Domain.Entities;

public class UserType : BaseEntity
{
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
