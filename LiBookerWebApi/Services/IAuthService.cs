using LiBooker.Shared.ApiResponses;
using LiBooker.Shared.DTOs;
using LiBookerWebApi.Endpoints.ResultWrappers;
using LiBookerWebApi.Utils;
using System.Security.Claims;

namespace LiBookerWebApi.Services
{
    public interface IAuthService
    {
        public Task<List<PersonUploader.UserAccountDto>> CreateUserForPerson(List<PersonUploader.UserAccountDto> users, ILogger<Program> logger, CancellationToken token);
        public Task<UserInfoResponse?> GetUserInfoAsync(ClaimsPrincipal user);
        public Task<RegistrationResult> RegisterUserAsync(PersonRegistration dto, CancellationToken ct);
    }
}
