using Playlist.Api.Core.Entities;

public interface ISongLikeRepository
{
    Task<SongLike?> GetUserLikeAsync(int songId, int userId, CancellationToken ct = default);
    Task UpsertLikeAsync(int songId, int userId, int value, CancellationToken ct = default);
    Task DeleteAsync(int songId, int userId, CancellationToken ct = default);
    Task<int> GetScoreAsync(int songId, CancellationToken ct = default);
    Task<(IReadOnlyList<(Song song, int likes)> items, int total)> GetTopLikedGlobalAsync(
        int minLikes, int page, int pageSize, CancellationToken ct = default);
}
