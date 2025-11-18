// Application/Services/IServices/IArtistService.cs
using Application.DTOs;

namespace Application.Services.IServices
{
    public interface IArtistService
    {
        Task<ArtistDto> CreateAsync(ArtistCreateDto dto, CancellationToken ct = default);
        Task<ArtistDto> UpdateAsync(int id, ArtistUpdateDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<ArtistDto>> GetAllAsync(CancellationToken ct = default);

        Task<IReadOnlyList<ArtistWithSongsListItemDto>> GetAllWithSongsAsync(CancellationToken ct = default);
        Task<ArtistDetailsDto?> GetAsync(int id, CancellationToken ct = default);

    
        Task AddSongAsync(int artistId, int songId, CancellationToken ct = default);
        Task DeleteSongFromArtistAsync(int artistId, int songId, CancellationToken ct = default);
    }
}
