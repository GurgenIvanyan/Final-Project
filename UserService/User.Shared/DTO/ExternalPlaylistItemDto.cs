using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Shared.DTO
{
    public record ExternalPlaylistListItemDto(
       int Id,
       string Name,
       string? Description,
       string? Genre,
        int SongCount
   );
}
