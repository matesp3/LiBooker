using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs;
using static LiBooker.Shared.EndpointParams.PublicationParams;

namespace LiBookerWasmApp.Services.Clients
{
    public class PublicationClient(HttpClient http) : ICustomClient
    {
        private readonly HttpClient _http = http;

        /// <summary>
        /// Retrieves a paginated list of publications with main info only (no images, becauses it costs vast amount of resources).
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<List<PublicationMainInfo>?>> GetPublicationsAsync(int pageNumber, int pageSize,
            int? bookId, int? authorId, int? genreId,
            PublicationAvailability availability = PublicationAvailability.All, 
            PublicationsSorting sorting = PublicationsSorting.None, 
            CancellationToken ct = default)
        {
            var requestUrl = $"api/publications?" +
                    $"pageNumber={pageNumber}&pageSize={pageSize}&" +
                    $"{(bookId.HasValue ? $"bookId={bookId.Value}&" : string.Empty)}" +
                    $"{(authorId.HasValue ? $"authorId={authorId.Value}&" : string.Empty)}" +
                    $"{(genreId.HasValue ? $"genreId={genreId.Value}&" : string.Empty)}" +
                    $"availability={GetAvailabilityText(availability)}&sort={GetSortingText(sorting)}";
            return await ApiClient<List<PublicationMainInfo>?>.GetJsonAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Retrieves basic details for a single details specified by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PublicationMainInfo?>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var requestUrl = $"api/publications/{id}";
            return await ApiClient<PublicationMainInfo?>.GetJsonAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Multiple image retrieval (after initial details data loading) to avoid long query strings.
        /// </summary>
        /// <param name="imageIds">Image IDs that are cached within details DTO</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<List<PublicationImage>>> GetImagesAsync(List<int> imageIds, CancellationToken ct = default)
        {

            var requestUrl = $"api/publications/image_ids?{string.Join("&", imageIds.Select(id => $"ids={id}"))}";
            return await ApiClient<List<PublicationImage>>.GetJsonAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Retrieves the total count of publications from database.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<int>> GetPublicationsCountAsync(int? bookId, int? authorId, int? genreId, 
            PublicationAvailability availability, CancellationToken ct = default)
        {
            string requestUrl = "api/publications/count?" +
                $"{(bookId.HasValue ? $"bookId={bookId.Value}&" : string.Empty)}" +
                $"{(authorId.HasValue ? $"authorId={authorId.Value}&" : string.Empty)}" +
                $"{(genreId.HasValue ? $"genreId={genreId.Value}&" : string.Empty)}" +
                $"onlyAvailable={(availability == PublicationAvailability.AvailableOnly)}";
            return await ApiClient<int>.GetJsonAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Retrieves details for specific details identified by its id.
        /// </summary>
        /// <param name="publicationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PublicationDetails?>> GetPublicationDetailsAsync(int publicationId, CancellationToken ct)
        {
            string requestUrl = $"/api/publications/details/{publicationId}";
            return await ApiClient<PublicationDetails?>.GetJsonAsync(requestUrl, _http, ct);
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
            return await ApiClient<List<FoundMatch>>.GetJsonAsync(requestUrl, _http, ct);
        }
    }
}
