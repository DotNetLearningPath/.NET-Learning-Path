using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.User;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken);
}
