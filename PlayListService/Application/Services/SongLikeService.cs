using Application.Common;
using Application.DTOs;
using Playlist.Api.Core.Interfaces.Repositories;

public class SongLikeService : ISongLikeService
{
    private readonly ISongLikeRepository _likes;
    private readonly IUnitOfWork _uow;

    public SongLikeService(ISongLikeRepository likes, IUnitOfWork uow)
    { _likes = likes; _uow = uow; }

    public async Task<int> LikeAsync(int songId, int userId, int value, CancellationToken ct = default)
    {
        if (value != 1 && value != -1) value = 1;
        await _uow.ExecuteInTransactionAsync(async _ => { await _likes.UpsertLikeAsync(songId, userId, value, ct); }, ct);
        return await _likes.GetScoreAsync(songId, ct);
    }

    public async Task RemoveLikeAsync(int songId, int userId, CancellationToken ct = default)
        => await _uow.ExecuteInTransactionAsync(async _ => { await _likes.DeleteAsync(songId, userId, ct); }, ct);

    public async Task<PagedResult<SongWithLikesDto>> GetTopLikedGlobalAsync(int minLikes, int page, int pageSize, CancellationToken ct = default)
    {
        var (rows, total) = await _likes.GetTopLikedGlobalAsync(minLikes, page, pageSize, ct);
        var items = rows.Select(r => new SongWithLikesDto(r.song.Id, r.song.Title, r.song.Genre, r.song.ArtistId, r.song.Artist.Name, r.likes)).ToList();
        return new PagedResult<SongWithLikesDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }
    public Task<int> GetScoreAsync(int songId, CancellationToken ct = default)
    => _likes.GetScoreAsync(songId, ct);

}
