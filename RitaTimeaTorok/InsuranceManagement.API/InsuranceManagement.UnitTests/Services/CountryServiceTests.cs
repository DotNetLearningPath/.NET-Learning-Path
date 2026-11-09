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
    public class CountryServiceTests
    {
        private Mock<IGenericRepository<Country>> _countryRepository = null!;
        private IMapper _mapper = null!;
        private CountryService _countryService = null!;

        [TestInitialize]
        public void Setup()
        {
            _countryRepository = new Mock<IGenericRepository<Country>>();

            var configuration = new MapperConfiguration(
                cfg => cfg.AddProfile<GeographyMappingProfile>(),
                NullLoggerFactory.Instance);

            _mapper = configuration.CreateMapper();

            _countryService = new CountryService(
                _countryRepository.Object,
                Mock.Of<ICountyRepository>(),
                _mapper,
                NullLogger<CountryService>.Instance);
        }

        [TestMethod]
        public async Task GetAllCountries_WhenNoCountries_ReturnsEmptyList()
        {
            _countryRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<Country> { Items = new List<Country>(), PageNumber = 1, PageSize = 10, TotalCount = 0 });

            var result = await _countryService.GetAllCountriesAsync(1, 10);

            Assert.AreEqual(0, result.Items.Count);
        }

        [TestMethod]
        public async Task GetAllCountries_WhenCountryExists_ReturnsCountry()
        {
            var country = new Country { Name = "Romania" };

            _countryRepository
                .Setup(r => r.GetPageAsync(1, 10))
                .ReturnsAsync(new PagedResult<Country> { Items = new List<Country> { country }, PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var result = await _countryService.GetAllCountriesAsync(1, 10);

            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("Romania", result.Items[0].Name);
            Assert.AreEqual(country.Id, result.Items[0].Id);
        }

        [TestMethod]
        public async Task GetCountryByIdAsync_WhenCountryDoesNotExist_ThrowsNotFound()
        {
            var id = Guid.NewGuid();

            _countryRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Country?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(
                () => _countryService.GetCountryByIdAsync(id));
        }

        [TestMethod]
        public async Task GetCountryByIdAsync_WhenCountryExists_ReturnsCountry()
        {
            var country = new Country { Name = "Romania" };

            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);

            var result = await _countryService.GetCountryByIdAsync(country.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(country.Id, result.Id);
            Assert.AreEqual("Romania", result.Name);
        }

        [TestMethod]
        public async Task CreateCountryAsync_WhenNameIsEmpty_ThrowsValidationException()
        {
            var request = new CreateCountryDto { Name = "" };

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _countryService.CreateCountryAsync(request));

            _countryRepository.Verify(
                r => r.AddAsync(It.IsAny<Country>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateCountryAsync_WhenValid_CreatesCountry()
        {
            var request = new CreateCountryDto { Name = "Romania" };

            _countryRepository
                .Setup(r => r.AddAsync(It.IsAny<Country>()))
                .ReturnsAsync((Country country) => country);

            var result = await _countryService.CreateCountryAsync(request);

            Assert.AreEqual("Romania", result.Name);
            Assert.AreNotEqual(Guid.Empty, result.Id);

            _countryRepository.Verify(
                r => r.AddAsync(It.Is<Country>(c => c.Name == "Romania")),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateCountryAsync_WhenCountryDoesNotExist_ThrowsNotFound()
        {
            var id = Guid.NewGuid();
            var request = new UpdateCountryDto { Name = "Romania" };

            _countryRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Country?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(
                () => _countryService.UpdateCountryAsync(id, request));

            _countryRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Country>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateCountryAsync_WhenNameIsEmpty_ThrowsValidationException()
        {
            var country = new Country { Name = "Romania" };
            var request = new UpdateCountryDto { Name = "" };

            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);

            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _countryService.UpdateCountryAsync(country.Id, request));

            _countryRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Country>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateCountryAsync_WhenValid_UpdatesCountry()
        {
            var country = new Country { Name = "Romania" };
            var request = new UpdateCountryDto { Name = "Updated Romania" };

            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);

            var result = await _countryService.UpdateCountryAsync(country.Id, request);

            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Romania", result.Name);

            _countryRepository.Verify(
                r => r.UpdateAsync(country),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteCountryAsync_WhenCountryDoesNotExist_ThrowsNotFound()
        {
            var id = Guid.NewGuid();

            _countryRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Country?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(
                () => _countryService.DeleteCountryAsync(id));

            _countryRepository.Verify(
                r => r.DeleteAsync(It.IsAny<Country>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteCountryAsync_WhenCountryExists_DeletesCountry()
        {
            var country = new Country { Name = "Romania" };

            _countryRepository
                .Setup(r => r.GetAsync(country.Id))
                .ReturnsAsync(country);

            await _countryService.DeleteCountryAsync(country.Id);

            _countryRepository.Verify(
                r => r.DeleteAsync(country),
                Times.Once);
        }
    }

}