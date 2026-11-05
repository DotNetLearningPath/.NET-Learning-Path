using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IClientValidator
    {
        void Validate(Client client);
    }
}
