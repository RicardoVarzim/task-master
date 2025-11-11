using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaskMaster.Blazor.Models;

namespace TaskMaster.Blazor.Services;

/// <summary>
/// Service for interacting with the Metrics API
/// </summary>
public class MetricsService
{
    private readonly HttpClient _httpClient;

    public MetricsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5000");
    }

    public async System.Threading.Tasks.Task<ProjectMetrics?> GetProjectMetricsAsync(int projectId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProjectMetrics>($"/api/metrics/project/{projectId}");
        }
        catch
        {
            return null;
        }
    }

    public async System.Threading.Tasks.Task<OverviewMetrics?> GetOverviewMetricsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<OverviewMetrics>("/api/metrics/overview");
        }
        catch
        {
            return null;
        }
    }

    public async System.Threading.Tasks.Task<TeamMetrics?> GetTeamMetricsAsync(int teamId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TeamMetrics>($"/api/metrics/team/{teamId}");
        }
        catch
        {
            return null;
        }
    }
}

