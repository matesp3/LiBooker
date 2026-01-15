using LiBooker.Shared.DTOs;
using LiBookerWebApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static LiBooker.Shared.EndpointParams.PublicationParams;

namespace LiBookerWebApi.Services
{
    // Service that loads publications and maps them to PublicationMainInfo DTOs.
    public class PublicationService : IPublicationService
    {
        private readonly AppDbContext _db;

        public PublicationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PublicationMainInfo?> GetByIdAsync(int publicationId, CancellationToken ct = default)
        {
            var p = await _db.Publications
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Include(x => x.Publisher)
                .Include(x => x.CoverImage)
                .FirstOrDefaultAsync(x => x.Id == publicationId, ct);

            if (p == null) return null;

            var authorNames = p.Book.BookAuthors?
                .Select(ba =>
                {
                    var a = ba.Author;
                    if (a == null) return null;
                    return $"{a.FirstName} {a.LastName}".Trim();
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList()
                ?? [];

            return new PublicationMainInfo
            {
                Title = p.Book.Title,
                Author = authorNames.Count > 0 ? string.Join(", ", authorNames) : "Unknown",
                Publication = p.Publisher.Name,
                Year = p.Year,
                ImageId = p.CoverImageId,
                Image = p.CoverImage?.Image
            };
        }

        public async Task<List<PublicationImageDto>> GetPublicationImagesByIdsAsync(int[] ids, CancellationToken ct)
        {
            return await _db.CoverImages
                .AsNoTracking()
                .Where(img => ids.Contains(img.Id))
                .Select(img => new PublicationImageDto
                { 
                    ImageId = img.Id,
                    ImageData = img.Image
                }
                ).ToListAsync(ct);
        }

        public async Task<int> GetPublicationsCountAsync(CancellationToken ct)
        {
            return await _db.Publications.CountAsync(ct);
        }

        public async Task<List<PublicationMainInfo>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 50,
            PublicationAvailability availability = PublicationAvailability.All,
            PublicationsSorting sorting = PublicationsSorting.None,
            bool durLoggingEnabled = false,
            CancellationToken ct = default)
        {
            List<PublicationMainInfo>? result = null;
            Stopwatch? swTotal = durLoggingEnabled ? Stopwatch.StartNew() : null;

            result = await GetAvailableAsync(pageNumber, pageSize, availability, sorting, durLoggingEnabled, ct);

            EndLoggingIfNeeed();
            return result ?? [];

            void EndLoggingIfNeeed()
            {
                if (durLoggingEnabled) {
                    swTotal?.Stop();
                    Console.WriteLine($"[PublicationService] TOTAL took {swTotal?.ElapsedMilliseconds} ms, Records: {result?.Count}");
                }
            }
        }

        //public async Task<List<PublicationMainInfo>> GetAvailableAsync(
        //    int pageNumber = 1,
        //    int pageSize = 50,
        //    bool durLoggingEnabled = false,
        //    CancellationToken ct = default)
        //{
        //    var today = DateTime.Today;  // TRUNC(SYSDATE)

        //    // IDs of available publications
        //    var publicationIds = await _db.Publications
        //        .AsNoTracking()
        //        .Where(p => p.Copies != null && p.Copies
        //            .Any(v =>  // has any copy. Publication is available if at least one of its copies is available
        //                v.Loans == null || !v.Loans.Any(vp => // has no loans OR has loans but none are active
        //                vp.ReturnedAt == null || vp.ReturnedAt > today) // not returned yet OR returned in the future
        //        ))
        //        .OrderBy(p => p.Id)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(p => p.Id)
        //        .ToListAsync(ct);

        //    if (publicationIds.Count == 0)
        //        return [];

        //    // Retrieve raw data from the database
        //    var rawData = await _db.Publications
        //        .AsNoTracking()
        //        .Where(p => publicationIds.Contains(p.Id))
        //        .Select(p => new
        //        {
        //            p.Id,
        //            Title = p.Book.Title,
        //            Authors = p.Book.BookAuthors
        //                .Select(ba => ba.Author != null ? ba.Author.FirstName + " " + ba.Author.LastName : "Unknown")
        //                .ToList(),
        //            Publication = p.Publisher.Name,
        //            p.Year,
        //            p.CoverImageId
        //        })
        //        .ToListAsync(ct);

        //    var result = rawData.Select(p => new PublicationMainInfo
        //    {
        //        Title = p.Title ?? "Unknown",
        //        Author = p.Authors != null && p.Authors.Count != 0
        //            ? string.Join(", ", p.Authors
        //                .Select(a => a.Trim())
        //                .Where(a => !string.IsNullOrWhiteSpace(a))
        //                .Distinct())
        //            : "Unknown",
        //        Publication = p.Publication ?? "Unknown",
        //        Year = p.Year,
        //        ImageId = p.CoverImageId,
        //        Image = []
        //    }).ToList();

        //    return result;
        //}

        public async Task<List<PublicationMainInfo>> GetAvailableAsync(
            int pageNumber = 1,
            int pageSize = 50,
            PublicationAvailability availability = PublicationAvailability.All,
            PublicationsSorting sorting = PublicationsSorting.None,
            bool durLoggingEnabled = false,
            CancellationToken ct = default)
        {
            var swTotal = durLoggingEnabled ? Stopwatch.StartNew() : null;
            var today = DateTime.Today;

            var query = _db.Publications
                .AsNoTracking()
                .AsQueryable();
            query = BuildParametrizedQuery(availability, sorting, today, query);

            var rawData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Book.Title,
                    Authors = p.Book.BookAuthors
                        .Select(ba => ba.Author != null
                            ? ba.Author.FirstName + " " + ba.Author.LastName
                            : "Unknown")
                        .ToList(),
                    Publication = p.Publisher.Name,
                    p.Year,
                    p.CoverImageId
                })
                .ToListAsync(ct);

            var result = rawData.Select(p => new PublicationMainInfo
            {
                Title = p.Title ?? "Unknown",
                Author = p.Authors != null && p.Authors.Count != 0
                    ? string.Join(", ", p.Authors
                        .Select(a => a.Trim())
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .Distinct())
                    : "Unknown",
                Publication = p.Publication ?? "Unknown",
                Year = p.Year,
                ImageId = p.CoverImageId,
                Image = []
            }).ToList();

            swTotal?.Stop();
            if (durLoggingEnabled)
                Console.WriteLine($"[PublicationService] Single query took {swTotal?.ElapsedMilliseconds} ms, Records: {result.Count}");

            return result;
        }

        private static IQueryable<Models.Entities.Publication> BuildParametrizedQuery(
            PublicationAvailability availability, 
            PublicationsSorting sorting, 
            DateTime today, 
            IQueryable<Models.Entities.Publication> query)
        {
            query = availability switch
            {
            PublicationAvailability.AvailableOnly => query.Where(p => p.Copies != null && p.Copies.Any(v => // Publication is available if at least one of its copies is available
                    v.Loans == null || !v.Loans.Any(vp =>   // has no loans OR has loans but none are active
                        vp.ReturnedAt == null || vp.ReturnedAt > today))),  // not returned yet OR returned in the future
                _ => query
            };

            query = sorting switch
            {
                PublicationsSorting.ByTitleAsc => query.OrderBy(p => p.Book.Title),
                PublicationsSorting.ByTitleDesc => query.OrderByDescending(p => p.Book.Title),
                PublicationsSorting.ByPublicationYearAsc => query.OrderBy(p => p.Year),
                PublicationsSorting.ByPublicationYearDesc => query.OrderByDescending(p => p.Year),
                PublicationsSorting.ByGreatestPopularity => query
                    .OrderByDescending(p => p.Copies!.Sum(c => c.Loans!.Count)),
                _ => query.OrderBy(p => p.Id)
            };
            return query;
        }
    }
}