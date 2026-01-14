using LiBookerWebApi.Services;

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
            MapGetAllPersonsEndpoint(group);
            MapGetPersonByIdEndpoint(group);
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
                    var person = await service.GetByIdAsync(id, ct);
                    return person is null ? Results.NotFound() : Results.Ok(person);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            })
            .WithName("GetPersonById");
        }
    }
}