using DocumentationApp.Application.Documents.Responses;
using DocumentationApp.Domain.Entities;
using DocumentationAppApi.Infrastructure.Persistence;
using DocumentationAppApi.Infrastructure.Services.FileUploadService;
using DocumentationAppApi.Requests.Applications;
using DocumentationAppApi.Responses.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DocumentationAppApi.API.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public ApplicationController(
    AppDbContext context,
    IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var apps = _context.Applications
            .Select(x => new ApplicationResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                LogoPath = x.LogoPath
            })
            .ToList();

        return Ok(apps);
    }

    [HttpPost]
    public IActionResult Create(CreateApplicationRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var app = new Application
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = userId
        };

        _context.Applications.Add(app);
        _context.SaveChanges();

        return Ok(app.Id);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdateApplicationRequest request)
    {
        var app = _context.Applications.FirstOrDefault(x => x.Id == id);
        if (app == null)
            return NotFound();

        app.Name = request.Name;
        app.Description = request.Description;

        _context.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var app = _context.Applications.FirstOrDefault(x => x.Id == id);
        if (app == null)
            return NotFound();

        app.Status = "D";
        _context.SaveChanges();

        return NoContent();
    }

    [HttpGet("{applicationId}/documents")]
    public async Task<IActionResult> GetDocumentsByApplication(int applicationId)
    {
        var documents = await _context.Documents
            .Where(d => d.ApplicationId == applicationId && d.Status == "A")
            .Select(d => new DocumentResponse
            {
                Id = d.Id,
                FileName = d.FileName,
                FileType = d.FileType,
                Url = $"{Request.Scheme}://{Request.Host}/{d.FilePath.Replace("\\", "/")}",
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return Ok(documents);
    }
    [HttpPost("{applicationId}/logo")]
    public async Task<IActionResult> UploadLogo(int applicationId, IFormFile logo)
    {
        var app = await _context.Applications.FindAsync(applicationId);
        if (app == null) return NotFound();

        var fileName = await _fileUploadService.UploadAsync(logo, "logos");
        app.LogoPath = $"Uploads/logos/{fileName}";
        await _context.SaveChangesAsync();

        return Ok(new { logoPath = app.LogoPath });
    }
}

