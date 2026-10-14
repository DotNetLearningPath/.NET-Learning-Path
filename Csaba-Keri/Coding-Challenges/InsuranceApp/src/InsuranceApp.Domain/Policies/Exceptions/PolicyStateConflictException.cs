namespace InsuranceApp.Domain.Policies.Exceptions;

public class PolicyStateConflictException(string message) : Exception(message)
{
}
