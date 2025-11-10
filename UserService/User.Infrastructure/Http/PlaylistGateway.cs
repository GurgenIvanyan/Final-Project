using System.Net.Http.Json;
using Application.Common.Errors;
using User.Application.Abstractions.Http;
using User.Application.DTOs;
using User.Shared.Common;
using User.Shared.DTO;

namespace User.Infrastructure.Http
{
    public class PlaylistGateway : IPlaylistGateway
    {
        private sealed record PlaylistListItemWire(int Id, string Name, string? Description, string? Genre);
        private sealed record PagedWire<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
        private readonly HttpClient _http;
        public PlaylistGateway(HttpClient http) => _http = http;

        public Task<ExternalPlaylistDetailsDto?> GetPlaylistAsync(int playlistId, CancellationToken ct = default)
            => _http.GetFromJsonAsync<ExternalPlaylistDetailsDto>($"/playlists/{playlistId}", ct);

        public Task<ExternalSongDto?> GetSongAsync(int songId, CancellationToken ct = default)
            => _http.GetFromJsonAsync<ExternalSongDto>($"/songs/{songId}", ct);


        public async Task<PagedResult<ExternalSongDto>> SearchSongsAsync(string? title, string? genre, int page, int pageSize, CancellationToken ct = default)
        {
            var t = Uri.EscapeDataString(title ?? "");
            var g = Uri.EscapeDataString(genre ?? "");
            var uri = $"/songs/search?title={t}&genre={g}&page={page}&pageSize={pageSize}";
            var res = await _http.GetFromJsonAsync<PagedResult<ExternalSongDto>>(uri, ct);
            return res ?? new PagedResult<ExternalSongDto>(Array.Empty<ExternalSongDto>(), 0, page, pageSize);
        }

        public async Task<PagedResult<SongWithLikesDto>> GetTopLikedAsync(int minLikes, int page, int pageSize, CancellationToken ct = default)
        {
            var uri = $"/songs/top-liked?minLikes={minLikes}&page={page}&pageSize={pageSize}";
            var res = await _http.GetFromJsonAsync<PagedResult<SongWithLikesDto>>(uri, ct);
            return res ?? new PagedResult<SongWithLikesDto>(Array.Empty<SongWithLikesDto>(), 0, page, pageSize);
        }

       

     
        public async Task<int> LikeSongAsync(int songId, int _ignored, CancellationToken ct = default)
        {
            var resp = await _http.PostAsync($"/songs/{songId}/likes", content: null, ct);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        }

        public async Task<int> UnlikeSongAsync(int songId, int _ignored, CancellationToken ct = default)
        {
            var resp = await _http.DeleteAsync($"/songs/{songId}/likes", ct);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        }

        public async Task<int> GetSongLikesAsync(int songId, CancellationToken ct = default)
        {
            var score = await _http.GetFromJsonAsync<int>($"/songs/{songId}/likes/score", ct);
            return score;
        }

        public async Task<Dictionary<int, string>> GetSongTitlesByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var arr = ids?.Distinct().ToArray() ?? Array.Empty<int>();
            if (arr.Length == 0) return new Dictionary<int, string>();

            var q = string.Join(",", arr);
            var list = await _http.GetFromJsonAsync<List<SlimSongDto>>($"/songs/by-ids?ids={q}", ct)
                       ?? new List<SlimSongDto>();

            return list.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Title);
        }

        public async Task<PagedResult<ExternalPlaylistListItemDto>> GetExternalPlaylistsAsync(
         string? genre, int page, int pageSize, CancellationToken ct = default)
        {
            var g = Uri.EscapeDataString(genre ?? "");
            var uri = $"/playlists?genre={g}&page={page}&pageSize={pageSize}";

            var raw = await _http.GetFromJsonAsync<PagedWire<PlaylistListItemWire>>(uri, ct);
            if (raw is null || raw.Items is null)
                return new PagedResult<ExternalPlaylistListItemDto>(Array.Empty<ExternalPlaylistListItemDto>(), 0, page, pageSize);

            var items = raw.Items
                .Select(it => new ExternalPlaylistListItemDto(it.Id, it.Name, it.Description, it.Genre))
                .ToList();

            return new PagedResult<ExternalPlaylistListItemDto>(items, raw.Total, raw.Page, raw.PageSize);
        }

        public async Task<Dictionary<int, ExternalSongMetaDto>> GetSongMetadataByIdsAsync(
          IEnumerable<int> ids, CancellationToken ct = default)
        {
            var arr = ids?.Distinct().ToArray() ?? Array.Empty<int>();
            if (arr.Length == 0) return new Dictionary<int, ExternalSongMetaDto>();

            var q = string.Join(",", arr);
            try
            {
              
                var list = await _http.GetFromJsonAsync<List<ExternalSongMetaDto>>($"/songs/meta/by-ids?ids={q}", ct);
                if (list is not null)
                    return list.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());
            }
            catch
            {
                
            }

            var result = new Dictionary<int, ExternalSongMetaDto>(arr.Length);
            foreach (var id in arr)
            {
                var s = await _http.GetFromJsonAsync<ExternalSongDto>($"/songs/{id}", ct);
                if (s is null) continue;

                var meta = new ExternalSongMetaDto(
                    Id: id,
                    Title: s.Title,
                    ArtistName: s.ArtistName ?? "Unknown Artist",
                    Album: null,
                   
                    Year: null,
                    Genre: s.Genre
                  
                );
                result[id] = meta;
            }
            return result;
        }
    }
}
