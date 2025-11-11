using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaskMaster.Blazor.Extensions;
using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;

namespace TaskMaster.Blazor.Services;

/// <summary>
/// Service for interacting with the Tasks API
/// </summary>
public class TaskService
{
    private readonly HttpClient _httpClient;

    public TaskService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5000"); // TODO: Make configurable
    }

    public async System.Threading.Tasks.Task<List<TaskModel>> GetTasksAsync(
        int? projectId = null,
        bool? isCompleted = null,
        TaskPriority? priority = null,
        TaskStatusModel? status = null,
        string? tags = null)
    {
        var queryParams = new List<string>();
        if (projectId.HasValue) queryParams.Add($"projectId={projectId.Value}");
        if (isCompleted.HasValue) queryParams.Add($"isCompleted={isCompleted.Value}");
        if (priority.HasValue) queryParams.Add($"priority={priority.Value}");
        if (status.HasValue) queryParams.Add($"status={status.Value}");
        if (!string.IsNullOrEmpty(tags)) queryParams.Add($"tags={Uri.EscapeDataString(tags)}");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        var response = await _httpClient.GetFromJsonAsync<List<TaskModel>>($"/api/tasks{queryString}");
        return response ?? new List<TaskModel>();
    }

    public async System.Threading.Tasks.Task<TaskModel?> GetTaskAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<TaskModel>($"/api/tasks/{id}");
    }

    public async System.Threading.Tasks.Task<string> GetTaskDocumentAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/tasks/{id}/document");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async System.Threading.Tasks.Task<TaskModel?> UpdateTaskAsync(int id, UpdateTaskRequest request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"/api/tasks/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TaskModel>();
    }

    public async System.Threading.Tasks.Task UpdateTaskDocumentAsync(int id, string content)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/tasks/{id}/document", new { Content = content });
        response.EnsureSuccessStatusCode();
    }

    public async System.Threading.Tasks.Task SyncAllProjectsAsync()
    {
        var response = await _httpClient.PostAsync("/api/sync", null);
        response.EnsureSuccessStatusCode();
    }
}

public class UpdateTaskRequest
{
    public bool? IsCompleted { get; set; }
    public TaskStatusModel? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public string? Description { get; set; }
}

