using LiBooker.Shared.DTOs;
using LiBookerWebApi.Model;
using LinqKit; // For PredicateBuilder [OR] and .Expand()
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LiBookerWebApi.Services
{
    public class MatchSearchService(LiBookerDbContext db) : IMatchSearchService
    {
        private const int MaxResultsPerCategory = 4;
        private readonly LiBookerDbContext _db = db;

        public async Task<List<FoundMatch>> MatchSearchAsync(string queryString, CancellationToken ct)
        {
            queryString = queryString.Trim();
            var tokens = queryString.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(5); // maximum is 5 tokens

            var results = new List<FoundMatch>();

            var res1 = await SearchBooksAsync(tokens, queryString, ct);
            results.AddRange(res1);

            var res2 = await SearchAuthorsAsync(tokens, queryString,  ct);
            results.AddRange(res2);

            var res3 = await SearchGenresAsync(tokens, queryString, ct);
            results.AddRange(res3);

            return results;
        }

        private async Task<List<BookMatch>> SearchBooksAsync(IEnumerable<string> tokens, string queryString, CancellationToken ct)
        {
            var res1 = await _db.Books
                .Where(b => b.Title.ToLower().Contains(queryString.ToLower()))
                .Select(b => new BookMatch{
                    Id = b.Id,
                    Title = b.Title,
                    Authors = string.Join(", ", b.BookAuthors
                        .Select(ba => ba.Author != null ? ba.Author.FirstName + " " + ba.Author.LastName : "Unknown author"))
                })
                .Take(MaxResultsPerCategory)
                .ToListAsync(ct);
            //await MapAuthors(res1, ct);

            if (res1.Count >= MaxResultsPerCategory)
                return res1;

            var predicate = PredicateBuilder.New<Models.Entities.Book>(false);

            foreach (var token in tokens)
            {
                var expr = $"%{token.ToLower()}%";
                predicate = predicate.Or(b => EF.Functions.Like(b.Title.ToLower(), expr));
            }

            var res2 = await _db.Books
                .Where((System.Linq.Expressions.Expression<Func<Models.Entities.Book, bool>>)predicate.Expand())
                .Select(b => new BookMatch
                {
                    Id = b.Id,
                    Title = b.Title,
                    Authors = string.Join(", ", b.BookAuthors
                        .Select(ba => ba.Author != null ? ba.Author.FirstName + " " + ba.Author.LastName : "Unknown author"))
                })
                .Take(MaxResultsPerCategory - res1.Count)
                .ToListAsync(ct);
            //await MapAuthors(res2, ct);

            res1.AddRange(res2);
            return res1;
        }

        private async Task<List<AuthorMatch>> SearchAuthorsAsync(IEnumerable<string> tokens, string queryString, CancellationToken ct)
        {
            var predicate = PredicateBuilder.New<Models.Entities.Author>(false)
                            .Or(a => EF.Functions.Like(a.FirstName.ToLower(), queryString))
                            .Or(a => EF.Functions.Like(a.LastName.ToLower(), queryString));
            foreach (var token in tokens)
            {
                var expr = $"%{token.ToLower()}%";
                predicate = predicate.Or(a => EF.Functions.Like(a.FirstName.ToLower(), expr))
                                     .Or(a => EF.Functions.Like(a.LastName.ToLower(), expr));
            }
            return await _db.Authors
                .Where(predicate.Expand())
                .Select(a => new AuthorMatch {
                    Id = a.Id,
                    FullName = $"{a.FirstName} {a.LastName}"
                })
                .Take(MaxResultsPerCategory)
                .ToListAsync(ct);
        }

        private async Task<List<GenreMatch>> SearchGenresAsync(IEnumerable<string> tokens, string queryString, CancellationToken ct)
        {
            var predicate = PredicateBuilder.New<Models.Entities.Genre>(false)
                            .Or(g => EF.Functions.Like(g.Name.ToLower(), queryString));
            foreach (var token in tokens)
            {
                var expr = $"%{token.ToLower()}%";
                predicate = predicate.Or(g => EF.Functions.Like(g.Name.ToLower(), expr));
            }
            return await _db.Genres
                .Where(predicate.Expand())
                .Select(g => new GenreMatch
                {
                    Id = g.Id,
                    Name = g.Name,
                })
                .Take(MaxResultsPerCategory)
                .ToListAsync(ct);
        }

        private async Task MapAuthors(List<BookMatch> books, CancellationToken ct)
        {
            if (books.Count == 0)
                return;
            var r = await _db.BookAuthors
                .Where(ba => books.Select(b => b.Id).Contains(ba.BookId))
                .Select(ba => new
                {
                    ba.BookId,
                    AuthorFullName = ba.Author != null ? ba.Author.FirstName + " " + ba.Author.LastName : "Unknown author"
                })
                .ToListAsync(ct);

            foreach (var book in books)
            {
                var authors = r.Where(x => x.BookId == book.Id)
                               .Select(x => x.AuthorFullName);
                book.Authors = string.Join(", ", authors);
            }
        }
    }
}
