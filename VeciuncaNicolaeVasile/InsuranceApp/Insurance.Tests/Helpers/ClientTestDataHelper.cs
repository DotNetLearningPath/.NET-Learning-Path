using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Tests.Helpers
{
    public static class ClientTestDataHelper
    {
        public static Client CreateValidClient()
        {
            return new Client
            {
                Id = Guid.NewGuid(),
                ClientType = ClientType.Individual,
                Name = "CRRRAa",
                IdentificationNumber = "6767676767676767",
                Email = "CRRRAa@papapapa.com",
                Phone = "0764626172"
            };
        }
    }
}
