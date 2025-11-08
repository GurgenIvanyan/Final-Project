// Playlist.Api.Infrastructure/Persistence/Repositories/SongLikeRepository.cs
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Persistence;

public class SongLikeRepository : ISongLikeRepository
{
    private readonly AppDbContext _db;
    public SongLikeRepository(AppDbContext db) => _db = db;

    public Task<SongLike?> GetUserLikeAsync(int songId, int userId, CancellationToken ct = default)
        => _db.SongLikes.FirstOrDefaultAsync(x => x.SongId == songId && x.UserId == userId, ct);

    public async Task UpsertLikeAsync(int songId, int userId, int value, CancellationToken ct = default)
    {
        var existing = await GetUserLikeAsync(songId, userId, ct);
        if (existing is null)
            await _db.SongLikes.AddAsync(new SongLike { SongId = songId, UserId = userId, Value = value }, ct);
        else
        {
            existing.Value = value;
            _db.SongLikes.Update(existing);
        }
    }

    public Task DeleteAsync(int songId, int userId, CancellationToken ct = default)
        => _db.SongLikes.Where(x => x.SongId == songId && x.UserId == userId).ExecuteDeleteAsync(ct);

    public Task<int> GetScoreAsync(int songId, CancellationToken ct = default)
        => _db.SongLikes.Where(x => x.SongId == songId).SumAsync(x => x.Value, ct);

    public async Task<(IReadOnlyList<(Song song, int likes)> items, int total)> GetTopLikedGlobalAsync(
        int minLikes, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var agg = _db.SongLikes
            .GroupBy(x => x.SongId)
            .Select(g => new { SongId = g.Key, Likes = g.Sum(v => v.Value) })
            .Where(a => a.Likes >= minLikes);

        var q = from a in agg
                join s in _db.Songs.Include(x => x.Artist) on a.SongId equals s.Id
                select new { s, a.Likes };

        var total = await q.CountAsync(ct);
        var rows = await q
            .OrderByDescending(x => x.Likes).ThenBy(x => x.s.Title).ThenBy(x => x.s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows.Select(r => (r.s, r.Likes)).ToList();
        return (items, total);
    }
}
