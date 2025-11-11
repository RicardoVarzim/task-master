using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service for managing weekly reports
/// </summary>
public class WeeklyReportService : IWeeklyReportService
{
    private readonly AppDbContext _context;

    public WeeklyReportService(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<WeeklyReport> GenerateWeeklyReportAsync(int projectId, DateTime weekStart, DateTime weekEnd, string? templateContent = null)
    {
        var weekIdentifier = GetWeekIdentifier(weekStart);
        
        // Get tasks for the period
        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Where(t => t.CreatedAt >= weekStart && t.CreatedAt <= weekEnd || 
                       t.UpdatedAt >= weekStart && t.UpdatedAt <= weekEnd)
            .Include(t => t.Tags)
            .Include(t => t.Assignments)
            .ThenInclude(a => a.TeamMember)
            .ToListAsync();

        var completedTasks = tasks.Where(t => t.IsCompleted && t.UpdatedAt >= weekStart && t.UpdatedAt <= weekEnd).ToList();
        var inProgressTasks = tasks.Where(t => !t.IsCompleted && t.Status == TaskStatusModel.InProgress).ToList();
        var bugsResolved = tasks.Count(t => t.Tags.Any(tag => tag.Name.Contains("Bug", StringComparison.OrdinalIgnoreCase)) && t.IsCompleted);
        var codeReviews = tasks.Count(t => t.Tags.Any(tag => tag.Name.Contains("Review", StringComparison.OrdinalIgnoreCase)) && t.IsCompleted);

        var statistics = new
        {
            TasksCompleted = completedTasks.Count,
            TasksCreated = tasks.Count(t => t.CreatedAt >= weekStart && t.CreatedAt <= weekEnd),
            BugsResolved = bugsResolved,
            CodeReviewsCompleted = codeReviews,
            TasksInProgress = inProgressTasks.Count
        };

        var report = new WeeklyReport
        {
            ProjectId = projectId,
            WeekIdentifier = weekIdentifier,
            WeekStartDate = weekStart,
            WeekEndDate = weekEnd,
            Content = templateContent ?? GenerateDefaultReportContent(weekStart, weekEnd, completedTasks, inProgressTasks),
            StatisticsJson = JsonSerializer.Serialize(statistics)
        };

        _context.WeeklyReports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async System.Threading.Tasks.Task<List<WeeklyReport>> GetWeeklyReportsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.WeeklyReports
            .Where(r => r.ProjectId == projectId);

        if (startDate.HasValue)
            query = query.Where(r => r.WeekStartDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.WeekEndDate <= endDate.Value);

        return await query.OrderByDescending(r => r.WeekStartDate).ToListAsync();
    }

    public async System.Threading.Tasks.Task<WeeklyReport?> GetWeeklyReportByIdAsync(int reportId)
    {
        return await _context.WeeklyReports
            .Include(r => r.Project)
            .FirstOrDefaultAsync(r => r.Id == reportId);
    }

    public async System.Threading.Tasks.Task<WeeklyReport> SaveWeeklyReportAsync(WeeklyReport report)
    {
        if (report.Id == 0)
        {
            _context.WeeklyReports.Add(report);
        }
        else
        {
            report.UpdatedAt = DateTime.UtcNow;
            _context.WeeklyReports.Update(report);
        }

        await _context.SaveChangesAsync();
        return report;
    }

    public async System.Threading.Tasks.Task DeleteWeeklyReportAsync(int reportId)
    {
        var report = await _context.WeeklyReports.FindAsync(reportId);
        if (report != null)
        {
            _context.WeeklyReports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }

    private string GetWeekIdentifier(DateTime weekStart)
    {
        var year = weekStart.Year;
        var week = GetWeekOfYear(weekStart);
        return $"{year}-W{week:D2}";
    }

    private int GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var calendar = culture.Calendar;
        return calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
    }

    private string GenerateDefaultReportContent(DateTime weekStart, DateTime weekEnd, List<TaskModel> completedTasks, List<TaskModel> inProgressTasks)
    {
        var content = $"# Weekly Report\n\n";
        content += $"**Week:** {weekStart:dd/MM} to {weekEnd:dd/MM/yyyy}\n\n";
        content += $"## Tasks Completed\n\n";
        
        foreach (var task in completedTasks)
        {
            content += $"- [x] {task.Description}\n";
        }

        content += $"\n## Tasks In Progress\n\n";
        foreach (var task in inProgressTasks)
        {
            content += $"- [ ] {task.Description}\n";
        }

        return content;
    }
}

