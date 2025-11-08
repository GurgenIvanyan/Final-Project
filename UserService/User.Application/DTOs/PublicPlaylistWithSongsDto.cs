using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Application.DTOs
{
    public record PublicPlaylistWithSongsDto(
        int Id,
        string Name,
        string? Description,
        bool IsPublic,
        IReadOnlyList<UserPlaylistSongItemDto> Songs
    );
}
