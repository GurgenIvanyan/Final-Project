
using Playlist.Api.Core.Entities;

namespace Playlist.Api.Core.Interfaces.Repositories;

public interface ISongRepository : IGenericRepository<Song>
{
    Task<(IReadOnlyList<Song> items, int total)> SearchAsync(
        string? title, string? genre, int page, int pageSize, CancellationToken ct = default);

    Task<Song?> GetFullAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<List<Song>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    IQueryable<Song> Query();
}
