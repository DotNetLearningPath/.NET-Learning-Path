using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Auth;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository, 
    IPasswordService passwordService, 
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokenRepository
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

        var username = loginDto.Username.Trim();

        var user = await userRepository.GetUserByUsernameAsync(username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        if (!passwordService.VerifyPassword(user, loginDto.Password))
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        var (accessToken, accessTokenExpiresAt) = jwtService.GenerateAccessToken(user);

        var (refreshToken, refreshTokenExpiresAt) = refreshTokenService.GenerateRefreshToken();

        var userRefreshToken = new UserRefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = refreshTokenService.HashRefreshToken(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshTokenExpiresAt
        };

        await refreshTokenRepository.AddRefreshTokenAsync(userRefreshToken, cancellationToken);

        return Result<LoginResponseDto>.Success(
            new LoginResponseDto(
                accessToken,
                accessTokenExpiresAt,
                refreshToken,
                refreshTokenExpiresAt
            ));
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync( RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        var tokenHash = refreshTokenService.HashRefreshToken(refreshTokenDto.RefreshToken);

        var dbToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (dbToken is null || dbToken.RevokedAt.HasValue || dbToken.ExpiresAt <= DateTime.UtcNow || !dbToken.User.IsActive)
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        var tokenUser = dbToken.User;

        if (tokenUser == null)
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        var (newAccessToken, newAccessTokenExpiresAt) = jwtService.GenerateAccessToken(tokenUser);

        var (newRefreshToken, newRefreshTokenExpiresAt) = refreshTokenService.GenerateRefreshToken();

        var newTokenEntity = new UserRefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = tokenUser.UserId,
            TokenHash = refreshTokenService.HashRefreshToken(newRefreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = newRefreshTokenExpiresAt
        };

        var rotatedToken = await refreshTokenRepository.RotateRefreshTokenAsync(
            tokenHash,
            newTokenEntity,
            DateTime.UtcNow,
            cancellationToken);

        if (!rotatedToken)
        {
            return Result<LoginResponseDto>.Failure(AuthErrors.NotAuthenticated);
        }

        return Result<LoginResponseDto>.Success(new LoginResponseDto(newAccessToken, newAccessTokenExpiresAt, newRefreshToken, newRefreshTokenExpiresAt));
    }

    public async Task<Result> LogoutAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(AuthErrors.NotAuthenticated);
        }

        await refreshTokenRepository.RevokeAllUserTokensAsync(userId,DateTime.UtcNow,cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LogoutAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
        {
            return Result.Failure(AuthErrors.NotAuthenticated);
        }

        var tokenHash = refreshTokenService.HashRefreshToken(refreshTokenDto.RefreshToken);

        var revoked = await refreshTokenRepository.RevokeRefreshTokenAsync(tokenHash, DateTime.UtcNow, cancellationToken);

        if (!revoked)
        {
            return Result.Failure(AuthErrors.NotAuthenticated);
        }

        return Result.Success();
    }

}
