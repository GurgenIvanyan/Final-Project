using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record SongDetailsDto(
       int Id,
       string Title,
       string? Genre,
       int ArtistId,
       string ArtistName,
       IReadOnlyList<SongPlaylistRefDto> Playlists
   );
}
