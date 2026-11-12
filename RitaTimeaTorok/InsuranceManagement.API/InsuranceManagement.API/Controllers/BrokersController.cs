using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.Constants;
using InsuranceManagement.Application.DTOs.Broker;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin/brokers")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BrokersController(IBrokerService brokerService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<BrokerDto>>> GetAll(
            [FromQuery, Range(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)]
                int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery, Range(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)]
                int pageSize = PaginationLimits.DefaultPageSize) =>
            Ok(await brokerService.GetAllBrokersAsync(pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BrokerDto>> GetById(Guid id) =>
            Ok(await brokerService.GetBrokerByIdAsync(id));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BrokerDto>> Create(CreateBrokerDto request)
        {
            var broker = await brokerService.CreateBrokerAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = broker.Id }, broker);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BrokerDto>> Update(Guid id, UpdateBrokerDto request) =>
            Ok(await brokerService.UpdateBrokerAsync(id, request));
    }
}
