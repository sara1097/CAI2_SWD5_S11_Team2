using System;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context,UserManager<User> userManager,
         RoleManager<IdentityRole> roleManager,IServiceProvider serviceProvider)
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Create roles if they don't exist
            await SeedRoles(roleManager);

            // Create admin user if none exists
            await SeedAdminUser(context, userManager, serviceProvider);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminUser(AppDbContext context, UserManager<User> userManager,
        IServiceProvider serviceProvider)
        {
            string adminEmail = "admin@techxpress.com";
            string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "System Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Create Admin record using the existing context
                    context.Admins.Add(new Admin { UserId = adminUser.Id });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
