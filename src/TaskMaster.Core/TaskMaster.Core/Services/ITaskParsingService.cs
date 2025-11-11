using TaskMaster.Core.Models;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service interface for parsing Markdown files and extracting tasks
/// </summary>
public interface ITaskParsingService
{
    /// <summary>
    /// Parses a Markdown file and extracts all tasks
    /// </summary>
    /// <param name="filePath">Path to the Markdown file</param>
    /// <param name="content">Content of the Markdown file</param>
    /// <param name="projectId">ID of the project this file belongs to</param>
    /// <returns>List of extracted tasks</returns>
    List<ParsedTask> ParseTasks(string filePath, string content, int projectId);

    /// <summary>
    /// Extracts tags from text
    /// </summary>
    /// <param name="text">Text to search for tags</param>
    /// <returns>List of tag names (without #)</returns>
    List<string> ExtractTags(string text);

    /// <summary>
    /// Extracts task assignments from text (format: @username:role or [role:username])
    /// </summary>
    /// <param name="text">Text to search for assignments</param>
    /// <returns>List of assignments</returns>
    List<TaskAssignmentInfo> ExtractAssignments(string text);

    /// <summary>
    /// Detects priority from text (emojis or text)
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <returns>Detected priority or null</returns>
    TaskPriority? DetectPriority(string text);

    /// <summary>
    /// Detects status from text (emojis or text)
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <returns>Detected status or null</returns>
    TaskStatusModel? DetectStatus(string text);
}

/// <summary>
/// Represents a parsed task with all extracted information
/// </summary>
public class ParsedTask
{
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int LineNumber { get; set; }
    public TaskPriority? Priority { get; set; }
    public TaskStatusModel? Status { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<TaskAssignmentInfo> Assignments { get; set; } = new();
}

/// <summary>
/// Represents a task assignment extracted from text
/// </summary>
public class TaskAssignmentInfo
{
    public string Username { get; set; } = string.Empty;
    public TaskRole Role { get; set; }
}

