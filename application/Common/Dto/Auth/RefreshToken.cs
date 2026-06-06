using Microsoft.AspNetCore.Identity;

namespace Common.Dto.Auth
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; }

        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = default!;
    }
}
