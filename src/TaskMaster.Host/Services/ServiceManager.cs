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
            var apiDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskMaster.API.dll");
            if (!File.Exists(apiDll))
            {
                // Try to find it in the referenced project output
                var apiProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "TaskMaster.API", "TaskMaster.API", "bin", "Debug", "net8.0", "TaskMaster.API.dll");
                if (File.Exists(apiProjectPath))
                {
                    apiDll = apiProjectPath;
                }
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
                    RedirectStandardError = true
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
        }
    }

    public async Task StartWorkerAsync()
    {
        if (IsWorkerRunning) return;

        try
        {
            var workerDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskMaster.Worker.dll");
            if (!File.Exists(workerDll))
            {
                var workerProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "TaskMaster.Worker", "TaskMaster.Worker", "bin", "Debug", "net8.0", "TaskMaster.Worker.dll");
                if (File.Exists(workerProjectPath))
                {
                    workerDll = workerProjectPath;
                }
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
                    RedirectStandardError = true
                }
            };

            _workerProcess.Start();
            IsWorkerRunning = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting Worker: {ex.Message}");
            IsWorkerRunning = false;
        }
    }

    public async Task StartBlazorAsync()
    {
        if (IsBlazorRunning) return;

        try
        {
            var blazorDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskMaster.Blazor.dll");
            if (!File.Exists(blazorDll))
            {
                var blazorProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "TaskMaster.Blazor", "TaskMaster.Blazor", "bin", "Debug", "net8.0", "TaskMaster.Blazor.dll");
                if (File.Exists(blazorProjectPath))
                {
                    blazorDll = blazorProjectPath;
                }
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
                    RedirectStandardError = true
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

    public void Dispose()
    {
        StopAll();
        _httpClient?.Dispose();
    }
}

