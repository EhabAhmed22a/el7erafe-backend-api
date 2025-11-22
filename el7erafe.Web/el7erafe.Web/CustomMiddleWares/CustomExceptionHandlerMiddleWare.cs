using DomainLayer.Exceptions;
using Shared.ErrorModels;
using System.Text.Json;
using ServiceAbstraction;

namespace el7erafe.Web.CustomMiddleWares
{
    public class CustomExceptionHandlerMiddleWare(RequestDelegate _next,
                   ILogger<CustomExceptionHandlerMiddleWare> _logger)
    {

        public async Task InvokeAsync(HttpContext httpContext, ITokenBlocklistService tokenBlocklistService)
        {
            try
            {
                // === NEW AUTHENTICATION MIDDLEWARE LOGIC ===
                // Skip token validation for public endpoints
                if (!IsPublicEndpoint(httpContext.Request.Path))
                {
                    // Check if authorization header exists
                    var token = ExtractTokenFromHeader(httpContext);
                    if (!string.IsNullOrEmpty(token))
                    {
                        // Check if token is revoked
                        var isRevoked = await tokenBlocklistService.IsTokenRevokedAsync(token);
                        if (isRevoked)
                        {
                            _logger.LogWarning("[AUTH] Attempt to use revoked token for endpoint: {Endpoint}", httpContext.Request.Path);
                            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await httpContext.Response.WriteAsJsonAsync(new ErrorToReturn
                            {
                                StatusCode = StatusCodes.Status401Unauthorized,
                                ErrorMessage = "Token has been revoked. Please login again."
                            });
                            return; // Stop further processing
                        }

                        _logger.LogDebug("[AUTH] Token validation passed for endpoint: {Endpoint}", httpContext.Request.Path);
                    }
                    else
                    {
                        _logger.LogDebug("[AUTH] No token found for endpoint: {Endpoint}", httpContext.Request.Path);
                    }
                }
                // === END AUTHENTICATION MIDDLEWARE LOGIC ===

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
                UnverifiedClientLogin => 452,
                PendingTechnicianRequest => 460,
                RejectedTechnician => 461,
                OtpAlreadySent => StatusCodes.Status429TooManyRequests,
                _ => StatusCodes.Status500InternalServerError
            };

            Response.StatusCode = httpContext.Response.StatusCode;
            Response.ErrorMessage = ex.Message;

            //Return Object As Json 
            await httpContext.Response.WriteAsJsonAsync(Response);
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

        // === NEW HELPER METHODS FOR AUTHENTICATION ===

        private static string ExtractTokenFromHeader(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader.Substring("Bearer ".Length).Trim();
        }

        private static bool IsPublicEndpoint(PathString path)
        {
            var publicPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register/client",
                "/api/auth/register/technician",
                "/api/public/services",
                "/api/auth/verify-otp",
                "/api/auth/resend-otp",
                "/swagger"
            };

            return publicPaths.Any(publicPath => path.StartsWithSegments(publicPath));
        }
    }
}