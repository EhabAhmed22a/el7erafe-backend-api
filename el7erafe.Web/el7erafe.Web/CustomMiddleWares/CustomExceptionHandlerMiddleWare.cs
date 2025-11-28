using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Authentication;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
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
            ErrorToReturn Response = new ErrorToReturn();

                httpContext.Response.StatusCode = ex switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    UnauthorizedException => StatusCodes.Status401Unauthorized,
                    BadRequestException badRequestException => GetBadRequestErrors(badRequestException, Response),
                    { } when ex is AlreadyExistException or EmailAlreadyVerified or  TechnicianAcceptedOrPendingException=> StatusCodes.Status409Conflict,
                    InvalidOtpException => StatusCodes.Status400BadRequest,
                    { } when ex is ForgotPasswordDisallowed or ResetTokenExpiredException => StatusCodes.Status403Forbidden,
                    UnverifiedClientLogin unverifiedClientLogin => GetEmail(unverifiedClientLogin, Response),
                    RejectedTechnician rejectedTechnician => CreateRejectionResponse(rejectedTechnician, Response),
                    PendingTechnicianRequest pendingTechnicianRequest => GetTempToken(pendingTechnicianRequest, Response),
                    OtpAlreadySent => StatusCodes.Status429TooManyRequests,
                    BlockedTechnician => 462,
                    TechnicalException => StatusCodes.Status500InternalServerError,
                    _ => StatusCodes.Status500InternalServerError
                };

                Response.StatusCode = httpContext.Response.StatusCode;
                Response.ErrorMessage = ex.Message;

            await httpContext.Response.WriteAsJsonAsync(Response);
        }

        private static int GetTempToken(PendingTechnicianRequest pendingTechnicianRequest, ErrorToReturn response)
        {
            response.tempToken = pendingTechnicianRequest._tempToken;
            return 460;
        }

        private static int CreateRejectionResponse(RejectedTechnician rejectedTechnician, ErrorToReturn response)
        {
            response.ErrorMessage = rejectedTechnician.Message;
            response.RejectionReason = rejectedTechnician.RejectionReason;
            response.Data = new RejectedTechnicanDTO()
            {
                Name = rejectedTechnician.TechnicianName,
                Phone = rejectedTechnician.UserName,
                Governorate = rejectedTechnician.GovernorateName,
                City = rejectedTechnician.CityName,
                ServiceType = rejectedTechnician.ServiceName,
                FrontId = rejectedTechnician.IsNationalIdFrontVerified,
                BackId = rejectedTechnician.IsNationalIdBackVerified,
                CriminalRecord = rejectedTechnician.IsCriminalHistoryVerified
            };
            return 461;
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