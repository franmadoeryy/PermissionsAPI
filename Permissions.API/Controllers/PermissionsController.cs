using Elastic.Transport.Products.Elasticsearch;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Permissions.Application.Permissions.Commands;
using Permissions.Application.Permissions.Queries;

namespace Permissions.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PermissionsController> _logger;
        public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> RequestPermission([FromBody] RequestPermissionCommand command)
        {
            _logger.LogInformation("Entering RequestPermission endpoint...");
            if (command == null)
                return BadRequest("Invalid request data");
            

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(new { error = result.Error });


                return BadRequest(new { error = result.Error });
            }

            return CreatedAtAction("RequestPermission", result);
        }


        [HttpPut]
        public async Task<IActionResult> ModifyPermission([FromBody] ModifyPermissionCommand command)
        {
            _logger.LogInformation("Entering ModifyPermission endpoint...");

            if (command == null)
                return BadRequest("Invalid request data");

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(new { error = result.Error });

                return BadRequest(new { error = result.Error });
            }

            return Ok(result);
        }

        [HttpGet()]
        public async Task<IActionResult> GetPermissions()
        {
            _logger.LogInformation("Entering GetPermissions endpoint...");

            var result = await _mediator.Send(new GetAllPermissionsQuery());

            return Ok(result);
        }

    }
}
