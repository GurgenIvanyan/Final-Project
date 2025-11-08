namespace Playlist.Api.Core.Entities;


public class SongMetadata
{
    public int SongMetadataId { get; set; }
    public int SongId { get; set; }
    public string? Album { get; set; }
    public int? Year { get; set; }
    public string? ExternalId { get; set; } 
    public string? Mood { get; set; }


    public Song Song { get; set; } = default!;
}