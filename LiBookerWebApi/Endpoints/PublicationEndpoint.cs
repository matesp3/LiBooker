using System.Diagnostics;

namespace LiBookerWebApi.Endpoints
{
    public static class PublicationEndpoint
    {
        private const int MaxPagePublications = 15;
        public static void MapPublicationEndpoints(this WebApplication app)
        {
            // GET /api/publications?pageNumber=1&pageSize=50
            app.MapGet("/api/publications", async (
                Services.IPublicationService svc,
                int pageNumber = 1,
                int pageSize = MaxPagePublications,
                CancellationToken ct = default) =>
            {
                try
                {
                    Stopwatch sw1 = Stopwatch.StartNew();
                    Stopwatch sw2 = Stopwatch.StartNew();
                    // Validate pagination parameters
                    if (pageNumber < 1)
                        pageNumber = 1;
                    if (pageSize < 1) 
                        pageSize = MaxPagePublications;
                    if (pageSize > MaxPagePublications) 
                        pageSize = MaxPagePublications; // max page size to prevent abuse
                    sw2.Stop();
                    Console.WriteLine($"[PublicationEndpoint] Pagination validation took {sw2.ElapsedMilliseconds} ms");
                    var list = await svc.GetAllAsync(pageNumber, pageSize, ct);
                    sw1.Stop();
                    Console.WriteLine($"[PublicationEndpoint] GET /api/publications page {pageNumber} size {pageSize} took {sw1.ElapsedMilliseconds} ms");
                    return Results.Ok(list);
                }
                catch (OperationCanceledException)
                {
                    // Client cancelled the request (tab closed / request aborted).
                    return Results.StatusCode(499); // non-standard: client closed request
                }
            });

            // GET /api/publications/{id}
            app.MapGet("/api/publications/{id:int}", async (Services.IPublicationService svc, int id, CancellationToken ct) =>
            {
                try
                {
                    var dto = await svc.GetByIdAsync(id, ct);
                    return dto is null ? Results.NotFound() : Results.Ok(dto);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            });
        }
    }
}