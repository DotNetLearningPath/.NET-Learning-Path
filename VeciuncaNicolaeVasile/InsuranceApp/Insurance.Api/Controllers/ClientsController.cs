using Insurance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Insurance.Api.DTOs;
using Insurance.Domain.Entities;

namespace Insurance.Api.Controllers
{
    [ApiController]
    [Route("api/brokers/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("{clientId:guid}", Name = "GetClientById")]
        [ProducesResponseType(typeof(ClientResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(Guid clientId, CancellationToken cancellationToken)
        {
            var client = await _clientService.GetByIdWithBuildingsAsync(clientId, cancellationToken);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(ToResponse(client));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ClientResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchAsync([FromQuery] string? name, [FromQuery] string? identifier, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var clients = await _clientService.SearchAsync(
                name,
                identifier,
                page,
                pageSize,
                cancellationToken);

            return Ok(clients.Select(ToResponse).ToList());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClientResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateClientDto request, CancellationToken cancellationToken)
        {
            var client = new Client
            {
                ClientType = request.ClientType,
                Name = request.Name,
                IdentificationNumber = request.IdentificationNumber,
                Email = request.Email,
                Phone = request.Phone,
                PrimaryAddress = request.PrimaryAddress
            };

            var createdClient = await _clientService.CreateAsync(client, cancellationToken);

            return CreatedAtRoute(
                "GetClientById",
                new { clientId = createdClient.Id },
                ToResponse(createdClient));
        }

        [HttpPut("{clientId:guid}")]
        [ProducesResponseType(typeof(ClientResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(Guid clientId, [FromBody] UpdateClientDto request, CancellationToken cancellationToken)
        {
            var client = new Client
            {
                Id = clientId,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PrimaryAddress = request.PrimaryAddress
            };

            var updatedClient = await _clientService.UpdateAsync(client, cancellationToken);

            if (updatedClient == null)
            {
                return NotFound();
            }

            return Ok(ToResponse(updatedClient));
        }

        private static ClientResponseDto ToResponse(Client client)
        {
            return new ClientResponseDto
            {
                Id = client.Id,
                ClientType = client.ClientType,
                Name = client.Name,
                IdentificationNumber = client.IdentificationNumber,
                Email = client.Email,
                Phone = client.Phone,
                PrimaryAddress = client.PrimaryAddress,
                Buildings = client.Buildings.Select(building => new ClientBuildingDto
                {
                    Id = building.Id,
                    Street = building.Street,
                    StreetNumber = building.StreetNumber,
                    CityId = building.CityId,
                    ConstructionYear = building.ConstructionYear,
                    BuildingType = building.BuildingType,
                    NumberOfFloors = building.NumberOfFloors,
                    SurfaceArea = building.SurfaceArea,
                    InsuredValue = building.InsuredValue
                }).ToList()
            };
        }
    }
}
