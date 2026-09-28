namespace InsuranceApp.Application.Clients.Exceptions;

public class DuplicateClientIdentificationException(Exception? innerException = null)
    : Exception("A client with this identification number already exists.", innerException)
{
}
