// Core/Interfaces/Repositories/IVoteRepository.cs
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;

public interface IVoteRepository : IGenericRepository<PlaylistSongVote>
{
    
    Task<(IReadOnlyList<(Song song, int likes)> items, int total)> GetTopLikedSongsAsync(
        int minLikes, int page, int pageSize, CancellationToken ct = default);
}
