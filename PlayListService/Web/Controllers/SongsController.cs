using Application.Common;
using Application.DTOs;
using Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Playlist.Api.Web.Controllers
{
    [ApiController]
    [Route("songs")]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songSvc;
        private readonly ISongLikeService _songLikeSvc;

        public SongsController(ISongService songSvc,  ISongLikeService songLikeSvc)
        {
            _songSvc = songSvc;
            _songLikeSvc = songLikeSvc;

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SongDto), StatusCodes.Status200OK)]
        public Task<SongDto> Create([FromBody] SongCreateDto dto, CancellationToken ct)
            => _songSvc.CreateAsync(dto, ct);

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SongDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<SongDto> Update(int id, [FromBody] SongUpdateDto dto, CancellationToken ct)
            => _songSvc.UpdateAsync(id, dto, ct);

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SongDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var dto = await _songSvc.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<SongDto>), StatusCodes.Status200OK)]
        public Task<PagedResult<SongDto>> Search(
            [FromQuery] string? title,
            [FromQuery] string? genre,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
            => _songSvc.SearchAsync(title, genre, page, pageSize, ct);

       
        [HttpGet("by-ids")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SlimSongDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SlimSongDto>>> GetByIds([FromQuery] string? ids, CancellationToken ct)
        {
            var parsed = (ids ?? "")
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var x) ? x : 0)
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (parsed.Length == 0) return Ok(new List<SlimSongDto>());

            var res = await _songSvc.GetSlimByIdsAsync(parsed, ct);
            return Ok(res.ToList());
        }


        [HttpGet("top-liked")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<SongWithLikesDto>), StatusCodes.Status200OK)]
        public Task<PagedResult<SongWithLikesDto>> GetTopLiked(
        [FromQuery] int minLikes = 10,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _songLikeSvc.GetTopLikedGlobalAsync(minLikes, page, pageSize, ct); 




        [HttpGet("meta/by-ids")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SongMetaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SongMetaDto>>> GetMetaByIds([FromQuery] string? ids, CancellationToken ct)
        {
            var parsed = (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var x) ? x : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToArray();

            if (parsed.Length == 0) return Ok(new List<SongMetaDto>());

            var res = await _songSvc.GetMetadataByIdsAsync(parsed, ct);
            return Ok(res.ToList());
        }
    }
}
