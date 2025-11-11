namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a tag that can be associated with tasks
/// </summary>
public class TaskTag
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the tag (without the # symbol)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tasks associated with this tag
    /// </summary>
    public ICollection<Task> Tasks { get; set; } = new List<Task>();

    /// <summary>
    /// Date when the tag was first created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

