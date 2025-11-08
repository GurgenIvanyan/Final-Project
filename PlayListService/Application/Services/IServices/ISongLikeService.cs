using Application.Common;
using Application.DTOs;

public interface ISongLikeService
{
    Task<int> LikeAsync(int songId, int userId, int value, CancellationToken ct = default);
    Task RemoveLikeAsync(int songId, int userId, CancellationToken ct = default);
    Task<PagedResult<SongWithLikesDto>> GetTopLikedGlobalAsync(int minLikes, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetScoreAsync(int songId, CancellationToken ct = default);

}
