using Microsoft.EntityFrameworkCore;
using User.Core.Interfaces.Repositories;
using User.Infrastructure.Persistence;
using User.Infrastructure.Persistence.Repositories;
using User.Infrastructure.Persistence;
using UserEntity = User.Core.Entities.User;

namespace User.Infrastructure.Persistence.Repositories 
{
    public class UserRepository : GenericRepository<UserEntity>, IUserRepository
    {
        public UserRepository(AppDbContext db) : base(db) { }

        public Task<UserEntity?> GetByUserNameAsync(string userName, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct);

        public Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.UserName == userName, ct);

        public Task<bool> ExistsByUserNameExceptAsync(string userName, int exceptId, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.UserName == userName && u.Id != exceptId, ct);
    }
}
