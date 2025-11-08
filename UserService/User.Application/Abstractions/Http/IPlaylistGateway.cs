using User.Shared.Common;
using User.Shared.DTO;

namespace User.Application.Abstractions.Http
{
    public interface IPlaylistGateway
    {
        Task<ExternalPlaylistDetailsDto?> GetPlaylistAsync(int playlistId, CancellationToken ct = default);
        Task<ExternalSongDto?> GetSongAsync(int songId, CancellationToken ct = default);
        Task<PagedResult<SongWithLikesDto>> GetTopLikedAsync(int minLikes, int page, int pageSize, CancellationToken ct = default);
        Task<PagedResult<ExternalSongDto>> SearchSongsAsync(string? title, string? genre, int page, int pageSize, CancellationToken ct = default);

       

        Task<int> LikeSongAsync(int songId, int userId, CancellationToken ct = default);
        Task<int> UnlikeSongAsync(int songId, int userId, CancellationToken ct = default);
        Task<int> GetSongLikesAsync(int songId, CancellationToken ct = default);

        // batch titles
        Task<Dictionary<int, string>> GetSongTitlesByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<PagedResult<ExternalPlaylistListItemDto>> GetExternalPlaylistsAsync(
       string? genre, int page, int pageSize, CancellationToken ct = default);
        Task<Dictionary<int, ExternalSongMetaDto>> GetSongMetadataByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
