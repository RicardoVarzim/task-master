namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a member of a team
/// </summary>
public class TeamMember
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the team member
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the team member
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Git username for mapping commits
    /// </summary>
    public string? GitUsername { get; set; }

    /// <summary>
    /// Foreign key to the Team
    /// </summary>
    public int TeamId { get; set; }

    /// <summary>
    /// Navigation property to the Team
    /// </summary>
    public Team Team { get; set; } = null!;

    /// <summary>
    /// Task assignments for this team member
    /// </summary>
    public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    /// <summary>
    /// Date when the member was added to the team
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

