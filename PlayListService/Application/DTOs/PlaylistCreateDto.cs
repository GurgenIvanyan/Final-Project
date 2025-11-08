using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record PlaylistCreateDto(string Name, string? Description, string? Genre);
}
