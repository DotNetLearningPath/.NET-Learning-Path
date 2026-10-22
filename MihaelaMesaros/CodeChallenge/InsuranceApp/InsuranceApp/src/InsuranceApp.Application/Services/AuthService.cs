using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Auth;

namespace InsuranceApp.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository, 
    IPasswordService passwordService, 
    IJwtService jwtService
    ) : IAuthService
{
    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Username))
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.InvalidUsername);
        }
        if (string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.InvalidPassword);
        }

        var user = await userRepository.GetUserByUsernameAsync(loginDto.Username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        if (!passwordService.VerifyPassword(user, loginDto.Password))
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        var generatedToken = jwtService.GenerateToken(user);

        return Result<LoginResponseDto>.Success(generatedToken);
    }
}
