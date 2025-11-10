using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Core.Entities
{
    public class UserPlaylist
    {
        public int Id { get; set; }
        public int OwnerUserId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsPublic { get; set; } = false;

      
        public int? SourcePlaylistId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<UserPlaylistSong> Songs { get; set; } = new List<UserPlaylistSong>();
    }
}
