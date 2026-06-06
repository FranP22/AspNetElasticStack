using Microsoft.AspNetCore.Identity;

namespace Database.Model
{
    public class DbRefreshToken
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
