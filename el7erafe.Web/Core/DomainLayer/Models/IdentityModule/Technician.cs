using DomainLayer.Models.IdentityModule.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models.IdentityModule
{
    public class Technician
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string NationalId { get; set; } = default!;
        public string NationalIdFrontURL { get; set; } = default!;
        public string NationalIdBackURL { get; set; } = default!;
        public string CriminalHistoryURL { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
        public string UserId { get; set; } = default!; //FK 
        public TechnicianStatus Status { get; set; } 
    }
}
