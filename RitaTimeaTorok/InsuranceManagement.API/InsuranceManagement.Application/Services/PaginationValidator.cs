using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.Services
{
    internal static class PaginationValidator
    {
        public static void Validate(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ValidationException("Page number must be at least 1.");

            if (pageSize < 1 || pageSize > 100)
                throw new ValidationException("Page size must be between 1 and 100.");

            if ((long)(pageNumber - 1) * pageSize > int.MaxValue)
                throw new ValidationException("Requested page number is too large.");
        }
    }
}
