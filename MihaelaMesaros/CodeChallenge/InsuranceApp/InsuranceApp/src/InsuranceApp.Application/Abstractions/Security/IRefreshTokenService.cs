namespace InsuranceApp.Application.Abstractions.Security;

public interface IRefreshTokenService
{
    (string Token, DateTime ExpiresAt) GenerateRefreshToken();

    string HashRefreshToken(string token);
}
