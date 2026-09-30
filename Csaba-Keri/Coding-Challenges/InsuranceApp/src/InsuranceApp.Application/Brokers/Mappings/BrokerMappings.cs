using InsuranceApp.Application.Brokers.Results;
using InsuranceApp.Domain.Brokers;

namespace InsuranceApp.Application.Brokers.Mappings;

internal static class BrokerMappings
{
    public static BrokerResult ToResult(this Broker broker)
    {
        return new(
            Id: broker.Id,
            Code: broker.Code,
            Name: broker.Name,
            Email: broker.Email,
            Phone: broker.Phone,
            Status: broker.Status
        );
    }
}
