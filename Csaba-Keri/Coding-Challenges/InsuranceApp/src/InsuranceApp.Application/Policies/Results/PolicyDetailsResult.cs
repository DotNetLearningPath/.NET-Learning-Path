using InsuranceApp.Application.Brokers.Results;
using InsuranceApp.Application.Buildings.Results;
using InsuranceApp.Application.Clients.Results;

namespace InsuranceApp.Application.Policies.Results;

public record PolicyDetailsResult(
    PolicyResult Policy,
    ClientResult Client,
    BuildingDetailsResult Building,
    BrokerResult Broker
);
