namespace InsuranceApp.Application.Brokers.Exceptions;

public class DuplicateBrokerCodeException(Exception? innerException = null)
    : Exception("A broker with this code already exists.", innerException)
{
}
