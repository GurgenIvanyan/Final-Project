// Application/Services/IServices/ISongService.cs
using Application.Common;
using Application.DTOs;

namespace Application.Services.IServices;

public interface ISongService
{
    Task<SongDto> CreateAsync(SongCreateDto dto, CancellationToken ct = default);
    Task<SongDto> UpdateAsync(int id, SongUpdateDto dto, CancellationToken ct = default);
    Task<SongDetailsDto?> GetAsync(int id, CancellationToken ct = default);
    Task<PagedResult<SongDto>> SearchAsync(string? title, string? genre, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<SlimSongDto>> GetSlimByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<SongMetaDto>> GetMetadataByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}