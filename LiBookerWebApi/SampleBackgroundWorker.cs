using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using LiBookerWebApi.Model;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System;

namespace LiBookerWebApi.Services
{
    public class SampleBackgroundWorker : BackgroundService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public SampleBackgroundWorker(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var block = new ActionBlock<int>(async id =>
            {
                // create a short-lived context per operation
                await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);

                var person = await db.Persons.FindAsync(new object[] { id }, stoppingToken);
                // ... do background work with person ...
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = stoppingToken
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                // example: enqueue IDs 1 to 100 for processing
                for (int i = 1; i <= 100; i++)
                {
                    block.Post(i);
                }

                block.Complete();
                await block.Completion;
            }
        }
    }

    public class ParallelProcessor
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ParallelProcessor(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

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