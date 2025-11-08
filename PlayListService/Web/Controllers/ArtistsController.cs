// Web/Controllers/ArtistsController.cs
using Application.DTOs;
using Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Playlist.Api.Web.Controllers;

[ApiController]
[Route("artists")]
public class ArtistsController : ControllerBase
{
    private readonly IArtistService _svc;
    public ArtistsController(IArtistService svc) => _svc = svc;

    // Базовый список без песен (оставил как было)
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ArtistDto>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<ArtistDto>> GetAll(CancellationToken ct) => _svc.GetAllAsync(ct);

    // Новый: список артистов С песнями (под твой формат)
    // GET /artists/with-songs
    [HttpGet("with-songs")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ArtistWithSongsListItemDto>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<ArtistWithSongsListItemDto>> GetAllWithSongs(CancellationToken ct)
        => _svc.GetAllWithSongsAsync(ct);

    // Новый: детали артиста (с песнями)
    // GET /artists/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ArtistDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dto = await _svc.GetAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ArtistDto), StatusCodes.Status200OK)]
    public Task<ArtistDto> Create([FromBody] ArtistCreateDto dto, CancellationToken ct)
        => _svc.CreateAsync(dto, ct);

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ArtistDto), StatusCodes.Status200OK)]
    public Task<ArtistDto> Update(int id, [FromBody] ArtistUpdateDto dto, CancellationToken ct)
        => _svc.UpdateAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _svc.DeleteAsync(id, ct);
        return NoContent();
    }

    // Новый: удалить песню у артиста
    // DELETE /artists/{artistId}/songs/{songId}
    [HttpDelete("{artistId:int}/songs/{songId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSongFromArtist(int artistId, int songId, CancellationToken ct)
    {
        await _svc.DeleteSongFromArtistAsync(artistId, songId, ct);
        return NoContent();
    }


    [HttpPost("{artistId:int}/songs")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSongToArtist(
            [FromRoute] int artistId,
            [FromBody] ArtistAddSongRequestDto body,
            CancellationToken ct)
    {
        await _svc.AddSongAsync(artistId, body.SongId, ct);
        return NoContent();
    }
}
