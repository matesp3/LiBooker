
using LiBooker.Shared.DTOs;

namespace LiBookerWebApi.Services
{
    public interface ILoanService
    {
        Task<LoanInfo?> AddNewLoanRequestAsync(LoanRequest dto, CancellationToken ct);
        Task<LoanInfo?> EditLoanDatesAsync(LoanInfo dto, CancellationToken ct);
        Task<List<LoanInfo>?> GetLoansByPersonIdAsync(int personId, CancellationToken ct);
    }
}
