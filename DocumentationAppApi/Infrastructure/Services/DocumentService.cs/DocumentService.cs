using DocumentationApp.Domain.Entities;
using DocumentationAppApi.API.Models.Requests.Document;
using DocumentationAppApi.Application.Documents;
using DocumentationAppApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Playwright;

namespace DocumentationAppApi.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DocumentService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<Document> CreateDocumentAsync(CreateDocumentRequest request)
    {
        var safeTitle = string.Join("_", request.Title.Split(Path.GetInvalidFileNameChars()));
        var folder = Path.Combine(_env.ContentRootPath, "Uploads", "documents");
        Directory.CreateDirectory(folder);

        string fileName = $"{safeTitle}.html";
        string filePath = Path.Combine(folder, fileName);
        string fileType = ".html";

        // WordEditor CSS
        var css = @"
@page {
  size: A4;
  margin: 20mm 15mm;
}

html, body {
  margin: 0;
  padding: 0;
}

.ck-content {
  font-family: Inter, system-ui, sans-serif;
  font-size: 12pt;
  line-height: 1.6;
  box-sizing: border-box;
}
p { margin: 1em 0; }

figure.image-style-inline { display: inline-block; margin: 0 1em 1em 0; }
figure.image-style-block { display: block; margin: 1em 0; text-align: center; }
figure.image-style-side { float: right; margin: 0 0 1em 1em; }

img { max-width: 100%; height: auto; }
";


        var html = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>{css}</style>
</head>
<body>
  <div class='ck-content'>
    {request.Content}
  </div>
</body>
</html>";

        await File.WriteAllTextAsync(filePath, html);

        var relativePath = Path.Combine("Uploads", "documents", fileName).Replace("\\", "/");

        var document = new Document
        {
            ApplicationId = request.ApplicationId,
            Title = request.Title,
            FileName = fileName,
            FilePath = relativePath,
            FileType = fileType,
            Status = "A",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return document;
    }

}
