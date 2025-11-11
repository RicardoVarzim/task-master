namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a Git commit that modified task files
/// </summary>
public class GitCommit
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Commit hash (SHA)
    /// </summary>
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>
    /// Author name from Git
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Author email from Git
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>
    /// Commit message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Date and time of the commit
    /// </summary>
    public DateTime CommitDate { get; set; }

    /// <summary>
    /// Foreign key to the Project
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the Project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Branch name where the commit was made
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Change history entries associated with this commit
    /// </summary>
    public ICollection<TaskChangeHistory> ChangeHistory { get; set; } = new List<TaskChangeHistory>();

    /// <summary>
    /// Date when this commit was first tracked
    /// </summary>
    public DateTime TrackedAt { get; set; } = DateTime.UtcNow;
}

