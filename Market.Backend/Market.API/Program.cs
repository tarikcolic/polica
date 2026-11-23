using Market.API;
using Market.API.Middleware;
using Market.Application;
using Market.Infrastructure;
using Market.Shared.Options;
using Microsoft.Extensions.Options;
using Serilog;

public partial class Program
{
    private const string CorsPolicyName = "Frontend";

    private static async Task Main(string[] args)
    {
        //
        // 0) Bootstrap logger (very early, no full config yet)
        //
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() // minimal sink so we see startup errors
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Market API...");

            //
            // 1) Standard builder (includes appsettings.json, appsettings.{ENV}.json,
            //    environment variables, user-secrets (Dev), and command-line args)
            //    Polica (#4): the starter also set WebRootPath = "wwwroot" here for uploaded product images.
            //    Product images were removed, so the API serves no static files and uses the default builder.
            //
            var builder = WebApplication.CreateBuilder(args);

            // 2) Promote Serilog to full configuration from builder.Configuration
            //    (reads "Serilog" section from appsettings + ENV overrides)
            //
            builder.Host.UseSerilog((ctx, services, cfg) =>
            {
                cfg.ReadFrom.Configuration(ctx.Configuration)   // Serilog section in appsettings
                   .ReadFrom.Services(services)                 // DI enrichers if any
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .Enrich.WithProcessId()
                   .Enrich.WithMachineName();
            });

            // Optional: remove default providers to have only Serilog
            builder.Logging.ClearProviders();

            // ---------------------------------------------------------
            // 3. Layer registrations
            // ---------------------------------------------------------
            builder.Services
                .AddAPI(builder.Configuration, builder.Environment)
                .AddInfrastructure(builder.Configuration, builder.Environment)
                .AddApplication();

            // CORS: allowed frontend origins come from configuration ("Cors:AllowedOrigins"),
            // e.g. http://localhost:4200 in appsettings.Development.json and appsettings.Testing.json; empty in appsettings.json
            // (production adds its own origins via appsettings.Production.json or environment variables).
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName,
                    policy =>
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            var app = builder.Build();

            // ---------------------------------------------------------
            // 4. Middleware pipeline
            // ---------------------------------------------------------
            // Swagger only in Development; Testing and Production behave like a real deployment without it.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Request logging is the outermost middleware, so it also measures the exception handler
            // and logs the final status code (e.g. 404/409/500 set by MarketExceptionHandler).
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            // Polica (#5): regional settings for every request. The middleware sets CultureInfo.CurrentCulture
            // and CurrentUICulture to "Regional:Culture" (bs-BA) for the rest of the request, so
            //   - numbers and dates formatted in backend messages look Bosnian: 1.234,56 and 15.10.2026.
            //   - FluentValidation rules without WithMessage(...) give its built-in Bosnian messages.
            // bs-BA is the ONLY supported culture: a browser sending "Accept-Language: en" still gets bs-BA,
            // so the backend speaks one language. Not affected (always culture-invariant): JSON bodies and
            // query strings, so the frontend keeps sending ISO dates ("2026-10-15") and a decimal point.
            // Placed before the exception handler, so error responses are built with the same culture.
            var regional = app.Services.GetRequiredService<IOptions<RegionalOptions>>().Value;
            app.UseRequestLocalization(new RequestLocalizationOptions()
                .SetDefaultCulture(regional.Culture)
                .AddSupportedCultures(regional.Culture)
                .AddSupportedUICultures(regional.Culture));

            // Global exception handler (IExceptionHandler)
            app.UseExceptionHandler();

            app.UseHttpsRedirection();
            // UseCors ide prije UseAuthentication i UseAuthorization
            app.UseCors(CorsPolicyName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Database migrations + seeding
            await app.Services.InitializeDatabaseAsync(app.Environment);

            Log.Information("Market API started successfully.");
            app.Run();
        }
        catch (HostAbortedException)
        {
            // EF Core tools abortiraju host nakon što uzmu DbContext.
            // Ovo nije runtime greška – samo tiho izađi.
            Log.Information("Host aborted by EF Core tooling (design-time) - its ok.");
        }
        catch (Exception ex)
        {
            // Any startup failure will be logged here
            Log.Fatal(ex, "Market API terminated unexpectedly.");
        }
        finally
        {
            // Ensure all logs are flushed before the app exits
            Log.CloseAndFlush();
        }
    }
}
