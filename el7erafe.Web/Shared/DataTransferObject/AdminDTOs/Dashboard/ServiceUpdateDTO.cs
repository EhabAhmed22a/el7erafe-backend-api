
using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class ServiceUpdateDTO
    {
        public string? service_name { get; set; }
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? service_image { get; set; }
    }
}
