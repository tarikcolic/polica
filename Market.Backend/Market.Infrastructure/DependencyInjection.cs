using Market.Application.Abstractions;
using Market.Infrastructure.Common;
using Market.Infrastructure.Database;
using Market.Infrastructure.Email;
using Market.Shared.Constants;
using Market.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Market.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Typed ConnectionStrings + validation
        services.AddOptions<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // DbContext: InMemory for automated integration tests; SQL Server otherwise
        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            if (env.IsIntegrationTests())
            {
                options.UseInMemoryDatabase("IntegrationTestsDb");

                return;
            }

            var cs = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Main;
            options.UseSqlServer(cs);
        });

        // IAppDbContext mapping
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<DatabaseContext>());

        // Identity hasher
        services.AddScoped<IPasswordHasher<MarketUserEntity>, PasswordHasher<MarketUserEntity>>();

        // Token service (reads JwtOptions via IOptions<JwtOptions>)
        services.AddTransient<IJwtTokenService, JwtTokenService>();

        // HttpContext accessor + current user
        services.AddHttpContextAccessor();
        services.AddScoped<IAppCurrentUser, AppCurrentUser>();

        // Polica (#5): regional settings ("Regional" section: culture bs-BA, time zone Europe/Sarajevo).
        // Read here by the clock below and in Program.cs by UseRequestLocalization.
        services.AddOptions<RegionalOptions>()
            .Bind(configuration.GetSection(RegionalOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // The application clock: system UTC time + the Sarajevo time zone for "today" (see AppTimeProvider).
        // The only TimeProvider registration; the starter registered TimeProvider.System here and again in
        // Market.Application/DependencyInjection.cs, and the later registration silently won.
        // A factory (sp => ...) because the time zone comes from options, which exist only once the
        // container is built. IOptions.Value runs the [Required] validation of RegionalOptions.
        services.AddSingleton<TimeProvider>(sp =>
            new AppTimeProvider(sp.GetRequiredService<IOptions<RegionalOptions>>().Value.TimeZoneId));

        AddEmail(services, configuration, env);

        return services;
    }

    /// <summary>
    /// Emails ("Email:Smtp" section) and links in them ("Frontend:BaseUrl").
    /// <list type="bullet">
    /// <item>Email:Smtp:Host set → <see cref="SmtpEmailSender"/> (real emails).</item>
    /// <item>Email:Smtp:Host empty → <see cref="LoggingEmailSender"/>: emails are only written to the log.
    /// Allowed only in Development, Testing and IntegrationTests (see <see cref="EnsureEmailIsConfigured"/>).</item>
    /// </list>
    /// </summary>
    private static void AddEmail(IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        EnsureEmailIsConfigured(configuration, env);

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FrontendOptions>()
            .Bind(configuration.GetSection(FrontendOptions.SectionName));

        var smtpHost = configuration[$"{EmailOptions.SectionName}:{nameof(EmailOptions.Host)}"];

        if (string.IsNullOrWhiteSpace(smtpHost))
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        else
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
    }

    /// <summary>
    /// Stops the startup (before the database is touched), like the Jwt:Key check in Market.API, when the application
    /// runs outside Development, Testing and IntegrationTests and:
    /// <list type="bullet">
    /// <item>Email:Smtp:Host is empty — emails (with password reset links) would end up in the log instead of the inbox,</item>
    /// <item>Frontend:BaseUrl is empty — links in emails would not point to the application.</item>
    /// </list>
    /// </summary>
    private static void EnsureEmailIsConfigured(IConfiguration configuration, IHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsTesting() || env.IsIntegrationTests())
            return;

        if (string.IsNullOrWhiteSpace(configuration[$"{EmailOptions.SectionName}:{nameof(EmailOptions.Host)}"]))
        {
            throw new InvalidOperationException(
                $"Email:Smtp:Host nije postavljen u okruženju '{env.EnvironmentName}'. " +
                $"Postavite SMTP server (npr. varijabla okruženja Email__Smtp__Host); bez njega se emailovi samo ispisuju u log. / " +
                $"Email:Smtp:Host is not set in the '{env.EnvironmentName}' environment. " +
                $"Configure an SMTP server (e.g. environment variable Email__Smtp__Host); without it emails are only written to the log.");
        }

        if (string.IsNullOrWhiteSpace(configuration[$"{FrontendOptions.SectionName}:{nameof(FrontendOptions.BaseUrl)}"]))
        {
            throw new InvalidOperationException(
                $"Frontend:BaseUrl nije postavljen u okruženju '{env.EnvironmentName}'. " +
                $"Postavite adresu frontenda (npr. varijabla okruženja Frontend__BaseUrl) za linkove u emailovima. / " +
                $"Frontend:BaseUrl is not set in the '{env.EnvironmentName}' environment. " +
                $"Configure the frontend address (e.g. environment variable Frontend__BaseUrl) for links in emails.");
        }
    }
}
