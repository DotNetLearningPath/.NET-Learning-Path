using Domain.Enums;

namespace Domain.Entities
{
    public class Client
    {
        public Guid Id { get; private set; }
        public ClientType Type { get; private set; }
        public string Name { get; private set; }
        public string IdentificationNumber { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? Address { get; private set; }


        private static void CheckClientType(ClientType type)
        {
            if (!Enum.IsDefined(type))
            {
                throw new ArgumentException("Client type is not valid.", nameof(type));
            }
        }

        private static void CheckClientName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Client name is required.");
        }

        private static void CheckClientId(string identificationNumber)
        {

            if (string.IsNullOrWhiteSpace(identificationNumber))
                throw new ArgumentException(
                    "Identification number is required.");
        }

        public Client(
            ClientType type,
            string name,
            string identificationNumber,
            string? email = null,
            string? phone = null,
            string? address = null)
        {
            CheckClientType(type);
            CheckClientName(name);
            CheckClientId(identificationNumber);


            Id = Guid.NewGuid();
            Type = type;
            Name = name;
            IdentificationNumber = identificationNumber;
            Email = email;
            Phone = phone;
            Address = address;
        }
        public void UpdateContactDetails(
            string? email,
            string? phone,
            string? address)
        {
            Email = email;
            Phone = phone;
            Address = address;
        }

        public void ChangeName(string name)
        {
            CheckClientName(name);

            Name = name;
        }
        public void ChangeType(ClientType type)
        {
            CheckClientType(type);
            Type = type;
        }

        public void ChangeIdentificationNumber(string identificationNumber)
        {
            CheckClientId(identificationNumber);

            IdentificationNumber = identificationNumber;
        }
    }
}
