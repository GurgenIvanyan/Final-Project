using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Application.Abstractions.Security
{
    public interface IJwtTokenService
    {
        string Create(int userId, string userName, string role);
    }
}
