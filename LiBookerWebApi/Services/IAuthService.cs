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
        /// Creates an admin user without admin privileges.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="dto"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<RegistrationResult> CreateAdminAsync(UserManager<ApplicationUser> userManager, PersonRegistration dto, CancellationToken ct);
    }
}
