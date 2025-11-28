using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Authentication;
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
                // === TOKEN VALIDATION ===
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
                { } when ex is ForgotPasswordDisallowed or ResetTokenExpiredException => StatusCodes.Status403Forbidden,
                UnverifiedClientLogin unverifiedClientLogin => GetEmail(unverifiedClientLogin, Response),
                PendingTechnicianRequest pendingTechnicianRequest => GetTempToken(pendingTechnicianRequest, Response),
                RejectedTechnician => 461,
                PasswordReuseException => StatusCodes.Status422UnprocessableEntity,
                OtpAlreadySent => StatusCodes.Status429TooManyRequests,
                TechnicalException => StatusCodes.Status500InternalServerError,
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

        private static int GetEmail(UnverifiedClientLogin unverifiedClientLogin, ErrorToReturn response)
        {
            response.email = unverifiedClientLogin._email;
            return 452;
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

        private static string? ExtractTokenFromHeader(HttpContext context)
        {
            if (context.Request.Headers.Authorization.FirstOrDefault() is not string authHeader)
                return null;

            return authHeader.Split(' ') switch
            {
                ["Bearer", var token] => token.Trim(),
                ["bearer", var token] => token.Trim(),
                _ => null
            };
        }
    }
}