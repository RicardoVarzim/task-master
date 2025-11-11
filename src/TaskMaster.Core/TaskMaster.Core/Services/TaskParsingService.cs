using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using TaskMaster.Core.Models;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;

namespace TaskMaster.Core.Services;

/// <summary>
/// Service for parsing Markdown files and extracting tasks
/// </summary>
public class TaskParsingService : ITaskParsingService
{
    private static readonly Regex TagRegex = new(@"#(\w+)", RegexOptions.Compiled);
    private static readonly Regex AssignmentRegex1 = new(@"@(\w+):(\w+)", RegexOptions.Compiled);
    private static readonly Regex AssignmentRegex2 = new(@"\[(\w+):(\w+)\]", RegexOptions.Compiled);

    public List<ParsedTask> ParseTasks(string filePath, string content, int projectId)
    {
        var tasks = new List<ParsedTask>();
        var pipeline = new MarkdownPipelineBuilder().UseTaskLists().Build();
        var document = Markdown.Parse(content, pipeline);

        var lines = content.Split('\n');

        // Parse using Markdig - look for task list items
        foreach (var block in document.Descendants())
        {
            if (block is ListItemBlock listItem)
            {
                // Check if this list item contains a task list inline
                var taskListInline = listItem.Descendants<Markdig.Syntax.Inlines.Inline>()
                    .FirstOrDefault(i => i is Markdig.Syntax.Inlines.LiteralInline lit && 
                        (lit.Content.ToString().Contains("[ ]") || lit.Content.ToString().Contains("[x]")));
                
                if (taskListInline != null)
                {
                    var line = listItem.Line;
                    var isCompleted = false;
                    
                    // Check if it's checked
                    var contentStr = listItem.Descendants<Markdig.Syntax.Inlines.LiteralInline>()
                        .Select(l => l.Content.ToString())
                        .FirstOrDefault(s => s.Contains("[x]"));
                    isCompleted = contentStr != null;

                    if (line > 0 && line <= lines.Length)
                    {
                        var lineText = lines[line - 1];
                        var description = ExtractDescription(lineText);

                        var parsedTask = new ParsedTask
                        {
                            Description = description,
                            IsCompleted = isCompleted,
                            LineNumber = line,
                            Priority = DetectPriority(lineText),
                            Status = DetectStatus(lineText),
                            Tags = ExtractTags(lineText),
                            Assignments = ExtractAssignments(lineText)
                        };

                        tasks.Add(parsedTask);
                    }
                }
            }
        }

        // Also parse simple task list format: - [ ] or - [x]
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("- [ ]") || trimmedLine.StartsWith("- [x]") ||
                trimmedLine.StartsWith("* [ ]") || trimmedLine.StartsWith("* [x]"))
            {
                var isCompleted = trimmedLine.Contains("[x]");
                var description = ExtractDescription(trimmedLine);

                // Check if we already have this task from Markdig parsing
                if (!tasks.Any(t => t.LineNumber == i + 1))
                {
                    var parsedTask = new ParsedTask
                    {
                        Description = description,
                        IsCompleted = isCompleted,
                        LineNumber = i + 1,
                        Priority = DetectPriority(trimmedLine),
                        Status = DetectStatus(trimmedLine),
                        Tags = ExtractTags(trimmedLine),
                        Assignments = ExtractAssignments(trimmedLine)
                    };

                    tasks.Add(parsedTask);
                }
            }
        }

        return tasks;
    }

    public List<string> ExtractTags(string text)
    {
        var tags = new List<string>();
        var matches = TagRegex.Matches(text);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var tagName = match.Groups[1].Value;
                if (!tags.Contains(tagName, StringComparer.OrdinalIgnoreCase))
                {
                    tags.Add(tagName);
                }
            }
        }

        return tags;
    }

    public List<TaskAssignmentInfo> ExtractAssignments(string text)
    {
        var assignments = new List<TaskAssignmentInfo>();

        // Format: @username:role
        var matches1 = AssignmentRegex1.Matches(text);
        foreach (Match match in matches1)
        {
            if (match.Groups.Count >= 3)
            {
                var username = match.Groups[1].Value;
                var roleStr = match.Groups[2].Value;
                if (TryParseRole(roleStr, out var role))
                {
                    assignments.Add(new TaskAssignmentInfo { Username = username, Role = role });
                }
            }
        }

        // Format: [role:username]
        var matches2 = AssignmentRegex2.Matches(text);
        foreach (Match match in matches2)
        {
            if (match.Groups.Count >= 3)
            {
                var roleStr = match.Groups[1].Value;
                var username = match.Groups[2].Value;
                if (TryParseRole(roleStr, out var role))
                {
                    assignments.Add(new TaskAssignmentInfo { Username = username, Role = role });
                }
            }
        }

        return assignments;
    }

    public TaskPriority? DetectPriority(string text)
    {
        // Check for emoji indicators
        if (text.Contains("🔴") || text.Contains("Máxima") || text.Contains("Maximum", StringComparison.OrdinalIgnoreCase))
            return TaskPriority.Maximum;
        if (text.Contains("🟠") || text.Contains("Alta") || text.Contains("High", StringComparison.OrdinalIgnoreCase))
            return TaskPriority.High;
        if (text.Contains("🟡") || text.Contains("Média") || text.Contains("Medium", StringComparison.OrdinalIgnoreCase))
            return TaskPriority.Medium;
        if (text.Contains("🔵") || text.Contains("Baixa") || text.Contains("Low", StringComparison.OrdinalIgnoreCase))
            return TaskPriority.Low;
        if (text.Contains("🟣") || text.Contains("Estratégico") || text.Contains("Strategic", StringComparison.OrdinalIgnoreCase))
            return TaskPriority.Strategic;
        if (text.Contains("📝") || text.Contains("Manutenção") || text.Contains("Maintenance", StringComparison.OrdinalIgnoreCase))
            return TaskPriority.Maintenance;
        if (text.Contains("📌") || text.Contains("Administrativo") || text.Contains("Administrative", StringComparison.OrdinalIgnoreCase))
            return TaskPriority.Administrative;

        return null;
    }

    public TaskStatusModel? DetectStatus(string text)
    {
        // Check for emoji indicators
        if (text.Contains("✅") || text.Contains("Concluído") || text.Contains("Completed", StringComparison.OrdinalIgnoreCase))
            return TaskStatusModel.Completed;
        if (text.Contains("🔄") || text.Contains("Em Progresso") || text.Contains("In Progress", StringComparison.OrdinalIgnoreCase))
            return TaskStatusModel.InProgress;
        if (text.Contains("🟠") || text.Contains("Em Planeamento") || text.Contains("Planned", StringComparison.OrdinalIgnoreCase))
            return TaskStatusModel.Planned;
        if (text.Contains("🚧") || text.Contains("Bloqueado") || text.Contains("Blocked", StringComparison.OrdinalIgnoreCase))
            return TaskStatusModel.Blocked;
        if (text.Contains("📝") || text.Contains("Pendente") || text.Contains("Pending", StringComparison.OrdinalIgnoreCase))
            return TaskStatusModel.Pending;

        return null;
    }

    private string ExtractDescription(string line)
    {
        // Remove task list markers: - [ ] or - [x] or * [ ] or * [x]
        var description = Regex.Replace(line, @"^[\s]*[-*]\s*\[[x\s]\]\s*", "", RegexOptions.IgnoreCase);
        
        // Remove tags, assignments, and other metadata for clean description
        description = TagRegex.Replace(description, "");
        description = AssignmentRegex1.Replace(description, "");
        description = AssignmentRegex2.Replace(description, "");
        
        return description.Trim();
    }

    private bool TryParseRole(string roleStr, out TaskRole role)
    {
        roleStr = roleStr.Trim();
        
        if (Enum.TryParse<TaskRole>(roleStr, true, out role))
            return true;

        // Try common variations
        var roleLower = roleStr.ToLowerInvariant();
        role = roleLower switch
        {
            "req" or "requester" => TaskRole.Requester,
            "analyst" or "analysis" => TaskRole.Analyst,
            "dev" or "developer" => TaskRole.Developer,
            "rev" or "reviewer" or "review" => TaskRole.Reviewer,
            "test" or "tester" or "qa" => TaskRole.Tester,
            "mgr" or "manager" or "pm" => TaskRole.Manager,
            _ => TaskRole.Other
        };

        return true;
    }
}

