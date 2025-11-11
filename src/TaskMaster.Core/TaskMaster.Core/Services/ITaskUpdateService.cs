using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service for updating tasks in Markdown files
/// </summary>
public interface ITaskUpdateService
{
    /// <summary>
    /// Updates a task in its source Markdown file
    /// </summary>
    Task<bool> UpdateTaskInFileAsync(TaskModel task, TaskUpdateOptions options);
}

public class TaskUpdateOptions
{
    public bool UpdateCompletion { get; set; }
    public bool UpdateStatus { get; set; }
    public bool UpdatePriority { get; set; }
    public bool UpdateDescription { get; set; }
}

