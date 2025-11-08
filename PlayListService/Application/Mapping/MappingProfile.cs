using Application.DTOs;
using AutoMapper;
using Playlist.Api.Core.Entities;
using PlaylistEntity = Playlist.Api.Core.Entities.Playlist;
// 👇 алиас, чтобы не конфликтовать с корневым namespace "User"
using CoreUser = Playlist.Api.Core.Entities.User;

namespace Playlist.Api.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Artist -> ArtistDto
            CreateMap<Artist, ArtistDto>();

            // Song -> SongDto (если SongDto имеет параметр ArtistName в ctor)
            CreateMap<Song, SongDto>()
                .ForCtorParam("ArtistName", opt => opt.MapFrom(s => s.Artist != null ? s.Artist.Name : string.Empty));

            // Playlist -> PlaylistDto
            CreateMap<PlaylistEntity, PlaylistDto>();

        
        }
    }
}
