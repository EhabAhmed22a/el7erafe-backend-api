
namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechRegisterToReturnDTO
    {
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string NationalIdFrontPath { get; set; } = default!;  
        public string NationalIdBackPath { get; set; } = default!;
        public string CriminalRecordPath { get; set; } = default!;
        public int ServiceType { get; set; }
    }
}
