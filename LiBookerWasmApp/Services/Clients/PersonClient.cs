using System.Net.Http;
using System.Net.Http.Json;
using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.Models.DTOs;

namespace LiBooker.Blazor.Client.Services
{
    public class PersonClient
    {
        private readonly HttpClient _http;
        public PersonClient(HttpClient http) => _http = http;

        public async Task<ApiResponse<List<PersonDto>?>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetAsync("api/persons", ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    var data = await res.Content.ReadFromJsonAsync<List<PersonDto>>(cancellationToken: ct).ConfigureAwait(false);
                    return ApiResponse<List<PersonDto>?>.Success(data ?? new List<PersonDto>());
                }

                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ApiResponse<List<PersonDto>?>.Fail("Not found", (int)res.StatusCode);

                var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return ApiResponse<List<PersonDto>?>.Fail($"Server returned {(int)res.StatusCode}: {text}", (int)res.StatusCode);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<List<PersonDto>?>.Fail("Request cancelled");
            }
            catch (Exception ex)
            {
                // Log exception as needed
                return ApiResponse<List<PersonDto>?>.Fail($"Request failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PersonDto?>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetAsync($"api/persons/{id}", ct).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    var data = await res.Content.ReadFromJsonAsync<PersonDto>(cancellationToken: ct).ConfigureAwait(false);
                    return ApiResponse<PersonDto?>.Success(data);
                }

                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return ApiResponse<PersonDto?>.Fail("Not found", (int)res.StatusCode);

                var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return ApiResponse<PersonDto?>.Fail($"Server returned {(int)res.StatusCode}: {text}", (int)res.StatusCode);
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<PersonDto?>.Fail("Request cancelled");
            }
            catch (Exception ex)
            {
                return ApiResponse<PersonDto?>.Fail($"Request failed: {ex.Message}");
            }
        }
    }
}