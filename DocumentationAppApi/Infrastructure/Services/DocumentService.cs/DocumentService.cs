using DocumentationApp.Domain.Entities;
using DocumentationAppApi.API.Models.Requests.Document;
using DocumentationAppApi.Application.Documents;
using DocumentationAppApi.Infrastructure.Persistence;
using DocumentFormat.OpenXml.Packaging;
using HtmlToOpenXml;
using Microsoft.AspNetCore.Hosting;
using System.Text;

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
        // --- Dosya adını güvenli hale getir ---
        var safeTitle = string.Join("_", request.Title.Split(Path.GetInvalidFileNameChars()));

        // Uploads/documents klasörünün tam yolu
        var folder = Path.Combine(_env.ContentRootPath, "Uploads", "documents");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // --- DOCX Dosya Adı ---
        var docxFileName = $"{safeTitle}.docx";
        var docxFilePath = Path.Combine(folder, docxFileName);

        // Aynı isim varsa GUID ekle
        if (File.Exists(docxFilePath))
        {
            var uniqueSuffix = Guid.NewGuid().ToString();
            docxFileName = $"{safeTitle}_{uniqueSuffix}.docx";
            docxFilePath = Path.Combine(folder, docxFileName);
        }

        // DOCX oluştur
        using (var wordDoc = WordprocessingDocument.Create(docxFilePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(
                new DocumentFormat.OpenXml.Wordprocessing.Body());

            var converter = new HtmlConverter(mainPart);
            converter.ParseHtml(request.Content);
        }

        // --- DB kaydı ---
        var relativePath = Path.Combine("Uploads", "documents", docxFileName).Replace("\\", "/");

        var document = new Document
        {
            ApplicationId = request.ApplicationId,
            Title = request.Title,
            FileName = docxFileName,
            FilePath = relativePath, // ✅ Relative path kaydediyoruz
            FileType = ".docx",
            Status = "A",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return document;
    }
}