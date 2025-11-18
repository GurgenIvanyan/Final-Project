using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Shared.DTO
{
    public record ExternalPlaylistSongItemDto(
    int SongId,
    string Title,
    string ArtistName,
    string? Genre,
    int Order);
}
