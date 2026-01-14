using System;
using System.ComponentModel.DataAnnotations;

namespace DocumentationApp.Domain.Entities;

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    public int ApplicationId { get; set; }

    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public string FileType { get; set; } = null!; // PDF, VIDEO, WORD
    public string Title { get; set; } = null!; 

    public string Status { get; set; } = "A";

    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }

    // Navigation
    public App Application { get; set; } = null!;
}
