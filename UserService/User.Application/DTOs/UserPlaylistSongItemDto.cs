using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Application.DTOs
{
    public record UserPlaylistSongItemDto(
        int SongId,
       string Title,
        int Order
    );
}
