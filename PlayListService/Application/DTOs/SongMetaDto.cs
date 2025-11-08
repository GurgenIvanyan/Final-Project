// Application/DTOs/SongMetaDto.cs  (Playlist.Api)
namespace Application.DTOs
{
    public sealed record SongMetaDto(
        int Id,
        string Title,
        string ArtistName,
        string? Album,
        int? Year,
        string? Genre
    );
}
