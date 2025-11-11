namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a weekly report generated from tasks
/// </summary>
public class WeeklyReport
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Week number/year identifier (e.g., "2024-W01")
    /// </summary>
    public string WeekIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the week
    /// </summary>
    public DateTime WeekStartDate { get; set; }

    /// <summary>
    /// End date of the week
    /// </summary>
    public DateTime WeekEndDate { get; set; }

    /// <summary>
    /// Foreign key to the Project
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the Project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Report content in Markdown format
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Statistics JSON (tasks completed, bugs resolved, etc.)
    /// </summary>
    public string? StatisticsJson { get; set; }

    /// <summary>
    /// Date when the report was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the report was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

