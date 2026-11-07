using FluentValidation.Results;

namespace Market.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs all FluentValidation validators for the request
/// before the handler. Any failure is thrown as <see cref="ValidationException"/>
/// (mapped to HTTP 400 "validation.error" by the global exception handler).
/// </summary>
/// <remarks>
/// Rule used in this project (validator vs handler):
/// <list type="bullet">
/// <item>A validator checks only the request itself: required fields, lengths, formats, ranges and relations
/// between fields. It never reads the database, so it has no <c>IAppDbContext</c>.</item>
/// <item>Rules that depend on data in the database are checked in the handler: a missing record throws
/// <c>MarketNotFoundException</c> (404), a duplicate or another conflict with existing data throws
/// <c>MarketConflictException</c> or <c>MarketBusinessRuleException</c> (409).</item>
/// <item>A unique index in the database stays as the last guard: two simultaneous requests can both pass the
/// check in the handler, and the second insert then fails with <c>DbUpdateException</c> (409).</item>
/// </list>
/// Why: the status code keeps its meaning (400 = bad request, 404/409 = conflict with the current data),
/// a record is read once (in the handler, where it is also used), and validators stay easy to test.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = new List<ValidationFailure>();

        // One ValidationContext is shared by all validators, so they run one after another
        // (ValidationContext is not thread-safe and must not be used concurrently).
        var context = new ValidationContext<TRequest>(request);
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, ct);
            failures.AddRange(result.Errors);
        }

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
