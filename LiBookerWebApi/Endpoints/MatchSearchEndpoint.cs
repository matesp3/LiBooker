using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LiBookerWebApi.Endpoints
{
    public static class MatchSearchEndpoint
    {
        public static void MapMatchSearchEndpoint(this WebApplication app, bool durLoggingEnabled)
        {
            app.MapGet("/api/matchsearch", async (Services.IMatchSearchService svc, 
                string query,
                CancellationToken ct) =>
            {
                try
                {
                    Stopwatch? swOverall = null;
                    if (durLoggingEnabled)
                        swOverall = Stopwatch.StartNew();

                    var results = await svc.MatchSearchAsync(query, ct);

                    if (durLoggingEnabled)
                    {
                        swOverall?.Stop();
                        Console.WriteLine($"[MatchSearchEndpoint] GET /api/matchsearch?query={query} took {swOverall?.ElapsedMilliseconds} ms");
                    }
                    return Results.Ok(results);
                }
                catch (OperationCanceledException)
                {
                    return Results.StatusCode(499);
                }
            }).WithName("MatchSearch");
        }
    }
}
