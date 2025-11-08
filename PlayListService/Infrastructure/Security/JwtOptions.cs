namespace Playlist.Api.Infrastructure.Security;


public class JwtOptions
{
    public string Issuer { get; set; } = "playlist-api";
    public string Audience { get; set; } = "playlist-clients";
    public string SigningKey { get; set; } = "PLEASE_CHANGE_ME_SUPER_SECRET_KEY_32+"; // replace via env
    public int ExpMinutes { get; set; } = 60;
}