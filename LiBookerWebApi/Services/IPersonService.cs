using LiBookerWebApi.Models.DTOs;

namespace LiBookerWebApi.Services
{
    public interface IPersonService
    {
        Task<List<PersonDto>> GetAllAsync(CancellationToken ct = default);
        Task<PersonDto?> GetByIdAsync(int id, CancellationToken ct = default);
        // add Create/Update/Delete signatures here
    }
}