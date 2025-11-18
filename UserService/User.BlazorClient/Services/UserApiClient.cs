using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace User.BlazorClient.Services;

// --- DTO-ներ Blazor-ի համար --- //
public record LoginRequestDto(string UserName, string Password);
public record RegisterRequestDto(string UserName, string Password);
public record AuthTokenDto(string AccessToken);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize);

public record SongWithLikesDto(
    int Id,
    string Title,
    string? Genre,
    int ArtistId,
    string ArtistName,
    int Likes);
public record ExternalPlaylistSongDto(
    int Id,
    string Title,
    string ArtistName,
    string? Genre);

public record ExternalPlaylistDetailsDto(
    int Id,
    string Name,
    string? Description,
    string? Genre,
    IReadOnlyList<ExternalPlaylistSongDto> Songs);


public record UserPlaylistDto(
    int Id,
    string Name,
    string? Description,
    bool IsPublic,
    int? SourcePlaylistId);

public record PublicPlaylistSongRichDto(
    int SongId,
    string Title,
    string ArtistName,
    string? Genre,
    int Likes,
    int Order);

public record PublicPlaylistWithSongsRichDto(
    int Id,
    string Name,
    string? Description,
    string OwnerUserName,
    IReadOnlyList<PublicPlaylistSongRichDto> Songs);


public record UserPlaylistSongItemDto(
    int SongId,
    string Title,
    int Order);

public record ExternalPlaylistListItemDto(
    int Id,
    string Name,
    string? Description,
    string? Genre,
    int SongsCount
);
public record UserPlaylistDetailsDto(
    int Id,
    string Name,
    string? Description,
    bool IsPublic,
    int? SourcePlaylistId,
    IReadOnlyList<UserPlaylistSongItemDto> Songs);

// Նոր DTO՝ search-ի համար (UserService → /songs/search → ExternalSongDto)
public record ExternalSongDto(
    int Id,
    string Title,
    string? Genre,
    int ArtistId,
    string ArtistName);

public class UserApiClient
{
    private readonly HttpClient _http;
    private readonly AuthState _auth;

    public UserApiClient(HttpClient http, AuthState auth)
    {
        _http = http;
        _auth = auth;
    }

    // ---------- helpers ----------
    private void AttachAuthHeader(HttpRequestMessage req)
    {
        if (!string.IsNullOrEmpty(_auth.Token))
        {
            req.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _auth.Token);
        }
    }

    private static PagedResult<T> EmptyPage<T>(int page, int pageSize)
        => new(Array.Empty<T>(), 0, page, pageSize);

    // ---------- Auth ----------
    public async Task<(bool Ok, string? Error)> LoginAsync(
        string userName,
        string password,
        CancellationToken ct = default)
    {
        var req = new LoginRequestDto(userName, password);
        HttpResponseMessage resp;

        try
        {
            resp = await _http.PostAsJsonAsync("/auth/login", req, ct);
        }
        catch (Exception ex)
        {
            return (false, $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var bodyErr = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {bodyErr}";
            return (false, msg);
        }

        var body = await resp.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
            return (false, "Empty response body from /auth/login.");

        var jsonOpts = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // 1) AuthTokenDto
        try
        {
            var dto = JsonSerializer.Deserialize<AuthTokenDto>(body, jsonOpts);
            if (dto is not null && !string.IsNullOrWhiteSpace(dto.AccessToken))
            {
                _auth.SetAuth(userName, dto.AccessToken);
                return (true, null);
            }
        }
        catch { }

        // 2) ուղղակի string JWT
        try
        {
            var token = JsonSerializer.Deserialize<string>(body, jsonOpts) ??
                        body.Trim().Trim('"');

            if (!string.IsNullOrWhiteSpace(token))
            {
                _auth.SetAuth(userName, token);
                return (true, null);
            }
        }
        catch
        {
            var token = body.Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(token))
            {
                _auth.SetAuth(userName, token);
                return (true, null);
            }
        }

        return (false, $"Could not parse token. Raw response: {body}");
    }

    public async Task<(bool Ok, string? Error)> RegisterAsync(
      string userName,
      string password,
      CancellationToken ct = default)
    {
        var dto = new RegisterRequestDto(userName, password);
        HttpResponseMessage resp;

        try
        {
            resp = await _http.PostAsJsonAsync("/auth/register", dto, ct);
        }
        catch (Exception ex)
        {
            return (false, $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (false, msg);
        }

        return (true, null);
    }

    public void Logout() => _auth.Logout();

    // ---------- Top liked songs ----------
    public async Task<(PagedResult<SongWithLikesDto> Page, string? Error)>
     GetTopLikedSongsAsync(
         int minLikes = 3,
         int page = 1,
         int pageSize = 20,
         CancellationToken ct = default)
    {
        var url = $"/user-playlists/top-liked?minLikes={minLikes}&page={page}&pageSize={pageSize}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);

        // 🔑 JWT-ը կպցնում ենք
        AttachAuthHeader(req);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return (EmptyPage<SongWithLikesDto>(page, pageSize),
                $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (EmptyPage<SongWithLikesDto>(page, pageSize), msg);
        }

        var dto = await resp.Content.ReadFromJsonAsync<PagedResult<SongWithLikesDto>>(cancellationToken: ct);
        if (dto is null)
            return (EmptyPage<SongWithLikesDto>(page, pageSize),
                "Empty response from /user-playlists/top-liked");

        return (dto, null);
    }


    // ---------- Like / Unlike song ----------
    public async Task<(int Score, string? Error)> LikeSongAsync(int songId, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"/songs/{songId}/like");
        AttachAuthHeader(req);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return (0, $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (0, msg);
        }

        var score = await resp.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        return (score, null);
    }

    public async Task<(int Score, string? Error)> UnlikeSongAsync(int songId, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"/songs/{songId}/like");
        AttachAuthHeader(req);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return (0, $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (0, msg);
        }

        var score = await resp.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        return (score, null);
    }

    // ---------- My playlists (list) ----------
    public async Task<PagedResult<UserPlaylistDto>> GetMyPlaylistsAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"/user-playlists/mine?page={page}&pageSize={pageSize}");

        AttachAuthHeader(req);

        var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        var result =
            await resp.Content.ReadFromJsonAsync<PagedResult<UserPlaylistDto>>(cancellationToken: ct);

        return result ?? new PagedResult<UserPlaylistDto>(
            Array.Empty<UserPlaylistDto>(), 0, page, pageSize);
    }

    // ---------- My playlist details ----------
    public async Task<(UserPlaylistDetailsDto? Playlist, string? Error)>
        GetMyPlaylistDetailsAsync(int playlistId, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/user-playlists/{playlistId}");
        AttachAuthHeader(req);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return (null, $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (null, msg);
        }

        var dto = await resp.Content.ReadFromJsonAsync<UserPlaylistDetailsDto>(cancellationToken: ct);
        if (dto is null)
            return (null, "Empty playlist response.");

        return (dto, null);
    }

    // ---------- Remove song from my playlist ----------
    public async Task<string?> RemoveSongFromMyPlaylistAsync(
        int playlistId,
        int songId,
        CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/user-playlists/{playlistId}/songs/{songId}");

        AttachAuthHeader(req);

        try
        {
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            }
            return null; // success
        }
        catch (Exception ex)
        {
            return $"HTTP error: {ex.Message}";
        }
    }

    // ---------- Move song (reorder) ----------
    public async Task<string?> MoveSongInMyPlaylistAsync(
        int playlistId,
        int songId,
        int newOrder,
        CancellationToken ct = default)
    {
        var payload = new
        {
            SongId = songId,
            Order = newOrder
        };

        var req = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/user-playlists/{playlistId}/songs/reorder")
        {
            Content = JsonContent.Create(payload)
        };

        AttachAuthHeader(req);

        try
        {
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            }
            return null; // success
        }
        catch (Exception ex)
        {
            return $"HTTP error: {ex.Message}";
        }
    }

    // ---------- Create user playlist ----------
    public async Task<(bool Ok, string? Error)> CreateUserPlaylistAsync(
        string name,
        string? description,
        bool isPublic,
        CancellationToken ct = default)
    {
        var payload = new
        {
            Name = name,
            Description = description,
            IsPublic = isPublic
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "/user-playlists")
        {
            Content = JsonContent.Create(payload)
        };

        AttachAuthHeader(req);

        try
        {
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return (false, $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"HTTP error: {ex.Message}");
        }
    }

    // ---------- 🔍 Search songs (UserService → /songs/search) ----------
    public async Task<(PagedResult<ExternalSongDto> Page, string? Error)>
        SearchSongsAsync(string? title, string? genre, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var url = $"/songs/search?title={Uri.EscapeDataString(title ?? "")}&genre={Uri.EscapeDataString(genre ?? "")}&page={page}&pageSize={pageSize}";

        HttpResponseMessage resp;
        try
        {
            resp = await _http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            return (EmptyPage<ExternalSongDto>(page, pageSize), $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (EmptyPage<ExternalSongDto>(page, pageSize), msg);
        }

        var dto = await resp.Content.ReadFromJsonAsync<PagedResult<ExternalSongDto>>(cancellationToken: ct);
        if (dto is null)
            return (EmptyPage<ExternalSongDto>(page, pageSize), "Empty response from /songs/search");

        return (dto, null);
    }

    // ---------- ➕ Add single song to my playlist ----------
    public async Task<string?> AddSongToMyPlaylistAsync(
        int playlistId,
        int songId,
        CancellationToken ct = default)
    {
        // AddSongDto { int SongId, int? Order } – մենք Order = null ենք տալիս,
        // որպեսզի service-ը երգը գցի playlist-ի վերջում
        var payload = new
        {
            SongId = songId,
            Order = (int?)null
        };

        var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"/user-playlists/{playlistId}/songs")
        {
            Content = JsonContent.Create(payload)
        };

        AttachAuthHeader(req);

        try
        {
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            }

            return null; // success
        }
        catch (Exception ex)
        {
            return $"HTTP error: {ex.Message}";
        }
    }
    public async Task<(PagedResult<PublicPlaylistWithSongsRichDto> Page, string? Error)>
    GetPublicPlaylistsWithSongsRichAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var url = $"/user-playlists/public-with-songs-rich?page={page}&pageSize={pageSize}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);

        // JWT-ը կցում ենք
        AttachAuthHeader(req);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return (EmptyPage<PublicPlaylistWithSongsRichDto>(page, pageSize),
                $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (EmptyPage<PublicPlaylistWithSongsRichDto>(page, pageSize), msg);
        }

        var dto = await resp.Content.ReadFromJsonAsync<PagedResult<PublicPlaylistWithSongsRichDto>>(cancellationToken: ct);
        if (dto is null)
            return (EmptyPage<PublicPlaylistWithSongsRichDto>(page, pageSize),
                "Empty response from /user-playlists/public-with-songs-rich");

        return (dto, null);
    }
    public async Task<string?> DeleteUserPlaylistAsync(
      int playlistId,
      CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"/user-playlists/{playlistId}");
        AttachAuthHeader(req);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return $"HTTP error: {ex.Message}";
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
        }

        return null; // success
    }
    public async Task<(PagedResult<ExternalPlaylistListItemDto> Page, string? Error)>
    GetExternalPlaylistsAsync(
        string? genre = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var url =
            $"/user-playlists/external?genre={Uri.EscapeDataString(genre ?? "")}&page={page}&pageSize={pageSize}";

        HttpResponseMessage resp;
        try
        {
            resp = await _http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            return (EmptyPage<ExternalPlaylistListItemDto>(page, pageSize),
                $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (EmptyPage<ExternalPlaylistListItemDto>(page, pageSize), msg);
        }

        var dto = await resp.Content
            .ReadFromJsonAsync<PagedResult<ExternalPlaylistListItemDto>>(cancellationToken: ct);

        if (dto is null)
            return (EmptyPage<ExternalPlaylistListItemDto>(page, pageSize),
                "Empty response from /user-playlists/external");

        return (dto, null);
    }
    public async Task<(UserPlaylistDto? Playlist, string? Error)>
        ImportPlaylistAsync(int sourcePlaylistId, CancellationToken ct = default)
    {
        var payload = new
        {
            SourcePlaylistId = sourcePlaylistId
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "/user-playlists/import")
        {
            Content = JsonContent.Create(payload)
        };

        // JWT
        AttachAuthHeader(req);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return (null, $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (null, msg);
        }

        // server-ը վերադարձնում է UserPlaylistDto
        var dto = await resp.Content.ReadFromJsonAsync<UserPlaylistDto>(cancellationToken: ct);
        if (dto is null)
            return (null, "Empty response from /user-playlists/import");

        return (dto, null);
    }
    public async Task<(ExternalPlaylistDetailsDto? Playlist, string? Error)>
     GetExternalPlaylistDetailsAsync(int id, CancellationToken ct = default)
    {
        var url = $"/user-playlists/external/{id}";

        HttpResponseMessage resp;
        try
        {
            resp = await _http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            return (null, $"HTTP error: {ex.Message}");
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            var msg = $"[{(int)resp.StatusCode}] {resp.StatusCode} {body}";
            return (null, msg);
        }

        var dto = await resp.Content
            .ReadFromJsonAsync<ExternalPlaylistDetailsDto>(cancellationToken: ct);

        if (dto is null)
            return (null, "Empty response from /user-playlists/external/{id}");

        return (dto, null);
    }



}
