
using LiBookerWebApi.Model;
using Microsoft.EntityFrameworkCore;

namespace LiBookerWebApi.Services
{
    public class BookService(LiBookerDbContext db) : IBookService
    {
        private readonly LiBookerDbContext _db = db;

        public async Task<string?> GetBookDescriptionAsync(int bookId, CancellationToken ct)
        {
            var desc = await _db.Books
                .AsNoTracking()
                .Where(b => b.Id == bookId)
                .Select(b => b.Description)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            return desc is not null ? desc : string.Empty;
        }
    }
}
