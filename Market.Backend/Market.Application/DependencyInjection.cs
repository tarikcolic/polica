using Market.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Market.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR only from the Application layer
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation from the Application layer
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline behaviors (npr. ValidationBehavior)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Polica (#5): TimeProvider is no longer registered here. Handlers still inject it; the single
        // registration (AppTimeProvider, system clock + Sarajevo time zone) is in Market.Infrastructure.

        return services;
    }
}