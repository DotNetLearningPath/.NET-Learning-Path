using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using InsuranceManagement.Application.Constants;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/clients")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ClientsController(IClientService clientService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ClientDto>>> GetAll(
            [FromQuery, Range(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)]
                int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery, Range(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)]
                int pageSize = PaginationLimits.DefaultPageSize,
            [FromQuery] string? name = null,
            [FromQuery] string? identifier = null) =>
            Ok(await clientService.GetAllClientsAsync(pageNumber, pageSize, name, identifier));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientDetailsDto>> GetById(Guid id)
        {
            var client = await clientService.GetClientByIdAsync(id);
            return Ok(client);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ClientDto>> Create(CreateClientDto createClient)
        {
            var client = await clientService.CreateClientAsync(createClient);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ClientDto>> Update(Guid id, UpdateClientDto updateClient)
        {
            var client = await clientService.UpdateClientAsync(id, updateClient);
            return Ok(client);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await clientService.DeleteClientAsync(id);
            return NoContent();
        }
    }
}
