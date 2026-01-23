using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs.Admin;

namespace LiBookerWasmApp.Services.Clients
{
    public class UserClient(HttpClient http) : ICustomClient
    {
        private readonly HttpClient _http = http;

        /// <summary>
        /// Searches for users by email fragment.
        /// </summary>
        public async Task<ApiResponse<List<UserManagement>>> SearchUsersByEmailAsync(string query, CancellationToken ct = default)
        {
            var requestUrl = $"api/users/search?email={Uri.EscapeDataString(query ?? string.Empty)}";
            //Console.WriteLine($"[UserClient] GET {requestUrl}");
            return await ApiClient<List<UserManagement>>.GetJsonAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Updates roles for a specific user.
        /// </summary>
        public async Task<ApiResponse<UserRolesUpdate>> UpdateUserRolesAsync(UserRolesUpdate request, CancellationToken ct = default)
        {
            var requestUrl = "api/users/edit/roles";
            return await ApiClient<UserRolesUpdate>.PutJsonAsync(requestUrl, request, _http, ct);
        }
    }
}