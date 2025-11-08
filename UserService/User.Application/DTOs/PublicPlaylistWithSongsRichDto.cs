using System.Collections.Generic;

namespace User.Application.DTOs
{
    public sealed record PublicPlaylistWithSongsRichDto(
        int Id,
        string Name,
        string? Description,
        bool IsPublic,
        IReadOnlyList<UserPlaylistSongRichItemDto> Songs
    );
}
