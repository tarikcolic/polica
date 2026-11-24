using Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Create;
using Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Delete;
using Market.Application.Modules.CodeLists.WriteOffReasons.Commands.Update;
using Market.Application.Modules.CodeLists.WriteOffReasons.Queries.GetById;
using Market.Application.Modules.CodeLists.WriteOffReasons.Queries.List;
using Market.Application.Modules.CodeLists.WriteOffReasons.Queries.Lookup;
using Market.Shared.Constants;
using Market.Shared.Dtos;

namespace Market.API.Controllers;

/// <summary>
/// Polica (#6): the write-off reasons code list. Same structure and authorization as ManufacturersController:
/// reads for any logged-in user (fallback policy; pharmacists read the lookup in #20), writes only with the
/// "Admin" policy (403 for others).
/// </summary>
[ApiController]
[Route("api/write-off-reasons")]
public sealed class WriteOffReasonsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// One page of write-off reasons, sorted by name, optionally filtered by part of the name.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PageResult<ListWriteOffReasonsQueryDto>), StatusCodes.Status200OK)]
    public async Task<PageResult<ListWriteOffReasonsQueryDto>> List([FromQuery] ListWriteOffReasonsQuery query, CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }

    /// <summary>
    /// All write-off reasons as (id, name, requiresNote) for dropdowns, sorted by name. Not paged.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<WriteOffReasonLookupQueryDto>), StatusCodes.Status200OK)]
    public async Task<List<WriteOffReasonLookupQueryDto>> Lookup(CancellationToken ct)
    {
        return await sender.Send(new GetWriteOffReasonsLookupQuery(), ct);
    }

    /// <summary>
    /// One write-off reason; 404 if it does not exist.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetWriteOffReasonByIdQueryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<GetWriteOffReasonByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        return await sender.Send(new GetWriteOffReasonByIdQuery { Id = id }, ct);
    }

    /// <summary>
    /// Creates a write-off reason (administrator). 201 with { id }; a duplicate name returns 400 on "name".
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Create([FromBody] CreateWriteOffReasonCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Updates a write-off reason (administrator). 204 on success.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Update(int id, [FromBody] UpdateWriteOffReasonCommand command, CancellationToken ct)
    {
        command.Id = id; // route Id wins; the body cannot set it
        await sender.Send(command, ct);
    }

    /// <summary>
    /// Deletes a write-off reason (administrator, soft delete). 204 on success.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteWriteOffReasonCommand { Id = id }, ct);
    }
}
