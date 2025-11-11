using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskMaster.API.Extensions;
using TaskMaster.API.Hubs;
using TaskMaster.API.Models;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;
using TaskModel = TaskMaster.Core.Models.Task;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;

namespace TaskMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITaskParsingService _parsingService;
    private readonly ITaskUpdateService _updateService;
    private readonly IHubContext<SyncHub> _hubContext;
    private readonly IHistoryService _historyService;

    public TasksController(
        AppDbContext context, 
        ITaskParsingService parsingService,
        ITaskUpdateService updateService,
        IHubContext<SyncHub> hubContext,
        IHistoryService historyService)
    {
        _context = context;
        _parsingService = parsingService;
        _updateService = updateService;
        _hubContext = hubContext;
        _historyService = historyService;
    }

    /// <summary>
    /// Gets all tasks with optional filters and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskModel>>> GetTasks(
        [FromQuery] int? projectId = null,
        [FromQuery] bool? isCompleted = null,
        [FromQuery] TaskPriority? priority = null,
        [FromQuery] TaskStatusModel? status = null,
        [FromQuery] string? tags = null,
        [FromQuery] string? sortBy = "created",
        [FromQuery] string? sortOrder = "desc",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validate pagination parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Include(t => t.Assignments)
            .ThenInclude(a => a.TeamMember)
            .AsQueryable();

        // Apply filters
        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (isCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == isCompleted.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(t => t.Tags.Any(tag => tagList.Contains(tag.Name)));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "priority" => sortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.Priority.HasValue).ThenBy(t => t.Priority ?? TaskPriority.Low)
                : query.OrderByDescending(t => t.Priority.HasValue).ThenByDescending(t => t.Priority ?? TaskPriority.Low),
            "status" => sortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.Status)
                : query.OrderByDescending(t => t.Status),
            "project" => sortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.Project.Name)
                : query.OrderByDescending(t => t.Project.Name),
            "updated" => sortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.UpdatedAt)
                : query.OrderByDescending(t => t.UpdatedAt),
            _ => sortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt)
        };

        var pagedQuery = new PagedQuery { PageNumber = pageNumber, PageSize = pageSize };
        var result = await query.ToPagedResultAsync(pagedQuery);

        return Ok(result);
    }

    /// <summary>
    /// Gets a task by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskModel>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Include(t => t.Assignments)
            .ThenInclude(a => a.TeamMember)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        return task;
    }

    /// <summary>
    /// Gets the raw Markdown content of the file associated with a task
    /// </summary>
    [HttpGet("{id}/document")]
    public async Task<ActionResult<string>> GetTaskDocument(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        if (!System.IO.File.Exists(task.SourceFilePath))
        {
            return NotFound("Source file not found");
        }

        var content = await System.IO.File.ReadAllTextAsync(task.SourceFilePath);
        return content;
    }

    /// <summary>
    /// Updates a task partially (PATCH)
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ActionResult<TaskModel>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Include(t => t.Assignments)
            .ThenInclude(a => a.TeamMember)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound($"Task with ID {id} not found");
        }

        var changes = new List<string>();

        // Track changes for history
        if (request.IsCompleted.HasValue && task.IsCompleted != request.IsCompleted.Value)
        {
            changes.Add($"Completion changed from {task.IsCompleted} to {request.IsCompleted.Value}");
            task.IsCompleted = request.IsCompleted.Value;
            
            // Auto-update status if completing/uncompleting
            if (request.IsCompleted.Value)
            {
                task.Status = TaskStatusModel.Completed;
            }
            else if (task.Status == TaskStatusModel.Completed)
            {
                task.Status = TaskStatusModel.Pending;
            }
        }

        if (request.Status.HasValue && task.Status != request.Status.Value)
        {
            changes.Add($"Status changed from {task.Status} to {request.Status.Value}");
            task.Status = request.Status.Value;
            
            // Auto-update completion if status is Completed
            if (request.Status.Value == TaskStatusModel.Completed)
            {
                task.IsCompleted = true;
            }
        }

        if (request.Priority.HasValue && task.Priority != request.Priority.Value)
        {
            changes.Add($"Priority changed from {task.Priority} to {request.Priority.Value}");
            task.Priority = request.Priority.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && task.Description != request.Description)
        {
            changes.Add("Description updated");
            task.Description = request.Description.Trim();
        }

        task.UpdatedAt = DateTime.UtcNow;

        // Update the Markdown file
        var updateOptions = new TaskUpdateOptions
        {
            UpdateCompletion = request.IsCompleted.HasValue,
            UpdateStatus = request.Status.HasValue,
            UpdatePriority = request.Priority.HasValue,
            UpdateDescription = !string.IsNullOrWhiteSpace(request.Description)
        };

        var fileUpdated = await _updateService.UpdateTaskInFileAsync(task, updateOptions);

        if (!fileUpdated)
        {
            // Log warning but continue - task is updated in DB even if file update fails
        }

        await _context.SaveChangesAsync();

        // Record change history
        if (changes.Any())
        {
            await _historyService.RecordTaskChangeAsync(
                task.Id,
                "Updated",
                string.Join("; ", changes),
                null, // Author name - could be extracted from request in the future
                null, // Author email
                null  // Git commit ID
            );
        }

        // Notify clients via SignalR
        await _hubContext.Clients.All.SendAsync("TasksUpdated");

        // Reload task with all includes
        await _context.Entry(task).ReloadAsync();
        task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Include(t => t.Assignments)
            .ThenInclude(a => a.TeamMember)
            .FirstOrDefaultAsync(t => t.Id == id);

        return Ok(task);
    }

    /// <summary>
    /// Updates the Markdown file associated with a task
    /// </summary>
    [HttpPut("{id}/document")]
    public async Task<IActionResult> UpdateTaskDocument(int id, [FromBody] UpdateDocumentRequest request)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        if (!System.IO.File.Exists(task.SourceFilePath))
        {
            return NotFound("Source file not found");
        }

        await System.IO.File.WriteAllTextAsync(task.SourceFilePath, request.Content);
        
        // Trigger sync notification
        await _hubContext.Clients.All.SendAsync("TasksUpdated");
        
        return NoContent();
    }
}

public class UpdateTaskRequest
{
    public bool? IsCompleted { get; set; }
    public TaskStatusModel? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public string? Description { get; set; }
}

public class UpdateDocumentRequest
{
    [Required(ErrorMessage = "Content is required")]
    [MinLength(1, ErrorMessage = "Content cannot be empty")]
    public string Content { get; set; } = string.Empty;
}

