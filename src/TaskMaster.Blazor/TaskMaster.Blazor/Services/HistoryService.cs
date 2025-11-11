using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaskMaster.Core.Models;

namespace TaskMaster.Blazor.Services;

/// <summary>
/// Service for interacting with the History API
/// </summary>
public class HistoryService
{
    private readonly HttpClient _httpClient;

    public HistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5000"); // TODO: Make configurable
    }

    public async System.Threading.Tasks.Task<List<CheckInHistory>> GetCheckInHistoryAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var queryParams = new List<string> { $"projectId={projectId}" };
        if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var queryString = "?" + string.Join("&", queryParams);
        var response = await _httpClient.GetFromJsonAsync<List<CheckInHistory>>($"/api/history{queryString}");
        return response ?? new List<CheckInHistory>();
    }
}

