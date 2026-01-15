using LiBooker.Shared.DTOs;
using static LiBooker.Shared.EndpointParams.PublicationParams;

namespace LiBookerWebApi.Services
{
    public interface IPublicationService
    {
        // Returns all desired publications mapped to PublicationMainInfo
        Task<List<PublicationMainInfo>> GetAllAsync(int pageNumber, int pageSize, 
            PublicationAvailability availabilityOption, 
            PublicationsSorting sortOption,
            bool durLoggingEnabled, CancellationToken ct);

        // Returns single publication by id mapped to PublicationMainInfo
        Task<PublicationMainInfo?> GetByIdAsync(int publicationId, CancellationToken ct = default);
        
        // Returns images mapped by image ID: Dictionary<imageId, imageBytes>
        Task<List<PublicationImage>> GetPublicationImagesByIdsAsync(int[] ids, CancellationToken ct);
        Task<int> GetPublicationsCountAsync(CancellationToken ct);
    }
}