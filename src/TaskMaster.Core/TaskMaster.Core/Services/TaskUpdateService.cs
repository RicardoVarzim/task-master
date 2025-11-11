using System.Text;
using System.Text.RegularExpressions;
using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service for updating tasks in Markdown files
/// </summary>
public class TaskUpdateService : ITaskUpdateService
{
    private static readonly Regex TaskLineRegex = new(@"^(\s*)[-*]\s*\[([x\s])\]\s*(.+)$", RegexOptions.Compiled | RegexOptions.Multiline);

    public async Task<bool> UpdateTaskInFileAsync(TaskModel task, TaskUpdateOptions options)
    {
        if (string.IsNullOrWhiteSpace(task.SourceFilePath) || !File.Exists(task.SourceFilePath))
        {
            return false;
        }

        try
        {
            var content = await File.ReadAllTextAsync(task.SourceFilePath);
            var lines = content.Split('\n').ToList();

            if (task.LineNumber <= 0 || task.LineNumber > lines.Count)
            {
                return false;
            }

            var lineIndex = task.LineNumber - 1;
            var originalLine = lines[lineIndex];
            var updatedLine = BuildUpdatedTaskLine(originalLine, task, options);

            if (updatedLine != originalLine)
            {
                lines[lineIndex] = updatedLine;
                var newContent = string.Join("\n", lines);
                await File.WriteAllTextAsync(task.SourceFilePath, newContent, Encoding.UTF8);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private string BuildUpdatedTaskLine(string originalLine, TaskModel task, TaskUpdateOptions options)
    {
        var match = TaskLineRegex.Match(originalLine);
        if (!match.Success)
        {
            return originalLine;
        }

        var indent = match.Groups[1].Value;
        var checkbox = match.Groups[2].Value;
        var description = match.Groups[3].Value.Trim();

        var sb = new StringBuilder();
        sb.Append(indent);
        sb.Append("- [");

        // Update completion status
        if (options.UpdateCompletion)
        {
            sb.Append(task.IsCompleted ? "x" : " ");
        }
        else
        {
            sb.Append(checkbox.Trim().Equals("x", StringComparison.OrdinalIgnoreCase) ? "x" : " ");
        }

        sb.Append("] ");

        // Update description
        if (options.UpdateDescription && !string.IsNullOrWhiteSpace(task.Description))
        {
            sb.Append(task.Description);
        }
        else
        {
            sb.Append(description);
        }

        // Add priority indicator if set
        if (options.UpdatePriority && task.Priority.HasValue)
        {
            var priorityEmoji = GetPriorityEmoji(task.Priority.Value);
            if (!sb.ToString().Contains(priorityEmoji))
            {
                sb.Append(" ");
                sb.Append(priorityEmoji);
            }
        }

        // Add status indicator if set
        if (options.UpdateStatus && task.Status != TaskStatusModel.Pending)
        {
            var statusText = GetStatusText(task.Status);
            if (!sb.ToString().Contains(statusText, StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(" ");
                sb.Append(statusText);
            }
        }

        // Preserve existing tags and assignments (they're in the description)
        // Tags and assignments are parsed from the description, so they should be preserved

        return sb.ToString();
    }

    private string GetPriorityEmoji(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Maximum => "🔴",
            TaskPriority.High => "🟠",
            TaskPriority.Medium => "🟡",
            TaskPriority.Low => "🔵",
            TaskPriority.Strategic => "🟣",
            TaskPriority.Maintenance => "📝",
            TaskPriority.Administrative => "📌",
            _ => ""
        };
    }

    private string GetStatusText(TaskStatusModel status)
    {
        return status switch
        {
            TaskStatusModel.Completed => "✅",
            TaskStatusModel.InProgress => "🔄",
            TaskStatusModel.Planned => "📋",
            TaskStatusModel.Blocked => "🚧",
            _ => ""
        };
    }
}

