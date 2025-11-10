

namespace Playlist.Api.Core.Entities;


public class Song
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Genre { get; set; } = default!;
    public TimeSpan Duration { get; set; }
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = default!;
    public SongMetadata Metadata { get; set; } = default!; // 1:1

  
    public ICollection<SongLike> SongLikes { get; set; } = new List<SongLike>();

    public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}