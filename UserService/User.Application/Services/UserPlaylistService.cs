using System.Linq;
using Application.Common.Errors;
using User.Application.Abstractions.Http;
using User.Application.Common.Errors;
using User.Application.DTOs;
using User.Application.Services.IServices;
using User.Core.Entities;
using User.Core.Interfaces.Repositories;
using User.Shared.Common;
using User.Shared.DTO;

namespace User.Application.Services
{
    public class UserPlaylistService : IUserPlaylistService
    {
        private readonly IUserPlaylistRepository _playlists;
        private readonly IUnitOfWork _uow;
        private readonly IPlaylistGateway _gateway;

        public UserPlaylistService(IUserPlaylistRepository playlists, IUnitOfWork uow, IPlaylistGateway gateway)
        { _playlists = playlists; _uow = uow; _gateway = gateway; }

        public async Task<UserPlaylistDto> CreateAsync(int ownerUserId, UserPlaylistCreateDto dto, CancellationToken ct = default)
        {
            // Field-level validation → красивый 422
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException(AppErrors.ValidationFailed(), new Dictionary<string, string[]>
                {
                    ["name"] = new[] { AppErrors.NameRequired() }
                });

            var entity = new UserPlaylist
            {
                OwnerUserId = ownerUserId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsPublic = dto.IsPublic ?? false
            };

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _playlists.AddAsync(entity, ct);
            }, ct);

            return new UserPlaylistDto(entity.Id, entity.Name, entity.Description, entity.IsPublic, entity.SourcePlaylistId);
        }

        public async Task<UserPlaylistDto> ImportAsync(int ownerUserId, ImportPlaylistDto dto, CancellationToken ct = default)
        {
            // внешняя проверка: красиво 404, если плейлист не найден в Playlist.Api
            var src = await _gateway.GetPlaylistAsync(dto.SourcePlaylistId, ct)
                      ?? throw new NotFoundException(AppErrors.PlaylistNotFound(dto.SourcePlaylistId));

            var entity = new UserPlaylist
            {
                OwnerUserId = ownerUserId,
                Name = src.Name,
                Description = src.Description,
                IsPublic = false,
                SourcePlaylistId = src.Id
            };

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _playlists.AddAsync(entity, ct);

                int order = 0;
                foreach (var s in src.Songs.OrderBy(x => x.Order))
                {
                    await _playlists.AddSongAsync(entity.Id, s.SongId, ++order, ct);
                }
            }, ct);

            return new UserPlaylistDto(entity.Id, entity.Name, entity.Description, entity.IsPublic, entity.SourcePlaylistId);
        }

        public async Task SetPublicAsync(int ownerUserId, int playlistId, bool isPublic, CancellationToken ct = default)
        {
            var pl = await _playlists.GetByIdAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            pl.IsPublic = isPublic;

            await _uow.ExecuteInTransactionAsync(async _ => { await _playlists.UpdateAsync(pl, ct); }, ct);
        }

        public async Task AddSongAsync(int ownerUserId, int playlistId, int songId, int? order, CancellationToken ct = default)
        {
            var pl = await _playlists.GetFullAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            var song = await _gateway.GetSongAsync(songId, ct);
            if (song is null)
                throw new NotFoundException(AppErrors.SongNotFound(songId));

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                if (await _playlists.ContainsSongAsync(pl.Id, songId, ct)) return;

                int ordTail = await _playlists.GetMaxOrderAsync(pl.Id, ct) + 1;
                int ord = order ?? ordTail;

                if (order is int desired && desired >= 1)
                    await _playlists.ShiftOrdersDownAsync(pl.Id, desired, ct);

                await _playlists.AddSongAsync(pl.Id, songId, ord, ct);
            }, ct);
        }

        public async Task AddSongsAsync(int ownerUserId, int playlistId, IReadOnlyList<int> songIds, CancellationToken ct = default)
        {
            var pl = await _playlists.GetFullAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            if (songIds is null || songIds.Count == 0) return;

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                int order = await _playlists.GetMaxOrderAsync(pl.Id, ct);
                foreach (var sid in songIds.Distinct())
                {
                    var s = await _gateway.GetSongAsync(sid, ct);
                    if (s is null) continue; // молча пропускаем неизвестные id

                    if (await _playlists.ContainsSongAsync(pl.Id, sid, ct)) continue;

                    await _playlists.AddSongAsync(pl.Id, sid, ++order, ct);
                }
            }, ct);
        }

        public async Task RemoveSongAsync(int ownerUserId, int playlistId, int songId, CancellationToken ct = default)
        {
            var pl = await _playlists.GetByIdAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                if (!await _playlists.ContainsSongAsync(pl.Id, songId, ct)) return;
                await _playlists.RemoveSongAsync(pl.Id, songId, ct);
            }, ct);
        }

        public async Task ReorderAsync(int ownerUserId, int playlistId, int songId, int newOrder, CancellationToken ct = default)
        {
            var pl = await _playlists.GetByIdAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            if (newOrder < 1)
                throw new InvalidOperationException(AppErrors.OrderOutOfRange());

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                var max = await _playlists.GetMaxOrderAsync(pl.Id, ct);
                if (max == 0) return;

                if (newOrder > max) newOrder = max;

                await _playlists.ShiftOrdersDownAsync(pl.Id, newOrder, ct);
                await _playlists.UpdateSongOrderAsync(pl.Id, songId, newOrder, ct);
            }, ct);
        }

        public async Task<PagedResult<UserPlaylistDto>> GetMineAsync(int ownerUserId, int page, int pageSize, CancellationToken ct = default)
        {
            // нормализуем пагинацию → если хочешь 422 — бросай ValidationException с errors
            if (page < 1)
                throw new ValidationException(AppErrors.ValidationFailed(), new Dictionary<string, string[]> { ["page"] = new[] { AppErrors.PageOutOfRange() } });
            if (pageSize <= 0)
                throw new ValidationException(AppErrors.ValidationFailed(), new Dictionary<string, string[]> { ["pageSize"] = new[] { AppErrors.PageSizeOutOfRange() } });

            var (items, total) = await _playlists.GetByOwnerPagedAsync(ownerUserId, page, pageSize, ct);
            var list = items.Select(p => new UserPlaylistDto(p.Id, p.Name, p.Description, p.IsPublic, p.SourcePlaylistId)).ToList();
            return new PagedResult<UserPlaylistDto>(list, total, page, pageSize);
        }

        public async Task<UserPlaylistDetailsDto?> GetDetailsAsync(int requesterUserId, int playlistId, CancellationToken ct = default)
        {
            var pl = await _playlists.GetFullAsync(playlistId, ct);
            if (pl is null)
                throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId)); // красивый 404

            if (pl.OwnerUserId != requesterUserId && !pl.IsPublic)
                throw new ForbiddenException(AppErrors.ForbiddenAction());           // красивый 403

            var refs = pl.Songs
                         .OrderBy(s => s.Order)
                         .Select(s => new UserPlaylistSongRefDto(s.SongId, s.Order))
                         .ToList();

            var ids = refs.Select(r => r.SongId).Distinct().ToArray();
            var titles = await _gateway.GetSongTitlesByIdsAsync(ids, ct);

            var view = refs.Select(r => new UserPlaylistSongItemDto(
                                    Title: titles.TryGetValue(r.SongId, out var t) ? t : $"Song #{r.SongId}",
                                    Order: r.Order))
                           .ToList();

            return new UserPlaylistDetailsDto(
                pl.Id, pl.Name, pl.Description, pl.IsPublic, pl.SourcePlaylistId, view
            );
        }

        public async Task<PagedResult<UserPlaylistDto>> GetPublicByOthersAsync(
     int requesterUserId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1)
                throw new ValidationException(AppErrors.ValidationFailed(), new Dictionary<string, string[]> { ["page"] = new[] { AppErrors.PageOutOfRange() } });
            if (pageSize <= 0)
                throw new ValidationException(AppErrors.ValidationFailed(), new Dictionary<string, string[]> { ["pageSize"] = new[] { AppErrors.PageSizeOutOfRange() } });

            var (items, total) = await _playlists.GetPublicByOthersPagedAsync(requesterUserId, page, pageSize, ct);

            // NEW — пустой список считаем логической «не найдено»
            if (total == 0 || items.Count == 0)
                throw new NotFoundException(AppErrors.NoPublicPlaylists());

            var list = items
                .Select(p => new UserPlaylistDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.IsPublic,
                    p.SourcePlaylistId))
                .ToList();

            return new PagedResult<UserPlaylistDto>(list, total, page, pageSize);
        }

        public async Task<PagedResult<PublicPlaylistWithSongsDto>> GetPublicByOthersWithSongsAsync(
    int requesterUserId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1)
                throw new ValidationException(AppErrors.ValidationFailed(), new Dictionary<string, string[]> { ["page"] = new[] { AppErrors.PageOutOfRange() } });
            if (pageSize <= 0)
                throw new ValidationException(AppErrors.ValidationFailed(), new Dictionary<string, string[]> { ["pageSize"] = new[] { AppErrors.PageSizeOutOfRange() } });

            var (items, total) = await _playlists.GetPublicByOthersPagedAsync(requesterUserId, page, pageSize, ct);

            // NEW — тоже 404, но текст другой (для ясности)
            if (total == 0 || items.Count == 0)
                throw new NotFoundException(AppErrors.NoPublicPlaylistsWithSongs());

            var songRefsByPlaylist = new Dictionary<int, List<UserPlaylistSongRefDto>>();
            var allSongIds = new HashSet<int>();

            foreach (var pl in items)
            {
                var full = await _playlists.GetFullAsync(pl.Id, ct)
                           ?? throw new NotFoundException(AppErrors.PlaylistDeletedDuringRead());

                var refs = full.Songs
                    .OrderBy(s => s.Order)
                    .Select(s => new UserPlaylistSongRefDto(s.SongId, s.Order))
                    .ToList();

                songRefsByPlaylist[pl.Id] = refs;
                foreach (var r in refs) allSongIds.Add(r.SongId);
            }

            var titles = allSongIds.Count == 0
                ? new Dictionary<int, string>()
                : await _gateway.GetSongTitlesByIdsAsync(allSongIds, ct);

            var result = items.Select(pl =>
            {
                var refs = songRefsByPlaylist.TryGetValue(pl.Id, out var r) ? r : new List<UserPlaylistSongRefDto>();
                var viewSongs = refs
                    .Select(x => new UserPlaylistSongItemDto(
                        Title: titles.TryGetValue(x.SongId, out var t) ? t : $"Song #{x.SongId}",
                        Order: x.Order))
                    .ToList();

                return new PublicPlaylistWithSongsDto(
                    Id: pl.Id,
                    Name: pl.Name,
                    Description: pl.Description,
                    IsPublic: pl.IsPublic,
                    Songs: viewSongs
                );
            }).ToList();

            return new PagedResult<PublicPlaylistWithSongsDto>(result, total, page, pageSize);
        }


        public async Task<PagedResult<PublicPlaylistWithSongsRichDto>> GetPublicByOthersWithSongsRichAsync(
            int requesterUserId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1)
                throw new ValidationException(AppErrors.ValidationFailed(),
                    new Dictionary<string, string[]> { ["page"] = new[] { AppErrors.PageOutOfRange() } });
            if (pageSize <= 0)
                throw new ValidationException(AppErrors.ValidationFailed(),
                    new Dictionary<string, string[]> { ["pageSize"] = new[] { AppErrors.PageSizeOutOfRange() } });

            // 1) Берём публичные плейлисты других пользователей (лайт-список)
            var (items, total) = await _playlists.GetPublicByOthersPagedAsync(requesterUserId, page, pageSize, ct);
            if (total == 0 || items.Count == 0)
                throw new NotFoundException(AppErrors.NoPublicPlaylistsWithSongs());

            // 2) Для каждого плейлиста грузим полную версию, чтобы получить SongId + Order
            var refsByPlaylist = new Dictionary<int, List<UserPlaylistSongRefDto>>();
            var allSongIds = new HashSet<int>();

            foreach (var pl in items)
            {
                var full = await _playlists.GetFullAsync(pl.Id, ct)
                           ?? throw new NotFoundException(AppErrors.PlaylistDeletedDuringRead());

                var refs = full.Songs
                    .OrderBy(s => s.Order)
                    .Select(s => new UserPlaylistSongRefDto(s.SongId, s.Order))
                    .ToList();

                refsByPlaylist[pl.Id] = refs;
                foreach (var r in refs) allSongIds.Add(r.SongId);
            }

            // 3) Тянем расширенную мету пачкой из Playlist.Api
            Dictionary<int, ExternalSongMetaDto> metaById;
            if (allSongIds.Count == 0)
            {
                metaById = new Dictionary<int, ExternalSongMetaDto>();
            }
            else
            {
                metaById = await _gateway.GetSongMetadataByIdsAsync(allSongIds, ct);
            }

            var result = items.Select(pl =>
            {
                var refs = refsByPlaylist.TryGetValue(pl.Id, out var r) ? r : new List<UserPlaylistSongRefDto>();
                var songs = refs.Select(x =>
                {
                    if (metaById.TryGetValue(x.SongId, out var m))
                    {
                        return new UserPlaylistSongRichItemDto(
                            Order: x.Order,
                            SongId: m.Id,
                            Title: m.Title,
                            // У тебя ArtistName в DTO — not-nullable string, чтобы избежать варнингов:
                            ArtistName: m.ArtistName ?? "Unknown Artist",
                            Album: m.Album,
                            Year: m.Year,
                            Genre: m.Genre
                        );
                    }

                    // fallback если мета не вернулась
                    return new UserPlaylistSongRichItemDto(
                        Order: x.Order,
                        SongId: x.SongId,
                        Title: $"Song #{x.SongId}",
                        ArtistName: "Unknown Artist",
                        Album: null,
                        Year: null,
                        Genre: null
                    );
                }).ToList();

                return new PublicPlaylistWithSongsRichDto(
                    Id: pl.Id,
                    Name: pl.Name,
                    Description: pl.Description,
                    IsPublic: pl.IsPublic,
                    Songs: songs
                );
            }).ToList();

            return new PagedResult<PublicPlaylistWithSongsRichDto>(result, total, page, pageSize);
        }

    }
}
