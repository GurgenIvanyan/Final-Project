
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Services.IServices;
using AutoMapper;
using Playlist.Api.Core.Interfaces.Repositories;

namespace Playlist.Api.Application.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artists;
        private readonly ISongRepository _songs;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _map;

        public ArtistService(
            IArtistRepository artists,
            ISongRepository songs,
            IUnitOfWork uow,
            IMapper map)
        {
            _artists = artists;
            _songs = songs;
            _uow = uow;
            _map = map;
        }

        // -------------------- Create --------------------
        public async Task<ArtistDto> CreateAsync(ArtistCreateDto dto, CancellationToken ct = default)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Artist name is required.", nameof(dto.Name));

            var name = dto.Name.Trim();
            var country = dto.Country?.Trim();

            if (await _artists.ExistsByNameAsync(name, ct))
                throw new InvalidOperationException("Artist with the same name already exists.");

            var entity = new Core.Entities.Artist { Name = name, Country = country };

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _artists.AddAsync(entity, ct);
            }, ct);

            return _map.Map<ArtistDto>(entity);
        }

        // -------------------- Update --------------------
        public async Task<ArtistDto> UpdateAsync(int id, ArtistUpdateDto dto, CancellationToken ct = default)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Artist name is required.", nameof(dto.Name));

            var entity = await _artists.GetByIdAsync(id, ct)
                         ?? throw new KeyNotFoundException("Artist not found.");

            var newName = dto.Name.Trim();
            var newCountry = dto.Country?.Trim();

           
            if (!string.Equals(entity.Name, newName, StringComparison.OrdinalIgnoreCase) &&
                await _artists.ExistsByNameExceptAsync(newName, id, ct))
                throw new InvalidOperationException("Artist with the same name already exists.");

            entity.Name = newName;
            entity.Country = newCountry;

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _artists.UpdateAsync(entity, ct);
            }, ct);

            return _map.Map<ArtistDto>(entity);
        }

        // -------------------- Delete --------------------
        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _artists.GetByIdAsync(id, ct)
                         ?? throw new KeyNotFoundException("Artist not found.");

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _artists.DeleteAsync(entity, ct);
            }, ct);
        }

        // --------------- GetAll (без песен) ---------------
        public async Task<IReadOnlyList<ArtistDto>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _artists.GetAllAsync(ct);
            return items.Select(_map.Map<ArtistDto>).ToList();
        }

    
        public async Task<IReadOnlyList<ArtistWithSongsListItemDto>> GetAllWithSongsAsync(CancellationToken ct = default)
        {
            var items = await _artists.GetAllWithSongsAsync(ct);
            return items.Select(a => new ArtistWithSongsListItemDto(
                a.Id,
                a.Name,
                a.Country,
                a.Songs
                    .OrderBy(s => s.Title)
                    .Select(s => new SongRefDto(s.Id, s.Title))
                    .ToList()
            )).ToList();
        }

     
        public async Task<ArtistDetailsDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var a = await _artists.GetWithSongsAsync(id, ct);
            if (a is null) return null;

            var songs = a.Songs
                .OrderBy(s => s.Title)
                .Select(s => new SongRefDto(s.Id, s.Title))
                .ToList();

            return new ArtistDetailsDto(a.Id, a.Name, a.Country, songs);
        }

   
        public async Task AddSongAsync(int artistId, int songId, CancellationToken ct = default)
        {
           
            var artist = await _artists.GetByIdAsync(artistId, ct)
                         ?? throw new KeyNotFoundException("Artist not found.");
            var song = await _songs.GetByIdAsync(songId, ct)
                       ?? throw new KeyNotFoundException("Song not found.");

          
            if (song.ArtistId == artistId) return;

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
               
                song.ArtistId = artistId;
                await _songs.UpdateAsync(song, ct);
            }, ct);
        }

        // --------- Delete song from artist ---------
       
        public async Task DeleteSongFromArtistAsync(int artistId, int songId, CancellationToken ct = default)
        {
            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _artists.DeleteSongOfArtistAsync(artistId, songId, ct);
            }, ct);
        }
    }
}
