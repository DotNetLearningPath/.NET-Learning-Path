using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Auth;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);

    Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken);

    Task<Result> LogoutAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken);
}
