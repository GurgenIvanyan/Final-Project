// Infrastructure/Persistence/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;

namespace Playlist.Api.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct);

    public Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.UserName == userName, ct);

    public Task<bool> ExistsByUserNameExceptAsync(string userName, int exceptId, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.UserName == userName && u.Id != exceptId, ct);
}
