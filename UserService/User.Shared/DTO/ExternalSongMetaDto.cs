namespace User.Shared.DTO
{
    public sealed record ExternalSongMetaDto(
        int Id,
        string Title,
        string ArtistName,
        string? Album,
        
        int? Year,
        string? Genre
       
    );
}
