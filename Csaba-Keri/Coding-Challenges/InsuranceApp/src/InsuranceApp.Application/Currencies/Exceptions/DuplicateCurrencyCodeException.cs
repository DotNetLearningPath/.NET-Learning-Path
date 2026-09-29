namespace InsuranceApp.Application.Currencies.Exceptions;

public class DuplicateCurrencyCodeException(Exception? innerException = null)
    : Exception("A currency with this code already exists.", innerException)
{
}
