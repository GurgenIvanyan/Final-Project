
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Persistence;
using Playlist.Api.Infrastructure.Persistence.Repositories;
using PlaylistEntity = Playlist.Api.Core.Entities.Playlist;

public class PlaylistRepository : GenericRepository<PlaylistEntity>, IPlaylistRepository
{
    public PlaylistRepository(AppDbContext db) : base(db) { }

    public async Task<(IReadOnlyList<PlaylistEntity> items, int total)> GetByGenrePagedAsync(string? genre, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Playlists.AsQueryable();
        if (!string.IsNullOrWhiteSpace(genre)) q = q.Where(p => p.Genre == genre);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(p => p.Id)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .Include(p => p.PlaylistSongs)
                           .ToListAsync(ct);
        return (items, total);
    }

    public Task<PlaylistEntity?> GetFullAsync(int id, CancellationToken ct = default)
        => _db.Playlists
              .Include(p => p.PlaylistSongs).ThenInclude(ps => ps.Song).ThenInclude(s => s.Artist)
              .Include(p => p.PlaylistSongs).ThenInclude(ps => ps.Votes)
              .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ContainsSongAsync(int playlistId, int songId, CancellationToken ct = default)
        => _db.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId, ct);

    public async Task AddSongAsync(int playlistId, int songId, int order, int? addedByUserId = null, CancellationToken ct = default)
    {
        await _db.PlaylistSongs.AddAsync(new PlaylistSong
        {
            PlaylistId = playlistId,
            SongId = songId,
            Order = order,
            AddedByUserId = addedByUserId
        }, ct);
    }

    public Task RemoveSongAsync(int playlistId, int songId, CancellationToken ct = default)
    {
        _db.PlaylistSongs.Remove(new PlaylistSong { PlaylistId = playlistId, SongId = songId });
        return Task.CompletedTask;
    }

    public async Task<int> GetMaxOrderAsync(int playlistId, CancellationToken ct = default)
        => await _db.PlaylistSongs.Where(ps => ps.PlaylistId == playlistId)
                                  .MaxAsync(ps => (int?)ps.Order, ct) ?? 0;

    public Task ShiftOrdersDownAsync(int playlistId, int fromOrderInclusive, CancellationToken ct = default)
        => _db.PlaylistSongs
              .Where(ps => ps.PlaylistId == playlistId && ps.Order >= fromOrderInclusive)
              .ExecuteUpdateAsync(s => s.SetProperty(ps => ps.Order, ps => ps.Order + 1), ct);

    public Task UpdateSongOrderAsync(int playlistId, int songId, int newOrder, CancellationToken ct = default)
        => _db.PlaylistSongs
              .Where(ps => ps.PlaylistId == playlistId && ps.SongId == songId)
              .ExecuteUpdateAsync(s => s.SetProperty(ps => ps.Order, _ => newOrder), ct);
}
