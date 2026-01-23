using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs;

namespace LiBookerWasmApp.Services.Clients
{
    public class LoanClient(HttpClient http) : ICustomClient
    {
        private readonly HttpClient _http = http;

        /// <summary>
        /// Gets all loans for a specific person ID.
        /// </summary>
        public async Task<ApiResponse<List<LoanInfo>>> GetLoansForPersonAsync(int personId, CancellationToken ct = default)
        {
            var requestUrl = $"api/loans/loan/{personId}";
            return await ApiClient<List<LoanInfo>>.GetJsonAsync(requestUrl, _http, ct);
        }

        /// <summary>
        /// Updates loan details.
        /// </summary>
        public async Task<ApiResponse<LoanInfo>> UpdateLoanAsync(LoanInfo loan, CancellationToken ct = default)
        {
            var requestUrl = "api/loans/loan/edit/dates";
            return await ApiClient<LoanInfo>.PutJsonAsync(requestUrl, loan, _http, ct);
        }
    }
}