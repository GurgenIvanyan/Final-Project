using Playlist.Api.Core.Entities;

namespace Playlist.Api.Core.Entities;


public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Country { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


    public ICollection<Song> Songs { get; set; } = new List<Song>();
}