using DocumentationApp.Domain.Entities;
using DocumentationAppApi.Infrastructure.Persistence;
using DocumentationAppApi.Requests.Applications;
using DocumentationAppApi.Responses.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentationAppApi.API.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationController : ControllerBase
{
    private readonly AppDbContext _context;

    public ApplicationController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var apps = _context.Applications
            .Select(x => new ApplicationResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
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
}

