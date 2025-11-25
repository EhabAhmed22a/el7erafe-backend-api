using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule.Enums;
using ServiceAbstraction;
using Shared.ErrorModels;
using System.Text.Json;

namespace el7erafe.Web.CustomMiddleWares
{
    public class CustomExceptionHandlerMiddleWare(RequestDelegate _next,
                   ILogger<CustomExceptionHandlerMiddleWare> _logger)
    {

        public async Task InvokeAsync(HttpContext httpContext, IUserTokenRepository tokenRepository)
        {
            try
            {
                // Skip token validation for public endpoints
                if (!IsPublicEndpoint(httpContext.Request.Path))
                {
                    var token = ExtractTokenFromHeader(httpContext);
                    if (!string.IsNullOrEmpty(token))
                    {
                        // Check if token exists in database
                        var tokenExists = await tokenRepository.TokenExistsAsync(token);
                        if (!tokenExists)
                        {
                            _logger.LogWarning("[AUTH] Token not found in database: {Token}", token);
                            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await httpContext.Response.WriteAsJsonAsync(new
                            {
                                success = false,
                                message = "انتهت صلاحية الجلسة، يرجى تسجيل الدخول مرة أخرى"
                            });
                            return;
                        }
                    }
                    else
                    {
                        // No token provided for protected endpoint
                        _logger.LogWarning("[AUTH] No token provided for protected endpoint: {Endpoint}", httpContext.Request.Path);
                        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await httpContext.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "يرجى تسجيل الدخول للوصول إلى هذه الصفحة"
                        });
                        return;
                    }
                }

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