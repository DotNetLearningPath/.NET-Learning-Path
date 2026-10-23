using SupportAPI.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportAPI.Application.Abstractions
{
    public interface IAccessTokenGenerator
    {
        IssuedAccessToken Create(Guid userId, string name);
    }
}
