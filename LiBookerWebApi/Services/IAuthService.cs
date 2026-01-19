using LiBooker.Shared.DTOs;
using LiBookerWebApi.Endpoints.ResultWrappers;
using LiBookerWebApi.Models;
using Microsoft.AspNetCore.Identity;

namespace LiBookerWebApi.Services
{
    public interface IAuthService
    {
        Task<RegistrationResult> RegisterUserAsync(UserManager<ApplicationUser> userManager, PersonRegistration dto, CancellationToken ct);

        /// <summary>
        /// For ensuring immediate action of changing user's role
        /// </summary>
        /// <returns></returns>
        Task RefreshUserAsync(CancellationToken ct);
    }
}
