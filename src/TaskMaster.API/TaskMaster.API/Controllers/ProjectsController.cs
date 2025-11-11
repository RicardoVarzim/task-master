using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all monitored projects
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a project by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Teams)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        return project;
    }

    /// <summary>
    /// Adds a new project to monitor
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject([FromBody] CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path) || !System.IO.Directory.Exists(request.Path))
        {
            return BadRequest("Invalid project path");
        }

        // Check if project already exists
        var existingProject = await _context.Projects
            .FirstOrDefaultAsync(p => p.FullPath == request.Path);

        if (existingProject != null)
        {
            return Conflict("Project already exists");
        }

        var projectName = System.IO.Path.GetFileName(request.Path.TrimEnd('\\', '/'));
        var gitPath = FindGitRepository(request.Path);

        var project = new Project
        {
            Name = projectName,
            FullPath = request.Path,
            GitRepositoryPath = gitPath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    /// <summary>
    /// Removes a project from monitoring
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private string? FindGitRepository(string projectPath)
    {
        var currentPath = projectPath;
        while (!string.IsNullOrEmpty(currentPath))
        {
            var gitPath = System.IO.Path.Combine(currentPath, ".git");
            if (System.IO.Directory.Exists(gitPath) || System.IO.File.Exists(gitPath))
            {
                return currentPath;
            }

            var parent = System.IO.Directory.GetParent(currentPath);
            if (parent == null)
                break;

            currentPath = parent.FullName;
        }

        return null;
    }
}

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Path is required")]
    [MinLength(1, ErrorMessage = "Path cannot be empty")]
    public string Path { get; set; } = string.Empty;
}

