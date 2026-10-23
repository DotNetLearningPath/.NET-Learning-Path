using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace InsuranceApp.Infrastructure.Security;

internal sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(user.PasswordHash);

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return (result != PasswordVerificationResult.Failed ? true : false);
    }
}
