using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using System.IO;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ITechnicianFileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(ITechnicianFileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet("technician/{blobName}")]
        public async Task<IActionResult> GetTechnicianFile(string blobName, [FromQuery] bool download = false)
        {
            try
            {
                _logger.LogInformation("Requesting technician file: {BlobName}", blobName);

                // Get file properties first to check content type
                var properties = await _fileService.GetFilePropertiesAsync(blobName, "technician-documents");
                var fileStream = await _fileService.GetFileStreamAsync(blobName, "technician-documents");

                if (download)
                {
                    // Extract original filename from blob name if possible
                    var fileName = blobName; // You might want to store original names in DB
                    return File(fileStream, "application/octet-stream", fileName);
                }
                else
                {
                    // Return for display in browser
                    return File(fileStream, properties.ContentType);
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found: {BlobName}", blobName);
                return NotFound(new { error = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving file: {BlobName}", blobName);
                return StatusCode(500, new { error = "Error retrieving file" });
            }
        }

        [HttpGet("service-image/{blobName}")]
        public async Task<IActionResult> GetServiceImageUriAsync(string blobName)
        {
            _logger.LogInformation("Requesting technician file: {BlobName}", blobName);

            var uri = await _fileService.GetImageURI(blobName, "services-documents");

            return Ok(new { URL = uri });
        }
    }
}