using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObject
{
    public class UserDTO
    {
        public string token { get; set; } = default!;
        public int userId { get; set; } = default!;
        public string userName { get; set; } = default!;
        public string type { get; set; } = default!;
    }
}
