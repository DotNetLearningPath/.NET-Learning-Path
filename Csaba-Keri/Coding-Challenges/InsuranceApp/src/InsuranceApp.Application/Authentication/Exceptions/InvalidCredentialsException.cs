namespace InsuranceApp.Application.Authentication.Exceptions;

public class InvalidCredentialsException()
    : Exception("The email or password is incorrect.")
{
}
