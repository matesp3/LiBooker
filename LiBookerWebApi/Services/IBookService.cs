
namespace LiBookerWebApi.Services
{
    public interface IBookService
    {
        /// <summary>
        /// Retrieves the description of a book by its ID.
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="ct"></param>
        /// <returns>Description. If null, nothing was found. If string.Empty,
        /// then description is not available for specified book.</returns>
        Task<string?> GetBookDescriptionAsync(int bookId, CancellationToken ct);
    }
}