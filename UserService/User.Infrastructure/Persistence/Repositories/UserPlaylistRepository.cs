using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using User.Core.Entities;
using User.Core.Interfaces.Repositories;
using User.Infrastructure.Persistence;

namespace User.Infrastructure.Persistence.Repositories
{
    public class UserPlaylistRepository : GenericRepository<UserPlaylist>, IUserPlaylistRepository
    {
        public UserPlaylistRepository(AppDbContext db) : base(db) { }

        public Task<UserPlaylist?> GetFullAsync(int id, CancellationToken ct = default)
            => _db.UserPlaylists
                  .Include(p => p.Songs)
                  .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<(IReadOnlyList<UserPlaylist> items, int total)> GetByOwnerPagedAsync(int ownerUserId, int page, int pageSize, CancellationToken ct = default)
        {
            var q = _db.UserPlaylists.Where(p => p.OwnerUserId == ownerUserId);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(ct);
            return (items, total);
        }

        public Task<bool> ContainsSongAsync(int playlistId, int songId, CancellationToken ct = default)
        {
            return _db.UserPlaylistSongs
                .AnyAsync(x => x.UserPlaylistId == playlistId && x.SongId == songId, ct);
        }

        public async Task<int> GetMaxOrderAsync(int playlistId, CancellationToken ct = default)
            => await _db.UserPlaylistSongs.Where(x => x.UserPlaylistId == playlistId).MaxAsync(x => (int?)x.Order, ct) ?? 0;

        public Task AddSongAsync(int playlistId, int songId, int order, CancellationToken ct = default)
        {
            _db.UserPlaylistSongs.Add(new UserPlaylistSong { UserPlaylistId = playlistId, SongId = songId, Order = order });
            return Task.CompletedTask;
        }

        public async Task RemoveSongAsync(int playlistId, int songId, CancellationToken ct = default)
        {
            var link = await _db.UserPlaylistSongs
                .FirstOrDefaultAsync(x => x.UserPlaylistId == playlistId && x.SongId == songId, ct);

            if (link is null)
                return; // already removed / not found

            _db.UserPlaylistSongs.Remove(link);
            // SaveChangesAsync կանչում ա UnitOfWork-ը, ոչ թե այստեղ
        }

        public Task ShiftOrdersDownAsync(int playlistId, int fromOrderInclusive, CancellationToken ct = default)
            => _db.UserPlaylistSongs
                  .Where(x => x.UserPlaylistId == playlistId && x.Order >= fromOrderInclusive)
                  .ExecuteUpdateAsync(s => s.SetProperty(x => x.Order, x => x.Order + 1), ct);

        public Task UpdateSongOrderAsync(int playlistId, int songId, int newOrder, CancellationToken ct = default)
            => _db.UserPlaylistSongs
                  .Where(x => x.UserPlaylistId == playlistId && x.SongId == songId)
                  .ExecuteUpdateAsync(s => s.SetProperty(x => x.Order, _ => newOrder), ct);
        public async Task<(IReadOnlyList<UserPlaylist> items, int total)> GetPublicByOthersPagedAsync(
       int requesterUserId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var q = _db.UserPlaylists
                .AsNoTracking()
                .Where(p => p.IsPublic && p.OwnerUserId != requesterUserId);

            var total = await q.CountAsync(ct);
            var items = await q
                .OrderBy(p => p.Name).ThenBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<UserPlaylist> items, int total)> GetPublicByOthersFullPagedAsync(
       int requesterUserId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var q = _db.UserPlaylists
                .AsNoTracking()
                .Where(p => p.IsPublic && p.OwnerUserId != requesterUserId)
                .Include(p => p.Songs); 

            var total = await q.CountAsync(ct);
            var items = await q
                .OrderBy(p => p.Name).ThenBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }
        // UserPlaylistRepository.cs
        public Task RemoveAllSongsAsync(int playlistId, CancellationToken ct = default)
            => _db.UserPlaylistSongs
                  .Where(x => x.UserPlaylistId == playlistId)
                  .ExecuteDeleteAsync(ct);


    }
}
