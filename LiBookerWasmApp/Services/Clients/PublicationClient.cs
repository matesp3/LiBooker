using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs;
using LiBooker.Shared.EndpointParams;
using System.Net.Http.Json;
using static LiBooker.Shared.EndpointParams.PublicationParams;

namespace LiBookerWasmApp.Services.Clients
{
    public class PublicationClient
    {
        private readonly HttpClient _http;

        public PublicationClient(HttpClient http) => _http = http;

        /// <summary>
        /// Retrieves a paginated list of publications with main info only (no images, becauses it costs vast amount of resources).
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<List<PublicationMainInfo>?>> GetAllAsync(int pageNumber = 1, int pageSize = 15, 
            PublicationAvailability availability = PublicationAvailability.All, 
            PublicationsSorting sorting = PublicationsSorting.None, 
            CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetAsync($"api/publications?" +
                    $"pageNumber={pageNumber}&pageSize={pageSize}&" +
                    $"availability={GetAvailabilityText(availability)}&sort={GetSortingText(sorting)}",
                    ct).ConfigureAwait(false);
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

        /// <summary>
        /// Retrieves all data for a single publication by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
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
        public async Task<ApiResponse<List<PublicationImage>>> GetImagesAsync(List<int> imageIds, CancellationToken ct = default)
        {
            try
            {
                var queryString = string.Join("&", imageIds.Select(id => $"ids={id}"));
                var res = await _http.GetAsync($"api/publications/image_ids?{queryString}", ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    var data = await res.Content.
                        ReadFromJsonAsync<List<PublicationImage>>(cancellationToken: ct).ConfigureAwait(false) ?? [];
                    return ApiResponse<List<PublicationImage>>.Success(data);
                }
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ApiResponse<List<PublicationImage>>.Fail("Not found", (int)res.StatusCode);
                var textFallback = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return ApiResponse<List<PublicationImage>>.Fail($"Server returned {(int)res.StatusCode}: {textFallback}", (int)res.StatusCode);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<List<PublicationImage>>.Fail("Request cancelled");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PublicationImage>>.Fail($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the total count of publications from database.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<int> GetPublicationsCountAsync(CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetAsync("api/publications/count", ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    var count = await res.Content.ReadFromJsonAsync<int>(ct).ConfigureAwait(false);
                    return count;
                }
                return -1;
            }
            catch (OperationCanceledException)
            {
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
