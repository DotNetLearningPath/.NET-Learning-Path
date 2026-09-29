using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Client;
using Microsoft.AspNetCore.Mvc;
using InsuranceApp.Application.Common;

namespace InsuranceApp.Api.Controllers.Broker;

/// <summary>
/// Controller for managing clients in the broker context.
/// </summary>
/// <param name="clientService">The client service.</param>
[ApiController]
[Route("api/brokers/clients")]
public sealed class ClientsController(IClientService clientService) : ControllerBase
{

    /// <summary>
    /// Searches for clients based on the provided search criteria.
    /// </summary>
    /// <param name="search">The search criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result of clients.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ClientDto>>> SearchClientsAsync([FromQuery] ClientSearchDto search, CancellationToken cancellationToken)
    {
        var result = await clientService.SearchClientsAsync(search, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a client by identifier.
    /// </summary>
    [HttpGet("{clientId:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetClientByIdAsync(int clientId, CancellationToken cancellationToken)
    {
        var result = await clientService.GetClientByIdAsync(clientId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new client.
    /// </summary>
    /// <param name="request">The client creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created client.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientDto>> CreateClientAsync(CreateClientDto request, CancellationToken cancellationToken)
    {
        var result = await clientService.CreateClientAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="request">The client update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated client.</returns>
    [HttpPut("{clientId:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> UpdateClientAsync(int clientId, UpdateClientDto request, CancellationToken cancellationToken)
    {
        var result = await clientService.UpdateClientAsync(clientId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }
}