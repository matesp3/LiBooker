using LiBookerWebApi.Infrastructure;
using LiBooker.Shared.DTOs.Admin;

namespace LiBookerWebApi.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/users");

            MapGetUsersByEmailMatchAsync(group);    // GET /api/users/search?email={emailFragment}
            MapPutUserRolesAsync(group);            // PUT /api/users/edit/roles
        }

        private static void MapPutUserRolesAsync(RouteGroupBuilder group)
        {
            group.MapPut("/edit/roles", async (Services.IAuthService svc,
                UserRolesUpdate dto,
                CancellationToken ct) =>
            {
                try
                {
                    var result = await svc.UpdateUserRolesAsync(dto, ct);
                    return result.IsSuccess
                        ? Results.Ok(result.UpdatedDto)
                        : Results.BadRequest(result.ErrorMessage);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
                catch (Exception ex)
                {
                    _ = ex;
                    return Results.InternalServerError();
                }
            })
            .RequireAuthorization(AuthPolicies.RequireAdmin)
            .WithName("EditUserRoles");
        }

        private static void MapGetUsersByEmailMatchAsync(RouteGroupBuilder group)
        {
            group.MapGet("/search", async (Services.IAuthService svc,
                string email,
                CancellationToken ct) =>
            {
                try
                {
                    Console.WriteLine($"GET api/users/search?email={email}");
                    var results = await svc.FindUsersWithEmailMatchAsync(email, ct);
                    return results is not null ? Results.Ok(results) : Results.BadRequest();
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
                catch (Exception ex)
                {
                    _ = ex;
                    return Results.InternalServerError();
                }
            })
            .RequireAuthorization(AuthPolicies.RequireAdmin)
            .WithName("FindUsersWithEmailMatch");
        }
    }
}
