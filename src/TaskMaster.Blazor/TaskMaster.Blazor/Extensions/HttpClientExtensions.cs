using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace TaskMaster.Blazor.Extensions;

/// <summary>
/// Extension methods for HttpClient to support PATCH requests
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Sends a PATCH request to the specified URI containing the value serialized as JSON.
    /// </summary>
    public static async Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value)
    {
        var content = JsonContent.Create(value);
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = content
        };
        return await client.SendAsync(request);
    }
}

