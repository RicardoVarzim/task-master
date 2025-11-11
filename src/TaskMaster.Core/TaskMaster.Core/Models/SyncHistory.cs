namespace TaskMaster.Core.Models;

/// <summary>
/// Represents a synchronization history entry for a project
/// </summary>
public class SyncHistory
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Project that was synchronized
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the project
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// Type of synchronization (Manual, Automatic)
    /// </summary>
    public SyncType SyncType { get; set; } = SyncType.Manual;

    /// <summary>
    /// Status of the synchronization
    /// </summary>
    public SyncStatus Status { get; set; } = SyncStatus.Success;

    /// <summary>
    /// Number of markdown files processed
    /// </summary>
    public int FilesProcessed { get; set; }

    /// <summary>
    /// Number of tasks found during sync
    /// </summary>
    public int TasksFound { get; set; }

    /// <summary>
    /// Number of tasks added
    /// </summary>
    public int TasksAdded { get; set; }

    /// <summary>
    /// Number of tasks updated
    /// </summary>
    public int TasksUpdated { get; set; }

    /// <summary>
    /// Number of tasks removed
    /// </summary>
    public int TasksRemoved { get; set; }

    /// <summary>
    /// Error message if sync failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When the sync started
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the sync completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Duration of the sync in milliseconds
    /// </summary>
    public int? DurationMs => CompletedAt.HasValue 
        ? (int)(CompletedAt.Value - StartedAt).TotalMilliseconds 
        : null;
}

/// <summary>
/// Type of synchronization
/// </summary>
public enum SyncType
{
    /// <summary>
    /// Manual synchronization triggered by user
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Automatic synchronization triggered by file watcher
    /// </summary>
    Automatic = 1
}

/// <summary>
/// Status of synchronization
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Synchronization completed successfully
    /// </summary>
    Success = 0,

    /// <summary>
    /// Synchronization failed
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Synchronization completed with warnings
    /// </summary>
    Partial = 2
}

