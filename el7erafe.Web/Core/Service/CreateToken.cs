using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service
{
    public class CreateToken(UserManager<ApplicationUser> _userManager, IConfiguration _configuration)
    {
        // for tempToken assign to true
        // for token assign tempToken to false
        public async Task<string> CreateTokenAsync(ApplicationUser user, bool tempToken)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.NameIdentifier , user.Id),
                new(ClaimTypes.MobilePhone , user.PhoneNumber!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID
                new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new("salt", Guid.NewGuid().ToString())
            };

            var Roles = await _userManager.GetRolesAsync(user);
            foreach (var role in Roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var SecretKey = _configuration.GetSection("JWTOptions")["SecretKey"];
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var Creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            var expiration = tempToken ?
            DateTime.UtcNow.AddDays(1) :    // Temp tokens: 1 day
            DateTime.UtcNow.AddDays(7);     // Regular tokens: 7 day

            var Token = new JwtSecurityToken(
                issuer: _configuration.GetSection("JWTOptions")["Issuer"],
                audience: _configuration.GetSection("JWTOptions")["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: Creds
            );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
