using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechLoginDTO
    {
        [Phone]
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!; 
    }
}
