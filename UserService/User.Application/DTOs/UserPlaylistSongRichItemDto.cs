namespace User.Application.DTOs
{
    public sealed record UserPlaylistSongRichItemDto(
        int Order,
        int SongId,
        string Title,
        string ArtistName,
        string? Album,
       
        int? Year,
        string? Genre
       
    );
}
