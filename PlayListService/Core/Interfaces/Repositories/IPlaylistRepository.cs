// Core/Interfaces/Repositories/IPlaylistRepository.cs
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;
using PlaylistEntity = Playlist.Api.Core.Entities.Playlist;

public interface IPlaylistRepository : IGenericRepository<PlaylistEntity>
{
    Task<(IReadOnlyList<PlaylistEntity> items, int total)> GetByGenrePagedAsync(string? genre, int page, int pageSize, CancellationToken ct = default);
    Task<PlaylistEntity?> GetFullAsync(int id, CancellationToken ct = default);

    Task<bool> ContainsSongAsync(int playlistId, int songId, CancellationToken ct = default);
    Task AddSongAsync(int playlistId, int songId, int order, int? addedByUserId = null, CancellationToken ct = default);
    Task RemoveSongAsync(int playlistId, int songId, CancellationToken ct = default);
    Task<int> GetMaxOrderAsync(int playlistId, CancellationToken ct = default);
    Task ShiftOrdersDownAsync(int playlistId, int fromOrderInclusive, CancellationToken ct = default); // for insert in middle
    Task UpdateSongOrderAsync(int playlistId, int songId, int newOrder, CancellationToken ct = default);
}
