
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Shared.ErrorModels
{
    public class ErrorToReturn
    {
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; } = default!;
        public string? tempToken { get; set; }
        public string? RejectionReason { get; set; }
        public RejectedTechnicanDTO? Data { get; set; }
    }
}
