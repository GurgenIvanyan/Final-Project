
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;

namespace Playlist.Api.Infrastructure.Persistence.Repositories;

public class ArtistRepository : GenericRepository<Artist>, IArtistRepository
{
    public ArtistRepository(AppDbContext db) : base(db) { }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => _db.Artists.AnyAsync(a => a.Name == name, ct);

    public Task<bool> ExistsByNameExceptAsync(string name, int exceptId, CancellationToken ct = default)
        => _db.Artists.AnyAsync(a => a.Name == name && a.Id != exceptId, ct);

    public async Task<IReadOnlyList<Artist>> GetAllWithSongsAsync(CancellationToken ct = default)
        => await _db.Artists
            .AsNoTracking()
            .Include(a => a.Songs)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    public Task<Artist?> GetWithSongsAsync(int id, CancellationToken ct = default)
        => _db.Artists
            .AsNoTracking()
            .Include(a => a.Songs)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task DeleteSongOfArtistAsync(int artistId, int songId, CancellationToken ct = default)
    {
        var song = await _db.Songs.FirstOrDefaultAsync(s => s.Id == songId && s.ArtistId == artistId, ct);
        if (song is null) return; 
        _db.Songs.Remove(song);   
    }
}
