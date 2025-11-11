using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using TaskMaster.Core.Models;

namespace TaskMaster.Blazor.Services;

/// <summary>
/// Service for interacting with the Sync API
/// </summary>
public class SyncService
{
    private readonly HttpClient _httpClient;

    public SyncService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5000"); // TODO: Make configurable
    }

    public async System.Threading.Tasks.Task<List<SyncHistory>> GetSyncHistoryAsync(int projectId, int limit = 50)
    {
        var response = await _httpClient.GetFromJsonAsync<List<SyncHistory>>($"/api/sync/history/project/{projectId}?limit={limit}");
        return response ?? new List<SyncHistory>();
    }

    public async System.Threading.Tasks.Task<List<SyncHistory>> GetAllSyncHistoryAsync(int limit = 100)
    {
        var response = await _httpClient.GetFromJsonAsync<List<SyncHistory>>($"/api/sync/history?limit={limit}");
        return response ?? new List<SyncHistory>();
    }
}

