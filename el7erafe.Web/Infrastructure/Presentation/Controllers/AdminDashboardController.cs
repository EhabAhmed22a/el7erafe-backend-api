
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.AdminDTOs.Dashboard;

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

        /// <summary>
        /// Creates a new service in the system.
        /// </summary>
        /// <remarks>
        /// This endpoint allows administrators to register new services in the platform.
        /// Validates service uniqueness and required fields before creation.
        /// </remarks>
        /// <param name="serviceRegisterDTO">Service registration data containing service details</param>
        /// <returns>Returns the created service object with its ID</returns>
        /// <response code="201">Returns when service is successfully created with the service object</response>
        /// <response code="400">Returns when required fields are missing or invalid</response>
        /// <response code="403">Returns when user is not authorized as an admin</response>
        /// <response code="409">Returns when service name already exists in the system</response>
        [HttpPost("admin/services")]
        public async Task<ActionResult> CreateServiceAsync(ServiceRegisterDTO serviceRegisterDTO)
        {
            logger.LogInformation("[API] CreateService endpoint called");

            logger.LogInformation("[API] Service creation request - Name: {ServiceName}",
                serviceRegisterDTO.Name);

            logger.LogInformation("[API] Calling adminDashboardService.CreateServiceAsync");

            return StatusCode(201,await adminDashboardService.CreateServiceAsync(serviceRegisterDTO));
        }

        /// <summary>
        /// Deletes (deactivates) a service from the system.
        /// </summary>
        /// <remarks>
        /// This endpoint allows administrators to permanently remove a service from the platform.
        /// The operation performs a physical deletion of the service record from the database.
        /// Use with caution as this action cannot be undone.
        /// </remarks>
        /// <param name="id">The unique identifier of the service to be deleted</param>
        /// <returns>Returns a confirmation of successful deletion</returns>
        /// <response code="200">Service deleted successfully</response>
        /// <response code="403">User is not authorized as an administrator</response>
        /// <response code="404">Service with the specified ID does not exist</response>
        [HttpDelete("admin/services")]
        public async Task<ActionResult> DeleteServiceAsync([FromQuery] int id)
        {
            logger.LogInformation("DELETE request received for service ID: {id}", id);

            await adminDashboardService.DeleteServiceAsync(id);

            logger.LogInformation("DELETE request completed successfully for service ID: {id}", id);
            return Ok();
        }

        /// <summary>
        /// Deletes a client from the system by user ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows administrators to permanently delete client accounts.
        /// Requires admin authorization to access.
        /// </remarks>
        /// <param name="id">The unique identifier of the user to delete</param>
        /// <returns>Returns success message upon successful deletion</returns>
        /// <response code="200">Returns when client is successfully deleted</response>
        /// <response code="403">Returns when unauthorized non-admin user attempts deletion</response>
        /// <response code="404">Returns when the specified user ID does not exist</response>
        [HttpDelete("admin/clients")]
        public async Task<ActionResult> DeleteClientAsync([FromQuery] string id)
        {
            logger.LogInformation("[API] DeleteClient endpoint called for UserId: {UserId}", id);

            logger.LogInformation("[API] Calling adminDashboardService.DeleteClientAsync for UserId: {UserId}", id);

            await adminDashboardService.DeleteClientAsync(id);

            logger.LogInformation("[API] Client successfully deleted. UserId: {UserId}", id);

            return Ok(new { message = "تم مسح العميل بنجاح" });
        }
    }
}
