// Web/Controllers/PlaylistsController.cs
using System.Security.Claims;
using Application.Common;
using Application.DTOs;
using Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Playlist.Api.Web.Controllers
{
    [ApiController]
    [Route("playlists")]
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistService _svc;
        public PlaylistsController(IPlaylistService svc) => _svc = svc;

        // ---------- Create ----------
        [HttpPost]
        [Authorize] // must be authenticated to create
        [ProducesResponseType(typeof(PlaylistDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] PlaylistCreateDto dto, CancellationToken ct)
        {
            if (!TryGetUserId(out var uid)) return Unauthorized();
            var result = await _svc.CreateAsync(dto, uid, ct);
            return Ok(result);
        }

        // ---------- List by genre (paged) ----------
        // GET /playlists?genre=rock&page=1&pageSize=20
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<PlaylistListItemDto>), StatusCodes.Status200OK)]
        public Task<PagedResult<PlaylistListItemDto>> Get(
            [FromQuery] string? genre,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
            => _svc.GetByGenreAsync(genre, page, pageSize, ct);

        // ---------- Details ----------
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PlaylistDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var dto = await _svc.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // ---------- Add one song (tail or at specific order) ----------
        // POST /playlists/{id}/songs
        // body: { "songId": 123, "order": 5 }  // order optional
        [HttpPost("{id:int}/songs")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddSong([FromRoute] int id, [FromBody] PlaylistAddSongDto dto, CancellationToken ct)
        {
            if (dto is null) return BadRequest();
            if (dto.Order is int order)
            {
                if (!TryGetUserId(out var uid)) return Unauthorized();
                await _svc.AddSongAtAsync(id, dto.SongId, order, uid, ct);
            }
            else
            {
                await _svc.AddSongAsync(id, dto.SongId, ct);
            }
            return NoContent();
        }

        // ---------- Bulk add ----------
        // POST /playlists/{id}/songs/bulk
        // body: { "songIds": [1,2,3] }
        [HttpPost("{id:int}/songs/bulk")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddSongs([FromRoute] int id, [FromBody] PlaylistAddSongsDto dto, CancellationToken ct)
        {
            if (!TryGetUserId(out var uid)) return Unauthorized();
            await _svc.AddSongsAsync(id, dto?.SongIds ?? Array.Empty<int>(), uid, ct);
            return NoContent();
        }

        // ---------- Remove song ----------
        // DELETE /playlists/{id}/songs/{songId}
        [HttpDelete("{id:int}/songs/{songId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveSong([FromRoute] int id, [FromRoute] int songId, CancellationToken ct)
        {
            await _svc.RemoveSongAsync(id, songId, ct);
            return NoContent();
        }

        // ---------- Reorder ----------
        // PATCH /playlists/{id}/songs/reorder
        // body: { "songId": 123, "newOrder": 2 }
        [HttpPatch("{id:int}/songs/reorder")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Reorder([FromRoute] int id, [FromBody] PlaylistReorderDto dto, CancellationToken ct)
        {
            await _svc.ReorderAsync(id, dto.SongId, dto.NewOrder, ct);
            return NoContent();
        }


        // -------- helper: extract user id from JWT (sub) --------
        private bool TryGetUserId(out int userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (claim is not null && int.TryParse(claim.Value, out userId)) return true;
            userId = default;
            return false;
        }
    }
}
