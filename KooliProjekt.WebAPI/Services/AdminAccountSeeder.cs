using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KooliProjekt.WebAPI.Services
{
    public static class AdminAccountSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            PasswordHasher<RentalUser> passwordHasher)
        {
            var email = configuration["Admin:Email"];
            var password = configuration["Admin:Password"];
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(email)
                || !new EmailAddressAttribute().IsValid(email)
                || string.IsNullOrWhiteSpace(password)
                || password.Length < 12)
            {
                throw new InvalidOperationException(
                    "Configure both Admin:Email and an Admin:Password of at least 12 characters.");
            }

            var normalizedEmail = AccountService.NormalizeEmail(email);
            var user = await dbContext.RentalUsers.FirstOrDefaultAsync(
                account => account.NormalizedEmail == normalizedEmail);

            if (user == null)
            {
                user = new RentalUser
                {
                    Name = "Administrator",
                    Email = email.Trim(),
                    NormalizedEmail = normalizedEmail,
                    IsAdmin = true
                };
                user.PasswordHash = passwordHasher.HashPassword(user, password);
                dbContext.RentalUsers.Add(user);
            }
            else if (!user.IsAdmin)
            {
                user.IsAdmin = true;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
