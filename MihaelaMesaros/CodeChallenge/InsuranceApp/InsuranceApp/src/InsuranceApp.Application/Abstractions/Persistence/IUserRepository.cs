using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
    Task AddUserAsync(User user, CancellationToken cancellationToken);
}
