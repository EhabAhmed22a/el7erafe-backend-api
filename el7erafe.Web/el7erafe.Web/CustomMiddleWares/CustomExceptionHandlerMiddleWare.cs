using DomainLayer.Exceptions;
using Shared.ErrorModels;
using System.Text.Json;

namespace el7erafe.Web.CustomMiddleWares
{
    public class CustomExceptionHandlerMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleWare> _logger;

        public CustomExceptionHandlerMiddleWare(RequestDelegate Next, ILogger<CustomExceptionHandlerMiddleWare> logger)
        {
            _next = Next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next.Invoke(httpContext);
                await HandleNotFoundEndPointAsync(httpContext);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something Went Wrong");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            var Response = new ErrorToReturn();

            //Set Status Code For Response
            httpContext.Response.StatusCode = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                BadRequestException badRequestException => GetBadRequestErrors(badRequestException, Response),
                { } when ex is AlreadyExistException or EmailAlreadyVerified => StatusCodes.Status409Conflict,
                InvalidOtpException => StatusCodes.Status400BadRequest,
                ForgotPasswordDisallowed => StatusCodes.Status403Forbidden,
                UnverifiedClientLogin => 452,
                PendingTechnicianRequest pendingTechnicianRequest => GetTempToken(pendingTechnicianRequest, Response),
                RejectedTechnician => 461,
                OtpAlreadySent => StatusCodes.Status429TooManyRequests,
                _ => StatusCodes.Status500InternalServerError
            };

            Response.StatusCode = httpContext.Response.StatusCode;
            Response.ErrorMessage = ex.Message;

            //Return Object As Json 
            await httpContext.Response.WriteAsJsonAsync(Response);
        }

        private static int GetTempToken(PendingTechnicianRequest pendingTechnicianRequest, ErrorToReturn response)
        {
            response.tempToken = pendingTechnicianRequest._tempToken;
            return 460;
        }

        private static int GetBadRequestErrors(BadRequestException badRequestException, ErrorToReturn response)
        {
            return StatusCodes.Status400BadRequest;
        }

        private static async Task HandleNotFoundEndPointAsync(HttpContext httpContext)
        {
            if (httpContext.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                var Response = new ErrorToReturn()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ErrorMessage = $"End Point {httpContext.Request.Path} is Not Found"
                };

                await httpContext.Response.WriteAsJsonAsync(Response);
            }
        }
    }
}
