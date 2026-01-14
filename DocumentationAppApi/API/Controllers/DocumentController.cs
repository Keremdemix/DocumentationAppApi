using DocumentationApp.Domain.Entities;
using DocumentationAppApi.API.Models.Requests.Document;
using DocumentationAppApi.Application.Documents;
using DocumentationAppApi.Infrastructure.Persistence;
using DocumentationAppApi.Infrastructure.Services.FileUploadService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System.Security.Claims;

namespace DocumentationAppApi.API.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly IWebHostEnvironment _env;
        private readonly IDocumentService _documentService;

        public DocumentController(
            IDocumentService documentService,
            IWebHostEnvironment env,
            AppDbContext context,
            IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _env = env;
            _documentService = documentService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            [FromForm] int applicationId,
            [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("Dosya seçilmedi");

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var uploadedDocuments = new List<Document>();

            foreach (var file in files)
            {
                var savedFileName = await _fileUploadService.UploadAsync(
                    file,
                    "documents"
                );

                var document = new Document
                {
                    ApplicationId = applicationId,
                    FileName = file.FileName,
                    FilePath = $"Uploads/documents/{savedFileName}",
                    FileType = Path.GetExtension(file.FileName),
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    Status = "A",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                uploadedDocuments.Add(document);
            }

            _context.Documents.AddRange(uploadedDocuments);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Dosyalar başarıyla yüklendi",
                Count = uploadedDocuments.Count
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentRequest request)
        {

            Console.WriteLine($"Update called for Id={id}, Title={request.Title}");
            var doc = await _context.Documents.FirstOrDefaultAsync(x => x.DocumentId == id);
            if (doc == null) return NotFound();

            doc.Title = request.Title;
            await _context.SaveChangesAsync();

            return Ok(doc);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(x => x.DocumentId == id);

            if (document == null)
                return NotFound();

            document.Status = "D";
            await _context.SaveChangesAsync();

            return Ok("Doküman pasif hale getirildi");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            request.CreatedBy = int.Parse(userIdClaim);

            var result = await _documentService.CreateDocumentAsync(request);
            return Ok(result);
        }

        [HttpGet("download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == id);
            if (document == null) return NotFound();

            var filePath = Path.Combine(_env.ContentRootPath, document.FilePath);

            var uploadsRoot = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "Uploads"));
            var fullPath = Path.GetFullPath(filePath);

            if (!fullPath.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file path.");

            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found on disk.");

            if (string.Equals(document.FileType, ".html", StringComparison.OrdinalIgnoreCase))
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

                var page = await browser.NewPageAsync();

                var fileUrl = new Uri(fullPath).AbsoluteUri;
                await page.GotoAsync(fileUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

                var pdfBytes = await page.PdfAsync(new PagePdfOptions
                {
                    Format = "A4",
                    PrintBackground = true,
                    Margin = new Microsoft.Playwright.Margin
                    {
                        Top = "20mm",
                        Bottom = "20mm",
                        Left = "15mm",
                        Right = "15mm"
                    }
                });

                return File(pdfBytes, "application/pdf", document.Title + ".pdf");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "application/octet-stream", document.FileName);
        }
    }
}
    