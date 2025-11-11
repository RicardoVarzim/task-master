namespace TaskMaster.Core.Models;

/// <summary>
/// Represents an assignment of a team member to a task with a specific role
/// </summary>
public class TaskAssignment
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
    /// Foreign key to the TeamMember
    /// </summary>
    public int TeamMemberId { get; set; }

    /// <summary>
    /// Navigation property to the TeamMember
    /// </summary>
    public TeamMember TeamMember { get; set; } = null!;

    /// <summary>
    /// Role assigned to this team member for this task
    /// </summary>
    public TaskRole Role { get; set; }

    /// <summary>
    /// Date when the assignment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the assignment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

