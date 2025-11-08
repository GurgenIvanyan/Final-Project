// Application/Services/IServices/IPlaylistService.cs
using Application.Common;
using Application.DTOs;

public interface IPlaylistService
{
    Task<PlaylistDto> CreateAsync(PlaylistCreateDto dto, int ownerUserId, CancellationToken ct = default);
    Task<PagedResult<PlaylistListItemDto>> GetByGenreAsync(string? genre, int page, int pageSize, CancellationToken ct = default);
    Task<PlaylistDetailsDto?> GetAsync(int id, CancellationToken ct = default);

    Task AddSongAsync(int playlistId, int songId, CancellationToken ct = default);
    Task AddSongAtAsync(int playlistId, int songId, int order, int? addedByUserId, CancellationToken ct = default);
    Task AddSongsAsync(int playlistId, IReadOnlyList<int> songIds, int? addedByUserId, CancellationToken ct = default);
    Task RemoveSongAsync(int playlistId, int songId, CancellationToken ct = default);
    Task ReorderAsync(int playlistId, int songId, int newOrder, CancellationToken ct = default);

    // TOP by likes
    Task<PagedResult<SongWithLikesDto>> GetTopLikedAsync(int minLikes, int page, int pageSize, CancellationToken ct = default);

   
}
