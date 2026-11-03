using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Auth;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
}
