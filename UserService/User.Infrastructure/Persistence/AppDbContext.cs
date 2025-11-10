using Microsoft.EntityFrameworkCore;
using User.Core.Entities;
using UserEntity = User.Core.Entities.User;
using UserPlaylistEntity = User.Core.Entities.UserPlaylist;
using UserPlaylistSongEntity = User.Core.Entities.UserPlaylistSong;

namespace User.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        public DbSet<UserEntity> Users => Set<UserEntity>();
        public DbSet<UserPlaylistEntity> UserPlaylists => Set<UserPlaylistEntity>();
        public DbSet<UserPlaylistSongEntity> UserPlaylistSongs => Set<UserPlaylistSongEntity>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // -------- users --------
            b.Entity<UserEntity>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);

                e.Property(x => x.UserName)
                    .IsRequired()
                    .HasMaxLength(128);

                e.HasIndex(x => x.UserName)
                    .IsUnique();

                e.Property(x => x.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(512);

                e.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            // -------- user_playlists --------
            b.Entity<UserPlaylistEntity>(e =>
            {
                e.ToTable("user_playlists");
                e.HasKey(x => x.Id);

                e.Property(x => x.OwnerUserId)
                    .IsRequired();

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(x => x.Description)
                    .HasMaxLength(2000);

               
                e.Property(x => x.IsPublic)
                    .HasDefaultValue(false);

                e.Property(x => x.SourcePlaylistId);

                e.HasIndex(x => new { x.OwnerUserId, x.Id });
            });

            // -------- user_playlist_songs --------
            b.Entity<UserPlaylistSongEntity>(e =>
            {
                e.ToTable("user_playlist_songs");
                e.HasKey(x => new { x.UserPlaylistId, x.SongId });

                e.Property(x => x.Order)
                    .IsRequired();

                e.HasIndex(x => new { x.UserPlaylistId, x.Order })
                    .IsUnique();

                e.HasOne(x => x.Playlist)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(x => x.UserPlaylistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
