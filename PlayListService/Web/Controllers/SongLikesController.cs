using System.Security.Claims;
using Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Playlist.Api.Web.Controllers
{
    [ApiController]
    [Route("songs")]
    public class SongLikesController : ControllerBase
    {
        private readonly ISongLikeService _svc;
        public SongLikesController(ISongLikeService svc) => _svc = svc;

        private int UserIdOrThrow()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (sub == null || !int.TryParse(sub.Value, out var id))
                throw new UnauthorizedAccessException("UserId missing in token");
            return id;
        }

        
        [HttpPost("{songId:int}/likes")]
        [Authorize]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Like(int songId, CancellationToken ct)
        {
            var uid = UserIdOrThrow();
            var score = await _svc.LikeAsync(songId, uid, +1, ct);
            return Ok(score);
        }

        [HttpDelete("{songId:int}/likes")]
        [Authorize]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Unlike(int songId, CancellationToken ct)
        {
            var uid = UserIdOrThrow();
            await _svc.RemoveLikeAsync(songId, uid, ct);       
            var score = await _svc.GetScoreAsync(songId, ct);  
            return Ok(score);
        }

        [HttpGet("{songId:int}/likes/score")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public Task<int> GetScore(int songId, CancellationToken ct)
    => _svc.GetScoreAsync(songId, ct);
    }
}
