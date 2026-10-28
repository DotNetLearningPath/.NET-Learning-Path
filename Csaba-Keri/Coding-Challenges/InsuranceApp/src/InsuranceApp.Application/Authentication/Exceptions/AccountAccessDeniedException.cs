namespace InsuranceApp.Application.Authentication.Exceptions;

public class AccountAccessDeniedException()
    : Exception("This account is not permitted to sign in.")
{
}
