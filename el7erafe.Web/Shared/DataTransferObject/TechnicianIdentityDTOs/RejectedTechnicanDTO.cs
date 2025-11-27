namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class RejectedTechnicanDTO
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Governorate { get; set; }
        public string? City { get; set; }
        public string? ServiceType { get; set; }
        public bool? FrontId { get; set; }
        public bool? BackId { get; set; }
        public bool? CriminalRecord { get; set; }
    }
}
