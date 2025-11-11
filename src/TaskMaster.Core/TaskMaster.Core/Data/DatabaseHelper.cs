using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TaskMaster.Core.Data;

/// <summary>
/// Helper class for database configuration
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// Gets the database path in %LOCALAPPDATA%\TaskMasterApp\
    /// </summary>
    public static string GetDatabasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDataFolder = Path.Combine(localAppData, "TaskMasterApp");
        
        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }

        return Path.Combine(appDataFolder, "taskmaster.db");
    }

    /// <summary>
    /// Configures DbContext options for SQLite
    /// </summary>
    public static DbContextOptions<AppDbContext> GetDbContextOptions()
    {
        var dbPath = GetDatabasePath();
        var connectionString = $"Data Source={dbPath}";

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    /// <summary>
    /// Creates a new AppDbContext instance
    /// </summary>
    public static AppDbContext CreateDbContext()
    {
        return new AppDbContext(GetDbContextOptions());
    }
}

