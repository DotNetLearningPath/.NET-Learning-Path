using InsuranceApp.Application.Clients;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Clients;
using InsuranceApp.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/brokers/clients")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        ArgumentNullException.ThrowIfNull(clientService);

        _clientService = clientService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ClientResponse>>> SearchClients(
        [FromQuery] SearchClientsRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _clientService.SearchClientsAsync(request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(client => client.ToResponse()));
    }

    [HttpGet("{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetClientById(
        [FromRoute] Guid clientId,
        CancellationToken cancellationToken
    )
    {
        var result = await _clientService.GetClientByIdAsync(clientId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> CreateClient(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _clientService.CreateClientAsync(request.ToCommand(), cancellationToken);
        
        return CreatedAtAction(nameof(GetClientById), new { clientId = result.Id }, result.ToResponse());
    }

    [HttpPut("{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> UpdateClient(
        [FromRoute] Guid clientId,
        [FromBody] UpdateClientRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _clientService.UpdateClientAsync(request.ToCommand(clientId), cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
