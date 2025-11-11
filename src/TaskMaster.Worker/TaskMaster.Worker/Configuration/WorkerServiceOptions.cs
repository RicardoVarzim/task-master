namespace TaskMaster.Worker.Configuration;

/// <summary>
/// Configuration options for the Task Master Worker Service
/// </summary>
public class WorkerServiceOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "WorkerService";

    /// <summary>
    /// Base URL of the Task Master API
    /// </summary>
    public string ApiBaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>
    /// Timeout for HTTP requests to the API (in seconds)
    /// </summary>
    public int ApiTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Delay in milliseconds to debounce rapid file system changes
    /// </summary>
    public int DebounceDelayMs { get; set; } = 500;

    /// <summary>
    /// Interval in minutes to check for new projects to monitor
    /// </summary>
    public int ProjectCheckIntervalMinutes { get; set; } = 1;

    /// <summary>
    /// Maximum number of retry attempts for failed sync operations
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay in seconds between retry attempts
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Pattern for task files to monitor (default: "*.md")
    /// </summary>
    public string TaskFilePattern { get; set; } = "*.md";

    /// <summary>
    /// Name of the tasks folder within each project (default: ".tasks")
    /// </summary>
    public string TasksFolderName { get; set; } = ".tasks";

    /// <summary>
    /// Whether to include subdirectories when monitoring task files
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;

    /// <summary>
    /// Maximum number of concurrent sync operations
    /// </summary>
    public int MaxConcurrentSyncs { get; set; } = 5;

    /// <summary>
    /// Whether to enable detailed logging for file system events
    /// </summary>
    public bool EnableDetailedFileLogging { get; set; } = false;
}

