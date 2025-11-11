using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TaskMaster.Host.Services;

/// <summary>
/// Manages the lifecycle of Task Master services (API, Worker, Blazor)
/// </summary>
public class ServiceManager
{
    private Process? _apiProcess;
    private Process? _workerProcess;
    private Process? _blazorProcess;
    private readonly HttpClient _httpClient;
    private const int ApiPort = 5000;
    private const int BlazorPort = 5001;

    public ServiceManager()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(2);
    }

    public bool IsApiRunning { get; private set; }
    public bool IsWorkerRunning { get; private set; }
    public bool IsBlazorRunning { get; private set; }

    /// <summary>
    /// Starts all services
    /// </summary>
    public async Task StartAllAsync()
    {
        await StartApiAsync();
        await Task.Delay(2000); // Wait for API to start
        await StartWorkerAsync();
        await Task.Delay(1000); // Wait a bit more
        await StartBlazorAsync();
    }

    /// <summary>
    /// Stops all services
    /// </summary>
    public void StopAll()
    {
        StopBlazor();
        StopWorker();
        StopApi();
    }

    public async Task StartApiAsync()
    {
        if (IsApiRunning) return;

        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var apiDll = FindDll("TaskMaster.API.dll", baseDir,
                Path.Combine(baseDir, "..", "..", "..", "..", "TaskMaster.API", "TaskMaster.API", "bin", "Debug", "net8.0"),
                Path.Combine(baseDir, "..", "..", "..", "..", "TaskMaster.API", "TaskMaster.API", "bin", "Release", "net8.0"));

            if (string.IsNullOrEmpty(apiDll))
            {
                throw new FileNotFoundException("TaskMaster.API.dll not found");
            }

            _apiProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{apiDll}\" --urls http://localhost:{ApiPort}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(apiDll)
                }
            };

            _apiProcess.Start();
            IsApiRunning = true;

            // Wait for API to be ready
            await WaitForServiceAsync($"http://localhost:{ApiPort}/api/projects", 30);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting API: {ex.Message}");
            IsApiRunning = false;
            throw;
        }
    }

    public async Task StartWorkerAsync()
    {
        if (IsWorkerRunning) return;

        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var workerDll = FindDll("TaskMaster.Worker.dll", baseDir,
                Path.Combine(baseDir, "..", "..", "..", "..", "TaskMaster.Worker", "TaskMaster.Worker", "bin", "Debug", "net8.0"),
                Path.Combine(baseDir, "..", "..", "..", "..", "TaskMaster.Worker", "TaskMaster.Worker", "bin", "Release", "net8.0"));

            if (string.IsNullOrEmpty(workerDll))
            {
                throw new FileNotFoundException("TaskMaster.Worker.dll not found");
            }

            _workerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{workerDll}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(workerDll)
                }
            };

            _workerProcess.Start();
            IsWorkerRunning = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting Worker: {ex.Message}");
            IsWorkerRunning = false;
            throw;
        }
    }

    public async Task StartBlazorAsync()
    {
        if (IsBlazorRunning) return;

        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var blazorDll = FindDll("TaskMaster.Blazor.dll", baseDir,
                Path.Combine(baseDir, "..", "..", "..", "..", "TaskMaster.Blazor", "TaskMaster.Blazor", "bin", "Debug", "net8.0"),
                Path.Combine(baseDir, "..", "..", "..", "..", "TaskMaster.Blazor", "TaskMaster.Blazor", "bin", "Release", "net8.0"));

            if (string.IsNullOrEmpty(blazorDll))
            {
                throw new FileNotFoundException("TaskMaster.Blazor.dll not found");
            }

            _blazorProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{blazorDll}\" --urls http://localhost:{BlazorPort}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(blazorDll)
                }
            };

            _blazorProcess.Start();
            IsBlazorRunning = true;

            // Wait for Blazor to be ready, then open browser
            await WaitForServiceAsync($"http://localhost:{BlazorPort}", 30);
            await Task.Delay(1000);
            Process.Start(new ProcessStartInfo
            {
                FileName = $"http://localhost:{BlazorPort}",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting Blazor: {ex.Message}");
            IsBlazorRunning = false;
            throw;
        }
    }

    public void StopApi()
    {
        if (_apiProcess != null && !_apiProcess.HasExited)
        {
            try
            {
                _apiProcess.Kill();
                _apiProcess.WaitForExit(5000);
            }
            catch { }
            _apiProcess.Dispose();
            _apiProcess = null;
        }
        IsApiRunning = false;
    }

    public void StopWorker()
    {
        if (_workerProcess != null && !_workerProcess.HasExited)
        {
            try
            {
                _workerProcess.Kill();
                _workerProcess.WaitForExit(5000);
            }
            catch { }
            _workerProcess.Dispose();
            _workerProcess = null;
        }
        IsWorkerRunning = false;
    }

    public void StopBlazor()
    {
        if (_blazorProcess != null && !_blazorProcess.HasExited)
        {
            try
            {
                _blazorProcess.Kill();
                _blazorProcess.WaitForExit(5000);
            }
            catch { }
            _blazorProcess.Dispose();
            _blazorProcess = null;
        }
        IsBlazorRunning = false;
    }

    private async Task WaitForServiceAsync(string url, int maxAttempts = 30)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch { }

            await Task.Delay(1000);
        }
    }

    private string? FindDll(string dllName, params string[] searchPaths)
    {
        foreach (var path in searchPaths)
        {
            var fullPath = Path.Combine(path, dllName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }
        return null;
    }

    public void Dispose()
    {
        StopAll();
        _httpClient?.Dispose();
    }
}

