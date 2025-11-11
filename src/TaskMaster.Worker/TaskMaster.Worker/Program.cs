using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaskMaster.Core.Data;
using TaskMaster.Worker.Configuration;
using TaskMaster.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure WorkerServiceOptions
builder.Services.Configure<WorkerServiceOptions>(
    builder.Configuration.GetSection(WorkerServiceOptions.SectionName));

// Validate configuration
var workerOptions = builder.Configuration.GetSection(WorkerServiceOptions.SectionName)
    .Get<WorkerServiceOptions>() ?? new WorkerServiceOptions();

if (string.IsNullOrWhiteSpace(workerOptions.ApiBaseUrl))
{
    throw new InvalidOperationException("WorkerService:ApiBaseUrl is required in configuration");
}

// Configure Entity Framework Core with SQLite
var dbPath = DatabaseHelper.GetDatabasePath();
var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register services
builder.Services.AddSingleton<FileWatcherService>();
builder.Services.AddHttpClient();

// Configure the worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    host.Run();
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Worker service terminated unexpectedly");
    throw;
}

/// <summary>
/// Worker service that monitors file changes
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly FileWatcherService _fileWatcherService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<WorkerServiceOptions> _options;
    private readonly HashSet<int> _monitoredProjectIds = new();

    public Worker(
        ILogger<Worker> logger,
        FileWatcherService fileWatcherService,
        IServiceProvider serviceProvider,
        IOptions<WorkerServiceOptions> options)
    {
        _logger = logger;
        _fileWatcherService = fileWatcherService;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task Master Worker Service starting...");
        _logger.LogInformation("API Base URL: {ApiBaseUrl}", _options.Value.ApiBaseUrl);
        _logger.LogInformation("Project check interval: {Interval} minutes", 
            _options.Value.ProjectCheckIntervalMinutes);

        try
        {
            // Start monitoring projects
            await _fileWatcherService.StartMonitoringAsync();

            // Periodically check for new projects to monitor
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForNewProjectsAsync(stoppingToken);
                    await Task.Delay(
                        TimeSpan.FromMinutes(_options.Value.ProjectCheckIntervalMinutes), 
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in worker loop");
                    // Wait a shorter time before retrying on error
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in worker service");
            throw;
        }
        finally
        {
            _logger.LogInformation("Task Master Worker Service stopping...");
        }
    }

    private async Task CheckForNewProjectsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var projects = await context.Projects
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var newProjectsCount = 0;
            var errorCount = 0;

            foreach (var project in projects)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Skip if already being monitored
                if (_monitoredProjectIds.Contains(project.Id))
                {
                    continue;
                }

                try
                {
                    // Validate project before monitoring
                    if (string.IsNullOrWhiteSpace(project.FullPath))
                    {
                        _logger.LogWarning(
                            "Skipping project {ProjectId} ({ProjectName}): FullPath is empty",
                            project.Id, project.Name);
                        continue;
                    }

                    if (!Directory.Exists(project.FullPath))
                    {
                        _logger.LogWarning(
                            "Skipping project {ProjectId} ({ProjectName}): Directory does not exist: {Path}",
                            project.Id, project.Name, project.FullPath);
                        continue;
                    }

                    await _fileWatcherService.StartMonitoringProjectAsync(project);
                    _monitoredProjectIds.Add(project.Id);
                    newProjectsCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex,
                        "Failed to start monitoring project {ProjectId} ({ProjectName})",
                        project.Id, project.Name);
                }
            }

            // Check for removed projects
            var removedProjects = _monitoredProjectIds
                .Where(id => !projects.Any(p => p.Id == id))
                .ToList();

            foreach (var projectId in removedProjects)
            {
                _fileWatcherService.StopMonitoringProject(projectId);
                _monitoredProjectIds.Remove(projectId);
                _logger.LogInformation("Stopped monitoring removed project {ProjectId}", projectId);
            }

            if (newProjectsCount > 0 || removedProjects.Count > 0)
            {
                _logger.LogInformation(
                    "Project monitoring update: {NewCount} new, {RemovedCount} removed, {ErrorCount} errors",
                    newProjectsCount, removedProjects.Count, errorCount);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for new projects");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}
