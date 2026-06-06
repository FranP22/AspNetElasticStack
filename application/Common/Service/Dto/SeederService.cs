using Common.Service.Interface;
using Microsoft.AspNetCore.Identity;

namespace Common.Service.Dto
{
    public class SeederService : ISeederService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly string[] defaultRoles = { "User", "Subscriber", "Admin" };

        public SeederService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task PopulateDatabaseAsync()
        {
            await SeedRolesAsync();
        }

        private async Task SeedRolesAsync()
        {
            foreach (var role in defaultRoles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    var newRole = new IdentityRole(role);
                    await _roleManager.CreateAsync(newRole);
                }
            }
        }
    }
}
