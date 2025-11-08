using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UserEntity = User.Core.Entities.User;

namespace User.Core.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<UserEntity>
    {
        Task<UserEntity?> GetByUserNameAsync(string userName, CancellationToken ct = default);
        Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default);
        Task<bool> ExistsByUserNameExceptAsync(string userName, int exceptId, CancellationToken ct = default);
    }
}
