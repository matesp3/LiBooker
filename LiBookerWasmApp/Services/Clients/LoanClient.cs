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
        /// Updates loan details (used here specifically for setting ReturnDate/Returning).
        /// </summary>
        public async Task<ApiResponse<LoanInfo>> UpdateLoanAsync(LoanInfo loan, CancellationToken ct = default)
        {
            var requestUrl = "api/loans/loan/edit/dates";
            return await ApiClient<LoanInfo>.PutJsonAsync(requestUrl, loan, _http, ct);
        }

        /// <summary>
        /// Creates a new loan/reservation request.
        /// </summary>
        public async Task<ApiResponse<LoanInfo>> CreateLoanRequestAsync(LoanRequest request, CancellationToken ct = default)
        {
            var requestUrl = "api/loans/loan/new";
            return await ApiClient<LoanInfo>.PostJsonAsync(requestUrl, request, _http, ct);
        }
    }
}