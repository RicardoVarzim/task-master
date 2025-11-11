using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service interface for managing weekly reports
/// </summary>
public interface IWeeklyReportService
{
    /// <summary>
    /// Generates a weekly report for a project based on a template
    /// </summary>
    System.Threading.Tasks.Task<WeeklyReport> GenerateWeeklyReportAsync(int projectId, DateTime weekStart, DateTime weekEnd, string? templateContent = null);

    /// <summary>
    /// Gets weekly reports for a project
    /// </summary>
    System.Threading.Tasks.Task<List<WeeklyReport>> GetWeeklyReportsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets a weekly report by ID
    /// </summary>
    System.Threading.Tasks.Task<WeeklyReport?> GetWeeklyReportByIdAsync(int reportId);

    /// <summary>
    /// Saves or updates a weekly report
    /// </summary>
    System.Threading.Tasks.Task<WeeklyReport> SaveWeeklyReportAsync(WeeklyReport report);

    /// <summary>
    /// Deletes a weekly report
    /// </summary>
    System.Threading.Tasks.Task DeleteWeeklyReportAsync(int reportId);
}

