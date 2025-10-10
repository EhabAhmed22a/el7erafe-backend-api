using System.ComponentModel.DataAnnotations;
namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechRegisterDTO
    {
        public string Name { get; set; } = default!;
        [Phone]
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string NationalId { get; set; } = default!;
        public string NationalIdFrontURL { get; set; } = default!; 
        public string NationalIdBackURL { get; set; } = default!; 
        public string CriminalRecordURL { get; set; } = default!; 

    }
}
