using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Helpers;

/// <summary>
/// Helper class for identifying specific database exceptions.
/// </summary>
public static class DbExceptionHelper
{
    /// <summary>
    /// Checks if the exception is caused by a unique constraint violation (SQL Server error 2601 or 2627).
    /// </summary>
    /// <param name="ex">The DbUpdateException to check.</param>
    /// <returns>True if the exception represents a unique constraint violation; otherwise, false.</returns>
    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // SQL Server unique constraint violation error numbers:
        // 2601 = Cannot insert duplicate key row with unique index
        // 2627 = Violation of PRIMARY KEY/UNIQUE KEY constraint
        return ex.InnerException is SqlException sqlEx && sqlEx.Number is 2601 or 2627;
    }
}
