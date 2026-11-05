
namespace Insurance.Application.Exceptions
{
    public class DuplicateClientException : Exception
    {
        public DuplicateClientException(string identificationNumber)
            : base($"A client with identification number '{identificationNumber}' already exists.")
        {
            
        }
        public DuplicateClientException(Exception innerException)
            : base( "A client with this identification number already exists.", innerException)
        {

        }
    }
}
