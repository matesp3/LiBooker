using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static LiBooker.Shared.EndpointParams.PublicationParams;

namespace LiBookerWebApi.Endpoints
{
    public static class PublicationEndpoints
    {

        private const int MaxPagePublications = 15;
        private const int MaxPublicationIds = 10;

        /// <summary>
        /// Maps the publication-related API endpoints to the provided WebApplication.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="durLoggingEnabled">Whether to log durations of queries</param>
        public static void MapPublicationEndpoints(this WebApplication app, bool durLoggingEnabled)
        {
            RouteGroupBuilder group = app.MapGroup("/api/publications");
            MapGetPublicationsPerPageEndpoint(group, durLoggingEnabled);    // GET /api/publications?pageNumber=x&pageSize=y&availability=all&sort=none
            MapGetPublicationByIdEndpoint(group, durLoggingEnabled);        // GET /api/publications/{id}
            MapGetPublicationImagesByIdsEndpoint(group, durLoggingEnabled); // GET /api/publications/image_ids?ids=1&ids=2&ids=3
            MapGetPublicationsCountEndpoint(group, durLoggingEnabled);      // GET /api/publications/count
        }

        /// <summary>
        /// GET /api/publications/{id}
        /// </summary>
        /// <param name="group"></param>
        private static void MapGetPublicationByIdEndpoint(RouteGroupBuilder group, bool durLoggingEnabled)
        {
            group.MapGet("/{id:int}", async (Services.IPublicationService svc, int id, CancellationToken ct) =>
            {
                try
                {
                    Stopwatch? swOverall = null;
                    if (durLoggingEnabled)
                        swOverall = Stopwatch.StartNew();

                    var dto = await svc.GetByIdAsync(id, ct);

                    if (durLoggingEnabled)
                    {
                        swOverall?.Stop();
                        Console.WriteLine($"[PublicationEndpoint] GET /api/publications/{id} took {swOverall?.ElapsedMilliseconds} ms");
                    }
                    return dto is null ? Results.NotFound() : Results.Ok(dto);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            }).WithName("GetPublicationById");
        }

        /// <summary>
        /// GET /api/publications?pageNumber=x&pageSize=y
        /// </summary>
        /// <param name="app"></param>
        private static void MapGetPublicationsPerPageEndpoint(RouteGroupBuilder group, bool durLoggingEnabled)
        {
            group.MapGet("/", async (
                Services.IPublicationService svc,
                int pageNumber = 1,
                int pageSize = MaxPagePublications,
                string? availability = null,
                string? sort = null,
                CancellationToken ct = default) =>
            {
                try
                {
                    Stopwatch? swOverall = null;
                    if (durLoggingEnabled)
                        swOverall = Stopwatch.StartNew();
                    // Validate pagination parameters
                    if (pageNumber < 1)
                        pageNumber = 1;
                    if (pageSize < 1)
                        pageSize = MaxPagePublications;
                    if (pageSize > MaxPagePublications)
                        pageSize = MaxPagePublications; // max page size to prevent abuse

                    var av = ParseAvailabilityParam(availability);
                    var sortOption = ParseSortParam(sort);

                    var list = await svc.GetAllAsync(pageNumber, pageSize, av, sortOption, durLoggingEnabled, ct);

                    if (durLoggingEnabled)
                    {
                        swOverall?.Stop();
                        Console.WriteLine($"[PublicationEndpoint] GET /api/publications page {pageNumber} size {pageSize} took {swOverall?.ElapsedMilliseconds} ms");
                    }
                    return Results.Ok(list);
                }
                catch (OperationCanceledException)
                {
                    // Client cancelled the request (tab closed / request aborted).
                    return Results.StatusCode(499); // non-standard: client closed request
                }
            }).WithName("GetPublicationPerPage");
        }

        /// <summary>
        /// GET /api/publications/image_ids?ids=1&ids=2&ids=3
        /// </summary>
        /// <param name="group"></param>
        /// <param name="durLoggingEnabled"></param>
        private static void MapGetPublicationImagesByIdsEndpoint(RouteGroupBuilder group, bool durLoggingEnabled)
        {
            group.MapGet("/image_ids", async (
                Services.IPublicationService svc,
                [FromQuery] int[] ids,
                CancellationToken ct) =>
            {
                try
                {
                    if (ids == null || ids.Length == 0)
                        return Results.BadRequest("No publication image ids provided. At least one must be provided.");
                    if (ids.Length > MaxPublicationIds)
                        return Results.BadRequest($"Too many publication image ids provided. Maximum is {MaxPublicationIds}.");

                    Stopwatch? swOverall = null;
                    if (durLoggingEnabled)
                        swOverall = Stopwatch.StartNew();

                    var images = await svc.GetPublicationImagesByIdsAsync(ids, ct);

                    if (durLoggingEnabled)
                    {
                        swOverall?.Stop();
                        Console.WriteLine($"[PublicationEndpoint] GET /api/publications/image_ids with {ids.Length} ids took {swOverall?.ElapsedMilliseconds} ms");
                    }
                    return Results.Ok(images);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            }
            ).WithName("GetPublicationImagesByIds");
        }

        private static void MapGetPublicationsCountEndpoint(RouteGroupBuilder group, bool durLoggingEnabled)
        {
            group.MapGet("/count", async (Services.IPublicationService svc, CancellationToken ct) =>
            {
                try
                {
                    Stopwatch? swOverall = null;
                    if (durLoggingEnabled)
                        swOverall = Stopwatch.StartNew();

                    var count = await svc.GetPublicationsCountAsync(ct);

                    if (durLoggingEnabled)
                    {
                        swOverall?.Stop();
                        Console.WriteLine($"[PublicationEndpoint] GET /api/publications/count took {swOverall?.ElapsedMilliseconds} ms");
                    }
                    return Results.Ok(count);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            }).WithName("GetPublicationsCount");
        }
    }
}