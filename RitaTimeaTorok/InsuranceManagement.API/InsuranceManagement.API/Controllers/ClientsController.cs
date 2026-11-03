using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await clientService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await clientService.GetAsync(id);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClientDto createClient)
    {
        try
        {
            var client = await clientService.CreateAsync(createClient);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateClientDto updateClient)
    {
        try
        {
            var client = await clientService.UpdateAsync(id, updateClient);
            return client is null ? NotFound() : Ok(client);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await clientService.DeleteAsync(id) ? NoContent() : NotFound();
}
