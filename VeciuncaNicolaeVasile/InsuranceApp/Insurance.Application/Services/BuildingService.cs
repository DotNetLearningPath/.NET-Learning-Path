using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Application.Exceptions;

namespace Insurance.Application.Services
{
    public class BuildingService : IBuildingService
    {
        private const int MaximumPageSize = 100;

        private readonly IBuildingRepository _buildingRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IBuildingValidator _buildingValidator;
        private readonly ICityRepository _cityRepository;

        public BuildingService(
            IBuildingRepository buildingRepository,
            IClientRepository clientRepository,
            IBuildingValidator buildingValidator,
            ICityRepository cityRepository)
        {
            _buildingRepository = buildingRepository;
            _clientRepository = clientRepository;
            _buildingValidator = buildingValidator;
            _cityRepository = cityRepository;
        }

        public async Task<Building> CreateAsync(Building building, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(building);

            _buildingValidator.Validate(building);

            var existingClient = await _clientRepository.GetClientByIdAsync(building.ClientId, cancellationToken);

            if (existingClient == null)
            {
                throw new EntityNotFoundException("Client", building.ClientId);
            }

            var cityExists = await _cityRepository.ExistsAsync(building.CityId, cancellationToken);

            if (!cityExists)
            {
                throw new EntityNotFoundException("City", building.CityId);
            }

            building.Id = Guid.NewGuid();

            await _buildingRepository.AddAsync(building, cancellationToken);

            await _buildingRepository.SaveChangesAsync(cancellationToken);

            return await _buildingRepository.GetByIdAsync(building.Id, cancellationToken) ?? building;
        }

        public async Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _buildingRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Building>> GetByClientIdAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken)
        {
            if (clientId == Guid.Empty)
            {
                throw new ArgumentException("Client ID cannot be empty.", nameof(clientId));
            }

            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
            }

            if (pageSize < 1 || pageSize > MaximumPageSize)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size must be between 1 and {MaximumPageSize}.");
            }

            return await _buildingRepository.GetByClientIdAsync(clientId, page, pageSize, cancellationToken);
        }

        public async Task<Building?> UpdateAsync(Building building, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(building);

            var existingBuilding = await _buildingRepository.GetByIdAsync(building.Id, cancellationToken);

            if (existingBuilding == null)
            {
                return null;
            }

            var buildingToValidate = new Building
            {
                Id = existingBuilding.Id,
                ClientId = existingBuilding.ClientId,
                Street = building.Street,
                StreetNumber = building.StreetNumber,
                CityId = building.CityId,
                ConstructionYear = building.ConstructionYear,
                BuildingType = building.BuildingType,
                NumberOfFloors = building.NumberOfFloors,
                SurfaceArea = building.SurfaceArea,
                InsuredValue = building.InsuredValue
            };

            _buildingValidator.Validate(buildingToValidate);

            var cityExists = await _cityRepository.ExistsAsync(buildingToValidate.CityId, cancellationToken);

            if (!cityExists)
            {
                throw new EntityNotFoundException("City", buildingToValidate.CityId);
            }

            existingBuilding.Street = buildingToValidate.Street;
            existingBuilding.StreetNumber = buildingToValidate.StreetNumber;
            existingBuilding.CityId = buildingToValidate.CityId;
            existingBuilding.ConstructionYear = buildingToValidate.ConstructionYear;
            existingBuilding.BuildingType = buildingToValidate.BuildingType;
            existingBuilding.NumberOfFloors = buildingToValidate.NumberOfFloors;
            existingBuilding.SurfaceArea = buildingToValidate.SurfaceArea;
            existingBuilding.InsuredValue = buildingToValidate.InsuredValue;

            await _buildingRepository.SaveChangesAsync(cancellationToken);

            return await _buildingRepository.GetByIdAsync(existingBuilding.Id, cancellationToken) ?? existingBuilding;
        }
    }
}
