using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace InsuranceManagement.UnitTests.Services
{
    [TestClass]
    public class CityServiceTests
    {
        private Mock<ICityRepository> _cityRepository = null!;
        private Mock<IGenericRepository<County>> _countyRepository = null!;
        private Mock<IBuildingRepository> _buildingRepository = null!;
        private IMapper _mapper = null!;
        private CityService _cityService = null!;

        [TestInitialize]
        public void Setup()
        {
            _cityRepository = new Mock<ICityRepository>();
            _countyRepository = new Mock<IGenericRepository<County>>();
            _buildingRepository = new Mock<IBuildingRepository>();

            _mapper = TestMapperFactory.Create();

            _cityService = new CityService(
                _cityRepository.Object,
                _countyRepository.Object,
                _buildingRepository.Object,
                _mapper,
                NullLogger<CityService>.Instance);
        }

        [TestMethod]
        public async Task GetAllCities_WhenNoCities_ReturnsEmptyList()
        {
            _cityRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<City> { Items = new List<City>(), PageNumber = 1, PageSize = 10, TotalCount = 0 });

            var result = await _cityService.GetAllCitiesAsync(1, 10);

            Assert.AreEqual(0, result.Items.Count);
        }

        [TestMethod]
        public async Task GetAllCities_WhenCityExists_ReturnsCity()
        {
            var city = TestDataFactory.CreateCity();
            _cityRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<City> { Items = new List<City> { city }, PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var result = await _cityService.GetAllCitiesAsync(1, 10);

            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("Cluj-Napoca", result.Items[0].Name);
            Assert.AreEqual(city.Id, result.Items[0].Id);
        }

        [TestMethod]
        public async Task GetCityByIdAsync_WhenCityDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            _cityRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((City?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _cityService.GetCityByIdAsync(id));
        }

        [TestMethod]
        public async Task GetCityByIdAsync_WhenCityExists_ReturnsCity()
        {
            var id = Guid.NewGuid();
            var city = TestDataFactory.CreateCity(id: id);
            _cityRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((City?)city);

            var result = await _cityService.GetCityByIdAsync(id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cluj-Napoca", result.Name);
            Assert.AreEqual(city.Id, result.Id);
        }

        [TestMethod]
        public async Task CreateCityAsync_WhenNameIsEmpty_ThrowsValidationException()
        {
            var request = new CreateCityDto { Name = "", CountyId = Guid.NewGuid() };

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _cityService.CreateCityAsync(request));

            _cityRepository.Verify(
                r => r.AddAsync(It.IsAny<City>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateCityAsync_WhenCountyDoesNotExist_ThrowsValidationException()
        {
            var request = new CreateCityDto { Name = "Cluj-Napoca", CountyId = Guid.NewGuid() };
            _countyRepository
                .Setup(r => r.GetAsync(request.CountyId))
                .ReturnsAsync((County?)null);

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _cityService.CreateCityAsync(request));

            _cityRepository.Verify(
                r => r.AddAsync(It.IsAny<City>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateCityAsync_WhenValid_CreatesCity()
        {
            var county = TestDataFactory.CreateCounty();
            var request = new CreateCityDto { Name = "Cluj-Napoca", CountyId = county.Id };
            _countyRepository
                .Setup(r => r.GetAsync(county.Id))
                .ReturnsAsync(county);
            _cityRepository
                .Setup(r => r.AddAsync(It.IsAny<City>()))
                .ReturnsAsync((City city) => city);

            var result = await _cityService.CreateCityAsync(request);

            Assert.AreEqual("Cluj-Napoca", result.Name);
            _cityRepository.Verify(
                r => r.AddAsync(It.Is<City>(c => c.Name == "Cluj-Napoca" && c.CountyId == county.Id)),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateCityAsync_WhenCityDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            var request = new UpdateCityDto { Name = "Cluj-Napoca", CountyId = Guid.NewGuid() };
            _cityRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((City?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _cityService.UpdateCityAsync(id, request));
            _cityRepository.Verify(
                r => r.UpdateAsync(It.IsAny<City>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateCityAsync_WhenValid_UpdatesCity()
        {
            var county = TestDataFactory.CreateCounty();
            var city = TestDataFactory.CreateCity(name: "Old City", countyId: county.Id);
            var request = new UpdateCityDto { Name = "Cluj-Napoca", CountyId = county.Id };
            _cityRepository
                .Setup(r => r.GetAsync(city.Id))
                .ReturnsAsync(city);
            _countyRepository
                .Setup(r => r.GetAsync(county.Id))
                .ReturnsAsync(county);

            var result = await _cityService.UpdateCityAsync(city.Id, request);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cluj-Napoca", result.Name);
            _cityRepository.Verify(
                r => r.UpdateAsync(city),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteCityAsync_WhenCityDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            _cityRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((City?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _cityService.DeleteCityAsync(id));
            _cityRepository.Verify(
                r => r.DeleteAsync(It.IsAny<City>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteCityAsync_WhenCityHasBuildings_ThrowsConflictException()
        {
            var city = TestDataFactory.CreateCity();
            _cityRepository
                .Setup(r => r.GetAsync(city.Id))
                .ReturnsAsync(city);
            _buildingRepository
                .Setup(r => r.ExistsByCityIdAsync(city.Id))
                .ReturnsAsync(true);

            await Assert.ThrowsExceptionAsync<ConflictException>(
                () => _cityService.DeleteCityAsync(city.Id));

            _cityRepository.Verify(
                r => r.DeleteAsync(It.IsAny<City>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteCityAsync_WhenCityExists_CallsRepositoryDelete()
        {
            var city = TestDataFactory.CreateCity();
            _cityRepository
                .Setup(r => r.GetAsync(city.Id))
                .ReturnsAsync(city);
            _buildingRepository
                .Setup(r => r.ExistsByCityIdAsync(city.Id))
                .ReturnsAsync(false);

            await _cityService.DeleteCityAsync(city.Id);
            _cityRepository.Verify(
                r => r.DeleteAsync(city),
                Times.Once);
        }
    }
}
