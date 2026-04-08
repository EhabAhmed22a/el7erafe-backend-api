namespace Shared.DataTransferObject.TechnicianDTOs
{
    public class PreviousJobDTO
    {
        public bool isCancelled { get; set; }
        public string? clientName { get; set; }
        public string? clientImage { get; set; }
        public string? serviceType { get; set; }
        public decimal? fees { get; set; }
        public DateOnly? day { get; set; }
        public string? techTimeInterval { get; set; }
    }
}