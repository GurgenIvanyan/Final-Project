using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record SongCreateDto(
     string Title,
     string? Genre,
     int ArtistId,
     string? Album,
     int? Year,
     int? PlaylistId = null,  // optional: attach to playlist when creating
     int? Order = null        // optional: desired order in that playlist
 );


}
