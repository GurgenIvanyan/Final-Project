
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;

namespace Playlist.Api.Infrastructure.Persistence.Repositories;

public class SongRepository : GenericRepository<Song>, ISongRepository
{
    public SongRepository(AppDbContext db) : base(db) { }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => _db.Songs.AnyAsync(s => s.Id == id, ct);

    public async Task<(IReadOnlyList<Song> items, int total)> SearchAsync(
        string? title, string? genre, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var q = _db.Songs.AsNoTracking()
            .Include(s => s.Artist)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            var pattern = $"%{title.Trim()}%";
            q = q.Where(s => EF.Functions.ILike(s.Title, pattern));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            q = q.Where(s => s.Genre == genre.Trim());
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(s => s.Title).ThenBy(s => s.Id)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync(ct);
        return (items, total);
    }

    public Task<Song?> GetFullAsync(int id, CancellationToken ct = default)
        => _db.Songs
              .Include(s => s.Artist)
              .Include(s => s.PlaylistSongs)
                  .ThenInclude(ps => ps.Playlist)
              .AsNoTracking()
              .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<Song>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var set = ids?.Distinct().ToArray() ?? Array.Empty<int>();
        return _db.Songs
            .Where(s => set.Contains(s.Id))
            .Select(s => new Song { Id = s.Id, Title = s.Title }) 
            .ToListAsync(ct);
    }

    public IQueryable<Song> Query() => _db.Songs.Include(s => s.Artist);
}
