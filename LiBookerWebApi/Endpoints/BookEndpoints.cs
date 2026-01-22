
using LiBookerWebApi.Services;

namespace LiBookerWebApi.Endpoints
{
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/books");

            MapGetBookDescriptionEndpoint(group);   // GET /api/books/description/{bookId}
        }

        private static void MapGetBookDescriptionEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/description/{bookId:int}", async (int bookId,
                IBookService svc, CancellationToken ct) =>
            {
                try
                {
                    if (ParamChecker.IsInvalidId(bookId))
                        return Results.BadRequest("The provided book ID is invalid.");

                    var description = await svc.GetBookDescriptionAsync(bookId, ct);
                    return description is not null ? Results.Ok(description) : Results.NotFound();
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            }).WithName("GetBookDescriptionById");
        }
    }
}
