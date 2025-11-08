using Application.Common;
using Application.Common.Errors;
using Application.DTOs;
using Application.Services.IServices;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Caching;

namespace Playlist.Api.Application.Services;

public class SongService : ISongService
{
    private readonly ISongRepository _songs;
    private readonly IPlaylistRepository _playlists;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _map;
    private readonly ISongLikeRepository _likes;
    private readonly RedisCacheService _cache;

    private static string SongKey(int id) => $"song:{id}";
    private const int HotLikeThreshold = 10;
    private static readonly TimeSpan HotSongTtl = TimeSpan.FromMinutes(15); // ← ДОБАВЛЕНО

    public SongService(
        ISongRepository songs,
        IPlaylistRepository playlists,
        IUnitOfWork uow,
        IMapper map,
        ISongLikeRepository likes,
        RedisCacheService cache)
    {
        _songs = songs;
        _playlists = playlists;
        _uow = uow;
        _map = map;
        _likes = likes;
        _cache = cache;
    }

    public async Task InvalidateSongCacheAsync(int songId, CancellationToken ct = default)
        => await _cache.RemoveAsync(SongKey(songId), ct);

    public async Task<SongDto> CreateAsync(SongCreateDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Title"] = new[] { "Title is required." }
            });

        var entity = new Song
        {
            Title = dto.Title.Trim(),
            Genre = dto.Genre?.Trim(),
            ArtistId = dto.ArtistId,
            Metadata = new SongMetadata { Album = dto.Album, Year = dto.Year }
        };

        await _uow.ExecuteInTransactionAsync(async _ =>
        {
            await _songs.AddAsync(entity, ct);
        }, ct);

        var full = await _songs.GetFullAsync(entity.Id, ct) ?? entity;
        return _map.Map<SongDto>(full);
    }

    public async Task<SongDto> UpdateAsync(int id, SongUpdateDto dto, CancellationToken ct = default)
    {
        var song = await _songs.GetFullAsync(id, ct) ?? throw new NotFoundException("Song not found.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Title"] = new[] { "Title is required." }
            });

        song.Title = dto.Title.Trim();
        song.Genre = dto.Genre?.Trim();
        song.ArtistId = dto.ArtistId;

        if (song.Metadata is null) song.Metadata = new SongMetadata { SongId = song.Id };
        song.Metadata.Album = dto.Album;
        song.Metadata.Year = dto.Year;

        await _uow.ExecuteInTransactionAsync(async _ =>
        {
            await _songs.UpdateAsync(song, ct);
        }, ct);

        // Инвалидация кэша — песня изменилась
        await _cache.RemoveAsync(SongKey(song.Id), ct);

        var full = await _songs.GetFullAsync(song.Id, ct) ?? song;
        return _map.Map<SongDto>(full);
    }

    public async Task<SongDetailsDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var score = await _likes.GetScoreAsync(id, ct);

        if (score > HotLikeThreshold)
        {
            var cached = await _cache.GetAsync<SongDetailsDto>(SongKey(id), ct);
            if (cached is not null) return cached;
        }

        var s = await _songs.GetFullAsync(id, ct);
        if (s is null) return null;

        var playlists = s.PlaylistSongs
            .OrderBy(ps => ps.Order)
            .Select(ps => new SongPlaylistRefDto(ps.PlaylistId, ps.Playlist.Name, ps.Order))
            .ToList();

        var dto = new SongDetailsDto(
            s.Id, s.Title, s.Genre, s.ArtistId, s.Artist?.Name ?? "", playlists);

        if (score > HotLikeThreshold)
            await _cache.SetAsync(SongKey(id), dto, HotSongTtl, ct); // ← ИСПОЛЬЗУЕМ TTL

        return dto;
    }

    public async Task<PagedResult<SongDto>> SearchAsync(string? title, string? genre, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _songs.SearchAsync(title, genre, page, pageSize, ct);
        return new PagedResult<SongDto>
        {
            Items = items.Select(_map.Map<SongDto>).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<SlimSongDto>> GetSlimByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var arr = ids?.Distinct().Where(x => x > 0).ToArray() ?? Array.Empty<int>();
        if (arr.Length == 0) return Array.Empty<SlimSongDto>();

        var (items, _) = await _songs.SearchAsync(null, null, 1, int.MaxValue, ct);
        return items.Where(s => arr.Contains(s.Id))
                    .Select(s => new SlimSongDto(s.Id, s.Title))
                    .ToList();
    }

    public async Task<IReadOnlyList<SongMetaDto>> GetMetadataByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var arr = ids?.Distinct().Where(x => x > 0).ToArray() ?? Array.Empty<int>();
        if (arr.Length == 0) return Array.Empty<SongMetaDto>();

        var data = await _songs.Query()
            .AsNoTracking()
            .Where(s => arr.Contains(s.Id))
            .Select(s => new SongMetaDto(
                s.Id,
                s.Title,
                s.Artist.Name,
                s.Metadata != null ? s.Metadata.Album : null,
                s.Metadata != null ? s.Metadata.Year : null,
                s.Genre
            ))
            .ToListAsync(ct);

        return data;
    }
}
