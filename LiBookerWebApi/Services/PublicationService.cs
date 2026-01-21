using LiBooker.Shared.DTOs;
using LiBooker.Shared.DTOs.VasProjekt.Shared.Dtos;
using LiBookerWebApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static LiBooker.Shared.EndpointParams.PublicationParams;

namespace LiBookerWebApi.Services
{
    // Service that loads publications and maps them to PublicationMainInfo DTOs.
    public class PublicationService(LiBookerDbContext db) : IPublicationService
    {
        private readonly LiBookerDbContext _db = db;

        public async Task<PublicationMainInfo?> GetPublicationByIdAsync(int publicationId, CancellationToken ct = default)
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

        public async Task<List<PublicationImage>> GetPublicationImagesByIdsAsync(int[] ids, CancellationToken ct)
        {
            return await _db.CoverImages
                .AsNoTracking()
                .Where(img => ids.Contains(img.Id))
                .Select(img => new PublicationImage
                { 
                    ImageId = img.Id,
                    RawImage = img.Image
                }
                ).ToListAsync(ct);
        }

        public async Task<int> GetPublicationsCountAsync(int? bookId, int? authorId, int? genreId, bool onlyAvailable, CancellationToken ct)
        {
            var query = _db.Publications
                .AsNoTracking()
                .AsQueryable();
            if (onlyAvailable)
                query = query.Where(p => p.Copies!.Any(c => // must have at least one copy that meets criteria = (is available == true)
                    c.Loans == null || c.Loans.Any(l => l.ReturnedAt != null && l.ReturnedAt <= DateTime.Today))
                );

            if (bookId.HasValue)
                query = query.Where(p => p.BookId == bookId);
            if (authorId.HasValue)
                query = query.Where(p => p.Book.BookAuthors!.Any(ba => ba.AuthorId == authorId));
            if (genreId.HasValue)
                query = query.Where(p => p.Book.BookGenres!.Any(bg => bg.GenreId == genreId));

            return await query.CountAsync(ct);
        }

        public async Task<List<PublicationMainInfo>> GetPublicationsAsync(
            int pageNumber,
            int pageSize,
            int? bookId, int? authorId, int? genreId,
            PublicationAvailability availability = PublicationAvailability.All,
            PublicationsSorting sorting = PublicationsSorting.None,
            bool durLoggingEnabled = false,
            CancellationToken ct = default)
        {
            List<PublicationMainInfo>? result = null;
            Stopwatch? swTotal = durLoggingEnabled ? Stopwatch.StartNew() : null;

            result = await GetFilteredPublicationsAsync(pageNumber, pageSize, bookId, authorId, genreId, availability, sorting, durLoggingEnabled, ct);

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

        public async Task<List<PublicationMainInfo>> GetFilteredPublicationsAsync(
            int pageNumber,
            int pageSize,
            int? bookId, int? authorId, int? genreId,
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
            query = BuildParametrizedQuery(availability, sorting, bookId, authorId, genreId, today, query);

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

        public async Task<PublicationDetails?> GetPublicationDetailsByIdAsync(int id, CancellationToken ct)
        {
            var res = await _db.PublicationDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(pd => pd.PublicationId == id, ct);
            if (res == null) 
                return null;
            return new PublicationDetails
            {
                PublicationId = res.PublicationId,
                BookId = res.BookId,
                BookTitle = res.BookTitle,
                PublisherId = res.PublisherId,
                PublisherName = res.PublisherName,
                LanguageId = res.LanguageId,
                LanguageName = res.LanguageName,
                AuthorIds = res.AuthorIds,
                AuthorFullNames = res.GetAuthorNamesWithoutIds(),
                GenreIds = res.GenreIds,
                GenreNames = res.GetGenreNamesWithoutIds(),
                PictureId = res.PictureId,
                Isbn = res.Isbn,
                PublicationYear = res.PublicationYear,
                PageCount = res.PageCount,
                Binding = res.Binding,
                Paper = res.Paper,
                Cover = res.Cover,
                Dimensions = res.Dimensions,
                Weight = res.Weight
            };
        }

        private static IQueryable<Models.Entities.Publication> BuildParametrizedQuery(
            PublicationAvailability availability, 
            PublicationsSorting sorting,
            int? bookId, int? authorId, int? genreId,
            DateTime today, 
            IQueryable<Models.Entities.Publication> query)
        {
            // 1. Availability filter
            query = availability switch // p.Copies.Any() translates to SQL "EXISTS". 
            { // It ensures the Publication is selected only if we find at least one Copy where the inner condition is true (Copy is available).
                PublicationAvailability.AvailableOnly =>
                    query.Where(p => p.Copies!.Any(
                        c => c.Loans == null || c.Loans.Any(l => l.ReturnedAt != null && l.ReturnedAt <= today))
                    ),
                _ => query
            };

            // 2. Chaining additional filters
            if (bookId.HasValue)
                query = query.Where(p => p.BookId == bookId);

            if (authorId.HasValue)
                query = query.Where(p => p.Book.BookAuthors!.Any(ba => ba.AuthorId == authorId));

            if (genreId.HasValue)
               query = query.Where(p => p.Book.BookGenres!.Any(bg => bg.GenreId == genreId));

            // 3. Sorting
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