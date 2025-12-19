namespace DocumentationApp.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    // Soft Delete
    public string Status { get; set; } = "A"; 

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
