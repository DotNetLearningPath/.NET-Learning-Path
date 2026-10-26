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
        public async Task<IActionResult> GetByIdAsync(Guid clientId, CancellationToken cancellationToken)
        {
            var client = await _clientService.GetByIdAsync(clientId, cancellationToken);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client);
        }

        [HttpGet]
        public async Task<IActionResult> SearchAsync([FromQuery] string? name, [FromQuery] string? identifier, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var clients = await _clientService.SearchAsync(
                name,
                identifier,
                page,
                pageSize,
                cancellationToken);

            return Ok(clients);
        }

        [HttpPost]
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

            return CreatedAtRoute("GetClientById", new { clientId = createdClient.Id }, createdClient);
        }

        [HttpPut("{clientId:guid}")]
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

            return Ok(updatedClient);
        }
    }
}
