using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.UnitTests.Helpers;

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

            _mapper = TestMapperFactory.Create();

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
            var building = TestDataFactory.CreateBuilding();
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
            var building = TestDataFactory.CreateBuilding();
            var country = TestDataFactory.CreateCountry();
            var county = TestDataFactory.CreateCounty(countryId: country.Id);
            var city = TestDataFactory.CreateCity(
                countyId: county.Id,
                id: building.CityId);

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
            var request = TestDataFactory.CreateBuildingRequest();
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
            var request = TestDataFactory.CreateBuildingRequest();
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
            var request = TestDataFactory.CreateBuildingRequest();
            _clientRepository
                .Setup(r => r.GetAsync(request.ClientId))
                .ReturnsAsync(TestDataFactory.CreateClient(id: request.ClientId));
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
            var request = TestDataFactory.CreateBuildingRequest();
            _clientRepository
                .Setup(r => r.GetAsync(request.ClientId))
                .ReturnsAsync(TestDataFactory.CreateClient(id: request.ClientId));
            _cityRepository
                .Setup(r => r.GetAsync(request.CityId))
                .ReturnsAsync(TestDataFactory.CreateCity(id: request.CityId));
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
            var request = TestDataFactory.CreateUpdateBuildingRequest();
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
            var building = TestDataFactory.CreateBuilding();
            var request = TestDataFactory.CreateUpdateBuildingRequest();
            _buildingRepository
                .Setup(r => r.GetAsync(building.Id))
                .ReturnsAsync(building);
            _clientRepository
                .Setup(r => r.GetAsync(building.ClientId))
                .ReturnsAsync(TestDataFactory.CreateClient(id: building.ClientId));
            _cityRepository
                .Setup(r => r.GetAsync(request.CityId))
                .ReturnsAsync(TestDataFactory.CreateCity(id: request.CityId));

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
        public async Task DeleteBuildingAsync_WhenBuildingExists_CallsRepositoryDelete()
        {
            var building = TestDataFactory.CreateBuilding();
            _buildingRepository
                .Setup(r => r.GetAsync(building.Id))
                .ReturnsAsync(building);

            await _buildingService.DeleteBuildingAsync(building.Id);
            _buildingRepository.Verify(
                r => r.DeleteAsync(building),
                Times.Once);
        }

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
    }
}
