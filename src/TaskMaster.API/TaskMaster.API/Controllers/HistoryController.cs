using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;

namespace TaskMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHistoryService _historyService;

    public HistoryController(AppDbContext context, IHistoryService historyService)
    {
        _context = context;
        _historyService = historyService;
    }

    /// <summary>
    /// Gets change history for a specific task
    /// </summary>
    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<IEnumerable<TaskChangeHistory>>> GetTaskHistory(int taskId)
    {
        // Verify task exists
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return NotFound($"Task with ID {taskId} not found");
        }

        var history = await _historyService.GetTaskChangeHistoryAsync(taskId);
        return Ok(history);
    }

    /// <summary>
    /// Gets check-in history for a specific project
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<CheckInHistory>>> GetProjectHistory(
        int projectId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Verify project exists
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return NotFound($"Project with ID {projectId} not found");
        }

        var history = await _historyService.GetCheckInHistoryAsync(projectId, startDate, endDate);
        return Ok(history);
    }

    /// <summary>
    /// Gets general history across all projects
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<HistoryOverview>> GetHistory(
        [FromQuery] int? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.TaskChangeHistories
            .Include(h => h.Task)
            .ThenInclude(t => t.Project)
            .Include(h => h.GitCommit)
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(h => h.Task.ProjectId == projectId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(h => h.ChangedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.ChangedAt <= endDate.Value);
        }

        var changes = await query
            .OrderByDescending(h => h.ChangedAt)
            .Take(100) // Limit to recent 100 changes
            .ToListAsync();

        var checkIns = new List<CheckInHistory>();
        if (projectId.HasValue)
        {
            checkIns = await _historyService.GetCheckInHistoryAsync(projectId.Value, startDate, endDate);
        }

        var overview = new HistoryOverview
        {
            TaskChanges = changes,
            CheckIns = checkIns,
            TotalChanges = changes.Count,
            TotalCheckIns = checkIns.Count
        };

        return Ok(overview);
    }

    /// <summary>
    /// Creates a new check-in entry
    /// </summary>
    [HttpPost("check-in")]
    public async Task<ActionResult<CheckInHistory>> CreateCheckIn([FromBody] CreateCheckInRequest request)
    {
        // Verify project exists
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
        {
            return NotFound($"Project with ID {request.ProjectId} not found");
        }

        var checkIn = await _historyService.CreateCheckInAsync(
            request.ProjectId,
            request.CheckInDate ?? DateTime.UtcNow,
            request.Content,
            request.CompletedTaskIds,
            request.InProgressTaskIds,
            request.Blockers);

        return CreatedAtAction(
            nameof(GetProjectHistory),
            new { projectId = checkIn.ProjectId },
            checkIn);
    }

    /// <summary>
    /// Records a task change manually
    /// </summary>
    [HttpPost("task-change")]
    public async Task<ActionResult<TaskChangeHistory>> RecordTaskChange([FromBody] RecordTaskChangeRequest request)
    {
        // Verify task exists
        var task = await _context.Tasks.FindAsync(request.TaskId);
        if (task == null)
        {
            return NotFound($"Task with ID {request.TaskId} not found");
        }

        var change = await _historyService.RecordTaskChangeAsync(
            request.TaskId,
            request.ChangeType,
            request.ChangeDescription,
            request.AuthorName,
            request.AuthorEmail,
            request.GitCommitId);

        return Ok(change);
    }
}

public class HistoryOverview
{
    public List<TaskChangeHistory> TaskChanges { get; set; } = new();
    public List<CheckInHistory> CheckIns { get; set; } = new();
    public int TotalChanges { get; set; }
    public int TotalCheckIns { get; set; }
}

public class CreateCheckInRequest
{
    [Required(ErrorMessage = "ProjectId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be greater than 0")]
    public int ProjectId { get; set; }
    
    public DateTime? CheckInDate { get; set; }
    
    [Required(ErrorMessage = "Content is required")]
    [MinLength(1, ErrorMessage = "Content cannot be empty")]
    public string Content { get; set; } = string.Empty;
    
    public List<int>? CompletedTaskIds { get; set; }
    public List<int>? InProgressTaskIds { get; set; }
    public List<string>? Blockers { get; set; }
}

public class RecordTaskChangeRequest
{
    [Required(ErrorMessage = "TaskId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TaskId must be greater than 0")]
    public int TaskId { get; set; }
    
    [Required(ErrorMessage = "ChangeType is required")]
    [MinLength(1, ErrorMessage = "ChangeType cannot be empty")]
    public string ChangeType { get; set; } = string.Empty;
    
    public string? ChangeDescription { get; set; }
    
    [MaxLength(100, ErrorMessage = "AuthorName cannot exceed 100 characters")]
    public string? AuthorName { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? AuthorEmail { get; set; }
    
    public int? GitCommitId { get; set; }
}

