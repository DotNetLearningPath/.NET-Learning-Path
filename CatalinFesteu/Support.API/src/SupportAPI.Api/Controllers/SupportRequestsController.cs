using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportAPI.Api.Requests;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Queries;
using SupportAPI.Application.Results;

namespace SupportAPI.Api.Controllers
{
    [ApiController]
    [Route("api/requests")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public class SupportRequestsController(
        ICommandHandler<CreateSupportRequestCommand, SupportRequestResult> commandHandler,
        IQueryHandler<GetSupportRequestByIdQuery, SupportRequestResult> queryHandler) : ControllerBase
    {
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(SupportRequestResult), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
          [FromBody] CreateSupportRequest body,
          CancellationToken cancellationToken)
        {
            var command = new CreateSupportRequestCommand(body.Title, body.RequestContent);
            var result = await commandHandler.Handle(command, cancellationToken);
            return CreatedAtAction
                (nameof(Get),
                new { requestId = result.RequestId },
                result);
        }

        [Authorize]
        [HttpGet("{requestId:guid}")]
        [ProducesResponseType(typeof(SupportRequestResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromRoute] Guid requestId,
            CancellationToken cancellationToken)
        {
            var query = new GetSupportRequestByIdQuery(requestId);
            var result = await queryHandler.Handle(query, cancellationToken);
            return Ok(result);
        }
    }
}
