using InsuranceApp.Application.Authentication.Results;
using InsuranceApp.Application.Common.Security;

namespace InsuranceApp.IntegrationTests.Authentication;

internal static class AuthenticationTestData
{
    public const string Issuer = "InsuranceApp.IntegrationTests";
    public const string Audience = "InsuranceApp.IntegrationTests.Api";
    public const string SigningKey = "TEST-ONLY-KEY-DO-NOT-USE-OUTSIDE-TESTS-0123456789-ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Email = "authentication@example.test";
    public const string Password = "Test-Only-Password123!";
    public static readonly Guid UserId = Guid.Parse("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee");
    public static readonly Guid BrokerId = Guid.Parse("dddddddd-dddd-4ddd-8ddd-dddddddddddd");

    public static AuthenticatedAccount Account(string role)
    {
        return new(
            UserId: UserId,
            Role: role,
            BrokerId: role == ApplicationRoles.Broker ? BrokerId : null
        );
    }
}
