using Application.DTOs;
using AutoMapper;
using Playlist.Api.Core.Entities;
using PlaylistEntity = Playlist.Api.Core.Entities.Playlist;

using CoreUser = Playlist.Api.Core.Entities.User;

namespace Playlist.Api.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
          
            CreateMap<Artist, ArtistDto>();

            
            CreateMap<Song, SongDto>()
                .ForCtorParam("ArtistName", opt => opt.MapFrom(s => s.Artist != null ? s.Artist.Name : string.Empty));

           
            CreateMap<PlaylistEntity, PlaylistDto>();

        
        }
    }
}
