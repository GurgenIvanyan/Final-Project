using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.Application.Abstractions.Http;
using User.Application.Abstractions.Security; 
using User.Application.DTOs;
using User.Application.Services.IServices;
using User.Shared.Common;
using User.Shared.DTO;

namespace User.Web.Controllers
{
    [ApiController]
    [Route("user-playlists")]
    public class UserPlaylistsController : ControllerBase
    {
        private readonly IUserPlaylistService _svc;
        private readonly IPlaylistGateway _gateway;
        private readonly ICurrentUserService _current;

        public UserPlaylistsController(
            IUserPlaylistService svc,
            IPlaylistGateway gateway,
            ICurrentUserService current)
        {
            _svc = svc;
            _gateway = gateway;
            _current = current;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<UserPlaylistDto>> Create([FromBody] UserPlaylistCreateDto dto, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            var res = await _svc.CreateAsync(uid, dto, ct);
            return Ok(res);
        }

        [HttpPost("import")]
        [Authorize]
        public async Task<ActionResult<UserPlaylistDto>> Import([FromBody] ImportPlaylistDto dto, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            var res = await _svc.ImportAsync(uid, dto, ct);
            return Ok(res);
        }

        [HttpPatch("{id:int}/public")]
        [Authorize]
        public async Task<IActionResult> SetPublic([FromRoute] int id, [FromBody] SetPublicDto dto, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            await _svc.SetPublicAsync(uid, id, dto.IsPublic, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/songs")]
        [Authorize]
        public async Task<IActionResult> AddSong([FromRoute] int id, [FromBody] AddSongDto dto, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            await _svc.AddSongAsync(uid, id, dto.SongId, dto.Order, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/songs/bulk")]
        [Authorize]
        public async Task<IActionResult> AddSongs([FromRoute] int id, [FromBody] AddSongsDto dto, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            await _svc.AddSongsAsync(uid, id, dto.SongIds ?? Array.Empty<int>(), ct);
            return NoContent();
        }

        [HttpDelete("{id:int}/songs/{songId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoveSong([FromRoute] int id, [FromRoute] int songId, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            await _svc.RemoveSongAsync(uid, id, songId, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/songs/reorder")]
        [Authorize]
        public async Task<IActionResult> Reorder([FromRoute] int id, [FromBody] AddSongDto dto, CancellationToken ct)
        {
            if (dto.Order is null) return BadRequest("Order is required");
            var uid = _current.UserIdOrThrow();
            await _svc.ReorderAsync(uid, id, dto.SongId, dto.Order.Value, ct);
            return NoContent();
        }

       
        [HttpGet("mine")]
        [Authorize]
        public async Task<ActionResult<PagedResult<UserPlaylistDto>>> GetMine(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var uid = _current.UserIdOrThrow();
            var res = await _svc.GetMineAsync(uid, page, pageSize, ct);
            return Ok(res);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<UserPlaylistDetailsDto>> GetDetails(int id, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            var res = await _svc.GetDetailsAsync(uid, id, ct);
            return res is null ? NotFound() : Ok(res);
        }

        [HttpGet("top-liked")]
        [Authorize] 
        public Task<PagedResult<SongWithLikesDto>> GetTopLiked(
        [FromQuery] int minLikes = 10,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _gateway.GetTopLikedAsync(minLikes, page, pageSize, ct);

        // Поиск песен — абсолютный маршрут (как в Playlist.Api)
        [HttpGet("~/songs/search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<ExternalSongDto>), StatusCodes.Status200OK)]
        public Task<PagedResult<ExternalSongDto>> SearchSongs(
            [FromQuery] string? title,
            [FromQuery] string? genre,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;
            return _gateway.SearchSongsAsync(title, genre, page, pageSize, ct);
        }


       
        [HttpPost("~/songs/{songId:int}/like")]
        [Authorize]
        public async Task<ActionResult<int>> LikeSong(int songId, CancellationToken ct)
        {
            var score = await _gateway.LikeSongAsync(songId, 0, ct);
            return Ok(score);
        }

        [HttpDelete("~/songs/{songId:int}/like")]
        [Authorize]
        public async Task<ActionResult<int>> UnlikeSong(int songId, CancellationToken ct)
        {
       
            var score = await _gateway.UnlikeSongAsync(songId, 0, ct);
            return Ok(score);
        }

        
        [HttpGet("~/songs/{songId:int}/likes")]
        [AllowAnonymous]
        public Task<int> GetSongLikes(int songId, CancellationToken ct)
            => _gateway.GetSongLikesAsync(songId, ct);

        [HttpGet("external")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<ExternalPlaylistListItemDto>), StatusCodes.Status200OK)]
        public Task<PagedResult<ExternalPlaylistListItemDto>> GetExternalPlaylists(
    [FromQuery] string? genre,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
    => _gateway.GetExternalPlaylistsAsync(genre, page, pageSize, ct);

        [HttpGet("public")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserPlaylistDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<UserPlaylistDto>>> GetPublicByOthers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
        {
            var uid = _current.UserIdOrThrow(); 
            var res = await _svc.GetPublicByOthersAsync(uid, page, pageSize, ct);
            return Ok(res);
        }

        [HttpGet("public-with-songs")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<PublicPlaylistWithSongsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<PublicPlaylistWithSongsDto>>> GetPublicByOthersWithSongs(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
        {
            var uid = _current.UserIdOrThrow();
            var res = await _svc.GetPublicByOthersWithSongsAsync(uid, page, pageSize, ct);
            return Ok(res);
        }

        [HttpGet("public-with-songs-rich")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<PublicPlaylistWithSongsRichDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<PublicPlaylistWithSongsRichDto>>> GetPublicByOthersWithSongsRich(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
        {
            var uid = _current.UserIdOrThrow();
            var res = await _svc.GetPublicByOthersWithSongsRichAsync(uid, page, pageSize, ct);
            return Ok(res);
        }

        [HttpGet("~/songs/{songId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExternalSongDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSongById(int songId, CancellationToken ct)
        {
            
            var dto = await _gateway.GetSongAsync(songId, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var uid = _current.UserIdOrThrow();
            await _svc.DeleteAsync(uid, id, ct);
            return NoContent();
        }
        [HttpGet("external/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExternalPlaylistDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExternalPlaylistDetails(int id, CancellationToken ct = default)
        {
            var dto = await _gateway.GetPlaylistAsync(id, ct);
            if (dto is null)
                return NotFound();

            return Ok(dto);
        }




    }

}
