using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.Mapping;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.UnitTests.Services
{
    [TestClass]
    public class BuildingServiceTests
    {
        private Mock<IBuildingRepository> _buildingRepository = null!;
        private Mock<IGenericRepository<Client>> _clientRepository = null!;
        private Mock<IGenericRepository<City>> _cityRepository = null!;
        private Mock<IGenericRepository<County>> _countyRepository = null!;
        private Mock<IGenericRepository<Country>> _countryRepository = null!;
        private IMapper _mapper = null!;
        private BuildingService _buildingService = null!;

        [TestInitialize]
        public void Setup()
        {
            _buildingRepository = new Mock<IBuildingRepository>();
            _clientRepository = new Mock<IGenericRepository<Client>>();
            _cityRepository = new Mock<IGenericRepository<City>>();
            _countyRepository = new Mock<IGenericRepository<County>>();
            _countryRepository = new Mock<IGenericRepository<Country>>();

            var mapperConfig = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile<BuildingMappingProfile>();
                    cfg.AddProfile<GeographyMappingProfile>();
                },
                NullLoggerFactory.Instance);
            _mapper = mapperConfig.CreateMapper();

            _buildingService = new BuildingService(
                _buildingRepository.Object,
                _clientRepository.Object,
                _cityRepository.Object,
                _countyRepository.Object,
                _countryRepository.Object,
                _mapper,
                NullLogger<BuildingService>.Instance);
        }

        [TestMethod]
        public async Task GetAllBuildings_WhenNoBuildings_ReturnsEmptyList()
        {
            _buildingRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<Building> { Items = new List<Building>(), PageNumber = 1, PageSize = 10, TotalCount = 0 });

            var result = await _buildingService.GetAllBuildingsAsync(1, 10);

            Assert.AreEqual(0, result.Items.Count);
        }

        [TestMethod]
        public async Task GetAllBuildings_WhenBuildingExists_ReturnsBuilding()
        {
            var building = CreateBuilding();
            _buildingRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<Building> { Items = new List<Building> { building }, PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var result = await _buildingService.GetAllBuildingsAsync(1, 10);

            Assert.AreEqual(1, result.Items.Count);
            AssertBuildingDtoMatches(building, result.Items[0]);
        }

        [TestMethod]
        public async Task GetBuildingByIdAsync_WhenBuildingDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            _buildingRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Building?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _buildingService.GetBuildingByIdAsync(id));
        }

        [TestMethod]
        public async Task GetBuildingByIdAsync_WhenBuildingExists_ReturnsBuilding()
        {
            var building = CreateBuilding();
            var city = new City { Name = "Cluj-Napoca", CountyId = Guid.NewGuid() };
            var county = new County { Name = "Cluj", CountryId = Guid.NewGuid() };
            var country = new Country { Name = "Romania" };
            city.CountyId = county.Id;
            county.CountryId = country.Id;

            _buildingRepository
                .Setup(r => r.GetAsync(building.Id))
                .ReturnsAsync(building);
            _cityRepository
                .Setup(r => r.GetAsync(building.CityId))
                .ReturnsAsync(city);
            _countyRepository
                .Setup(r => r.GetAsync(city.CountyId))
                .ReturnsAsync(county);
            _countryRepository
                .Setup(r => r.GetAsync(county.CountryId))
                .ReturnsAsync(country);

            var result = await _buildingService.GetBuildingByIdAsync(building.Id);

            Assert.IsNotNull(result);
            AssertBuildingDtoMatches(building, result);
            Assert.AreEqual("Cluj-Napoca", result.City.Name);
            Assert.AreEqual("Cluj", result.County.Name);
            Assert.AreEqual("Romania", result.Country.Name);
        }

        [TestMethod]
        public async Task CreateBuildingAsync_WhenStreetIsEmpty_ThrowsValidationException()
        {
            var request = CreateRequest();
            request.Street = "";

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _buildingService.CreateBuildingAsync(request));

            _buildingRepository.Verify(
                r => r.AddAsync(It.IsAny<Building>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateBuildingAsync_WhenClientDoesNotExist_ThrowsValidationException()
        {
            var request = CreateRequest();
            _clientRepository
                .Setup(r => r.GetAsync(request.ClientId))
                .ReturnsAsync((Client?)null);

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _buildingService.CreateBuildingAsync(request));

            _buildingRepository.Verify(
                r => r.AddAsync(It.IsAny<Building>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateBuildingAsync_WhenCityDoesNotExist_ThrowsValidationException()
        {
            var request = CreateRequest();
            _clientRepository
                .Setup(r => r.GetAsync(request.ClientId))
                .ReturnsAsync(new Client
                {
                    Id = request.ClientId,
                    Type = ClientType.Individual,
                    Name = "John Doe",
                    IdentificationNumber = "ID-123"
                });
            _cityRepository
                .Setup(r => r.GetAsync(request.CityId))
                .ReturnsAsync((City?)null);

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _buildingService.CreateBuildingAsync(request));

            _buildingRepository.Verify(
                r => r.AddAsync(It.IsAny<Building>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateBuildingAsync_WhenValid_CreatesBuilding()
        {
            var request = CreateRequest();
            _clientRepository
                .Setup(r => r.GetAsync(request.ClientId))
                .ReturnsAsync(new Client
                {
                    Id = request.ClientId,
                    Type = ClientType.Individual,
                    Name = "John Doe",
                    IdentificationNumber = "ID-123"
                });
            _cityRepository
                .Setup(r => r.GetAsync(request.CityId))
                .ReturnsAsync(new City { Id = request.CityId, Name = "Cluj-Napoca", CountyId = Guid.NewGuid() });
            Building? createdBuilding = null;
            _buildingRepository
                .Setup(r => r.AddAsync(It.IsAny<Building>()))
                .ReturnsAsync((Building building) => createdBuilding = building);

            var result = await _buildingService.CreateBuildingAsync(request);

            Assert.IsNotNull(createdBuilding);
            AssertBuildingDtoMatches(createdBuilding!, result);
            _buildingRepository.Verify(
                r => r.AddAsync(It.Is<Building>(b => b.Street == "Main Street" && b.StreetNumber == "10" && b.ClientId == request.ClientId)),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateBuildingAsync_WhenBuildingDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            var request = CreateUpdateRequest();
            _buildingRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Building?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _buildingService.UpdateBuildingAsync(id, request));
            _buildingRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Building>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateBuildingAsync_WhenValid_UpdatesBuilding()
        {
            var building = CreateBuilding();
            var request = CreateUpdateRequest();
            _buildingRepository
                .Setup(r => r.GetAsync(building.Id))
                .ReturnsAsync(building);
            _clientRepository
                .Setup(r => r.GetAsync(building.ClientId))
                .ReturnsAsync(new Client
                {
                    Id = building.ClientId,
                    Type = ClientType.Individual,
                    Name = "John Doe",
                    IdentificationNumber = "ID-123"
                });
            _cityRepository
                .Setup(r => r.GetAsync(request.CityId))
                .ReturnsAsync(new City { Id = request.CityId, Name = "Cluj-Napoca", CountyId = Guid.NewGuid() });

            var result = await _buildingService.UpdateBuildingAsync(building.Id, request);

            Assert.IsNotNull(result);
            AssertBuildingDtoMatches(building, result);
            _buildingRepository.Verify(
                r => r.UpdateAsync(building),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteBuildingAsync_WhenBuildingDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            _buildingRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Building?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _buildingService.DeleteBuildingAsync(id));
            _buildingRepository.Verify(
                r => r.DeleteAsync(It.IsAny<Building>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteBuildingAsync_WhenBuildingExists_ReturnsTrue()
        {
            var building = CreateBuilding();
            _buildingRepository
                .Setup(r => r.GetAsync(building.Id))
                .ReturnsAsync(building);

            await _buildingService.DeleteBuildingAsync(building.Id);
            _buildingRepository.Verify(
                r => r.DeleteAsync(building),
                Times.Once);
        }

        private static Building CreateBuilding() => new()
        {
            ClientId = Guid.NewGuid(),
            Street = "Main Street",
            StreetNumber = "10",
            CityId = Guid.NewGuid(),
            ConstructionYear = 2000,
            Type = BuildingType.Residential,
            NumberOfFloors = 2,
            SurfaceArea = 120,
            InsuredValue = 100000
        };

        private static void AssertBuildingDtoMatches(
            Building expectedBuilding, 
            BuildingDto actualBuilding)
        {
            Assert.AreEqual(expectedBuilding.Id, actualBuilding.Id);
            Assert.AreEqual(expectedBuilding.ClientId, actualBuilding.ClientId);
            Assert.AreEqual(expectedBuilding.Street, actualBuilding.Street);
            Assert.AreEqual(expectedBuilding.StreetNumber, actualBuilding.StreetNumber);
            Assert.AreEqual(expectedBuilding.CityId, actualBuilding.CityId);
            Assert.AreEqual(expectedBuilding.ConstructionYear, actualBuilding.ConstructionYear);
            Assert.AreEqual(expectedBuilding.Type, actualBuilding.Type);
            Assert.AreEqual(expectedBuilding.NumberOfFloors, actualBuilding.NumberOfFloors);
            Assert.AreEqual(expectedBuilding.SurfaceArea, actualBuilding.SurfaceArea);
            Assert.AreEqual(expectedBuilding.InsuredValue, actualBuilding.InsuredValue);
        }

        private static CreateBuildingDto CreateRequest() => new()
        {
            ClientId = Guid.NewGuid(),
            Street = "Main Street",
            StreetNumber = "10",
            CityId = Guid.NewGuid(),
            ConstructionYear = 2000,
            Type = BuildingType.Residential,
            NumberOfFloors = 2,
            SurfaceArea = 120,
            InsuredValue = 100000
        };

        private static UpdateBuildingDto CreateUpdateRequest() => new()
        {
            Street = "Updated Street",
            StreetNumber = "20A",
            CityId = Guid.NewGuid(),
            ConstructionYear = 2001,
            Type = BuildingType.Office,
            NumberOfFloors = 3,
            SurfaceArea = 150,
            InsuredValue = 150000
        };
    }
}
