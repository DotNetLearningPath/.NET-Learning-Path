using FluentValidation;
using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Application.Buildings.Mappings;
using InsuranceApp.Application.Buildings.Results;
using InsuranceApp.Application.Clients;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Geography;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Clients;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Buildings;

public class BuildingService : IBuildingService
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IGeographyRepository _geographyRepository;
    private readonly IValidator<CreateBuildingCommand> _createValidator;
    private readonly IValidator<UpdateBuildingCommand> _updateValidator;
    private readonly IValidator<PageQuery> _pageValidator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(
        IBuildingRepository buildingRepository,
        IClientRepository clientRepository,
        IGeographyRepository geographyRepository,
        IValidator<CreateBuildingCommand> createValidator,
        IValidator<UpdateBuildingCommand> updateValidator,
        IValidator<PageQuery> pageValidator,
        TimeProvider timeProvider,
        ILogger<BuildingService> logger
    )
    {
        ArgumentNullException.ThrowIfNull(buildingRepository);
        ArgumentNullException.ThrowIfNull(clientRepository);
        ArgumentNullException.ThrowIfNull(geographyRepository);
        ArgumentNullException.ThrowIfNull(createValidator);
        ArgumentNullException.ThrowIfNull(updateValidator);
        ArgumentNullException.ThrowIfNull(pageValidator);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _buildingRepository = buildingRepository;
        _clientRepository = clientRepository;
        _geographyRepository = geographyRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _pageValidator = pageValidator;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<BuildingDetailsResult> GetBuildingDetailsByIdAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (buildingId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("BuildingId", "Building identifier must not be empty.");
        }

        var building = await GetBuildingByIdOrThrowAsync(buildingId, cancellationToken);

        var geography = await _geographyRepository.GetCityGeographyAsync(building.Address.CityId, cancellationToken)
            ?? throw new InvalidOperationException("The stored building has no valid geography.");

        return building.ToDetailsResult(geography);
    }

    public async Task<PagedResult<BuildingResult>> GetBuildingsByClientIdAsync(Guid clientId, PageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        await _pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        if (clientId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("ClientId", "Client identifier must not be empty.");
        }

        await EnsureClientExistsAsync(clientId, cancellationToken);

        var page = await _buildingRepository.GetBuildingsByClientIdAsync(clientId, query, cancellationToken);

        return page.Map(building => building.ToResult());
    }

    public async Task<BuildingDetailsResult> CreateBuildingAsync(CreateBuildingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        ValidateConstructionYear(command.ConstructionYear);
        await EnsureClientExistsAsync(command.ClientId, cancellationToken);

        var address = command.Address!;
        var geography = await GetRequiredCityGeographyAsync(address.CityId, cancellationToken);

        var building = new Building(
            id: Guid.NewGuid(),
            clientId: command.ClientId,
            type: command.Type,
            address: address.ToDomain(),
            constructionYear: command.ConstructionYear,
            numberOfFloors: command.NumberOfFloors,
            surfaceArea: command.SurfaceArea,
            insuredValue: command.InsuredValue
        );

        await _buildingRepository.AddBuildingAsync(building, cancellationToken);
        _logger.LogInformation("Building {BuildingId} created.", building.Id);

        return building.ToDetailsResult(geography);
    }

    public async Task<BuildingDetailsResult> UpdateBuildingAsync(UpdateBuildingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        ValidateConstructionYear(command.ConstructionYear);

        var building = await GetBuildingByIdOrThrowAsync(command.BuildingId, cancellationToken);

        var address = command.Address!;
        var geography = await GetRequiredCityGeographyAsync(address.CityId, cancellationToken);

        building.UpdateDetails(
            type: command.Type,
            address: address.ToDomain(),
            constructionYear: command.ConstructionYear,
            numberOfFloors: command.NumberOfFloors,
            surfaceArea: command.SurfaceArea,
            insuredValue: command.InsuredValue
        );

        await _buildingRepository.UpdateBuildingAsync(building, cancellationToken);
        _logger.LogInformation("Building {BuildingId} updated.", building.Id);

        return building.ToDetailsResult(geography);
    }

    private void ValidateConstructionYear(int constructionYear)
    {
        var currentYear = _timeProvider.GetUtcNow().Year;

        if (constructionYear > currentYear)
        {
            throw ValidationExceptionFactory.Create("ConstructionYear", $"Construction year must not exceed {currentYear}.");
        }
    }

    private async Task EnsureClientExistsAsync(Guid clientId, CancellationToken cancellationToken)
    {
        if (!await _clientRepository.ClientExistsByIdAsync(clientId, cancellationToken))
        {
            throw new EntityNotFoundException(nameof(Client), clientId);
        }
    }

    private async Task<Building> GetBuildingByIdOrThrowAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        return await _buildingRepository.GetBuildingByIdAsync(buildingId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Building), buildingId);
    }

    private async Task<CityGeographyResult> GetRequiredCityGeographyAsync(Guid cityId, CancellationToken cancellationToken)
    {
        return await _geographyRepository.GetCityGeographyAsync(cityId, cancellationToken)
            ?? throw ValidationExceptionFactory.Create("Address.CityId", "The selected city does not exist.");
    }
}
