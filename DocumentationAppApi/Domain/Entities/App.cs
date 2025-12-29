using System;
using System.Collections.Generic;

namespace DocumentationApp.Domain.Entities;

public class App
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public string? LogoPath { get; set; }

    public string Status { get; set; } = "A";

    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }

    // Navigation
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
