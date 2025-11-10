namespace Playlist.Api.Core.Entities
{
    public class SongLike
    {
        public int Id { get; set; }               
        public int SongId { get; set; }             
        public int UserId { get; set; }             
        public int Value { get; set; } = 1;         
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Song Song { get; set; } = null!;
    }
}
