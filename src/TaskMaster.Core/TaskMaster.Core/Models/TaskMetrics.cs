namespace TaskMaster.Core.Models;

/// <summary>
/// Represents aggregated metrics for tasks
/// </summary>
public class TaskMetrics
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Project
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the Project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Period start date
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period end date
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Number of tasks completed in this period
    /// </summary>
    public int TasksCompleted { get; set; }

    /// <summary>
    /// Number of tasks created in this period
    /// </summary>
    public int TasksCreated { get; set; }

    /// <summary>
    /// Number of bugs resolved
    /// </summary>
    public int BugsResolved { get; set; }

    /// <summary>
    /// Number of code reviews completed
    /// </summary>
    public int CodeReviewsCompleted { get; set; }

    /// <summary>
    /// Additional metrics as JSON
    /// </summary>
    public string? AdditionalMetricsJson { get; set; }

    /// <summary>
    /// Date when the metrics were calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

