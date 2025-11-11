using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaskMaster.Core.Models;

namespace TaskMaster.Blazor.Services;

/// <summary>
/// Service for interacting with the Weekly Reports API
/// </summary>
public class WeeklyReportService
{
    private readonly HttpClient _httpClient;

    public WeeklyReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5000"); // TODO: Make configurable
    }

    public async System.Threading.Tasks.Task<List<WeeklyReport>> GetWeeklyReportsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var queryParams = new List<string> { $"projectId={projectId}" };
        if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var queryString = "?" + string.Join("&", queryParams);
        var response = await _httpClient.GetFromJsonAsync<List<WeeklyReport>>($"/api/weekly-reports{queryString}");
        return response ?? new List<WeeklyReport>();
    }

    public async System.Threading.Tasks.Task<WeeklyReport?> GetWeeklyReportAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<WeeklyReport>($"/api/weekly-reports/{id}");
    }
}

