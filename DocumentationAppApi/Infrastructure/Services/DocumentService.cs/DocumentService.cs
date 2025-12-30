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

        var pdfFileName = $"{safeTitle}.pdf";
        var pdfFilePath = Path.Combine(folder, pdfFileName);

        // Playwright ile HTML render + PDF
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        await page.SetContentAsync($@"
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 20px; }}
                </style>
            </head>
            <body>
                {request.Content}
            </body>
            </html>
        ");

        await page.PdfAsync(new PagePdfOptions
        {
            Path = pdfFilePath,
            Format = "A4"
        });

        var relativePath = Path.Combine("Uploads", "documents", pdfFileName).Replace("\\", "/");

        var document = new Document
        {
            ApplicationId = request.ApplicationId,
            Title = request.Title,
            FileName = pdfFileName,
            FilePath = relativePath,
            FileType = ".pdf",
            Status = "A",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return document;
    }
}
