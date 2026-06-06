using Common.Dto.Auth;
using Microsoft.AspNetCore.Identity;

namespace Common.Service.Interface
{
    public interface IAuthService
    {
        public Task<string> GenerateTokenAsync(IdentityUser user);
        public Task<bool> ValidateAccessTokenAsync(string token);

        public Task<RefreshToken> GenerateRefreshTokenAsync(IdentityUser user);
        public Task<string?> RefreshAccessTokenAsync(string refreshToken);
        public Task RevokeRefreshTokensAsync(string refreshToken);
    }
}
