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
        public int userId { get; set; } 
        public string userName { get; set; } = default!;
        public char type { get; set; } = default!; 

    }
}
