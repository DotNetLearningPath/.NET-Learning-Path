using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Security;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}
