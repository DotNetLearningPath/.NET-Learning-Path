using InsuranceApp.Application.Abstractions.Persistence;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IPolicyReferenceService
{
    IClientRepository ClientRepository { get; }
    IBuildingRepository BuildingRepository { get; }
    IBrokerRepository BrokerRepository { get; }
    ICurrencyRepository CurrencyRepository { get; }
}
