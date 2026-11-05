using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> GetAll(
        [FromQuery] string? name,
        [FromQuery] string? identifier) =>
        Ok(await clientService.GetAllAsync(name, identifier));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDetailsDto>> GetById(Guid id)
    {
        var client = await clientService.GetAsync(id);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create(CreateClientDto createClient)
    {
        var client = await clientService.CreateAsync(createClient);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, UpdateClientDto updateClient)
    {
        var client = await clientService.UpdateAsync(id, updateClient);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await clientService.DeleteAsync(id) ? NoContent() : NotFound();
}
