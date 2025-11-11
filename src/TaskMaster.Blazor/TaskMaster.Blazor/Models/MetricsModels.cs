namespace TaskMaster.Blazor.Models;

public class ProjectMetrics
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int PlannedTasks { get; set; }
    public double CompletionRate { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public int BugsResolved { get; set; }
    public int CodeReviewsCompleted { get; set; }
    public double AverageTasksPerDay { get; set; }
    public Dictionary<string, int> RecentActivity { get; set; } = new();
}

public class OverviewMetrics
{
    public int TotalProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public double OverallCompletionRate { get; set; }
    public Dictionary<string, ProjectTaskCounts> TasksByProject { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public int BugsResolved { get; set; }
    public int CodeReviewsCompleted { get; set; }
}

public class ProjectTaskCounts
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
}

public class TeamMetrics
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public double CompletionRate { get; set; }
    public Dictionary<string, int> TasksByMember { get; set; } = new();
}

