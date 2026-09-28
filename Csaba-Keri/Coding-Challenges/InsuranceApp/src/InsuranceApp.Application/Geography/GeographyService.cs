using FluentValidation;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Geography.Mappings;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Domain.Geography;

namespace InsuranceApp.Application.Geography;

public class GeographyService : IGeographyService
{
    private readonly IGeographyRepository _geographyRepository;
    private readonly IValidator<PageQuery> _pageValidator;

    public GeographyService(IGeographyRepository geographyRepository, IValidator<PageQuery> pageValidator)
    {
        ArgumentNullException.ThrowIfNull(geographyRepository);
        ArgumentNullException.ThrowIfNull(pageValidator);

        _geographyRepository = geographyRepository;
        _pageValidator = pageValidator;
    }

    public async Task<PagedResult<CountryResult>> GetCountriesAsync(PageQuery query, CancellationToken cancellationToken)
    {
        await ValidatePageAsync(query, cancellationToken);

        var page = await _geographyRepository.GetCountriesAsync(query, cancellationToken);

        return page.Map(country => country.ToResult());
    }

    public async Task<PagedResult<CountyResult>> GetCountiesByCountryIdAsync(Guid countryId, PageQuery query, CancellationToken cancellationToken)
    {
        await ValidatePageAsync(query, cancellationToken);

        if (countryId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("CountryId", "Country identifier must not be empty.");
        }

        if (!await _geographyRepository.CountryExistsAsync(countryId, cancellationToken))
        {
            throw new EntityNotFoundException(nameof(Country), countryId);
        }

        var page = await _geographyRepository.GetCountiesByCountryIdAsync(countryId, query, cancellationToken);

        return page.Map(county => county.ToResult());
    }

    public async Task<PagedResult<CityResult>> GetCitiesByCountyIdAsync(Guid countyId, PageQuery query, CancellationToken cancellationToken)
    {
        await ValidatePageAsync(query, cancellationToken);

        if (countyId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("CountyId", "County identifier must not be empty.");
        }

        if (!await _geographyRepository.CountyExistsAsync(countyId, cancellationToken))
        {
            throw new EntityNotFoundException(nameof(County), countyId);
        }

        var page = await _geographyRepository.GetCitiesByCountyIdAsync(countyId, query, cancellationToken);

        return page.Map(city => city.ToResult());
    }

    private async Task ValidatePageAsync(PageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        await _pageValidator.ValidateAndThrowAsync(query, cancellationToken);
    }
}
