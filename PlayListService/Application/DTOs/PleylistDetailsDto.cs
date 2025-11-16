using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    //public record PlaylistDetailsDto(int Id, string Name, string? Description, string? Genre, List<SongDto> Songs, Dictionary<int, int> Votes
    public record PlaylistDetailsDto(
    int Id,
    string Name,
    string? Description,
    string? Genre,
    List<SongDto> Songs
    
);

}
