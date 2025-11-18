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


        public async Task<UserPlaylistDto> ImportAsync(
    int ownerUserId,
    ImportPlaylistDto dto,
    CancellationToken ct = default)
        {
            var src = await _gateway.GetPlaylistAsync(dto.SourcePlaylistId, ct)
                      ?? throw new NotFoundException(AppErrors.PlaylistNotFound(dto.SourcePlaylistId));

            var entity = new UserPlaylist
            {
                OwnerUserId = ownerUserId,
                Name = src.Name,
                Description = src.Description,
                IsPublic = false,
                SourcePlaylistId = src.Id,
                // կարևոր է, որ collection-ը ինիցիալիզացված լինի
                Songs = new List<UserPlaylistSong>()
            };

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                // 👉 Կրկնվող երգերը հանենք SongId-ով
                var distinctSongs = src.Songs
                    .OrderBy(x => x.Order)
                    .GroupBy(x => x.SongId)
                    .Select(g => g.First());

                int order = 0;
                foreach (var s in distinctSongs)
                {
                    entity.Songs.Add(new UserPlaylistSong
                    {
                        SongId = s.SongId,
                        Order = ++order
                        // UserPlaylistId-ը EF-ն ինքը կդնի, որովհետև այս child-ը կապված է entity-ի հետ
                    });
                }

                // Մի անգամ playlist-ը ավելացնում ենք context-ին
                await _playlists.AddAsync(entity, ct);

                // action-ում այլ SaveChanges չկա, _uow.SaveChangesAsync-ը կանչվելու է ExecuteInTransactionAsync-ի վերջում
            }, ct);

            return new UserPlaylistDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.IsPublic,
                entity.SourcePlaylistId
            );
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
                    if (s is null) continue; // ignore undefined ids

                    if (await _playlists.ContainsSongAsync(pl.Id, sid, ct)) continue;

                    await _playlists.AddSongAsync(pl.Id, sid, ++order, ct);
                }
            }, ct);
        }

        public async Task RemoveSongAsync(
          int ownerUserId,
          int playlistId,
          int songId,
          CancellationToken ct = default)
        {
            // загружаем плейлист вместе с песнями
            var pl = await _playlists.GetFullAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                // 1) убираем связь плейлист-песня
                await _playlists.RemoveSongAsync(pl.Id, songId, ct);

                // 2) снова читаем песни и нормализуем порядковые номера 1..N
                var full = await _playlists.GetFullAsync(pl.Id, ct);
                if (full?.Songs is null || full.Songs.Count == 0)
                    return;

                int order = 0;
                foreach (var s in full.Songs.OrderBy(x => x.Order))
                {
                    s.Order = ++order;
                }

                await _playlists.UpdateAsync(full, ct);
            }, ct);
        }


        public async Task ReorderAsync(
     int ownerUserId,
     int playlistId,
     int songId,
     int newOrder,
     CancellationToken ct = default)
        {
            // читаем плейлист вместе с песнями
            var pl = await _playlists.GetFullAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            if (newOrder < 1)
                throw new InvalidOperationException(AppErrors.OrderOutOfRange());

            // текущий список песен в порядке Order
            var songs = pl.Songs
                .OrderBy(x => x.Order)
                .ToList();

            var link = songs.FirstOrDefault(x => x.SongId == songId);
            if (link is null)
                return; // этой песни уже нет в плейлисте

            // убираем трек из старой позиции
            songs.Remove(link);

            // clamp newOrder в [1; songs.Count + 1]
            if (newOrder > songs.Count + 1)
                newOrder = songs.Count + 1;

            // вставляем на новое место (index = newOrder - 1)
            songs.Insert(newOrder - 1, link);

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                // 1) очищаем все связи для этого плейлиста
                await _playlists.RemoveAllSongsAsync(pl.Id, ct);

                // 2) заново добавляем песни с новыми порядками 1..N
                int order = 0;
                foreach (var s in songs)
                {
                    await _playlists.AddSongAsync(pl.Id, s.SongId, ++order, ct);
                }
            }, ct);
        }




        public async Task<PagedResult<UserPlaylistDto>> GetMineAsync(int ownerUserId, int page, int pageSize, CancellationToken ct = default)
        {
           
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
                throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId)); 

            if (pl.OwnerUserId != requesterUserId && !pl.IsPublic)
                throw new ForbiddenException(AppErrors.ForbiddenAction());           

            var refs = pl.Songs
                         .OrderBy(s => s.Order)
                         .Select(s => new UserPlaylistSongRefDto(s.SongId, s.Order))
                         .ToList();

            var ids = refs.Select(r => r.SongId).Distinct().ToArray();
            var titles = await _gateway.GetSongTitlesByIdsAsync(ids, ct);

            var view = refs.Select(r => new UserPlaylistSongItemDto(
    SongId: r.SongId,
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
        SongId: x.SongId,
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

            var (items, total) = await _playlists.GetPublicByOthersPagedAsync(requesterUserId, page, pageSize, ct);
            if (total == 0 || items.Count == 0)
                throw new NotFoundException(AppErrors.NoPublicPlaylistsWithSongs());

          
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
                            ArtistName: m.ArtistName ?? "Unknown Artist",
                            Album: m.Album,
                            Year: m.Year,
                            Genre: m.Genre
                        );
                    }

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
        public async Task DeleteAsync(int ownerUserId, int playlistId, CancellationToken ct = default)
        {
            var pl = await _playlists.GetByIdAsync(playlistId, ct)
                     ?? throw new NotFoundException(AppErrors.PlaylistNotFound(playlistId));

            if (pl.OwnerUserId != ownerUserId)
                throw new ForbiddenException(AppErrors.ForbiddenAction());

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                
                await _playlists.DeleteAsync(pl, ct);
            }, ct);
        }
        public async Task NormalizeOrdersAsync(int playlistId, CancellationToken ct)
        {
            var full = await _playlists.GetFullAsync(playlistId, ct);
            if (full?.Songs is null || full.Songs.Count == 0)
                return;

            var ordered = full.Songs
                .OrderBy(s => s.Order)
                .ToList();

            var order = 0;
            foreach (var s in ordered)
            {
                order++;
                await _playlists.UpdateSongOrderAsync(playlistId, s.SongId, order, ct);
            }
        }


    }
}
