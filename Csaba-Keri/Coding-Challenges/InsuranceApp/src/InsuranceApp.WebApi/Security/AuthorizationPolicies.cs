namespace InsuranceApp.WebApi.Security;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string BrokerOnly = "BrokerOnly";
}
