using InsuranceApp.Application.Clients.Results;
using InsuranceApp.Domain.Clients;

namespace InsuranceApp.Application.Clients.Mappings;

internal static class ClientMappings
{
    public static ClientResult ToResult(this Client client)
    {
        return new(
            Id: client.Id,
            Type: client.Type,
            IdentificationNumber: client.IdentificationNumber,
            Name: client.Name,
            Email: client.Email,
            Phone: client.Phone,
            PrimaryAddress: client.PrimaryAddress
        );
    }
}
