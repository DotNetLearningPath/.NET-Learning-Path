using InsuranceApp.Application.Abstractions.Security;
using System.IdentityModel.Tokens.Jwt;

namespace InsuranceApp.Api.Security;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    public Guid? BrokerId
    {
        get
        {
            var brokerIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("brokerId")?.Value;

            if (Guid.TryParse(brokerIdClaim, out var brokerId))
            {
                return brokerId;
            }

            return null;
        }
    }
}

