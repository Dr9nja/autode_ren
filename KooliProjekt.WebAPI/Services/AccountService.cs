using System;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using KooliProjekt.WebAPI.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.WebAPI.Services
{
    public sealed class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PasswordHasher<RentalUser> _passwordHasher;

        public AccountService(
            ApplicationDbContext dbContext,
            PasswordHasher<RentalUser> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<AccountResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();
            var normalizedEmail = NormalizeEmail(email);
            if (await _dbContext.RentalUsers.AnyAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken))
            {
                return null;
            }

            var user = new RentalUser
            {
                Name = request.Name.Trim(),
                Email = email,
                NormalizedEmail = normalizedEmail,
                IsAdmin = false
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _dbContext.RentalUsers.Add(user);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                return null;
            }

            return ToResponse(user);
        }

        public async Task<AccountResponse> ValidateLoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var user = await _dbContext.RentalUsers.FirstOrDefaultAsync(
                account => account.NormalizedEmail == normalizedEmail,
                cancellationToken);

            if (user == null)
            {
                return null;
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);
            return verification == PasswordVerificationResult.Failed
                ? null
                : ToResponse(user);
        }

        public static string NormalizeEmail(string email)
        {
            return email.Trim().ToUpperInvariant();
        }

        private static AccountResponse ToResponse(RentalUser user)
        {
            return new AccountResponse(user.Id, user.Name, user.Email, user.IsAdmin);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.GetBaseException() is Microsoft.Data.SqlClient.SqlException
            {
                Number: 2601 or 2627
            };
        }
    }
}
