using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs;
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
        public async Task<ApiResponse<List<PublicationMainInfo>?>> GetPublicationsAsync(int pageNumber = 1, int pageSize = 15, 
            PublicationAvailability availability = PublicationAvailability.All, 
            PublicationsSorting sorting = PublicationsSorting.None, 
            CancellationToken ct = default)
        {
            var requestUrl = $"api/publications?" +
                    $"pageNumber={pageNumber}&pageSize={pageSize}&" +
                    $"availability={GetAvailabilityText(availability)}&sort={GetSortingText(sorting)}";
            return await ApiClient<List<PublicationMainInfo>?>.GetJsonResponseAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Retrieves all data for a single publication by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PublicationMainInfo?>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var requestUrl = $"api/publications/{id}";
            return await ApiClient<PublicationMainInfo?>.GetJsonResponseAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Multiple image retrieval (after initial publication data loading) to avoid long query strings.
        /// </summary>
        /// <param name="imageIds">Image IDs that are cached within publication DTO</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<List<PublicationImage>>> GetImagesAsync(List<int> imageIds, CancellationToken ct = default)
        {

            var requestUrl = $"api/publications/image_ids?{string.Join("&", imageIds.Select(id => $"ids={id}"))}";
            return await ApiClient<List<PublicationImage>>.GetJsonResponseAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Retrieves the total count of publications from database.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<int>> GetPublicationsCountAsync(CancellationToken ct = default)
        {
            string requestUrl = "api/publications/count";
            return await ApiClient<int>.GetJsonResponseAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Retrieves all search matches for a given query string.
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<List<FoundMatch>>> GetAllSearchMatchesAsync(string queryString, CancellationToken ct = default)
        {
            string requestUrl = $"api/matchsearch?query={Uri.EscapeDataString(queryString)}";
            return await ApiClient<List<FoundMatch>>.GetJsonResponseAsync(requestUrl, _http, ct);
        }
    }
}
