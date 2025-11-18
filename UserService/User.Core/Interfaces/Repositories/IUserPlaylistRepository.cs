using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Core.Entities;

namespace User.Core.Interfaces.Repositories
{
    public interface IUserPlaylistRepository : IGenericRepository<UserPlaylist>
    {
        Task<UserPlaylist?> GetFullAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<UserPlaylist> items, int total)> GetByOwnerPagedAsync(int ownerUserId, int page, int pageSize, CancellationToken ct = default);
        Task<bool> ContainsSongAsync(int playlistId, int songId, CancellationToken ct = default);
        Task<int> GetMaxOrderAsync(int playlistId, CancellationToken ct = default);
        Task AddSongAsync(int playlistId, int songId, int order, CancellationToken ct = default);
        Task RemoveSongAsync(int playlistId, int songId, CancellationToken ct = default);
        Task ShiftOrdersDownAsync(int playlistId, int fromOrderInclusive, CancellationToken ct = default);
        Task UpdateSongOrderAsync(int playlistId, int songId, int newOrder, CancellationToken ct = default);
        Task<(IReadOnlyList<UserPlaylist> items, int total)> GetPublicByOthersPagedAsync(
        int requesterUserId, int page, int pageSize, CancellationToken ct = default);
        Task<(IReadOnlyList<UserPlaylist> items, int total)> GetPublicByOthersFullPagedAsync(
        int requesterUserId, int page, int pageSize, CancellationToken ct = default);
        Task RemoveAllSongsAsync(int playlistId, CancellationToken ct = default);

    }
}
