using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ServiceAbstraction;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service
{
    public class CreateTokenService(UserManager<ApplicationUser> _userManager,
                IConfiguration _configuration,
                IUserTokenRepository _tokenRepository,
                ILogger<CreateTokenService> _logger) : ICreateTokenService
    {
        public async Task<string> CreateAndStoreTokenAsync(ApplicationUser user, TokenType tokenType)
        {
            try
            {
                _logger.LogInformation("[TOKEN] Creating {TokenType} token for user: {UserId}", tokenType, user.Id);

                var token = await GenerateJwtTokenAsync(user, tokenType);

                // Store in database
                var userToken = new UserToken
                {
                    Token = token,
                    Type = tokenType,
                    UserId = user.Id,
                    User = user
                };

                await _tokenRepository.CreateUserTokenAsync(userToken);

                _logger.LogInformation("[TOKEN] {TokenType} token created and stored for user: {UserId}", tokenType, user.Id);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TOKEN] Error creating token for user: {UserId}", user.Id);
                throw;
            }
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user, TokenType tokenType)
        {
            var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.MobilePhone, user.PhoneNumber!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("token_type", ((int)tokenType).ToString()),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var secretKey = _configuration.GetSection("JWTOptions")["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("JWTOptions")["Issuer"],
                audience: _configuration.GetSection("JWTOptions")["Audience"],
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
