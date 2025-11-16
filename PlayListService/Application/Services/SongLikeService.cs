using Application.Common;
using Application.DTOs;
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Caching;

public class SongLikeService : ISongLikeService
{
    private readonly ISongLikeRepository _likes;
    private readonly IUnitOfWork _uow;
    private readonly RedisCacheService _cache;

    private const int HotLikeThreshold = 3;                   
    private static readonly TimeSpan LikesTtl = TimeSpan.FromMinutes(5);

    private static string ScoreKey(int songId) => $"song:{songId}:likes";

    public SongLikeService(
        ISongLikeRepository likes,
        IUnitOfWork uow,
        RedisCacheService cache)
    {
        _likes = likes;
        _uow = uow;
        _cache = cache;
    }

    public async Task<int> LikeAsync(int songId, int userId, int value, CancellationToken ct = default)
    {
        if (value != 1 && value != -1)
            value = 1;

        await _uow.ExecuteInTransactionAsync(async _ =>
        {
            await _likes.UpsertLikeAsync(songId, userId, value, ct);
        }, ct);

        var score = await _likes.GetScoreAsync(songId, ct);

        await UpdateCacheAsync(songId, score, ct);

        return score;
    }

    public async Task RemoveLikeAsync(int songId, int userId, CancellationToken ct = default)
    {
        await _uow.ExecuteInTransactionAsync(async _ =>
        {
            await _likes.DeleteAsync(songId, userId, ct);
        }, ct);

        var score = await _likes.GetScoreAsync(songId, ct);

        await UpdateCacheAsync(songId, score, ct);
    }

    public async Task<PagedResult<SongWithLikesDto>> GetTopLikedGlobalAsync(
        int minLikes,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (rows, total) = await _likes.GetTopLikedGlobalAsync(minLikes, page, pageSize, ct);

        var items = rows
            .Select(r => new SongWithLikesDto(
                r.song.Id,
                r.song.Title,
                r.song.Genre,
                r.song.ArtistId,
                r.song.Artist.Name,
                r.likes))
            .ToList();

        return new PagedResult<SongWithLikesDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetScoreAsync(int songId, CancellationToken ct = default)
    {
        
        var cached = await _cache.GetAsync<int?>(ScoreKey(songId), ct);
        if (cached.HasValue)
            return cached.Value;

      
        var score = await _likes.GetScoreAsync(songId, ct);

        await UpdateCacheAsync(songId, score, ct);

        return score;
    }

    private Task UpdateCacheAsync(int songId, int score, CancellationToken ct)
    {
        if (score >= HotLikeThreshold)
        {
           
            return _cache.SetAsync(ScoreKey(songId), score, LikesTtl, ct);
        }

    
        return _cache.RemoveAsync(ScoreKey(songId), ct);
    }
}
