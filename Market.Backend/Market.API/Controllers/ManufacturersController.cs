using Market.Application.Modules.CodeLists.Manufacturers.Commands.Create;
using Market.Application.Modules.CodeLists.Manufacturers.Commands.Delete;
using Market.Application.Modules.CodeLists.Manufacturers.Commands.Update;
using Market.Application.Modules.CodeLists.Manufacturers.Queries.GetById;
using Market.Application.Modules.CodeLists.Manufacturers.Queries.List;
using Market.Application.Modules.CodeLists.Manufacturers.Queries.Lookup;
using Market.Shared.Constants;
using Market.Shared.Dtos;

namespace Market.API.Controllers;

/// <summary>
/// Polica (#6): the manufacturers code list.
/// </summary>
/// <remarks>
/// The controller has no logic: every action builds a command or query and sends it through MediatR
/// (ISender), which runs the validator and then the handler in the Application layer.
/// Authorization:
/// <list type="bullet">
/// <item>Reads (list, lookup, get by id): any logged-in user. No attribute is needed, because the fallback
/// policy in Market.API/DependencyInjection.cs already requires an authenticated user. Managers will read
/// the lookup in #10 (medication form).</item>
/// <item>Writes (create, update, delete): chain administrator only, through the "Admin" policy
/// (claim is_admin = true): 401 without a token, 403 for a manager or pharmacist. The handlers check it again.</item>
/// </list>
/// Exceptions from the handlers become HTTP responses in MarketExceptionHandler
/// (ValidationException → 400, MarketForbiddenException → 403, MarketNotFoundException → 404).
/// </remarks>
[ApiController]
[Route("api/manufacturers")]
public sealed class ManufacturersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// One page of manufacturers, sorted by name, optionally filtered by part of the name.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PageResult<ListManufacturersQueryDto>), StatusCodes.Status200OK)]
    public async Task<PageResult<ListManufacturersQueryDto>> List([FromQuery] ListManufacturersQuery query, CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }

    /// <summary>
    /// All manufacturers as (id, name) for dropdowns, sorted by name. Not paged.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<ManufacturerLookupQueryDto>), StatusCodes.Status200OK)]
    public async Task<List<ManufacturerLookupQueryDto>> Lookup(CancellationToken ct)
    {
        return await sender.Send(new GetManufacturersLookupQuery(), ct);
    }

    /// <summary>
    /// One manufacturer; 404 if it does not exist.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetManufacturerByIdQueryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<GetManufacturerByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        return await sender.Send(new GetManufacturerByIdQuery { Id = id }, ct);
    }

    /// <summary>
    /// Creates a manufacturer (administrator). 201 with { id } and a Location header pointing to GET {id}.
    /// A duplicate name returns 400 with a field error on "name".
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Create([FromBody] CreateManufacturerCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Updates a manufacturer (administrator). 204 on success.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Update(int id, [FromBody] UpdateManufacturerCommand command, CancellationToken ct)
    {
        // The Id comes from the route; the body cannot set it ([JsonIgnore] on the command).
        command.Id = id;
        await sender.Send(command, ct);
        // no return value → 204 No Content (EmptyResultToNoContentFilter)
    }

    /// <summary>
    /// Deletes a manufacturer (administrator, soft delete). 204 on success.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteManufacturerCommand { Id = id }, ct);
    }
}
