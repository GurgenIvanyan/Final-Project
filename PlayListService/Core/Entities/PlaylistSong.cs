// Core/Entities/PlaylistSong.cs
namespace Playlist.Api.Core.Entities;

public class PlaylistSong
{
    public int PlaylistId { get; set; }
    public Playlist Playlist { get; set; } = null!;

    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    public int Order { get; set; }                 // position in playlist
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
    public int? AddedByUserId { get; set; }        // optional: who added
    public ICollection<PlaylistSongVote> Votes { get; set; } = new List<PlaylistSongVote>();
}
