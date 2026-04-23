using Microsoft.AspNetCore.Identity;

namespace authService.Application.Services;

public static class Roles
{
    // creating roles
    public static async Task AddRoles(RoleManager<IdentityRole<Guid>> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("BusinessOwner"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("BusinessOwner"));

        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("Customer"));
    }
}