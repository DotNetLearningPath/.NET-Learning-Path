using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Fees.Commands;
using InsuranceApp.Application.Fees.Results;

namespace InsuranceApp.Application.Fees;

public interface IFeeService
{
    Task<FeeResult> GetFeeByIdAsync(Guid feeId, CancellationToken cancellationToken);
    
    Task<PagedResult<FeeResult>> GetFeesAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<FeeResult> CreateFeeAsync(CreateFeeCommand command, CancellationToken cancellationToken);
    
    Task<FeeResult> UpdateFeeAsync(UpdateFeeCommand command, CancellationToken cancellationToken);
}
