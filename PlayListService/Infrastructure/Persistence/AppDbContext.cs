
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Infrastructure.Security; 
using PlaylistEntity = Playlist.Api.Core.Entities.Playlist;

namespace Playlist.Api.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SongLike> SongLikes => Set<SongLike>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Artist> Artists => Set<Artist>();
        public DbSet<Song> Songs => Set<Song>();
        public DbSet<SongMetadata> SongMetadata => Set<SongMetadata>();
        public DbSet<PlaylistEntity> Playlists => Set<PlaylistEntity>();
        public DbSet<PlaylistSong> PlaylistSongs => Set<PlaylistSong>();
        public DbSet<PlaylistSongVote> PlaylistSongVotes => Set<PlaylistSongVote>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ---------------- User ----------------
            b.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);
                e.Property(x => x.UserName).IsRequired().HasMaxLength(128);
                e.HasIndex(x => x.UserName).IsUnique();
                e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
                e.Property(x => x.Role).IsRequired().HasMaxLength(64);
            });

            // ---------------- Artist --------------
            b.Entity<Artist>(e =>
            {
                e.ToTable("artists");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).IsRequired().HasMaxLength(256);
                e.Property(x => x.Country).HasMaxLength(128);

                e.HasIndex(x => x.Name).IsUnique();
            });

            // ---------------- Song ----------------
            b.Entity<Song>(e =>
            {
                e.ToTable("songs");
                e.HasKey(x => x.Id);

                e.Property(x => x.Title).IsRequired().HasMaxLength(256);
                e.Property(x => x.Genre).HasMaxLength(64);

                e.HasOne(x => x.Artist)
                 .WithMany(a => a.Songs)
                 .HasForeignKey(x => x.ArtistId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            
            b.Entity<SongMetadata>(e =>
            {
                e.ToTable("song_metadata");
                e.HasKey(m => m.SongMetadataId);

                e.Property(m => m.Album).HasMaxLength(256);
                e.Property(m => m.ExternalId).HasMaxLength(128);
                e.Property(m => m.Mood).HasMaxLength(64);

                e.HasOne(m => m.Song)
                 .WithOne(s => s.Metadata)
                 .HasForeignKey<SongMetadata>(m => m.SongId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(m => m.SongId).IsUnique(); 
            });

            // --------------- Playlist -------------
            b.Entity<PlaylistEntity>(e =>
            {
                e.ToTable("playlists");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).HasMaxLength(2000);
                e.Property(x => x.Genre).HasMaxLength(64);
                e.Property(x => x.OwnerUserId).IsRequired();

                e.HasIndex(x => new { x.Genre, x.Id }); 
            });

            // -------- PlaylistSong (N:M + order) --
            b.Entity<PlaylistSong>(e =>
            {
                e.ToTable("playlist_songs");
                e.HasKey(x => new { x.PlaylistId, x.SongId }); // prevent duplicates

                e.Property(x => x.Order).IsRequired();
                e.Property(x => x.AddedAtUtc).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                e.HasIndex(x => new { x.PlaylistId, x.Order }).IsUnique(); // stable ordering

                e.HasOne(x => x.Playlist)
                 .WithMany(p => p.PlaylistSongs)
                 .HasForeignKey(x => x.PlaylistId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Song)
                 .WithMany(s => s.PlaylistSongs)
                 .HasForeignKey(x => x.SongId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // -------- PlaylistSongVote (unique) ---
            b.Entity<PlaylistSongVote>(e =>
            {
                e.ToTable("playlist_song_votes");
                e.HasKey(v => new { v.PlaylistId, v.SongId, v.UserId });

                e.Property(v => v.Value).IsRequired();
                e.ToTable(tb => tb.HasCheckConstraint("CK_Vote_Value", "\"Value\" IN (-1, 1)"));

                e.HasOne(v => v.PlaylistSong)
                 .WithMany(ps => ps.Votes)
                 .HasForeignKey(v => new { v.PlaylistId, v.SongId })
                 .OnDelete(DeleteBehavior.Cascade);
            });
            b.Entity<SongLike>(e =>
            {
                e.ToTable("song_likes");
                e.HasKey(x => x.Id); // int identity

                e.Property(x => x.Value).IsRequired();
                e.ToTable(tb => tb.HasCheckConstraint("CK_SongLike_Value", "\"Value\" IN (-1, 1)"));
                e.Property(x => x.CreatedAtUtc)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                e.HasOne(x => x.Song)
                 .WithMany(s => s.SongLikes)
                 .HasForeignKey(x => x.SongId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.SongId, x.UserId }).IsUnique(); 
            });

            // -------- Seed admin user -------------
            b.Entity<User>().HasData(new User
            {
                Id = 1,
                UserName = "admin",
                PasswordHash = PasswordHasher.Hash("demo123"),
                Role = "Admin"
            });
        }
    }
}
