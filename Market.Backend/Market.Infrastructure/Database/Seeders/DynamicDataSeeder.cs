using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Market.Domain.Entities.Identity;

namespace Market.Infrastructure.Database.Seeders;

/// <summary>
/// Dynamic seeder koji se pokreće u runtime-u,
/// obično pri startu aplikacije (npr. u Program.cs).
/// Koristi se za unos demo/test podataka koji nisu dio migracije.
/// </summary>
/// <remarks>
/// Polica (#4): the starter's shop demo data (product categories, products, customer orders, promotions,
/// exchange rates and the three demo customers) was removed with those modules. Only the staff accounts
/// remain: one per role (admin, manager, employee) plus the "string" (Swagger) and "test" (tests) users.
/// The staff accounts use the @polica.local domain (the starter used @market.local); ".local" is a reserved
/// domain that never receives real email. Tests log in with the same addresses (CustomWebApplicationFactory).
/// Pharmacies, medications and batches with dates relative to today come in #22 (demo data seeder).
/// Polica (#6): demo manufacturers (code list) are seeded here too.
/// Polica (#7): three demo pharmacies (AP-01 to AP-03).
/// Called by <see cref="DatabaseInitializer"/> after migrations, in Development, Testing and IntegrationTests.
/// </remarks>
public static class DynamicDataSeeder
{
    public static async Task SeedAsync(DatabaseContext context)
    {
        // Šema baze se ne kreira ovdje: DatabaseInitializer prije seed-a pokreće migracije
        // (SQL Server) ili EnsureCreated (InMemory baza u testovima).

        await SeedUsersAsync(context);

        // Polica (#6): demo manufacturers. Dosage forms and write-off reasons are reference data and come
        // with the migration (StaticDataSeeder); manufacturers are business data, so only demo rows are here.
        await SeedManufacturersAsync(context);

        // Polica (#7): demo pharmacies. Needs the cities, which already exist (migration / EnsureCreated).
        await SeedPharmaciesAsync(context);
    }

    /// <summary>
    /// Polica (#7): three demo pharmacies, so the pharmacy list has data, the duplicate-code check can be tried
    /// (e.g. "ap-01"), and the employee screen in #8 has pharmacies to assign the demo manager and pharmacist to.
    /// Runs only while the table is empty (restart-safe, keeps the administrator's own changes).
    /// The full demo set (stock, orders, dates relative to today) comes in #22.
    /// </summary>
    /// <remarks>
    /// CityId values are the fixed Ids from StaticDataSeeder.SeedCities (1 = Sarajevo, 5 = Mostar, 3 = Tuzla).
    /// </remarks>
    private static async Task SeedPharmaciesAsync(DatabaseContext context)
    {
        if (await context.Pharmacies.AnyAsync())
            return;

        context.Pharmacies.AddRange(
            new PharmacyEntity
            {
                Code = "AP-01",
                Name = "Apoteka Centar",
                Address = "Ferhadija 12",
                Phone = "+387 33 200 101",
                CityId = 1,
                IsActive = true
            },
            new PharmacyEntity
            {
                Code = "AP-02",
                Name = "Apoteka Stari most",
                Address = "Kralja Tomislava 5",
                Phone = "+387 36 200 202",
                CityId = 5,
                IsActive = true
            },
            new PharmacyEntity
            {
                Code = "AP-03",
                Name = "Apoteka Kapija",
                Address = "Turalibegova 20",
                Phone = "+387 35 200 303",
                CityId = 3,
                IsActive = true
            });

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: demo pharmacies added.");
    }

    /// <summary>
    /// Polica (#6): a few real manufacturers from the region, so the code list screen and, later, the medication
    /// form (#10) have something to show in Development and Testing. Runs only while the table is empty,
    /// so restarting the API does not add them twice and the administrator's own changes are kept.
    /// </summary>
    private static async Task SeedManufacturersAsync(DatabaseContext context)
    {
        if (await context.Manufacturers.AnyAsync())
            return;

        context.Manufacturers.AddRange(
            new ManufacturerEntity { Name = "Bosnalijek", Country = "Bosna i Hercegovina" },
            new ManufacturerEntity { Name = "Hemofarm", Country = "Srbija" },
            new ManufacturerEntity { Name = "Galenika", Country = "Srbija" },
            new ManufacturerEntity { Name = "Pliva", Country = "Hrvatska" },
            new ManufacturerEntity { Name = "Belupo", Country = "Hrvatska" },
            new ManufacturerEntity { Name = "Krka", Country = "Slovenija" });

        // CreatedAtUtc is filled in by DatabaseContext.ApplyAuditAndSoftDelete.
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: demo manufacturers added.");
    }

    private static async Task SeedUsersAsync(DatabaseContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<MarketUserEntity>();

        var admin = new MarketUserEntity
        {
            Firstname = "Admin",
            Lastname = "Polica",
            Email = "admin@polica.local",
            PasswordHash = hasher.HashPassword(null!, "Admin123!"),
            Address = "Maršala Tita 1",
            City = "Sarajevo",
            IsAdmin = true,
            IsManager = false,
            IsEmployee = false,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var manager = new MarketUserEntity
        {
            Firstname = "Manager",
            Lastname = "Polica",
            Email = "manager@polica.local",
            PasswordHash = hasher.HashPassword(null!, "Manager123!"),
            Address = "Ferhadija 12",
            City = "Sarajevo",
            IsAdmin = false,
            IsManager = true,
            IsEmployee = false,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var employee = new MarketUserEntity
        {
            Firstname = "Employee",
            Lastname = "Polica",
            Email = "employee@polica.local",
            PasswordHash = hasher.HashPassword(null!, "Employee123!"),
            Address = "Kralja Tvrtka 5",
            City = "Zenica",
            IsAdmin = false,
            IsManager = false,
            IsEmployee = true,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var dummyForSwagger = new MarketUserEntity
        {
            Firstname = "Swagger",
            Lastname = "Demo",
            Email = "string",
            PasswordHash = hasher.HashPassword(null!, "string"),
            Address = "Zmaja od Bosne 8",
            City = "Sarajevo",
            IsAdmin = false,
            IsManager = false,
            IsEmployee = true,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var dummyForTests = new MarketUserEntity
        {
            Firstname = "Test",
            Lastname = "User",
            Email = "test",
            PasswordHash = hasher.HashPassword(null!, "test123"),
            Address = "Titova 3",
            City = "Tuzla",
            IsAdmin = false,
            IsManager = false,
            IsEmployee = true,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Users.AddRange(
            admin,
            manager,
            employee,
            dummyForSwagger,
            dummyForTests
        );

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: demo users added.");
    }
}
