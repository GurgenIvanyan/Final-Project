namespace Playlist.Api.Core.Entities
{
    public class SongLike
    {
        public int Id { get; set; }                 // PK (identity)
        public int SongId { get; set; }             // FK -> songs.Id
        public int UserId { get; set; }             // Кто лайкнул
        public int Value { get; set; } = 1;         // 1 или -1
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Song Song { get; set; } = null!;
    }
}
