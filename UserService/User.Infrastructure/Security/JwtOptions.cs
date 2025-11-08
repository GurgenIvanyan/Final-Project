using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Infrastructure.Security
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = "user-service";
        public string Audience { get; set; } = "user-clients";
        public string SigningKey { get; set; } = "CHANGE_ME_SUPER_SECRET_32+";
        public int ExpMinutes { get; set; } = 60;
    }
}
