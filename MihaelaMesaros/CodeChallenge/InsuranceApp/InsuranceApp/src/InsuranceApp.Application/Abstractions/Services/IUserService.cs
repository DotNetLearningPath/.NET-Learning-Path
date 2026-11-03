using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.User;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken);
}
