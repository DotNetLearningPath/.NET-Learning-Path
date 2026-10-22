using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Fees;

namespace InsuranceApp.Application.Fees;

public interface IFeeRepository
{
    Task<FeeConfiguration?> GetFeeByIdAsync(Guid feeId, CancellationToken cancellationToken);
    
    Task<PagedResult<FeeConfiguration>> GetFeesAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<FeeConfiguration>> GetApplicableFeesAsync(DateOnly date, CancellationToken cancellationToken);
    
    Task AddFeeAsync(FeeConfiguration fee, CancellationToken cancellationToken);
    
    Task UpdateFeeAsync(FeeConfiguration fee, CancellationToken cancellationToken);
}
