using LiBooker.Shared.DTOs;
using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Models;
using LiBookerWebApi.Services;
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
                IAuthService svc,
                CancellationToken ct = default) =>
            {
                try
                {
                    var result = await svc.RegisterUserAsync(dto, ct);

                    if (!result.IsSuccessful)
                    {
                        return Results.BadRequest(result.FailureReason);
                    }

                    return Results.Ok(new { message = "Registration succesfull" });
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
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
                ClaimsPrincipal user, IAuthService svc)=>
            {
                try
                {
                    if (user.Identity?.IsAuthenticated != true)
                    {
                        return Results.Unauthorized();
                    }
                    var info = await svc.GetUserInfoAsync(user);

                    return info is not null ? Results.Ok(info) : Results.NotFound();
                }
                catch (Exception ex)
                {
                    _ = ex;
                    return Results.Problem("Internal Server Error");
                }

            })
            .RequireAuthorization(AuthPolicies.RequireLoggedUser);
        }

    }
}
