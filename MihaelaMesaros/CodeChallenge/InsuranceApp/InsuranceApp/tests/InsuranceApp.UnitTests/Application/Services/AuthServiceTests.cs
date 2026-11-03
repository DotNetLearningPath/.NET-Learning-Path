using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Auth;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _service = new AuthService(
            _userRepositoryMock.Object,
            _passwordServiceMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    #region Login

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = TestData.CreateUser();
        var loginDto = TestData.CreateLoginDto(TestData.TestUsername, TestData.TestPassword);

        var accessExpiresAt = DateTime.UtcNow.AddMinutes(15);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(7);

        _userRepositoryMock.Setup(x => x.GetUserByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _passwordServiceMock.Setup(x => x.VerifyPassword(user, loginDto.Password)).Returns(true);

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns(new AccessTokenDto(TestData.AccessToken, accessExpiresAt));

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns((TestData.RefreshToken, refreshExpiresAt));

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        // Act
        var result = await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(TestData.AccessToken, result.Value.AccessToken);
        Assert.Equal(accessExpiresAt, result.Value.ExpiresAt);
        Assert.Equal(TestData.RefreshToken, result.Value.RefreshToken);
        Assert.Equal(refreshExpiresAt, result.Value.RefreshTokenExpiresAt);

        _refreshTokenRepositoryMock.Verify(x => 
            x.AddRefreshTokenAsync(
                It.Is<UserRefreshToken>(t => t.UserId == user.UserId && t.TokenHash == TestData.RefreshTokenHash && t.ExpiresAt == refreshExpiresAt && t.RevokedAt == null), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData(null, "TestPassword123!", "Auth.InvalidUsername")]
    [InlineData("", "TestPassword123!", "Auth.InvalidUsername")]
    [InlineData("   ", "TestPassword123!", "Auth.InvalidUsername")]
    [InlineData("broker1", null, "Auth.InvalidPassword")]
    [InlineData("broker1", "", "Auth.InvalidPassword")]
    [InlineData("broker1", "   ", "Auth.InvalidPassword")]
    public async Task LoginAsync_InvalidInput_ReturnsValidationError(string? username, string? password, string expectedErrorCode)
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = username,
            Password = password
        };

        // Act
        var result = await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.Error?.Code);

        _userRepositoryMock.Verify(x => x.GetUserByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_NonExistingUser_ReturnsNotAuthenticated()
    {
        // Arrange
        var loginDto = TestData.CreateLoginDto(TestData.TestUsername, TestData.TestPassword);

        _userRepositoryMock.Setup(x => x.GetUserByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);

        _passwordServiceMock.Verify(x => x.VerifyPassword(It.IsAny<User>(), It.IsAny<string>()),Times.Never);
        _jwtServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
    }

    [Theory]
    [InlineData(TestData.TestUsername, "WrongPassword")]
    public async Task LoginAsync_InvalidPassword_ReturnsNotAuthenticated(string username, string password)
    {
        // Arrange
        var user = TestData.CreateUser();
        var loginDto = TestData.CreateLoginDto(username, password);

        _userRepositoryMock.Setup(x => x.GetUserByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _passwordServiceMock.Setup(x => x.VerifyPassword(user, loginDto.Password)).Returns(false);

        // Act
        var result = await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);

        _jwtServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);

        _refreshTokenRepositoryMock.Verify(x => x.AddRefreshTokenAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNotAuthenticated()
    {
        // Arrange
        var user = TestData.CreateUser(isActive: false);
        var loginDto = TestData.CreateLoginDto(TestData.TestUsername, TestData.TestPassword);

        _userRepositoryMock.Setup(x => x.GetUserByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);

        _passwordServiceMock.Verify(x =>x.VerifyPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("  broker1  ")]
    public async Task LoginAsync_UsernameWithSpaces_UsesTrimmedUsername(string username)
    {
        // Arrange
        var user = TestData.CreateUser();

        var loginDto = TestData.CreateLoginDto(username, TestData.TestPassword);

        _userRepositoryMock.Setup(x => x.GetUserByUsernameAsync(TestData.TestUsername, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.GetUserByUsernameAsync(TestData.TestUsername, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Refresh Token

    [Theory]
    [InlineData("new-refresh-token", "new-refresh-token-hash")]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens(string refreshToken, string refreshTokenHash)
    {
        // Arrange
        var storedToken = TestData.CreateUserRefreshToken();

        var refreshTokenDto = new RefreshTokenDto(TestData.RefreshToken);

        var accessExpiresAt = DateTime.UtcNow.AddMinutes(15);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(7);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenHashAsync(TestData.RefreshTokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(storedToken.User)).Returns(new AccessTokenDto(TestData.AccessToken, accessExpiresAt));

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns((refreshToken, refreshExpiresAt));

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(x => x.RotateRefreshTokenAsync(
                TestData.RefreshTokenHash,
                It.IsAny<UserRefreshToken>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RefreshTokenAsync(refreshTokenDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(TestData.AccessToken, result.Value.AccessToken);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        Assert.Equal(accessExpiresAt, result.Value.ExpiresAt);
        Assert.Equal(refreshExpiresAt, result.Value.RefreshTokenExpiresAt);

        _refreshTokenRepositoryMock.Verify(x =>
            x.RotateRefreshTokenAsync(
                TestData.RefreshTokenHash,
                It.Is<UserRefreshToken>(t => t.UserId == storedToken.UserId && t.TokenHash == refreshTokenHash && t.ExpiresAt == refreshExpiresAt && t.RevokedAt == null),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(x => x.AddRefreshTokenAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RefreshTokenAsync_MissingToken_ReturnsNotAuthenticated(string? refreshToken)
    {
        // Arrange
        var dto = new RefreshTokenDto(refreshToken);

        // Act
        var result = await _service.RefreshTokenAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error.Code);

        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_NonExistingToken_ReturnsNotAuthenticated()
    {
        // Arrange
        var dto = new RefreshTokenDto(TestData.RefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenHashAsync(TestData.RefreshTokenHash, It.IsAny<CancellationToken>())).ReturnsAsync((UserRefreshToken?)null);

        // Act
        var result = await _service.RefreshTokenAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);

        _jwtServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);

        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Never);

        _refreshTokenRepositoryMock.Verify(x => x.RotateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<UserRefreshToken>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task RefreshTokenAsync_ExpiredOrRevokedToken_ReturnsNotAuthenticated(bool isExpired, bool isRevoked)
    {
        // Arrange
        var dto = new RefreshTokenDto(TestData.RefreshToken);

        var storedToken = TestData.CreateUserRefreshToken(isExpired, isRevoked);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenHashAsync(TestData.RefreshTokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        // Act
        var result = await _service.RefreshTokenAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error.Code);

        _refreshTokenRepositoryMock.Verify(x =>
            x.RotateRefreshTokenAsync(
                It.IsAny<string>(),
                It.IsAny<UserRefreshToken>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_InactiveUser_ReturnsNotAuthenticated()
    {
        // Arrange
        var dto = new RefreshTokenDto(TestData.RefreshToken);

        var storedToken = TestData.CreateUserRefreshToken(isUserActive: false);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenHashAsync(TestData.RefreshTokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        // Act
        var result = await _service.RefreshTokenAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);

        _jwtServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_RotationFails_ReturnsNotAuthenticated()
    {
        // Arrange
        var storedToken = TestData.CreateUserRefreshToken();
        var dto = new RefreshTokenDto(TestData.RefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenHashAsync(TestData.RefreshTokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(storedToken.User)).Returns(new AccessTokenDto(TestData.AccessToken, DateTime.UtcNow.AddMinutes(15)));

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(("new-refresh-token", DateTime.UtcNow.AddDays(7)));

        _refreshTokenRepositoryMock.Setup(x => x.RotateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<UserRefreshToken>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.RefreshTokenAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);
        Assert.Null(result.Value);
    }

    #endregion

    #region Logout

    [Fact]
    public async Task LogoutAsync_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var dto = new RefreshTokenDto(TestData.RefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        _refreshTokenRepositoryMock.Setup(x => x.RevokeRefreshTokenAsync(TestData.RefreshTokenHash, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.LogoutAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _refreshTokenRepositoryMock.Verify(x => x.RevokeRefreshTokenAsync(TestData.RefreshTokenHash, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LogoutAsync_MissingToken_ReturnsNotAuthenticated(
        string? refreshToken)
    {
        // Arrange
        var dto = new RefreshTokenDto(refreshToken!);

        // Act
        var result = await _service.LogoutAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);

        _refreshTokenRepositoryMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_InvalidToken_ReturnsNotAuthenticated()
    {
        // Arrange
        var dto = new RefreshTokenDto(TestData.RefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(TestData.RefreshToken)).Returns(TestData.RefreshTokenHash);

        _refreshTokenRepositoryMock.Setup(x => x.RevokeRefreshTokenAsync(TestData.RefreshTokenHash, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.LogoutAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error.Code);
    }

    [Fact]
    public async Task LogoutAllAsync_ValidUserId_ReturnsSuccess()
    {
        // Arrange
        var userId = TestData.UserId;

        // Act
        var result = await _service.LogoutAllAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllUserTokensAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAllAsync_EmptyUserId_ReturnsNotAuthenticated()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var result = await _service.LogoutAllAsync(userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotAuthenticated.Code, result.Error?.Code);

        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllUserTokensAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}