// Core/Interfaces/Repositories/IArtistRepository.cs
using Playlist.Api.Core.Entities;

namespace Playlist.Api.Core.Interfaces.Repositories;

public interface IArtistRepository : IGenericRepository<Artist>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameExceptAsync(string name, int exceptId, CancellationToken ct = default);

    Task<IReadOnlyList<Artist>> GetAllWithSongsAsync(CancellationToken ct = default);
    Task<Artist?> GetWithSongsAsync(int id, CancellationToken ct = default);

    // Удалить песню, принадлежащую артисту (из-за not null FK это будет именно удаление песни)
    Task DeleteSongOfArtistAsync(int artistId, int songId, CancellationToken ct = default);
}
