using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Mapping;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Domain.Entities.Geography;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.UnitTests.Services
{
    [TestClass]
    public class CountyServiceTests
    {
        private Mock<ICountyRepository> _countyRepository = null!;
        private Mock<IGenericRepository<Country>> _countryRepository = null!;
        private Mock<ICityRepository> _cityRepository = null!;
        private IMapper _mapper = null!;
        private CountyService _countyService = null!;

        [TestInitialize]
        public void Setup()
        {
            _countyRepository = new Mock<ICountyRepository>();
            _countryRepository = new Mock<IGenericRepository<Country>>();
            _cityRepository = new Mock<ICityRepository>();

            var mapperConfig = new MapperConfiguration(
                cfg => cfg.AddProfile<GeographyMappingProfile>(),
                NullLoggerFactory.Instance);

            _mapper = mapperConfig.CreateMapper();

            _countyService = new CountyService(
                _countyRepository.Object,
                _countryRepository.Object,
                _cityRepository.Object,
                _mapper,
                NullLogger<CountyService>.Instance);
        }

        [TestMethod]
        public async Task GetAllCounties_WhenNoCounties_ReturnsEmptyList()
        {
            _countyRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<County> { Items = new List<County>(), PageNumber = 1, PageSize = 10, TotalCount = 0 });

            var result = await _countyService.GetAllCountiesAsync(1, 10);

            Assert.AreEqual(0, result.Items.Count);
        }

        [TestMethod]
        public async Task GetAllCounties_WhenCountyExists_ReturnsCounty()
        {
            var county = new County { Name = "Cluj", CountryId = Guid.NewGuid() };
            _countyRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<County> { Items = new List<County> { county }, PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var result = await _countyService.GetAllCountiesAsync(1, 10);

            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("Cluj", result.Items[0].Name);
            Assert.AreEqual(county.Id, result.Items[0].Id);
        }

        [TestMethod]
        public async Task GetCountyByIdAsync_WhenCountyDoesNotExist_ReturnsNull()
        {
            var id = Guid.NewGuid();
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _countyService.GetCountyByIdAsync(id));
        }

        [TestMethod]
        public async Task GetCountyByIdAsync_WhenCountyDoesExists_ReturnsCounty()
        {
            var id = Guid.NewGuid();
            var county = new County { Id = id, Name = "Cluj", CountryId = Guid.NewGuid() };
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)county);

            var result = await _countyService.GetCountyByIdAsync(id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cluj", result.Name);
            Assert.AreEqual(county.Id, result.Id);
        }

        [TestMethod]
        public async Task CreateCountyAsync_WhenNameIsEmpty_ThrowsValidationException()
        {
            var request = new CreateCountyDto { Name = "", CountryId = Guid.NewGuid() };

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _countyService.CreateCountyAsync(request));

            _countyRepository.Verify(
                r => r.AddAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateCountyAsync_WhenCountryDoesNotExist_ThrowsValidationException()
        {
            var request = new CreateCountyDto { Name = "Cluj", CountryId = Guid.NewGuid() };
            _countryRepository
                .Setup(r => r.GetAsync(request.CountryId))
                .ReturnsAsync((Country?)null);

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _countyService.CreateCountyAsync(request));

            _countyRepository.Verify(
                r => r.AddAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateCountyAsync_WhenValid_CreatesCounty()
        {
            var country = new Country { Name = "Romania" };
            var request = new CreateCountyDto { Name = "Cluj", CountryId = country.Id };
            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);
            _countyRepository
                .Setup(r => r.AddAsync(It.IsAny<County>()))
                .ReturnsAsync((County county) => county);

            var result = await _countyService.CreateCountyAsync(request);

            Assert.AreEqual("Cluj", result.Name);
            _countyRepository.Verify(
                r => r.AddAsync(It.Is<County>(c => c.Name == "Cluj" && c.CountryId == country.Id)),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateCountyAsync_WhenCountyDoesNotExist_ReturnsNull()
        {
            var id = Guid.NewGuid();
            var request = new UpdateCountyDto { Name = "Cluj", CountryId = Guid.NewGuid() };
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _countyService.UpdateCountyAsync(id, request));
            _countyRepository.Verify(
                r => r.UpdateAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateCountyAsync_WhenValid_UpdatesCounty()
        {
            var country = new Country { Name = "Romania" };
            var county = new County { Name = "Old Cluj", CountryId = country.Id };
            var request = new UpdateCountyDto { Name = "Cluj", CountryId = country.Id };
            _countyRepository
                .Setup(r => r.GetAsync(county.Id))
                .ReturnsAsync(county);
            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);

            var result = await _countyService.UpdateCountyAsync(county.Id, request);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cluj", result.Name);
            _countyRepository.Verify(
                r => r.UpdateAsync(county),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteCountyAsync_WhenCountyDoesNotExist_ReturnsFalse()
        {
            var id = Guid.NewGuid();
            _countyRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((County?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _countyService.DeleteCountyAsync(id));
            _countyRepository.Verify(
                r => r.DeleteAsync(It.IsAny<County>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteCountyAsync_WhenCountyExists_ReturnsTrue()
        {
            var county = new County { Name = "Cluj", CountryId = Guid.NewGuid() };
            _countyRepository
                .Setup(r => r.GetAsync(county.Id))
                .ReturnsAsync(county);

            await _countyService.DeleteCountyAsync(county.Id);
            _countyRepository.Verify(
                r => r.DeleteAsync(county),
                Times.Once);
        }
    }
}
