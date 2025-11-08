using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ArtistDetailsDto(
    int Id,
    string Name,
    string? Country,
    IReadOnlyList<SongRefDto> Songs
);
}
