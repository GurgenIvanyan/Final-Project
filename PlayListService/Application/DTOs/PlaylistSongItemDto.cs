using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record PlaylistSongItemDto(
    int SongId,
    string Title,
    string ArtistName,
    string? Genre,
    int Order);
}
