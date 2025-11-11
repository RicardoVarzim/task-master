using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;

namespace TaskMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MetricsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets metrics for a specific project
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<ProjectMetrics>> GetProjectMetrics(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return NotFound($"Project with ID {projectId} not found");
        }

        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Tags)
            .ToListAsync();

        var metrics = new ProjectMetrics
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.IsCompleted),
            PendingTasks = tasks.Count(t => t.Status == TaskStatusModel.Pending),
            InProgressTasks = tasks.Count(t => t.Status == TaskStatusModel.InProgress),
            BlockedTasks = tasks.Count(t => t.Status == TaskStatusModel.Blocked),
            PlannedTasks = tasks.Count(t => t.Status == TaskStatusModel.Planned),
            CompletionRate = tasks.Count > 0 
                ? (double)tasks.Count(t => t.IsCompleted) / tasks.Count * 100 
                : 0,
            TasksByPriority = tasks
                .Where(t => t.Priority.HasValue)
                .GroupBy(t => t.Priority!.Value)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            TasksByStatus = tasks
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            BugsResolved = tasks.Count(t => 
                t.IsCompleted && 
                t.Tags.Any(tag => tag.Name.Contains("Bug", StringComparison.OrdinalIgnoreCase))),
            CodeReviewsCompleted = tasks.Count(t => 
                t.IsCompleted && 
                t.Tags.Any(tag => tag.Name.Contains("Review", StringComparison.OrdinalIgnoreCase))),
            AverageTasksPerDay = CalculateAverageTasksPerDay(tasks),
            RecentActivity = await GetRecentActivity(projectId)
        };

        return Ok(metrics);
    }

    /// <summary>
    /// Gets overview metrics across all projects
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<OverviewMetrics>> GetOverviewMetrics()
    {
        var projects = await _context.Projects.ToListAsync();
        var allTasks = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .ToListAsync();

        var metrics = new OverviewMetrics
        {
            TotalProjects = projects.Count,
            TotalTasks = allTasks.Count,
            CompletedTasks = allTasks.Count(t => t.IsCompleted),
            PendingTasks = allTasks.Count(t => t.Status == TaskStatusModel.Pending),
            InProgressTasks = allTasks.Count(t => t.Status == TaskStatusModel.InProgress),
            BlockedTasks = allTasks.Count(t => t.Status == TaskStatusModel.Blocked),
            OverallCompletionRate = allTasks.Count > 0 
                ? (double)allTasks.Count(t => t.IsCompleted) / allTasks.Count * 100 
                : 0,
            TasksByProject = allTasks
                .GroupBy(t => t.Project.Name)
                .ToDictionary(g => g.Key, g => new ProjectTaskCounts
                {
                    Total = g.Count(),
                    Completed = g.Count(t => t.IsCompleted),
                    Pending = g.Count(t => t.Status == TaskStatusModel.Pending),
                    InProgress = g.Count(t => t.Status == TaskStatusModel.InProgress)
                }),
            TasksByPriority = allTasks
                .Where(t => t.Priority.HasValue)
                .GroupBy(t => t.Priority!.Value)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            BugsResolved = allTasks.Count(t => 
                t.IsCompleted && 
                t.Tags.Any(tag => tag.Name.Contains("Bug", StringComparison.OrdinalIgnoreCase))),
            CodeReviewsCompleted = allTasks.Count(t => 
                t.IsCompleted && 
                t.Tags.Any(tag => tag.Name.Contains("Review", StringComparison.OrdinalIgnoreCase)))
        };

        return Ok(metrics);
    }

    /// <summary>
    /// Gets metrics for a specific team
    /// </summary>
    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<TeamMetrics>> GetTeamMetrics(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            return NotFound($"Team with ID {teamId} not found");
        }

        var memberIds = team.Members.Select(m => m.Id).ToList();
        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == team.ProjectId)
            .Include(t => t.Assignments)
            .ThenInclude(a => a.TeamMember)
            .Include(t => t.Tags)
            .Where(t => t.Assignments.Any(a => memberIds.Contains(a.TeamMemberId)))
            .ToListAsync();

        var metrics = new TeamMetrics
        {
            TeamId = teamId,
            TeamName = team.Name,
            ProjectId = team.ProjectId,
            ProjectName = team.Project.Name,
            MemberCount = team.Members.Count,
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.IsCompleted),
            PendingTasks = tasks.Count(t => t.Status == TaskStatusModel.Pending),
            InProgressTasks = tasks.Count(t => t.Status == TaskStatusModel.InProgress),
            BlockedTasks = tasks.Count(t => t.Status == TaskStatusModel.Blocked),
            CompletionRate = tasks.Count > 0 
                ? (double)tasks.Count(t => t.IsCompleted) / tasks.Count * 100 
                : 0,
            TasksByMember = tasks
                .SelectMany(t => t.Assignments)
                .Where(a => memberIds.Contains(a.TeamMemberId))
                .GroupBy(a => a.TeamMember.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Ok(metrics);
    }

    private double CalculateAverageTasksPerDay(List<TaskModel> tasks)
    {
        if (tasks.Count == 0)
            return 0;

        var oldestTask = tasks.Min(t => t.CreatedAt);
        var newestTask = tasks.Max(t => t.CreatedAt);
        var days = (newestTask - oldestTask).TotalDays;

        return days > 0 ? tasks.Count / days : tasks.Count;
    }

    private async Task<Dictionary<string, int>> GetRecentActivity(int projectId)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.UpdatedAt >= thirtyDaysAgo)
            .ToListAsync();

        return tasks
            .GroupBy(t => t.UpdatedAt.Date)
            .OrderByDescending(g => g.Key)
            .Take(30)
            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());
    }
}

public class ProjectMetrics
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int PlannedTasks { get; set; }
    public double CompletionRate { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public int BugsResolved { get; set; }
    public int CodeReviewsCompleted { get; set; }
    public double AverageTasksPerDay { get; set; }
    public Dictionary<string, int> RecentActivity { get; set; } = new();
}

public class OverviewMetrics
{
    public int TotalProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public double OverallCompletionRate { get; set; }
    public Dictionary<string, ProjectTaskCounts> TasksByProject { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public int BugsResolved { get; set; }
    public int CodeReviewsCompleted { get; set; }
}

public class ProjectTaskCounts
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
}

public class TeamMetrics
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public double CompletionRate { get; set; }
    public Dictionary<string, int> TasksByMember { get; set; } = new();
}

