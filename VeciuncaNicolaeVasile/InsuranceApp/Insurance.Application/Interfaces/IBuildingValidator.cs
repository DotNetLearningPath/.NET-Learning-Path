using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IBuildingValidator
    {
        void Validate(Building building);
    }
}
