using SupportAPI.Application.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SupportAPI.Api
{
    public class HttpCurrentUser : ICurrentUser
    {
        public Guid UserId { get; private set; }
           
        public void SetUserId(Guid userId)
        {
            UserId = userId;
        }
    }
}
