namespace InsuranceApp.Application.DTOs.Auth;

public sealed record AccessTokenDto(string Token, DateTime ExpiresAt);
