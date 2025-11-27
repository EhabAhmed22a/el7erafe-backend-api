using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Models.IdentityModule
{
    public class Technician
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string NationalIdFrontURL { get; set; } = default!;
        public bool  IsNationalIdFrontVerified { get; set; } = false;
        public string NationalIdBackURL { get; set; } = default!;
        public bool IsNationalIdBackVerified { get; set; } = false;
        public string CriminalHistoryURL { get; set; } = default!;
        public bool IsCriminalHistoryVerified { get; set; } = false;
        public int Rejection_Count { get; set; } = 0;
        public ApplicationUser User { get; set; } = default!;
        public string UserId { get; set; } = default!; //FK 
        public TechnicianStatus Status { get; set; }
        public City City { get; set; } = default!;
        public int CityId { get; set; }            
        public TechnicianService Service { get; set; } = default!;
        public int ServiceId { get; set; }
        
    }
}
