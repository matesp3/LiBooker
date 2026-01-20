using LiBooker.Shared.DTOs;
using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Models;
using LiBookerWebApi.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LiBookerWebApi.Endpoints
{
    public static class AuthEndpoint
    {
        /// <summary>
        /// Maps the registration-related API endpoints to the provided WebApplication.
        /// </summary>
        /// <param name="app"></param>
        public static void MapRegistrationEndpoints(this WebApplication app)
        {
            // Map Identity endpoints for authentication
            var authGroup = app.MapGroup("/api/auth");
            authGroup.MapIdentityApi<ApplicationUser>();

            MapPostRegisterEndpoint(authGroup);
            MapUserInfoEndpoint(authGroup);
        }

        private static void MapPostRegisterEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/register-extended", async (
                PersonRegistration dto,
                UserManager <ApplicationUser> userManager,
                IAuthService svc,
                CancellationToken ct = default) =>
            {
                try
                {
                    var result = await svc.RegisterUserAsync(userManager, dto, ct);

                    if (!result.IsSuccessful)
                    {
                        return Results.BadRequest(result.FailureReason);
                    }

                    return Results.Ok(new { message = "Registration succesfull" });
                }
                catch (Exception ex)
                {
                    _ = ex;
                    return Results.Problem("Internal Server Error");
                }
            });
        }

        private static void MapUserInfoEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/user-info", async (
                ClaimsPrincipal user, UserManager<ApplicationUser> userManager)=>
            {
                if (user.Identity?.IsAuthenticated != true)
                {
                    return Results.Unauthorized();
                }

                var appUser = await userManager.GetUserAsync(user);
                if (appUser == null) return Results.NotFound();

                var roles = await userManager.GetRolesAsync(appUser);

                return Results.Ok(new UserInfoResponse()
                {
                    PersonId = appUser?.PersonId,
                    Email = appUser?.Email ?? "Unknown",
                    Roles = [.. roles]
                });
            })
            .RequireAuthorization(AuthPolicies.RequireLoggedUser);
        }

    }
}
