using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Insurance.Application.Validators
{
    public class ClientValidator : IClientValidator
    {
        public void Validate(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client cannot be null.");
            }

            if (!Enum.IsDefined(client.ClientType))
            {
                throw new ArgumentException("Invalid client type.", nameof(client));
            }

            if (string.IsNullOrWhiteSpace(client.Name))
            {
                throw new ArgumentException("Name is required", nameof(client));
            }

            if (string.IsNullOrWhiteSpace(client.IdentificationNumber))
            {
                throw new ArgumentException("Identification number is required", nameof(client));
            }

            if (string.IsNullOrWhiteSpace(client.Email))
            {
                throw new ArgumentException("Email is required", nameof(client));
            }

            if (!new EmailAddressAttribute().IsValid(client.Email))
            {
                throw new ArgumentException("Invalid email format.", nameof(client));
            }

            if (string.IsNullOrWhiteSpace(client.Phone))
            {
                throw new ArgumentException("Phone number is required", nameof(client));
            }
        }
    }
}
