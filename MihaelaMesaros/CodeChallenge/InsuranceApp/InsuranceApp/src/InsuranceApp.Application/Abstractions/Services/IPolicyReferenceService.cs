using InsuranceApp.Application.Common;
using InsuranceApp.Application.Models.Services;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IPolicyReferenceService
{
    Task<Result<PolicyReferences>> ValidatePolicyReferencesAsync(
        Guid clientId,
        Guid buildingId,
        Guid brokerId,
        Guid currencyId,
        CancellationToken cancellationToken
    );
}
