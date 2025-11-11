namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a team associated with a project
/// </summary>
public class Team
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the team
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the Project
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the Project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Members of this team
    /// </summary>
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    /// <summary>
    /// Date when the team was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

