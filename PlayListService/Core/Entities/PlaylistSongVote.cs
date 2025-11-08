namespace Playlist.Api.Core.Entities;


public class PlaylistSongVote
{
    public int Id { get; set; }
    public int PlaylistId { get; set; }
    public int SongId { get; set; }
    public int UserId { get; set; }
    public int Value { get; set; } // +1 / -1
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


    public PlaylistSong PlaylistSong { get; set; } = default!;
}