namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a change to a task tracked via Git
/// </summary>
public class TaskChangeHistory
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Task
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Navigation property to the Task
    /// </summary>
    public Task Task { get; set; } = null!;

    /// <summary>
    /// Foreign key to the GitCommit
    /// </summary>
    public int? GitCommitId { get; set; }

    /// <summary>
    /// Navigation property to the GitCommit
    /// </summary>
    public GitCommit? GitCommit { get; set; }

    /// <summary>
    /// Type of change (Created, Updated, Completed, etc.)
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Description of what changed
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Author of the change (from Git or manual)
    /// </summary>
    public string? AuthorName { get; set; }

    /// <summary>
    /// Author email (from Git or manual)
    /// </summary>
    public string? AuthorEmail { get; set; }

    /// <summary>
    /// Date and time when the change occurred
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Date when this change was tracked
    /// </summary>
    public DateTime TrackedAt { get; set; } = DateTime.UtcNow;
}

