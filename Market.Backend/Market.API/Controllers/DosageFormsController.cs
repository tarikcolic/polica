using Market.Application.Modules.CodeLists.DosageForms.Commands.Create;
using Market.Application.Modules.CodeLists.DosageForms.Commands.Delete;
using Market.Application.Modules.CodeLists.DosageForms.Commands.Update;
using Market.Application.Modules.CodeLists.DosageForms.Queries.GetById;
using Market.Application.Modules.CodeLists.DosageForms.Queries.List;
using Market.Application.Modules.CodeLists.DosageForms.Queries.Lookup;
using Market.Shared.Constants;
using Market.Shared.Dtos;

namespace Market.API.Controllers;

/// <summary>
/// Polica (#6): the dosage forms code list. Same structure and authorization as ManufacturersController:
/// reads for any logged-in user (fallback policy), writes only with the "Admin" policy (403 for others).
/// </summary>
[ApiController]
[Route("api/dosage-forms")]
public sealed class DosageFormsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// One page of dosage forms, sorted by name, optionally filtered by part of the name.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PageResult<ListDosageFormsQueryDto>), StatusCodes.Status200OK)]
    public async Task<PageResult<ListDosageFormsQueryDto>> List([FromQuery] ListDosageFormsQuery query, CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }

    /// <summary>
    /// All dosage forms as (id, name) for dropdowns, sorted by name. Not paged.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<DosageFormLookupQueryDto>), StatusCodes.Status200OK)]
    public async Task<List<DosageFormLookupQueryDto>> Lookup(CancellationToken ct)
    {
        return await sender.Send(new GetDosageFormsLookupQuery(), ct);
    }

    /// <summary>
    /// One dosage form; 404 if it does not exist.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetDosageFormByIdQueryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<GetDosageFormByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        return await sender.Send(new GetDosageFormByIdQuery { Id = id }, ct);
    }

    /// <summary>
    /// Creates a dosage form (administrator). 201 with { id }; a duplicate name returns 400 on "name".
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Create([FromBody] CreateDosageFormCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Renames a dosage form (administrator). 204 on success.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Update(int id, [FromBody] UpdateDosageFormCommand command, CancellationToken ct)
    {
        command.Id = id; // route Id wins; the body cannot set it
        await sender.Send(command, ct);
    }

    /// <summary>
    /// Deletes a dosage form (administrator, soft delete). 204 on success.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteDosageFormCommand { Id = id }, ct);
    }
}
