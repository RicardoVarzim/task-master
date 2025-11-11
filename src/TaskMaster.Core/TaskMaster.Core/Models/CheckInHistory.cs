namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a check-in entry in the history
/// </summary>
public class CheckInHistory
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Date of the check-in
    /// </summary>
    public DateTime CheckInDate { get; set; }

    /// <summary>
    /// Foreign key to the Project
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the Project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Check-in content/notes
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tasks completed (JSON array of task IDs)
    /// </summary>
    public string? CompletedTasksJson { get; set; }

    /// <summary>
    /// Tasks in progress (JSON array of task IDs)
    /// </summary>
    public string? InProgressTasksJson { get; set; }

    /// <summary>
    /// Blockers/issues (JSON array)
    /// </summary>
    public string? BlockersJson { get; set; }

    /// <summary>
    /// Date when the check-in was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

