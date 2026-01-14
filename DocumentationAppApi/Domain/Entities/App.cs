using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DocumentationApp.Domain.Entities;

public class App
{
    [Key]
    public int ApplicationId { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public string? LogoPath { get; set; }

    public string Status { get; set; } = "A";

    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }


    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
