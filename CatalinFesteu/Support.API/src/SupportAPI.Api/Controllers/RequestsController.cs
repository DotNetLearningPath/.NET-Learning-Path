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
    public class RequestsController(
        ICommandHandler<CreateUserRequestCommand,UserRequestResult> commandHandler,
        IQueryHandler<GetRequestByIdQuery, UserRequestResult> queryHandler): ControllerBase
    {
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(UserRequestResult), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
          [FromBody] CreateRequest body,
          CancellationToken cancellationToken)
        {
            var command = new CreateUserRequestCommand(body.Title, body.RequestContent);
            var result = await commandHandler.Handle(command, cancellationToken);
            return CreatedAtAction
                (nameof(Get),
                new {requestId = result.RequestId},
                result);
        }

        [Authorize]
        [HttpGet("{requestId:guid}")]
        [ProducesResponseType(typeof(UserRequestResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(
            [FromRoute] Guid requestId,
            CancellationToken cancellationToken)
        {
            var query = new GetRequestByIdQuery(requestId);
            var result = await queryHandler.Handle(query, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
