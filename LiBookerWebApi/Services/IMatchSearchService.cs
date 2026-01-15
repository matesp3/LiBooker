
using LiBookerShared.DTOs;

namespace LiBookerWebApi.Services
{
    public interface IMatchSearchService
    {
        Task<List<FoundMatch>> MatchSearchAsync(string query, CancellationToken ct);
    }
}