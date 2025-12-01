
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class AdminDashboardController(IAdminDashboardService adminDashboardService, ILogger<AdminDashboardController> logger) : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of clients with optional pagination support.
        /// </summary>
        /// <remarks>
        /// This method provides flexible client retrieval:
        /// - When both pageNumber and pageSize are provided: returns paginated results
        /// - When either parameter is null: returns all clients without pagination
        /// - Invalid values are corrected: pageNumber &lt; 1 becomes 1, pageSize &lt; 1 becomes 10
        /// - The count in response represents the number of clients in the current page/set
        /// </remarks>
        /// <param name="pageNumber">The page number for pagination (optional). If null, all clients are returned. If &lt; 1, set to 1.</param>
        /// <param name="pageSize">The number of items per page (optional). If null, all clients are returned. If &lt; 1, set to 10.</param>
        /// <returns>Returns a ClientListDTO containing count of returned clients and the client data</returns>
        /// <response code="200">Returns when clients are successfully retrieved with count and data</response>
        /// <response code="403">Returns when unauthorized calls occur</response>
        /// <response code="500">Returns when internal server error occurs during client retrieval</response>
        [HttpGet("clients")]
        public async Task<ActionResult> GetClientsAsync([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            logger.LogInformation("[API] GetClients endpoint called. PageNumber: {PageNumber}, PageSize: {PageSize}",
        pageNumber, pageSize);

            logger.LogInformation("[API] Calling adminDashboardService.GetClientsAsync with PageNumber: {PageNumber}, PageSize: {PageSize}",
                pageNumber, pageSize);

            return Ok(await adminDashboardService.GetClientsAsync(pageNumber, pageSize));
        }

        /// <summary>
        /// Retrieves a list of services with optional pagination support.
        /// </summary>
        /// <remarks>
        /// This method provides flexible services retrieval:
        /// - When both pageNumber and pageSize are provided: returns paginated results
        /// - When either parameter is null: returns all services without pagination
        /// - Invalid values are corrected: pageNumber &lt; 1 becomes 1, pageSize &lt; 1 becomes 10
        /// - The count in response represents the number of services in the current page/set
        /// </remarks>
        /// <param name="pageNumber">The page number for pagination (optional). If null, all services are returned. If &lt; 1, set to 1.</param>
        /// <param name="pageSize">The number of items per page (optional). If null, all services are returned. If &lt; 1, set to 10.</param>
        /// <returns>Returns a ServiceListDTO containing count of returned services and the service data</returns>
        /// <response code="200">Returns when services are successfully retrieved with count and data</response>
        /// <response code="403">Returns when unauthorized calls occur</response>
        /// <response code="500">Returns when internal server error occurs during services retrieval</response>
        [HttpGet("services")]
        public async Task<ActionResult> GetServicesAsync([FromQuery] int? pageNumber, int? pageSize)
        {
            logger.LogInformation("[API] GetServices endpoint called. PageNumber: {PageNumber}, PageSize: {PageSize}",
        pageNumber, pageSize);

            logger.LogInformation("[API] Calling adminDashboardService.GetServicesAsync with PageNumber: {PageNumber}, PageSize: {PageSize}",
                pageNumber, pageSize);

            return Ok(await adminDashboardService.GetServicesAsync(pageNumber, pageSize));
        }
    }
}
