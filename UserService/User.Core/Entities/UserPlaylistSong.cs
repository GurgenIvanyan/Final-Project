using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Core.Entities
{
    public class UserPlaylistSong
    {
        public int UserPlaylistId { get; set; }
        public UserPlaylist Playlist { get; set; } = null!;

        // Песня из PlaylistService
        public int SongId { get; set; }

        public int Order { get; set; } = 1;
        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
