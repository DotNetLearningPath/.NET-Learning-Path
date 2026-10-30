using InsuranceApp.Application.DTOs.Auth;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Security;

public interface IJwtService
{
    AccessTokenDto GenerateAccessToken(User user);
}