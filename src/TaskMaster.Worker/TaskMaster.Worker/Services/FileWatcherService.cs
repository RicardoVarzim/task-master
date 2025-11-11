using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskMaster.Worker.Configuration;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.Worker.Services;

/// <summary>
/// Service that monitors file system changes for task files
/// </summary>
public class FileWatcherService : IDisposable
{
    private readonly ILogger<FileWatcherService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly WorkerServiceOptions _options;
    private readonly Dictionary<int, FileSystemWatcher> _watchers = new();
    private readonly Dictionary<string, DateTime> _lastChangeTime = new();
    private readonly SemaphoreSlim _syncSemaphore;
    private readonly Dictionary<int, System.Threading.Tasks.Task> _activeSyncs = new();

    public FileWatcherService(
        ILogger<FileWatcherService> logger,
        IServiceProvider serviceProvider,
        IOptions<WorkerServiceOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _syncSemaphore = new SemaphoreSlim(_options.MaxConcurrentSyncs, _options.MaxConcurrentSyncs);
    }

    /// <summary>
    /// Starts monitoring all projects
    /// </summary>
    public async System.Threading.Tasks.Task StartMonitoringAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var projects = await context.Projects.ToListAsync();
            
            var successCount = 0;
            var failureCount = 0;

            foreach (var project in projects)
            {
                try
                {
                    await StartMonitoringProjectAsync(project);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex, "Failed to start monitoring project {ProjectId} ({ProjectName})", 
                        project.Id, project.Name);
                }
            }

            _logger.LogInformation("Started monitoring {SuccessCount} projects ({FailureCount} failed)", 
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting project monitoring");
            throw;
        }
    }

    /// <summary>
    /// Starts monitoring a specific project
    /// </summary>
    public async System.Threading.Tasks.Task StartMonitoringProjectAsync(Project project)
    {
        if (project == null)
        {
            _logger.LogWarning("Cannot start monitoring: project is null");
            throw new ArgumentNullException(nameof(project));
        }

        if (_watchers.ContainsKey(project.Id))
        {
            _logger.LogWarning("Project {ProjectId} ({ProjectName}) is already being monitored", 
                project.Id, project.Name);
            return;
        }

        // Validate project path
        if (string.IsNullOrWhiteSpace(project.FullPath))
        {
            _logger.LogError("Cannot start monitoring project {ProjectId}: FullPath is empty", project.Id);
            throw new InvalidOperationException($"Project {project.Id} has an invalid FullPath");
        }

        if (!Directory.Exists(project.FullPath))
        {
            _logger.LogWarning("Project directory not found for project {ProjectId}: {Path}", 
                project.Id, project.FullPath);
            return;
        }

        var tasksFolder = Path.Combine(project.FullPath, _options.TasksFolderName);
        if (!Directory.Exists(tasksFolder))
        {
            _logger.LogWarning("Tasks folder not found for project {ProjectId}: {Path}. " +
                "The folder will be created automatically when tasks are added.", project.Id, tasksFolder);
            // Don't return - we can still monitor the parent directory
        }

        try
        {
            var watcher = new FileSystemWatcher(tasksFolder)
            {
                Filter = _options.TaskFilePattern,
                IncludeSubdirectories = _options.IncludeSubdirectories,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };

            watcher.Created += async (sender, e) => await OnFileChangedAsync(project.Id, e.FullPath, "Created");
            watcher.Changed += async (sender, e) => await OnFileChangedAsync(project.Id, e.FullPath, "Changed");
            watcher.Deleted += async (sender, e) => await OnFileChangedAsync(project.Id, e.FullPath, "Deleted");
            watcher.Renamed += async (sender, e) => await OnFileRenamedAsync(project.Id, e.OldFullPath, e.FullPath);
            watcher.Error += (sender, e) => OnWatcherError(project.Id, e.GetException());

            watcher.EnableRaisingEvents = true;
            _watchers[project.Id] = watcher;

            _logger.LogInformation("Started monitoring project {ProjectId} ({ProjectName}) at {Path}", 
                project.Id, project.Name, tasksFolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file watcher for project {ProjectId} at {Path}", 
                project.Id, tasksFolder);
            throw;
        }
    }

    /// <summary>
    /// Stops monitoring a specific project
    /// </summary>
    public void StopMonitoringProject(int projectId)
    {
        if (_watchers.TryGetValue(projectId, out var watcher))
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                _watchers.Remove(projectId);
                
                // Clean up change tracking for this project
                var keysToRemove = _lastChangeTime.Keys
                    .Where(k => k.StartsWith($"{projectId}:", StringComparison.Ordinal))
                    .ToList();
                foreach (var key in keysToRemove)
                {
                    _lastChangeTime.Remove(key);
                }

                _logger.LogInformation("Stopped monitoring project {ProjectId}", projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping monitoring for project {ProjectId}", projectId);
            }
        }
    }

    /// <summary>
    /// Handles file watcher errors
    /// </summary>
    private void OnWatcherError(int projectId, Exception? exception)
    {
        _logger.LogError(exception, "File watcher error for project {ProjectId}", projectId);
        
        // Attempt to restart monitoring for this project
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5)); // Wait before retry
                
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var project = await context.Projects.FindAsync(projectId);
                
                if (project != null)
                {
                    StopMonitoringProject(projectId);
                    await StartMonitoringProjectAsync(project);
                    _logger.LogInformation("Restarted monitoring for project {ProjectId} after error", projectId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart monitoring for project {ProjectId}", projectId);
            }
        });
    }

    private async System.Threading.Tasks.Task OnFileChangedAsync(int projectId, string filePath, string changeType)
    {
        // Validate file path
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("Received file change event with empty file path for project {ProjectId}", projectId);
            return;
        }

        // Debounce: ignore rapid successive changes
        var key = $"{projectId}:{filePath}";
        var now = DateTime.UtcNow;
        
        if (_lastChangeTime.TryGetValue(key, out var lastChange))
        {
            var timeSinceLastChange = (now - lastChange).TotalMilliseconds;
            if (timeSinceLastChange < _options.DebounceDelayMs)
            {
                if (_options.EnableDetailedFileLogging)
                {
                    _logger.LogDebug("Ignoring rapid change to {FilePath} (debounce)", filePath);
                }
                return;
            }
        }

        _lastChangeTime[key] = now;

        if (_options.EnableDetailedFileLogging)
        {
            _logger.LogInformation("File {ChangeType}: {FilePath} in project {ProjectId}", changeType, filePath, projectId);
        }
        else
        {
            _logger.LogDebug("File {ChangeType}: {FilePath} in project {ProjectId}", changeType, filePath, projectId);
        }

        // Wait a bit for file operations to complete
        await System.Threading.Tasks.Task.Delay(_options.DebounceDelayMs);

        await SyncProjectAsync(projectId);
    }

    private async System.Threading.Tasks.Task OnFileRenamedAsync(int projectId, string oldPath, string newPath)
    {
        if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newPath))
        {
            _logger.LogWarning("Received file rename event with invalid paths for project {ProjectId}", projectId);
            return;
        }

        _logger.LogInformation("File renamed in project {ProjectId}: {OldPath} -> {NewPath}", 
            projectId, oldPath, newPath);
        
        // Remove old path from change tracking
        var oldKey = $"{projectId}:{oldPath}";
        _lastChangeTime.Remove(oldKey);

        await System.Threading.Tasks.Task.Delay(_options.DebounceDelayMs);
        await SyncProjectAsync(projectId);
    }

    private async System.Threading.Tasks.Task SyncProjectAsync(int projectId)
    {
        // Check if there's already an active sync for this project
        if (_activeSyncs.ContainsKey(projectId))
        {
            if (_options.EnableDetailedFileLogging)
            {
                _logger.LogDebug("Sync already in progress for project {ProjectId}, skipping", projectId);
            }
            return;
        }

        // Limit concurrent syncs
        await _syncSemaphore.WaitAsync();
        
        var syncTask = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                _activeSyncs[projectId] = System.Threading.Tasks.Task.CompletedTask;
                
                await SyncProjectWithRetryAsync(projectId);
            }
            finally
            {
                _activeSyncs.Remove(projectId);
                _syncSemaphore.Release();
            }
        });

        await syncTask;
    }

    private async System.Threading.Tasks.Task SyncProjectWithRetryAsync(int projectId)
    {
        var attempt = 0;
        var maxAttempts = _options.MaxRetryAttempts;

        while (attempt < maxAttempts)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                
                var httpClient = httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_options.ApiBaseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(_options.ApiTimeoutSeconds);

                var response = await httpClient.PostAsync($"/api/sync/project/{projectId}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully synced project {ProjectId} (attempt {Attempt})", 
                        projectId, attempt + 1);
                    return;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Failed to sync project {ProjectId}: {StatusCode} - {Error} (attempt {Attempt}/{MaxAttempts})",
                        projectId, response.StatusCode, errorContent, attempt + 1, maxAttempts);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                attempt++;
                _logger.LogWarning(
                    "Timeout syncing project {ProjectId} (attempt {Attempt}/{MaxAttempts})",
                    projectId, attempt, maxAttempts);
            }
            catch (HttpRequestException ex)
            {
                attempt++;
                _logger.LogWarning(ex,
                    "HTTP error syncing project {ProjectId} (attempt {Attempt}/{MaxAttempts}): {Message}",
                    projectId, attempt, maxAttempts, ex.Message);
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogError(ex,
                    "Unexpected error syncing project {ProjectId} (attempt {Attempt}/{MaxAttempts})",
                    projectId, attempt, maxAttempts);
            }

            // Wait before retry (exponential backoff)
            if (attempt < maxAttempts)
            {
                var delaySeconds = _options.RetryDelaySeconds * attempt;
                _logger.LogInformation(
                    "Retrying sync for project {ProjectId} in {DelaySeconds} seconds...",
                    projectId, delaySeconds);
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        _logger.LogError(
            "Failed to sync project {ProjectId} after {MaxAttempts} attempts",
            projectId, maxAttempts);
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing FileWatcherService...");

        foreach (var watcher in _watchers.Values)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing file watcher");
            }
        }
        _watchers.Clear();
        _lastChangeTime.Clear();
        _syncSemaphore?.Dispose();

        _logger.LogInformation("FileWatcherService disposed");
    }
}

