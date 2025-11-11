using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskMaster.API.Hubs;
using TaskMaster.Core.Data;
using TaskMaster.Core.Services;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITaskParsingService _parsingService;
    private readonly IHubContext<SyncHub> _hubContext;

    public SyncController(
        AppDbContext context,
        ITaskParsingService parsingService,
        IHubContext<SyncHub> hubContext)
    {
        _context = context;
        _parsingService = parsingService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Triggers a manual synchronization of all projects
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SyncAllProjects()
    {
        var projects = await _context.Projects.ToListAsync();

        foreach (var project in projects)
        {
            await SyncProject(project.Id);
        }

        // Notify clients
        await _hubContext.Clients.All.SendAsync("TasksUpdated");

        return Ok(new { message = "Sync completed", projectsSynced = projects.Count });
    }

    /// <summary>
    /// Syncs a specific project
    /// </summary>
    [HttpPost("project/{projectId}")]
    public async Task<IActionResult> SyncProject(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return NotFound();
        }

        var tasksFolder = Path.Combine(project.FullPath, ".tasks");
        if (!Directory.Exists(tasksFolder))
        {
            return BadRequest("Tasks folder not found");
        }

        var markdownFiles = Directory.GetFiles(tasksFolder, "*.md", SearchOption.AllDirectories);
        
        foreach (var filePath in markdownFiles)
        {
            var content = await System.IO.File.ReadAllTextAsync(filePath);
            var parsedTasks = _parsingService.ParseTasks(filePath, content, projectId);

            // Remove existing tasks from this file
            var existingTasks = await _context.Tasks
                .Where(t => t.SourceFilePath == filePath)
                .ToListAsync();
            _context.Tasks.RemoveRange(existingTasks);

            // Add new tasks
            foreach (var parsedTask in parsedTasks)
            {
                var task = new TaskModel
                {
                    Description = parsedTask.Description,
                    IsCompleted = parsedTask.IsCompleted,
                    SourceFilePath = filePath,
                    LineNumber = parsedTask.LineNumber,
                    ProjectId = projectId,
                    Priority = parsedTask.Priority,
                    Status = parsedTask.Status ?? TaskMaster.Core.Models.TaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add tags
                foreach (var tagName in parsedTask.Tags)
                {
                    var tag = await _context.TaskTags
                        .FirstOrDefaultAsync(t => t.Name == tagName);
                    
                    if (tag == null)
                    {
                        tag = new TaskMaster.Core.Models.TaskTag { Name = tagName };
                        _context.TaskTags.Add(tag);
                    }

                    task.Tags.Add(tag);
                }

                _context.Tasks.Add(task);
            }
        }

        await _context.SaveChangesAsync();

        // Notify clients
        await _hubContext.Clients.All.SendAsync("TasksUpdated");

        return Ok(new { message = "Project synced", tasksFound = markdownFiles.Length });
    }

    /// <summary>
    /// Internal endpoint for Worker Service to notify about updates
    /// </summary>
    [HttpPost("internal/notify-update")]
    public async Task<IActionResult> NotifyUpdate()
    {
        await _hubContext.Clients.All.SendAsync("TasksUpdated");
        return Ok();
    }
}

