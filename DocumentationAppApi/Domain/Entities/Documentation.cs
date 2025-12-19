namespace DocumentationApp.Domain.Entities;

public class Documentation : BaseEntity
{
    public int ApplicationId { get; set; }

    public string Title { get; set; } = null!;
    public string? Content { get; set; }

    public string? PdfPath { get; set; }
    public string? VideoPath { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;

    public int CreatedBy { get; set; }
}
