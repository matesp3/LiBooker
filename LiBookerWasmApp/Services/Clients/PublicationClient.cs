using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace LiBookerWasmApp.Services.Clients
{
    public class PublicationClient
    {
        private readonly HttpClient _http;

        public PublicationClient(HttpClient http) => _http = http;

        public async Task<ApiResponse<List<PublicationMainInfo>?>> GetAllAsync(int pageNumber = 1, int pageSize = 15, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetAsync($"api/publications?pageNumber={pageNumber}&pageSize={pageSize}", ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    var mediaType = res.Content.Headers.ContentType?.MediaType;
                    if (mediaType is null || !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
                    {
                        var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                        var preview = text.Length > 300 ? text.Substring(0, 300) + "..." : text;
                        return ApiResponse<List<PublicationMainInfo>?>.Fail($"Expected JSON but server returned content-type '{mediaType ?? "null"}'. Response preview: {preview}");
                    }
                    var data = await res.Content.ReadFromJsonAsync<List<PublicationMainInfo>>(cancellationToken: ct).ConfigureAwait(false);
                    return ApiResponse<List<PublicationMainInfo>?>.Success(data ?? new List<PublicationMainInfo>());
                }
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ApiResponse<List<PublicationMainInfo>?>.Fail("Not found", (int)res.StatusCode);
                var textFallback = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return ApiResponse<List<PublicationMainInfo>?>.Fail($"Server returned {(int)res.StatusCode}: {textFallback}", (int)res.StatusCode);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<List<PublicationMainInfo>?>.Fail("Request cancelled");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PublicationMainInfo>?>.Fail($"Request failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PublicationMainInfo?>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetAsync($"api/publications/{id}", ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    var data = await res.Content.ReadFromJsonAsync<PublicationMainInfo>(cancellationToken: ct).ConfigureAwait(false);
                    return ApiResponse<PublicationMainInfo?>.Success(data);
                }
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ApiResponse<PublicationMainInfo?>.Fail("Not found", (int)res.StatusCode);
                var textFallback = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return ApiResponse<PublicationMainInfo?>.Fail($"Server returned {(int)res.StatusCode}: {textFallback}", (int)res.StatusCode);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<PublicationMainInfo?>.Fail("Request cancelled");
            }
            catch (Exception ex)
            {
                return ApiResponse<PublicationMainInfo?>.Fail($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Multiple image retrieval (after initial publication data loading) to avoid long query strings.
        /// </summary>
        /// <param name="imageIds">Image IDs that are cached within publication DTO</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<List<PublicationImageDto>>> GetImagesAsync(List<int> imageIds, CancellationToken ct = default)
        {
            try
            {
                var queryString = string.Join("&", imageIds.Select(id => $"ids={id}"));
                var res = await _http.GetAsync($"api/publications/image_ids?{queryString}", ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    var data = await res.Content.
                        ReadFromJsonAsync<List<PublicationImageDto>>(cancellationToken: ct).ConfigureAwait(false) ?? [];
                    return ApiResponse<List<PublicationImageDto>>.Success(data);
                }
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ApiResponse<List<PublicationImageDto>>.Fail("Not found", (int)res.StatusCode);
                var textFallback = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return ApiResponse<List<PublicationImageDto>>.Fail($"Server returned {(int)res.StatusCode}: {textFallback}", (int)res.StatusCode);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<List<PublicationImageDto>>.Fail("Request cancelled");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PublicationImageDto>>.Fail($"Request failed: {ex.Message}");
            }
        }
    }
}
