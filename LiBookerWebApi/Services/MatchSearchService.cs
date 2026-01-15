using LiBookerShared.DTOs;
using LiBookerWebApi.Model;
using LinqKit; // For PredicateBuilder [OR] and .Expand()
using Microsoft.EntityFrameworkCore;

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

            List<string> res = await SearchBooksAsync(tokens, queryString, ct);
            results.AddRange(res.Select(title => new FoundMatch
            { Match = title, Type = FoundMatch.MatchType.Title }));

            res = await SearchAuthorsAsync(tokens, queryString,  ct);
            results.AddRange(res.Select(name => new FoundMatch
            { Match = name, Type = FoundMatch.MatchType.Author }));

            res = await SearchGenresAsync(tokens, queryString, ct);
            results.AddRange(res.Select(genre => new FoundMatch
            { Match = genre, Type = FoundMatch.MatchType.Genre }));

            return results;
        }

        private async Task<List<string>> SearchBooksAsync(IEnumerable<string> tokens, string queryString, CancellationToken ct)
        {
            var res = await _db.Books
                .Where(b => b.Title.ToLower().Contains(queryString.ToLower()))
                .Select(b => b.Title)
                .Take(MaxResultsPerCategory)
                .ToListAsync(ct);

            if (res.Count >= MaxResultsPerCategory)
                return res;

            var predicate = PredicateBuilder.New<Models.Entities.Book>(false);

            foreach (var token in tokens)
            {
                var expr = $"%{token.ToLower()}%";
                predicate = predicate.Or(b => EF.Functions.Like(b.Title.ToLower(), expr));
            }

            var res2 = await _db.Books
                .Where((System.Linq.Expressions.Expression<Func<Models.Entities.Book, bool>>)predicate.Expand())
                .Select(b => b.Title)
                .Take(MaxResultsPerCategory - res.Count)
                .ToListAsync(ct);

            res.AddRange(res2);
            return res;
        }

        private async Task<List<string>> SearchAuthorsAsync(IEnumerable<string> tokens, string queryString, CancellationToken ct)
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
                .Select(a => $"{a.FirstName} {a.LastName}")
                .Take(MaxResultsPerCategory)
                .ToListAsync(ct);
        }

        private async Task<List<string>> SearchGenresAsync(IEnumerable<string> tokens, string queryString, CancellationToken ct)
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
                .Select(g => g.Name)
                .Take(MaxResultsPerCategory)
                .ToListAsync(ct);
        }
    }
}
