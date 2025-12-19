namespace DocumentationApp.Domain.Entities;

public class Application : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CreatedBy { get; set; }
    // Navigation
    public ICollection<Documentation> Documentations { get; set; }
        = new List<Documentation>();
}
