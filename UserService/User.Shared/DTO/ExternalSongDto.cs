using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Shared.DTO
{
    public record ExternalSongDto(int Id, string Title, string? Genre, int ArtistId, string ArtistName);

}
