using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Application.DTOs;
using User.Shared.Common;

namespace User.Application.Services.IServices
{
    public interface IUserPlaylistService
    {
        Task<UserPlaylistDto> CreateAsync(int ownerUserId, UserPlaylistCreateDto dto, CancellationToken ct = default);
        Task<UserPlaylistDto> ImportAsync(int ownerUserId, ImportPlaylistDto dto, CancellationToken ct = default);

        Task SetPublicAsync(int ownerUserId, int playlistId, bool isPublic, CancellationToken ct = default);

        Task AddSongAsync(int ownerUserId, int playlistId, int songId, int? order, CancellationToken ct = default);
        Task AddSongsAsync(int ownerUserId, int playlistId, IReadOnlyList<int> songIds, CancellationToken ct = default);
        Task RemoveSongAsync(int ownerUserId, int playlistId, int songId, CancellationToken ct = default);
        Task ReorderAsync(int ownerUserId, int playlistId, int songId, int newOrder, CancellationToken ct = default);

        Task<PagedResult<UserPlaylistDto>> GetMineAsync(int ownerUserId, int page, int pageSize, CancellationToken ct = default);
        Task<UserPlaylistDetailsDto?> GetDetailsAsync(int requesterUserId, int playlistId, CancellationToken ct = default); 
        Task<PagedResult<UserPlaylistDto>> GetPublicByOthersAsync(int requesterUserId, int page, int pageSize, CancellationToken ct = default);

        Task<PagedResult<PublicPlaylistWithSongsDto>> GetPublicByOthersWithSongsAsync(int requesterUserId, int page, int pageSize, CancellationToken ct = default);
        Task<PagedResult<PublicPlaylistWithSongsRichDto>> GetPublicByOthersWithSongsRichAsync(
           int requesterUserId, int page, int pageSize, CancellationToken ct = default);
        Task DeleteAsync(int ownerUserId, int playlistId, CancellationToken ct = default);
        Task NormalizeOrdersAsync(int playlistId, CancellationToken ct);

    }

}
