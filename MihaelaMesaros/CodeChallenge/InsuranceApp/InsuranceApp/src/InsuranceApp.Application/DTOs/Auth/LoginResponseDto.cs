namespace InsuranceApp.Application.DTOs.Auth;

public sealed record LoginResponseDto(string AccessToken, DateTime ExpiresAt);
