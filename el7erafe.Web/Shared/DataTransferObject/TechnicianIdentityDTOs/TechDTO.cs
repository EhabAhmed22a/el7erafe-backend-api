using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechDTO
    {
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string Status { get; set; } = default!;

    }
}
