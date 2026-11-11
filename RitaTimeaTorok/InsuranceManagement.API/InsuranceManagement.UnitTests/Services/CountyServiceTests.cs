using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Domain.Entities.Geography;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.UnitTests.Helpers;

namespace InsuranceManagement.UnitTests.Services
{
    [TestClass]
    public class CountyServiceTests
    {
        private Mock<ICountyRepository> _countyRepository = null!;
        private Mock<IGenericRepository<Country>> _countryRepository = null!;
        private Mock<ICityRepository> _cityRepository = null!;
        private Mock<IRiskFactorConfigurationRepository> _riskFactorConfigurationRepository = null!;
        private IMapper _mapper = null!;
        private CountyService _countyService = null!;

        [TestInitialize]
        public void Setup()
        {
            _countyRepository = new Mock<ICountyRepository>();
            _countryRepository = new Mock<IGenericRepository<Country>>();
            _cityRepository = new Mock<ICityRepository>();
            _riskFactorConfigurationRepository = new Mock<IRiskFactorConfigurationRepository>();

            _mapper = TestMapperFactory.Create();

            _countyService = new CountyService(
                _countyRepository.Object,
                _countryRepository.Object,
                _cityRepository.Object,
                _riskFactorConfigurationRepository.Object,
                _mapper,
                NullLogger<CountyService>.Instance);
        }

        [TestMethod]
        public async Task GetAllCounties_WhenNoCounties_ReturnsEmptyList()
        {
            // Arrange
            _countyRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<County> { Items = new List<County>(), PageNumber = 1, PageSize = 10, TotalCount = 0 });

            // Act
            var result = await _countyService.GetAllCountiesAsync(1, 10);

            // Assert
            Assert.AreEqual(0, result.Items.Count);
        }

        [TestMethod]
        public async Task GetAllCounties_WhenCountyExists_ReturnsCounty()
        {
            // Arrange
            var county = TestDataFactory.CreateCounty();
            _countyRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<County> { Items = new List<County> { county }, PageNumber = 1, PageSize = 10, TotalCount = 1 });

            // Act
            var result = await _countyService.GetAllCountiesAsync(1, 10);

            // Assert
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("Cluj", result.Items[0].Name);
            Assert.AreEqual(county.Id, result.Items[0].Id);
        }

        [TestMethod]
        public async Task GetCountyByIdAsync_WhenCountyDoesNotExist_ThrowsEntityNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _countyService.GetCountyByIdAsync(id));
        }

        [TestMethod]
        public async Task GetCountyByIdAsync_WhenCountyExists_ReturnsCounty()
        {
            // Arrange
            var id = Guid.NewGuid();
            var county = TestDataFactory.CreateCounty(id: id);
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)county);

            // Act
            var result = await _countyService.GetCountyByIdAsync(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Cluj", result.Name);
            Assert.AreEqual(county.Id, result.Id);
        }

        [TestMethod]
        public async Task CreateCountyAsync_WhenNameIsEmpty_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateCountyDto { Name = "", CountryId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _countyService.CreateCountyAsync(request));

            _countyRepository.Verify(
                r => r.AddAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateCountyAsync_WhenCountryDoesNotExist_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateCountyDto { Name = "Cluj", CountryId = Guid.NewGuid() };
            _countryRepository
                .Setup(r => r.GetAsync(request.CountryId))
                .ReturnsAsync((Country?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _countyService.CreateCountyAsync(request));

            _countyRepository.Verify(
                r => r.AddAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateCountyAsync_WhenValid_CreatesCounty()
        {
            // Arrange
            var country = TestDataFactory.CreateCountry();
            var request = new CreateCountyDto { Name = "Cluj", CountryId = country.Id };
            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);
            _countyRepository
                .Setup(r => r.AddAsync(It.IsAny<County>()))
                .ReturnsAsync((County county) => county);

            // Act
            var result = await _countyService.CreateCountyAsync(request);

            // Assert
            Assert.AreEqual("Cluj", result.Name);
            _countyRepository.Verify(
                r => r.AddAsync(It.Is<County>(c => c.Name == "Cluj" && c.CountryId == country.Id)),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateCountyAsync_WhenCountyDoesNotExist_ThrowsEntityNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new UpdateCountyDto { Name = "Cluj", CountryId = Guid.NewGuid() };
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _countyService.UpdateCountyAsync(id, request));
            _countyRepository.Verify(
                r => r.UpdateAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateCountyAsync_WhenValid_UpdatesCounty()
        {
            // Arrange
            var country = TestDataFactory.CreateCountry();
            var county = TestDataFactory.CreateCounty(name: "Old Cluj", countryId: country.Id);
            var request = new UpdateCountyDto { Name = "Cluj", CountryId = country.Id };
            _countyRepository
                .Setup(r => r.GetAsync(county.Id))
                .ReturnsAsync(county);
            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);

            // Act
            var result = await _countyService.UpdateCountyAsync(county.Id, request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Cluj", result.Name);
            _countyRepository.Verify(
                r => r.UpdateAsync(county),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteCountyAsync_WhenCountyDoesNotExist_ThrowsEntityNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _countyService.DeleteCountyAsync(id));
            _countyRepository.Verify(
                r => r.DeleteAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteCountyAsync_WhenCountyExists_CallsRepositoryDelete()
        {
            // Arrange
            var county = TestDataFactory.CreateCounty();
            _countyRepository
                .Setup(r => r.GetAsync(county.Id))
                .ReturnsAsync(county);

            // Act
            await _countyService.DeleteCountyAsync(county.Id);

            // Assert
            _countyRepository.Verify(
                r => r.DeleteAsync(county),
                Times.Once);
        }
    }
}
