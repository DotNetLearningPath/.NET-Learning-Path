using FluentValidation;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Fees.Commands;
using InsuranceApp.Application.Fees.Mappings;
using InsuranceApp.Application.Fees.Results;
using InsuranceApp.Domain.Fees;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Fees;

public class FeeService(
    IFeeRepository feeRepository,
    IValidator<CreateFeeCommand> createValidator,
    IValidator<UpdateFeeCommand> updateValidator,
    IValidator<PageQuery> pageValidator,
    ILogger<FeeService> logger
) : IFeeService
{
    private readonly IFeeRepository _feeRepository = feeRepository;
    private readonly IValidator<CreateFeeCommand> _createValidator = createValidator;
    private readonly IValidator<UpdateFeeCommand> _updateValidator = updateValidator;
    private readonly IValidator<PageQuery> _pageValidator = pageValidator;
    private readonly ILogger<FeeService> _logger = logger;

    public async Task<FeeResult> GetFeeByIdAsync(Guid feeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fee = await GetFeeByIdOrThrowAsync(feeId, cancellationToken);

        return fee.ToResult();
    }

    public async Task<PagedResult<FeeResult>> GetFeesAsync(PageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        await _pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var page = await _feeRepository.GetFeesAsync(query, cancellationToken);

        return page.Map(fee => fee.ToResult());
    }

    public async Task<FeeResult> CreateFeeAsync(CreateFeeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var fee = new FeeConfiguration(
            id: Guid.NewGuid(),
            name: command.Name,
            type: command.Type,
            percentage: command.Percentage,
            effectiveFrom: command.EffectiveFrom,
            effectiveTo: command.EffectiveTo,
            isActive: command.IsActive
        );

        await _feeRepository.AddFeeAsync(fee, cancellationToken);
        _logger.LogInformation("Fee configuration {FeeId} created.", fee.Id);
        
        return fee.ToResult();
    }

    public async Task<FeeResult> UpdateFeeAsync(UpdateFeeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var fee = await GetFeeByIdOrThrowAsync(command.FeeId, cancellationToken);

        fee.UpdateDetails(
            name: command.Name,
            type: command.Type,
            percentage: command.Percentage,
            effectiveFrom: command.EffectiveFrom,
            effectiveTo: command.EffectiveTo,
            isActive: command.IsActive
        );

        await _feeRepository.UpdateFeeAsync(fee, cancellationToken);
        _logger.LogInformation("Fee configuration {FeeId} updated. IsActive: {IsActive}.", fee.Id, fee.IsActive);
        
        return fee.ToResult();
    }

    private async Task<FeeConfiguration> GetFeeByIdOrThrowAsync(Guid feeId, CancellationToken cancellationToken)
    {
        if (feeId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("FeeId", "Fee identifier must not be empty.");
        }

        return await _feeRepository.GetFeeByIdAsync(feeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(FeeConfiguration), feeId);
    }
}
