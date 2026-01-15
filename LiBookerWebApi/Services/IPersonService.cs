using LiBooker.Shared.DTOs;

namespace LiBookerWebApi.Services
{
    public interface IPersonService
    {
        Task<List<Person>> GetAllAsync(CancellationToken ct = default);
        Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);
        // add Create/Update/Delete signatures here
    }
}