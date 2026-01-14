using LiBooker.Shared.DTOs;

namespace LiBookerWebApi.Services
{
    public interface IPublicationService
    {
        // Returns all publications mapped to PublicationMainInfo
        Task<List<PublicationMainInfo>> GetAllAsync(int pageNumber, int pageSize, bool durLoggingEnabled, CancellationToken ct);

        // Returns single publication by id mapped to PublicationMainInfo
        Task<PublicationMainInfo?> GetByIdAsync(int publicationId, CancellationToken ct = default);
        
        // Returns images mapped by image ID: Dictionary<imageId, imageBytes>
        Task<List<PublicationImageDto>> GetPublicationImagesByIdsAsync(int[] ids, CancellationToken ct);
    }
}