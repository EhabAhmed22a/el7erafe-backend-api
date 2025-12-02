
namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class TechnicianDTO
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string phone { get; set; } = default!;
        public string governorate { get; set; } = default!;
        public string city { get; set; } = default!;
        public string faceIdImage { get; set; } = default!;
        public string backIdImage { get; set; } = default!;
        public string criminalRecordImage { get; set; } = default!;
        public string serviceType { get; set; } = default!;
    }
}
