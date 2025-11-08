using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Shared.Common
{
    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
}
