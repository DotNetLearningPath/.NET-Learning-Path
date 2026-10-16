using InsuranceApp.WebApi.Models.Brokers;
using InsuranceApp.WebApi.Models.Buildings;
using InsuranceApp.WebApi.Models.Clients;

namespace InsuranceApp.WebApi.Models.Policies;

public record PolicyDetailsResponse(
    PolicyResponse Policy,
    ClientResponse Client,
    BuildingDetailsResponse Building,
    BrokerResponse Broker
);
