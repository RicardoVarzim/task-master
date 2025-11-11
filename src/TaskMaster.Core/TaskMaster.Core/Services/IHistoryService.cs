using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service interface for managing history and check-ins
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Creates a check-in entry
    /// </summary>
    System.Threading.Tasks.Task<CheckInHistory> CreateCheckInAsync(int projectId, DateTime checkInDate, string content, 
        List<int>? completedTaskIds = null, List<int>? inProgressTaskIds = null, List<string>? blockers = null);

    /// <summary>
    /// Gets check-in history for a project
    /// </summary>
    System.Threading.Tasks.Task<List<CheckInHistory>> GetCheckInHistoryAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets check-in by ID
    /// </summary>
    System.Threading.Tasks.Task<CheckInHistory?> GetCheckInByIdAsync(int checkInId);

    /// <summary>
    /// Records a task change in history
    /// </summary>
    System.Threading.Tasks.Task<TaskChangeHistory> RecordTaskChangeAsync(int taskId, string changeType, string? changeDescription = null, 
        string? authorName = null, string? authorEmail = null, int? gitCommitId = null);

    /// <summary>
    /// Gets change history for a task
    /// </summary>
    System.Threading.Tasks.Task<List<TaskChangeHistory>> GetTaskChangeHistoryAsync(int taskId);
}

