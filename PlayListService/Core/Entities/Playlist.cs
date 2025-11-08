using Playlist.Api.Core.Entities;

namespace Playlist.Api.Core.Entities;


public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public int OwnerUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}