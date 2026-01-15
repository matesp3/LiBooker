using System.Net.Http.Json;
using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs;

namespace LiBookerWasmApp.Services.Clients;

public class PersonClient
{
    private readonly HttpClient _http;
    public PersonClient(HttpClient http) => _http = http;

    public async Task<ApiResponse<List<Person>?>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetAsync("api/persons", ct).ConfigureAwait(false);
            if (res.IsSuccessStatusCode)
            {
                var mediaType = res.Content.Headers.ContentType?.MediaType;
                if (mediaType is null || !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
                {
                    var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                    var preview = text.Length > 300 ? text.Substring(0, 300) + "..." : text;
                    return ApiResponse<List<Person>?>.Fail($"Expected JSON but server returned content-type '{mediaType ?? "null"}'. Response preview: {preview}");
                }

                var data = await res.Content.ReadFromJsonAsync<List<Person>>(cancellationToken: ct).ConfigureAwait(false);
                return ApiResponse<List<Person>?>.Success(data ?? new List<Person>());
            }

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ApiResponse<List<Person>?>.Fail("Not found", (int)res.StatusCode);

            var textFallback = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return ApiResponse<List<Person>?>.Fail($"Server returned {(int)res.StatusCode}: {textFallback}", (int)res.StatusCode);
        }
        catch (OperationCanceledException)
        {
            return ApiResponse<List<Person>?>.Fail("Request cancelled");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Person>?>.Fail($"Request failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Person?>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetAsync($"api/persons/{id}", ct).ConfigureAwait(false);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadFromJsonAsync<Person>(cancellationToken: ct).ConfigureAwait(false);
                return ApiResponse<Person?>.Success(data);
            }

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ApiResponse<Person?>.Fail("Not found", (int)res.StatusCode);

            var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return ApiResponse<Person?>.Fail($"Server returned {(int)res.StatusCode}: {text}", (int)res.StatusCode);
        }
        catch (OperationCanceledException)
        {
            return ApiResponse<Person?>.Fail("Request cancelled");
        }
        catch (Exception ex)
        {
            return ApiResponse<Person?>.Fail($"Request failed: {ex.Message}");
        }
    }
}