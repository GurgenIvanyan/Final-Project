using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Application.DTOs
{
    public record UserPlaylistDetailsDto(int Id, string Name, string? Description, bool IsPublic, int? SourcePlaylistId, IReadOnlyList<UserPlaylistSongItemDto> Songs);
}
