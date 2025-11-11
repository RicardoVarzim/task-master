using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service for managing history and check-ins
/// </summary>
public class HistoryService : IHistoryService
{
    private readonly AppDbContext _context;

    public HistoryService(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<CheckInHistory> CreateCheckInAsync(int projectId, DateTime checkInDate, string content,
        List<int>? completedTaskIds = null, List<int>? inProgressTaskIds = null, List<string>? blockers = null)
    {
        var checkIn = new CheckInHistory
        {
            ProjectId = projectId,
            CheckInDate = checkInDate,
            Content = content,
            CompletedTasksJson = completedTaskIds != null ? JsonSerializer.Serialize(completedTaskIds) : null,
            InProgressTasksJson = inProgressTaskIds != null ? JsonSerializer.Serialize(inProgressTaskIds) : null,
            BlockersJson = blockers != null ? JsonSerializer.Serialize(blockers) : null
        };

        _context.CheckInHistories.Add(checkIn);
        await _context.SaveChangesAsync();

        return checkIn;
    }

    public async System.Threading.Tasks.Task<List<CheckInHistory>> GetCheckInHistoryAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.CheckInHistories
            .Where(c => c.ProjectId == projectId);

        if (startDate.HasValue)
            query = query.Where(c => c.CheckInDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.CheckInDate <= endDate.Value);

        return await query.OrderByDescending(c => c.CheckInDate).ToListAsync();
    }

    public async System.Threading.Tasks.Task<CheckInHistory?> GetCheckInByIdAsync(int checkInId)
    {
        return await _context.CheckInHistories
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == checkInId);
    }

    public async System.Threading.Tasks.Task<TaskChangeHistory> RecordTaskChangeAsync(int taskId, string changeType, string? changeDescription = null,
        string? authorName = null, string? authorEmail = null, int? gitCommitId = null)
    {
        var change = new TaskChangeHistory
        {
            TaskId = taskId,
            ChangeType = changeType,
            ChangeDescription = changeDescription,
            AuthorName = authorName,
            AuthorEmail = authorEmail,
            GitCommitId = gitCommitId,
            ChangedAt = DateTime.UtcNow
        };

        _context.TaskChangeHistories.Add(change);
        await _context.SaveChangesAsync();

        return change;
    }

    public async System.Threading.Tasks.Task<List<TaskChangeHistory>> GetTaskChangeHistoryAsync(int taskId)
    {
        return await _context.TaskChangeHistories
            .Where(c => c.TaskId == taskId)
            .Include(c => c.GitCommit)
            .OrderByDescending(c => c.ChangedAt)
            .ToListAsync();
    }
}

