
using Playlist.Api.Core.Entities;

namespace Playlist.Api.Core.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default);
    Task<bool> ExistsByUserNameExceptAsync(string userName, int exceptId, CancellationToken ct = default);
}
