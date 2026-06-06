using Common.Dto.Auth;
using Common.Service.Interface;
using Common.Settings;
using Database;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Common.Service.Dto
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwt;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _db;

        public AuthService(IOptions<JwtSettings> jwtSettings, UserManager<IdentityUser> userManager, AppDbContext db)
        {
            _jwt = jwtSettings.Value;
            _userManager = userManager;
            _db = db;
        }

        public async Task<string> GenerateTokenAsync(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.First();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(IdentityUser user)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                IsRevoked = false
            };

            _db.RefreshTokens.Add(new DbRefreshToken
            {
                Id = refreshToken.Id,
                Token = refreshToken.Token,
                Created = refreshToken.Created,
                Expires = refreshToken.Expires,
                UserId = user.Id,
                IsRevoked = false
            });
            await _db.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
        {
            var storedToken = await _db.RefreshTokens
                .Include(x => x.User)
                .Where(x => x.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (storedToken == null || storedToken.IsRevoked || storedToken.Expires < DateTime.UtcNow)
            {
                return null;
            }

            return await GenerateTokenAsync(storedToken.User);
        }

        public async Task RevokeRefreshTokensAsync(string refreshToken)
        {
            var tokens = await _db.RefreshTokens
                .Where(x => x.Token == refreshToken)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _db.SaveChangesAsync();
        }

        public Task<bool> ValidateAccessTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwt.Key);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = _jwt.Issuer,
                    ValidAudience = _jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ClockSkew = TimeSpan.Zero
                }, out _);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
