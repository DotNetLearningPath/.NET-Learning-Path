using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.DTOs.Auth;
using InsuranceApp.Domain.Entities;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Auth;

public sealed class AuthEndpointsTests(WebApplicationFactorySqlServer factory) : IntegrationTestBaseSqlServer(factory), IClassFixture<WebApplicationFactorySqlServer>
{
    private const string BaseUrl = "/api/auth";

    #region Login

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/login", new LoginDto { Username = user.Username, Password = TestData.TestPassword });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));

        var storedToken = await DbContext.UserRefreshTokens.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.UserId);

        Assert.NotNull(storedToken);
        Assert.Null(storedToken.RevokedAt);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/login", new LoginDto {Username = user.Username, Password = "WrongPassword" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(await DbContext.UserRefreshTokens.AsNoTracking().AnyAsync());
    }

    [Fact]
    public async Task Login_NonExistingUser_ReturnsUnauthorized()
    {
        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/login", new LoginDto { Username = "NonExistingUser", Password = TestData.TestPassword });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var user = TestData.CreateAdminUser();
        user.IsActive = false;

        await SeedUserAsync(user);

        // Act
        var response = await HttpClient.PostAsJsonAsync( $"{BaseUrl}/login", new LoginDto { Username = user.Username, Password = TestData.TestPassword });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(await DbContext.UserRefreshTokens.AsNoTracking().AnyAsync());
    }
    #endregion

    #region Refresh Token

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());

        var login = await LoginAsync(user.Username);

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.NotEqual(login.RefreshToken, result.RefreshToken);

        var tokens = await DbContext.UserRefreshTokens.AsNoTracking().Where(x => x.UserId == user.UserId).ToListAsync();

        Assert.Equal(2, tokens.Count);
        Assert.Single(tokens, x => x.RevokedAt != null);
        Assert.Single(tokens, x => x.RevokedAt == null);
    }

    [Fact]
    public async Task Refresh_ReusedToken_ReturnsUnauthorized()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());
        var login = await LoginAsync(user.Username);

        // Act
        var firstRefresh = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login.RefreshToken));
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());
        var login = await LoginAsync(user.Username);

        var token = await DbContext.UserRefreshTokens.SingleAsync(x => x.UserId == user.UserId);

        token.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_RevokedAfterLogout_ReturnsUnauthorized()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());
        var login = await LoginAsync(user.Username);

        // Act
        var logoutResponse = await HttpClient.PostAsJsonAsync($"{BaseUrl}/logout", new RefreshTokenDto(login.RefreshToken));
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());
        var login = await LoginAsync(user.Username);

        user.IsActive = false;

        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    #endregion

    #region Logout

    [Fact]
    public async Task Logout_ValidToken_ReturnsNoContent()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());
        var login = await LoginAsync(user.Username);

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/logout", new RefreshTokenDto(login.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        DbContext.ChangeTracker.Clear();

        var token = await DbContext.UserRefreshTokens.SingleAsync(x => x.UserId == user.UserId);

        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public async Task LogoutAll_ValidJwt_RevokesAllTokens()
    {
        // Arrange
        var user = await SeedUserAsync(TestData.CreateAdminUser());

        var login1 = await LoginAsync(user.Username);
        var login2 = await LoginAsync(user.Username);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login1.AccessToken);

        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/logout-all", null);

        var refreshResponse1 = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login1.RefreshToken));
        var refreshResponse2 = await HttpClient.PostAsJsonAsync($"{BaseUrl}/refresh", new RefreshTokenDto(login2.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse1.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse2.StatusCode);

        var activeTokens = await DbContext.UserRefreshTokens.AsNoTracking().Where(x => x.UserId == user.UserId && x.RevokedAt == null).CountAsync();

        Assert.Equal(0, activeTokens);
    }

    [Fact]
    public async Task LogoutAll_WithoutJwt_ReturnsUnauthorized()
    {
        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/logout-all", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    #endregion
}
