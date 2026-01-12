using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using LiBookerWebApi.Model;

namespace LiBookerWebApi.Services
{
    public class SampleBackgroundWorker : BackgroundService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public SampleBackgroundWorker(IDbContextFactory<AppDbContext> dbFactory) => this._dbFactory = dbFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);
                var count = await db.Persons.AsNoTracking().CountAsync(stoppingToken);
                // ... background work ...
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    public class ParallelProcessor
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ParallelProcessor(IDbContextFactory<AppDbContext> dbFactory) => this._dbFactory = dbFactory;

        public void ProcessInParallel(IEnumerable<int> ids)
        {
            Parallel.ForEach(ids, id =>
            {
                using var db = _dbFactory.CreateDbContext();
                var person = db.Persons.Find(id);
                // safe per-thread context usage
            });
        }
    }
}