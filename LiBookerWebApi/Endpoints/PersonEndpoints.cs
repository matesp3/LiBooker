using LiBooker.Shared.DTOs;
using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Models;
using LiBookerWebApi.Services;
using Microsoft.AspNetCore.Identity;

namespace LiBookerWebApi.Endpoints
{
    public static class PersonEndpoints
    {
        /// <summary>
        /// Maps the person-related API endpoints to the provided WebApplication.
        /// </summary>
        /// <param name="app"></param>
        public static void MapPersonEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/persons");
            MapGetAllPersonsEndpoint(group);            // GET /api/persons
            MapGetPersonByIdEndpoint(group);            // GET /api/persons/{id}
            MapUpdatePersonWithIdEndpoint(group);       // PUT /api/persons/edit/{id}
        }

        /// <summary>
        /// PUT /api/persons/edit/{id}
        /// </summary>
        /// <param name="group"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void MapUpdatePersonWithIdEndpoint(RouteGroupBuilder group)
        {
            group.MapPut("/edit/{id:int}", async (int id, PersonUpdate dto,
                IPersonService svc, UserManager<ApplicationUser> userManager, CancellationToken ct) =>
            {
                try
                {
                    if (ParamChecker.IsInvalidId(id))
                        return Results.BadRequest("The provided person ID is invalid.");
                    var response = await svc.UpdateAsync(id, dto, userManager, ct);
                    return response.IsSuccess
                        ? Results.Ok(response.UpdatedDto)
                        : Results.BadRequest(response.ErrorMessage);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            }).
            RequireAuthorization(AuthPolicies.RequireLoggedUser)
            .WithName("UpdatePersonWithId");
        }

        /// <summary>
        /// GET /api/persons
        /// </summary>
        /// <param name="group"></param>
        private static void MapGetAllPersonsEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/", async (IPersonService service, CancellationToken ct) =>
            {
                try
                {
                    var list = await service.GetAllAsync(ct);
                    return Results.Ok(list);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            })
            .RequireAuthorization(AuthPolicies.RequireAdmin)
            .WithName("GetAllPersons");
        }

        /// <summary>
        /// GET /api/persons/{id}
        /// </summary>
        /// <param name="group"></param>
        private static void MapGetPersonByIdEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/{id:int}", async (int id, IPersonService service, CancellationToken ct) =>
            {
                try
                {
                    if (ParamChecker.IsInvalidId(id))
                        return Results.BadRequest("The provided ID is invalid.");
                    var person = await service.GetByIdAsync(id, ct);
                    return person is null ? Results.NotFound() : Results.Ok(person);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            })
            .RequireAuthorization(AuthPolicies.RequireLoggedUser)
            .WithName("GetPersonById");
        }
    }
}