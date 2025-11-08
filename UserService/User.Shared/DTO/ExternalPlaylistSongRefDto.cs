using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Shared.DTO
{
    public sealed record ExternalPlaylistSongRefDto(int SongId, int Order);
}
