using Market.Application.Modules.CodeLists.Manufacturers.Queries.GetById;
using Market.Shared.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace Market.Tests.CodeListTests.IntegrationTests;

/// <summary>
/// Polica (#6): the manufacturers code list through the real HTTP pipeline (routing, JWT, the "Admin" policy,
/// MediatR, validators, handlers, MarketExceptionHandler) on the InMemory test database.
/// </summary>
/// <remarks>
/// Manufacturers stand in for all three code lists: DosageForms and WriteOffReasons use the same handler
/// pattern, so these three cases cover the rules issue #6 asks to test (create, duplicate name, 403 for a
/// pharmacist). The demo manufacturers from DynamicDataSeeder (e.g. "Bosnalijek") exist in the test database,
/// because DatabaseInitializer runs the seeder in the IntegrationTests environment too.
/// </remarks>
[Collection(IntegrationTestCollection.Name)]
public class ManufacturerIntegrationTests(CustomWebApplicationFactory<Program> factory)
{
    private const string Url = "api/manufacturers";

    /// <summary>Body of "201 Created" (CreatedAtAction(..., new { id })).</summary>
    private sealed record CreatedResponse(int Id);

    [Fact]
    public async Task Create_AsAdmin_TrimsAndSaves_ThenGetByIdReturnsIt()
    {
        var client = await factory.GetAuthenticatedClientAsync();
        // Unique per run: the tests share one database (IntegrationTestCollection).
        var name = $"Proizvođač {Guid.NewGuid():N}";

        // Spaces around the name and an empty country: the handler trims the name and stores the country as null.
        var create = await client.PostAsJsonAsync(Url, new { name = $"  {name}  ", country = "" });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreatedResponse>();

        var get = await client.GetAsync($"{Url}/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var manufacturer = await get.Content.ReadFromJsonAsync<GetManufacturerByIdQueryDto>();
        Assert.Equal(name, manufacturer!.Name);
        Assert.Null(manufacturer.Country);
    }

    [Fact]
    public async Task Create_DuplicateNameInOtherLetterCase_Returns400WithFieldErrorOnName()
    {
        var client = await factory.GetAuthenticatedClientAsync();

        // "Bosnalijek" is seeded; the uniqueness check ignores letter case.
        var response = await client.PostAsJsonAsync(Url, new { name = "BOSNALIJEK" });

        // A field error on "name" (not a 409), so the Angular dialog can show it under the Name field.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Equal("name", Assert.Single(error!.Errors!).Field);
    }

    [Fact]
    public async Task Create_AsPharmacist_Returns403()
    {
        // The seeded pharmacist (IsEmployee): logged in, but without the is_admin claim.
        var client = await factory.GetAuthenticatedClientAsync(
            CustomWebApplicationFactory<Program>.EmployeeEmail,
            CustomWebApplicationFactory<Program>.EmployeePassword);

        var response = await client.PostAsJsonAsync(Url, new { name = "Nije dozvoljeno" });

        // Stopped by [Authorize(Policy = PolicyNames.Admin)] before the handler runs.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
