using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;

namespace Insurance.Application.Validators
{
    public class BuildingValidator : IBuildingValidator
    {
        public void Validate(Building building)
        {
            ArgumentNullException.ThrowIfNull(building);

            if (string.IsNullOrWhiteSpace(building.Street))
            {
                throw new ArgumentException("Street is required.");
            }

            if (string.IsNullOrWhiteSpace(building.StreetNumber))
            {
                throw new ArgumentException("Street number is required.");
            }

            if (building.SurfaceArea <= 0)
            {
                throw new ArgumentException( "Surface area must be greater than zero.");
            }

            if (building.InsuredValue <= 0)
            {
                throw new ArgumentException("Insured value must be greater than zero.");
            }

            if (!Enum.IsDefined(building.BuildingType))
            {
                throw new ArgumentException("Invalid building type.");
            }
            if (building.CityId == Guid.Empty)
            {
                throw new ArgumentException("City ID is required.");
            }

            if (building.ConstructionYear < 1900 || building.ConstructionYear > DateTime.UtcNow.Year)
            {
                throw new ArgumentException( "Construction year must be between 1900 and the current year.");
            }

            if (building.NumberOfFloors <= 0)
            {
                throw new ArgumentException( "Number of floors must be greater than zero.");
            }
        }
    }
}
