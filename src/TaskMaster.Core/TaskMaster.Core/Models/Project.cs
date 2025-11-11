namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a monitored project
/// </summary>
public class Project
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the project folder
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full absolute path to the project folder
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Git repository path (if Git is detected)
    /// </summary>
    public string? GitRepositoryPath { get; set; }

    /// <summary>
    /// Tasks associated with this project
    /// </summary>
    public ICollection<Task> Tasks { get; set; } = new List<Task>();

    /// <summary>
    /// Teams associated with this project
    /// </summary>
    public ICollection<Team> Teams { get; set; } = new List<Team>();

    /// <summary>
    /// Date when the project was added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the project was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

