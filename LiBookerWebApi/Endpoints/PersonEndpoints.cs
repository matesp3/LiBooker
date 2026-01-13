using LiBookerWebApi.Services;

namespace LiBookerWebApi.Endpoints
{
    public static class PersonEndpoints
    {
        public static void MapPersonEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/persons");

            // GET /api/persons
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

            // GET /api/persons/{id}
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