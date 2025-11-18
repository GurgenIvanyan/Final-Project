// Application/Services/PlaylistService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Errors;
using Application.DTOs;
using Application.Services.IServices;
using AutoMapper;
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Caching;
using PlaylistEntity = Playlist.Api.Core.Entities.Playlist;

namespace Playlist.Api.Application.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlists;
        private readonly ISongRepository _songs;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _map;
        private readonly RedisCacheService _cache;

        public PlaylistService(
            IPlaylistRepository playlists,
            ISongRepository songs,
            IUnitOfWork uow,
            IMapper map,
            RedisCacheService cache)
        {
            _playlists = playlists;
            _songs = songs;
            _uow = uow;
            _map = map;
            _cache = cache;
        }

        // ------------ cache helpers ------------
        private static string CacheKeyListPage(string? genre, int page, int size)
            => $"playlists:genre={(genre ?? "*")}:p{page}:s{size}";
        private static string PlaylistKey(int id) => $"playlist:{id}";
        private static string SongKey(int id) => $"song:{id}";

        // ----------------- Create -----------------
        public async Task<PlaylistDto> CreateAsync(PlaylistCreateDto dto, int ownerUserId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["name"] = new[] { "Name is required." }
                });

            var entity = new PlaylistEntity
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Genre = dto.Genre?.Trim(),
                OwnerUserId = ownerUserId
            };

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _playlists.AddAsync(entity, ct);
            }, ct);

           
            await _cache.RemoveAsync(CacheKeyListPage(dto.Genre, 1, 20), ct);

            return _map.Map<PlaylistDto>(entity);
        }

        // ------------- List (paged by genre) -------------
        public async Task<PagedResult<PlaylistListItemDto>> GetByGenreAsync(string? genre, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var cacheKey = CacheKeyListPage(genre, page, pageSize);
            var cached = await _cache.GetAsync<PagedResult<PlaylistListItemDto>>(cacheKey, ct);
            if (cached is not null) return cached;

            var (items, total) = await _playlists.GetByGenrePagedAsync(genre, page, pageSize, ct);

            var list = items
                .Select(p => new PlaylistListItemDto(
                    p.Id,
                    p.Name,
                    p.Genre,
                    p.Description,
                    p.PlaylistSongs.Count))
                .ToList();

            var result = new PagedResult<PlaylistListItemDto>
            {
                Items = list,
                Total = total,
                Page = page,
                PageSize = pageSize
            };

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);
            return result;
        }

        // ----------------- Get (full) -----------------
        public async Task<PlaylistDetailsDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var cacheKey = PlaylistKey(id);
            var cached = await _cache.GetAsync<PlaylistDetailsDto>(cacheKey, ct);
            if (cached is not null) return cached;

            var entity = await _playlists.GetFullAsync(id, ct);
            if (entity is null) return null;

            var dto = ToDetails(entity);

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), ct);
            return dto;
        }

        // ------------- Add song (tail) -------------
        public async Task AddSongAsync(int playlistId, int songId, CancellationToken ct = default)
        {
            if (!await _songs.ExistsAsync(songId, ct))
                throw new NotFoundException($"Song with id {songId} was not found.");

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                if (await _playlists.ContainsSongAsync(playlistId, songId, ct))
                    return;

                var nextOrder = await _playlists.GetMaxOrderAsync(playlistId, ct) + 1;
                await _playlists.AddSongAsync(playlistId, songId, nextOrder, addedByUserId: null, ct);
            }, ct);

            await _cache.RemoveAsync(PlaylistKey(playlistId), ct);
            await _cache.RemoveAsync(SongKey(songId), ct); 
        }

        // ------------- Insert at position -------------
        public async Task AddSongAtAsync(int playlistId, int songId, int order, int? addedByUserId, CancellationToken ct = default)
        {
            if (order < 1) order = 1;
            if (!await _songs.ExistsAsync(songId, ct))
                throw new NotFoundException($"Song with id {songId} was not found.");

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                if (await _playlists.ContainsSongAsync(playlistId, songId, ct))
                    throw new InvalidOperationException("Song already in playlist.");

                await _playlists.ShiftOrdersDownAsync(playlistId, order, ct);
                await _playlists.AddSongAsync(playlistId, songId, order, addedByUserId, ct);
            }, ct);

            await _cache.RemoveAsync(PlaylistKey(playlistId), ct);
            await _cache.RemoveAsync(SongKey(songId), ct); 
        }

        // ------------- Bulk add -------------
        public async Task AddSongsAsync(int playlistId, IReadOnlyList<int> songIds, int? addedByUserId, CancellationToken ct = default)
        {
            if (songIds == null || songIds.Count == 0) return;

            var touched = new HashSet<int>();

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                var order = await _playlists.GetMaxOrderAsync(playlistId, ct);
                foreach (var sid in songIds.Distinct())
                {
                    if (!await _songs.ExistsAsync(sid, ct)) continue;
                    if (await _playlists.ContainsSongAsync(playlistId, sid, ct)) continue;

                    await _playlists.AddSongAsync(playlistId, sid, ++order, addedByUserId, ct);
                    touched.Add(sid);
                }
            }, ct);

            await _cache.RemoveAsync(PlaylistKey(playlistId), ct);
            foreach (var sid in touched)
                await _cache.RemoveAsync(SongKey(sid), ct);
        }

        // ------------- Remove song -------------
        public async Task RemoveSongAsync(int playlistId, int songId, CancellationToken ct = default)
        {
            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                if (!await _playlists.ContainsSongAsync(playlistId, songId, ct)) return;
                await _playlists.RemoveSongAsync(playlistId, songId, ct);
            }, ct);

            await _cache.RemoveAsync(PlaylistKey(playlistId), ct);
            await _cache.RemoveAsync(SongKey(songId), ct); 
        }

        // ------------- Reorder -------------
        public async Task ReorderAsync(int playlistId, int songId, int newOrder, CancellationToken ct = default)
        {
            if (newOrder < 1) newOrder = 1;

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                var max = await _playlists.GetMaxOrderAsync(playlistId, ct);
                if (max == 0) return;

                if (newOrder > max) newOrder = max;

                await _playlists.ShiftOrdersDownAsync(playlistId, newOrder, ct);
                await _playlists.UpdateSongOrderAsync(playlistId, songId, newOrder, ct);
            }, ct);

            await _cache.RemoveAsync(PlaylistKey(playlistId), ct);
            await _cache.RemoveAsync(SongKey(songId), ct); 
        }



        // ----------------- mapping -----------------
        // PlaylistService

        private static PlaylistDetailsDto ToDetails(PlaylistEntity entity)
        {
            var songs = entity.PlaylistSongs
                .OrderBy(ps => ps.Order)
                .Select(ps => new PlaylistSongItemDto(
                    SongId: ps.SongId,
                    Title: ps.Song.Title,
                    ArtistName: ps.Song.Artist.Name,
                    Genre: ps.Song.Genre,
                    Order: ps.Order))
                .ToList();

            return new PlaylistDetailsDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Genre,
                songs);
        }


    }
}
