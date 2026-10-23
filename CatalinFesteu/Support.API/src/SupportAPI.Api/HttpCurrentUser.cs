using SupportAPI.Application.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SupportAPI.Api
{
    public class HttpCurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
    {
        public Guid UserId
        {
            get
            {
                var userIdClaim = contextAccessor.HttpContext!.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
               
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    throw new InvalidOperationException("Invalid UserId Claim");
                }

                return userId;
            }
        }
    }
}
