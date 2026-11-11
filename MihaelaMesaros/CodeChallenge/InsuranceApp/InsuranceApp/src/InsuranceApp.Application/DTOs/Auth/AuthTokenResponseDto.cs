namespace InsuranceApp.Application.DTOs.Auth;

public sealed record AuthTokenResponseDto(string AccessToken, DateTime ExpiresAt);
