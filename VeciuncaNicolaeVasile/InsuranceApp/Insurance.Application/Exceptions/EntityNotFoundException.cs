namespace Insurance.Application.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, Guid id) : 
            base($"{entityName} with ID '{id}' was not found.")
        {

        }
    }
}
