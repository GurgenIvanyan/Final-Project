// Infrastructure/Persistence/Repositories/VoteRepository.cs
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;

namespace Playlist.Api.Infrastructure.Persistence.Repositories
{
    public class VoteRepository : GenericRepository<PlaylistSongVote>, IVoteRepository
    {
        public VoteRepository(AppDbContext db) : base(db) { }

     
        public async Task<(IReadOnlyList<(Song song, int likes)> items, int total)> GetTopLikedSongsAsync(
            int minLikes, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            // агрегируем лайки по SongId
            var agg = _db.PlaylistSongVotes
                .GroupBy(v => v.SongId)
                .Select(g => new { SongId = g.Key, Likes = g.Sum(x => x.Value) })
                .Where(a => a.Likes >= minLikes);

            // ЯВНО присоединяем Songs и Artists
            var q = from a in agg
                    join s in _db.Songs on a.SongId equals s.Id
                    join ar in _db.Artists on s.ArtistId equals ar.Id
                    select new
                    {
                        SongId = s.Id,
                        s.Title,
                        s.Genre,
                        s.ArtistId,
                        ArtistName = ar.Name,
                        a.Likes
                    };

            var total = await q.CountAsync(ct);

            var rows = await q
                .OrderByDescending(x => x.Likes)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.SongId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            // строим "плоский" Song с вложенным Artist — без трекинга, без Include
            var items = rows.Select(r =>
                (new Song
                {
                    Id = r.SongId,
                    Title = r.Title,
                    Genre = r.Genre,
                    ArtistId = r.ArtistId,
                    Artist = new Artist { Id = r.ArtistId, Name = r.ArtistName }
                }, r.Likes)
            ).ToList();

            return (items, total);
        }

       
        
    }
}
