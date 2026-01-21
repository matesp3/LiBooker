using LiBooker.Shared.DTOs;
using LiBookerShared.ApiResponses;

namespace LiBookerWebApi.Services
{
    public interface IPersonService
    {
        Task<List<Person>> GetAllAsync(CancellationToken ct = default);
        Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<UpdateResponse<PersonUpdate>> UpdateAsync(
            int id, 
            PersonUpdate dto, 
            Microsoft.AspNetCore.Identity.UserManager<Models.ApplicationUser> userManager, 
            CancellationToken ct);
    }
}