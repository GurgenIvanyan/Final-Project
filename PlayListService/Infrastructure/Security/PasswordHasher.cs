using System.Security.Cryptography;
using System.Text;


namespace Playlist.Api.Infrastructure.Security;


public static class PasswordHasher
{
    public static string Hash(string input)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }
}