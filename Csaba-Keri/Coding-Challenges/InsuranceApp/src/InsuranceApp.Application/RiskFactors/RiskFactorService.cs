using FluentValidation;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.RiskFactors.Commands;
using InsuranceApp.Application.RiskFactors.Mappings;
using InsuranceApp.Application.RiskFactors.Results;
using InsuranceApp.Domain.RiskFactors;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.RiskFactors;

public class RiskFactorService(
    IRiskFactorRepository riskFactorRepository,
    IValidator<CreateRiskFactorCommand> createValidator,
    IValidator<UpdateRiskFactorCommand> updateValidator,
    IValidator<PageQuery> pageValidator,
    ILogger<RiskFactorService> logger
) : IRiskFactorService
{
    private readonly IRiskFactorRepository _riskFactorRepository = riskFactorRepository;
    private readonly IValidator<CreateRiskFactorCommand> _createValidator = createValidator;
    private readonly IValidator<UpdateRiskFactorCommand> _updateValidator = updateValidator;
    private readonly IValidator<PageQuery> _pageValidator = pageValidator;
    private readonly ILogger<RiskFactorService> _logger = logger;

    public async Task<RiskFactorResult> GetRiskFactorByIdAsync(Guid riskFactorId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var riskFactor = await GetRiskFactorByIdOrThrowAsync(riskFactorId, cancellationToken);

        return riskFactor.ToResult();
    }

    public async Task<PagedResult<RiskFactorResult>> GetRiskFactorsAsync(PageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        await _pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var page = await _riskFactorRepository.GetRiskFactorsAsync(query, cancellationToken);

        return page.Map(riskFactor => riskFactor.ToResult());
    }

    public async Task<RiskFactorResult> CreateRiskFactorAsync(CreateRiskFactorCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var riskFactor = new RiskFactorConfiguration(
            id: Guid.NewGuid(),
            target: command.ToTarget(),
            adjustmentPercentage: command.AdjustmentPercentage,
            isActive: command.IsActive
        );

        await _riskFactorRepository.AddRiskFactorAsync(riskFactor, cancellationToken);
        _logger.LogInformation("Risk factor {RiskFactorId} created.", riskFactor.Id);
        
        return riskFactor.ToResult();
    }

    public async Task<RiskFactorResult> UpdateRiskFactorAsync(UpdateRiskFactorCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var riskFactor = await GetRiskFactorByIdOrThrowAsync(command.RiskFactorId, cancellationToken);

        riskFactor.UpdateDetails(
            target: command.ToTarget(),
            adjustmentPercentage: command.AdjustmentPercentage,
            isActive: command.IsActive
        );

        await _riskFactorRepository.UpdateRiskFactorAsync(riskFactor, cancellationToken);
        _logger.LogInformation("Risk factor {RiskFactorId} updated. IsActive: {IsActive}.", riskFactor.Id, riskFactor.IsActive);
        
        return riskFactor.ToResult();
    }

    private async Task<RiskFactorConfiguration> GetRiskFactorByIdOrThrowAsync(Guid riskFactorId, CancellationToken cancellationToken)
    {
        if (riskFactorId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("RiskFactorId", "Risk factor identifier must not be empty.");
        }

        return await _riskFactorRepository.GetRiskFactorByIdAsync(riskFactorId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(RiskFactorConfiguration), riskFactorId);
    }
}
