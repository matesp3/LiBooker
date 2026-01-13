using Microsoft.EntityFrameworkCore;
using LiBooker.Shared.DTOs;
using LiBookerWebApi.Model;
using System.Diagnostics;

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

        // Raw SQL with LISTAGG - single query, bypasses EF Core overhead
        public async Task<List<PublicationMainInfo>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
        {
            //var swTotal = Stopwatch.StartNew();
            //Console.WriteLine($"[PublicationService] Starting raw SQL query at {DateTime.Now:HH:mm:ss.fff}");

            var offset = (pageNumber - 1) * pageSize;
            //var swExec = Stopwatch.StartNew();

            // Single optimized SQL query with LISTAGG (no EF Core overhead, no multiple costly queries)
            var results = await _db.Database
                .SqlQuery<PublicationMainInfo>($@"
                    SELECT 
                        k.NAZOV AS Title,
                        LISTAGG(a.MENO || ' ' || a.PRIEZVISKO, ', ') 
                            WITHIN GROUP (ORDER BY a.MENO, a.PRIEZVISKO) AS Author,
                        vyd.NAZOV AS Publication,
                        v.ROK_VYDANIA AS Year,
                        v.ID_OBRAZKU as ImageId,
                        null as Image
                    FROM VYDANIE v
                    INNER JOIN KNIHA k ON v.ID_KNIHY = k.ID_KNIHY
                    LEFT JOIN KNIHY ka ON k.ID_KNIHY = ka.ID_KNIHY
                    LEFT JOIN AUTOR a ON ka.ID_AUTORA = a.ID_AUTORA
                    LEFT JOIN VYDAVATELSTVO vyd ON v.ID_VYDAVATELSTVA = vyd.ID_VYDAVATELSTVA
                    GROUP BY v.ID_VYDANIA, k.NAZOV, vyd.NAZOV, v.ROK_VYDANIA, v.ID_OBRAZKU
                    ORDER BY v.ID_VYDANIA
                    OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY
                ")
                .ToListAsync(ct);

            //swExec.Stop();
            //Console.WriteLine($"[PublicationService] Raw SQL execution took {swExec.ElapsedMilliseconds} ms");
            //Console.WriteLine($"[PublicationService] Retrieved {results.Count} publications");
            //var swMap = Stopwatch.StartNew();

            var mapped = results.Select(r => new PublicationMainInfo
            {
                Title = r.Title,
                Author = r.Author,
                Publication = r.Publication,
                Year = r.Year,
                ImageId = r.ImageId,
                Image = []
            }).ToList();

            //swMap.Stop();
            //swTotal.Stop();
            //Console.WriteLine($"[PublicationService] Mapping took {swMap.ElapsedMilliseconds} ms");
            //Console.WriteLine($"[PublicationService] TOTAL took {swTotal.ElapsedMilliseconds} ms");
            //Console.WriteLine($"[PublicationService] Breakdown: Exec={swExec.ElapsedMilliseconds}ms, Map={swMap.ElapsedMilliseconds}ms");

            return mapped;
        }

        //public async Task<List<PublicationMainInfo>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
        //{
        //    var swTotal = Stopwatch.StartNew();
        //    Console.WriteLine($"[PublicationService] Starting GetAllAsync at {DateTime.Now:HH:mm:ss.fff}");

        //    var swQueryBuild = Stopwatch.StartNew();
        //    var query = _db.Publications
        //        .AsNoTracking()
        //        .AsSplitQuery()
        //        .Include(p => p.Book)
        //            .ThenInclude(b => b.BookAuthors)
        //                .ThenInclude(ba => ba.Author)
        //        .Include(p => p.Publisher)
        //        //.Include(p => p.CoverImage)
        //        .OrderBy(p => p.Id)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize);
        //    swQueryBuild.Stop();
        //    Console.WriteLine($"[PublicationService] Query build took {swQueryBuild.ElapsedMilliseconds} ms");

        //    var swExec = Stopwatch.StartNew();
        //    Console.WriteLine($"[PublicationService] Starting ToListAsync at {DateTime.Now:HH:mm:ss.fff}");
        //    var publications = await query.ToListAsync(ct);
        //    swExec.Stop();
        //    Console.WriteLine($"[PublicationService] ToListAsync (DB query) took {swExec.ElapsedMilliseconds} ms");
        //    Console.WriteLine($"[PublicationService] Retrieved {publications.Count} publications");

        //    var swMap = Stopwatch.StartNew();
        //    var result = publications.Select(p =>
        //    {
        //        var authorNames = p.Book?.BookAuthors?
        //            .Select(ba =>
        //            {
        //                var a = ba.Author;
        //                if (a == null) return null;
        //                return $"{a.FirstName} {a.LastName}".Trim();
        //            })
        //            .Where(s => !string.IsNullOrWhiteSpace(s))
        //            .Distinct()
        //            .ToList()
        //            ?? [];

        //        return new PublicationMainInfo
        //        {
        //            Title = p.Book?.Title,
        //            Author = authorNames.Count > 0 ? string.Join(", ", authorNames) : null,
        //            Publication = p.Publisher?.Name,
        //            Year = p.Year,
        //            Image = []
        //        };
        //    }).ToList();

        //    swMap.Stop();
        //    swTotal.Stop();
            
        //    Console.WriteLine($"[PublicationService] Mapping took {swMap.ElapsedMilliseconds} ms");
        //    Console.WriteLine($"[PublicationService] TOTAL took {swTotal.ElapsedMilliseconds} ms");
        //    Console.WriteLine($"[PublicationService] Breakdown: Build={swQueryBuild.ElapsedMilliseconds}ms, Exec={swExec.ElapsedMilliseconds}ms, Map={swMap.ElapsedMilliseconds}ms");

        //    return result;
        //}

        public async Task<PublicationMainInfo?> GetByIdAsync(int publicationId, CancellationToken ct = default)
        {
            //var sw = Stopwatch.StartNew();

            var p = await _db.Publications
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Include(x => x.Publisher)
                .Include(x => x.CoverImage)
                .FirstOrDefaultAsync(x => x.Id == publicationId, ct);

            //sw.Stop();
            //Console.WriteLine($"[PublicationService] GetByIdAsync took {sw.ElapsedMilliseconds} ms");

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
    }
}