namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a task extracted from a Markdown file
/// </summary>
public class Task
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Description/text of the task
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether the task is completed (true if [x], false if [ ])
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Absolute path to the source Markdown file
    /// </summary>
    public string SourceFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number in the source file where this task appears
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Foreign key to the Project
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the Project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Priority of the task
    /// </summary>
    public TaskPriority? Priority { get; set; }

    /// <summary>
    /// Status of the task
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>
    /// Tags associated with this task
    /// </summary>
    public ICollection<TaskTag> Tags { get; set; } = new List<TaskTag>();

    /// <summary>
    /// Assignments (roles) for this task
    /// </summary>
    public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();

    /// <summary>
    /// Change history tracked via Git
    /// </summary>
    public ICollection<TaskChangeHistory> ChangeHistory { get; set; } = new List<TaskChangeHistory>();

    /// <summary>
    /// Date when the task was first created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the task was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

