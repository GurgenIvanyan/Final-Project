using System.Security.Cryptography;
using System.Text;
using User.Application.Abstractions.Security;   

namespace User.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher   
    {
        public string Hash(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
    }
}
