using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;

namespace InsuranceApp.Application.Services;

public sealed class PolicyReferenceService(
    IClientRepository clientRepository,
    IBuildingRepository buildingRepository,
    IBrokerRepository brokerRepository,
    ICurrencyRepository currencyRepository
) : IPolicyReferenceService
{
    public IClientRepository ClientRepository { get; } = clientRepository;
    public IBuildingRepository BuildingRepository { get; } = buildingRepository;
    public IBrokerRepository BrokerRepository { get; } = brokerRepository;
    public ICurrencyRepository CurrencyRepository { get; } = currencyRepository;
}
