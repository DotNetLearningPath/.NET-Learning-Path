using Application.Abstractions;
using Application.DTO.Clients;
using Application.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Route("api/brokers/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet("{clientId:guid}", Name = nameof(GetClientByIdAsync))]
    public async Task<ActionResult<ClientDto>> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetClientByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClientDto>>> SearchClientAsync(
    [FromQuery] string? name,
    [FromQuery] string? identifier,
    [FromQuery] PaginationRequest pagination,
    CancellationToken cancellationToken)
    {
        var clients = await _clientService.SearchClientAsync(
            name,
            identifier,
            pagination,
            cancellationToken);

        return Ok(clients);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClientAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.CreateClientAsync(request, cancellationToken);

        return CreatedAtRoute(
            nameof(GetClientByIdAsync),
            new { clientId = client.Id },
            client);
    }

    [HttpPut("{clientId:guid}")]
    public async Task<ActionResult<ClientDto>> UpdateClientAsync(
        Guid clientId,
        UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.UpdateClientAsync(clientId, request, cancellationToken);

        return Ok(client);
    }
}
