using Market.Application.Modules.Pharmacies.Commands.Create;
using Market.Application.Modules.Pharmacies.Commands.SetActive;
using Market.Application.Modules.Pharmacies.Commands.Update;
using Market.Application.Modules.Pharmacies.Queries.GetById;
using Market.Application.Modules.Pharmacies.Queries.List;
using Market.Application.Modules.Pharmacies.Queries.Lookup;
using Market.Shared.Constants;
using Market.Shared.Dtos;

namespace Market.API.Controllers;

/// <summary>
/// Polica (#7): the pharmacy registry of the chain.
/// </summary>
/// <remarks>
/// No logic here: every action builds a command or query and sends it through MediatR (ISender), which runs the
/// validator and then the handler in the Application layer.
/// Authorization:
/// <list type="bullet">
/// <item>List, create, update, activate, deactivate: chain administrator only ("Admin" policy, claim
/// is_admin = true): 401 without a token, 403 for a manager or pharmacist. The handlers check it again.</item>
/// <item>Get by id: any logged-in user reaches the handler, and the HANDLER decides (#7: administrator; from #8:
/// also a manager or pharmacist for their own pharmacy).</item>
/// <item>Lookup: any logged-in user (dropdowns of other roles, e.g. the transfer destination in #18).</item>
/// </list>
/// No authorization attribute means "any logged-in user": the fallback policy in Market.API/DependencyInjection.cs
/// requires an authenticated user everywhere.
/// There is no DELETE: a pharmacy is deactivated instead, so its history stays readable.
/// </remarks>
[ApiController]
[Route("api/pharmacies")]
public sealed class PharmaciesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// One page of pharmacies, sorted by code; optional filters: search (name or code), cityId, isActive.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(typeof(PageResult<ListPharmaciesQueryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<PageResult<ListPharmaciesQueryDto>> List([FromQuery] ListPharmaciesQuery query, CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }

    /// <summary>
    /// The ACTIVE pharmacies as (id, code, name) for dropdowns, sorted by code. Not paged.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<PharmacyLookupQueryDto>), StatusCodes.Status200OK)]
    public async Task<List<PharmacyLookupQueryDto>> Lookup(CancellationToken ct)
    {
        return await sender.Send(new GetPharmaciesLookupQuery(), ct);
    }

    /// <summary>
    /// One pharmacy; 404 if it does not exist, 403 if the user may not see it (checked in the handler).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetPharmacyByIdQueryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<GetPharmacyByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        return await sender.Send(new GetPharmacyByIdQuery { Id = id }, ct);
    }

    /// <summary>
    /// Creates a pharmacy (administrator), always active. 201 with { id } and a Location header to GET {id}.
    /// A duplicate code returns 400 with a field error on "code".
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Create([FromBody] CreatePharmacyCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Updates a pharmacy's data (administrator). 204 on success. Does not change IsActive.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Update(int id, [FromBody] UpdatePharmacyCommand command, CancellationToken ct)
    {
        // The Id comes from the route; the body cannot set it ([JsonIgnore] on the command).
        command.Id = id;
        await sender.Send(command, ct);
        // no return value → 204 No Content (EmptyResultToNoContentFilter)
    }

    /// <summary>
    /// Activates a pharmacy (administrator). 204 on success, also when it was already active.
    /// </summary>
    [HttpPost("{id:int}/activate")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Activate(int id, CancellationToken ct)
    {
        await sender.Send(new SetPharmacyActiveCommand { Id = id, IsActive = true }, ct);
    }

    /// <summary>
    /// Deactivates a pharmacy (administrator): it disappears from the lookup (dropdowns). 204 on success.
    /// From #13 on: 409 while the pharmacy still holds stock.
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status409Conflict)]
    public async Task Deactivate(int id, CancellationToken ct)
    {
        await sender.Send(new SetPharmacyActiveCommand { Id = id, IsActive = false }, ct);
    }
}
