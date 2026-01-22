using LiBooker.Blazor.Client.Models;

namespace LiBookerWasmApp.Services.Clients
{
    public class BookClient(HttpClient http) : ICustomClient
    {
        private readonly HttpClient _http = http;

        /// <summary>
        /// Retrieves the description of a book by its ID.
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string?>> GetBookDescriptionAsync(int bookId, CancellationToken ct = default)
        {
            var requestUrl = $"api/books/description/{bookId}";
            return await ApiClient<string?>.GetJsonAsync(requestUrl, _http, ct);
        }
    }
}
