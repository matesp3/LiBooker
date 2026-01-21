using LiBooker.Blazor.Client.Models;
using System.Net.Http.Json;

namespace LiBookerWasmApp.Services.Clients
{
    public static class ApiClient<T>
    {
        public static async Task<ApiResponse<T>> GetJsonAsync(string requestedUrl, HttpClient http, CancellationToken ct = default)
        {
            try
            {
                var res = await http.GetAsync(requestedUrl, ct).ConfigureAwait(false);
                return await ParseHttpMessage(res, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<T>.Cancelation();
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Fail($"Request failed: {ex.Message}");
            }
        }

        public static async Task<ApiResponse<T>> PostJsonAsync(string requestedUrl, T content, HttpClient http, CancellationToken ct = default)
        {
            try
            {
                var res = await http.PostAsJsonAsync(requestedUrl, content, ct).ConfigureAwait(false);
                return await ParseHttpMessage(res, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<T>.Cancelation();
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Fail($"Request failed: {ex.Message}");
            }
        }

        public static async Task<ApiResponse<T>> PutJsonAsync(string requestedUrl, T content, HttpClient http, CancellationToken ct = default)
        {
            try
            {
                var res = await http.PutAsJsonAsync(requestedUrl, content, ct).ConfigureAwait(false);
                return await ParseHttpMessage(res, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<T>.Cancelation();
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Fail($"Request failed: {ex.Message}");
            }
        }

        private static async Task<ApiResponse<T>> ParseHttpMessage(HttpResponseMessage res, CancellationToken ct)
        {
            try
            {
                if (res.IsSuccessStatusCode)
                {
                    var mediaType = res.Content.Headers.ContentType?.MediaType;
                    if (mediaType is null || !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
                    {
                        var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                        var preview = text.Length > 300 ? text.Substring(0, 300) + "..." : text;
                        return ApiResponse<T>.Fail($"Expected JSON but server returned content-type '{mediaType ?? "null"}'. Response preview: {preview}");
                    }
                    var data = await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct).ConfigureAwait(false);
                    return ApiResponse<T>.Success(data);
                }
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ApiResponse<T>.Fail("Not found", (int)res.StatusCode);
                var textFallback = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return ApiResponse<T>.Fail($"Server returned {(int)res.StatusCode}: {textFallback}", (int)res.StatusCode);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<T>.Cancelation();
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Fail($"Request failed: {ex.Message}");
            }
        }
    }
}
