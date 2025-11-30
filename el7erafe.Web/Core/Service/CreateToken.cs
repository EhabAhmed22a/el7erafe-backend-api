using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service
{
    public class CreateToken
    {
        private readonly string secretKey;
        private readonly string issuer;
        private readonly string audience;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;

        public CreateToken(UserManager<ApplicationUser> _userManager, IConfiguration _configuration, IWebHostEnvironment _env)
        {
            userManager = _userManager;
            configuration = _configuration;
            env = _env;
            if (env.IsDevelopment())
            {
                secretKey = configuration.GetSection("JWTOptions")["SecretKey"];
                issuer = configuration.GetSection("JWTOptions")["Issuer"];
                audience = configuration.GetSection("JWTOptions")["Audience"];
            }
            else
            {
                secretKey = configuration.GetSection("JWTOptions")["SecretKey"];
                issuer = configuration.GetSection("JWTOptions")["Issuer"];
                audience = configuration.GetSection("JWTOptions")["Audience"];
            }
        }
        public async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.MobilePhone, user.PhoneNumber!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

            var Roles = await userManager.GetRolesAsync(user);
            foreach (var role in Roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var Creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            var Token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: Creds
            );

            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
