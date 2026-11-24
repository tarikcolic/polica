using Market.Application.Modules.Pharmacies.Queries.GetById;
using Market.Application.Modules.Pharmacies.Queries.Lookup;
using Market.Shared.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace Market.Tests.PharmacyTests.IntegrationTests;

/// <summary>
/// Polica (#7): the pharmacy registry through the real HTTP pipeline (routing, JWT, the "Admin" policy, MediatR,
/// validators, handlers, MarketExceptionHandler) on the InMemory test database.
/// </summary>
/// <remarks>
/// The seeded data exists in the test database: the cities from StaticDataSeeder (HasData, applied by
/// EnsureCreated) and the demo pharmacies AP-01 to AP-03 from DynamicDataSeeder.
/// The tests share one database (IntegrationTestCollection), so every new pharmacy gets a unique code.
/// </remarks>
[Collection(IntegrationTestCollection.Name)]
public class PharmacyIntegrationTests(CustomWebApplicationFactory<Program> factory)
{
    private const string Url = "api/pharmacies";

    /// <summary>City Id 1 = Sarajevo (StaticDataSeeder.SeedCities).</summary>
    private const int SarajevoCityId = 1;

    /// <summary>Body of "201 Created" (CreatedAtAction(..., new { id })).</summary>
    private sealed record CreatedResponse(int Id);

    /// <summary>
    /// A code that no other test uses: "T" + 8 random hex characters = 9 characters, under the limit of 10.
    /// Lowercase on purpose, so the tests also see the handler turn it into uppercase.
    /// </summary>
    private static string NewCode() => $"t{Guid.NewGuid():N}"[..9];

    private static object NewPharmacyBody(string code) => new
    {
        name = "Apoteka Test",
        code,
        address = "Titova 1",
        phone = "+387 33 111 222",
        cityId = SarajevoCityId
    };

    [Fact]
    public async Task Create_AsAdmin_StoresCodeInUppercase_ThenGetByIdReturnsIt()
    {
        var client = await factory.GetAuthenticatedClientAsync();
        var code = NewCode();

        var create = await client.PostAsJsonAsync(Url, NewPharmacyBody(code));

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreatedResponse>();

        var pharmacy = await client.GetFromJsonAsync<GetPharmacyByIdQueryDto>($"{Url}/{created!.Id}");
        Assert.Equal(code.ToUpperInvariant(), pharmacy!.Code);
        Assert.Equal("Sarajevo", pharmacy.CityName);
        Assert.True(pharmacy.IsActive); // a new pharmacy is active
    }

    [Fact]
    public async Task Create_DuplicateCodeInOtherLetterCase_Returns400WithFieldErrorOnCode()
    {
        var client = await factory.GetAuthenticatedClientAsync();

        // "AP-01" is seeded; "ap-01" becomes "AP-01" in the handler and collides with it.
        var response = await client.PostAsJsonAsync(Url, NewPharmacyBody("ap-01"));

        // A field error on "code" (not a 409), so the Angular form shows it under the Code field.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Equal("code", Assert.Single(error!.Errors!).Field);
    }

    [Fact]
    public async Task Create_AsPharmacist_Returns403()
    {
        // The seeded pharmacist (IsEmployee): logged in, but without the is_admin claim.
        var client = await factory.GetAuthenticatedClientAsync(
            CustomWebApplicationFactory<Program>.EmployeeEmail,
            CustomWebApplicationFactory<Program>.EmployeePassword);

        var response = await client.PostAsJsonAsync(Url, NewPharmacyBody(NewCode()));

        // Stopped by [Authorize(Policy = PolicyNames.Admin)] before the handler runs.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_RemovesPharmacyFromLookup_ActivateBringsItBack()
    {
        var client = await factory.GetAuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync(Url, NewPharmacyBody(NewCode()));
        var id = (await create.Content.ReadFromJsonAsync<CreatedResponse>())!.Id;

        // Deactivated → no longer offered in dropdowns (the #7 rule lives in GetPharmaciesLookupQueryHandler).
        var deactivate = await client.PostAsync($"{Url}/{id}/deactivate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);

        var lookup = await client.GetFromJsonAsync<List<PharmacyLookupQueryDto>>($"{Url}/lookup");
        Assert.DoesNotContain(lookup!, x => x.Id == id);

        // Activated again → back in the lookup.
        await client.PostAsync($"{Url}/{id}/activate", content: null);

        lookup = await client.GetFromJsonAsync<List<PharmacyLookupQueryDto>>($"{Url}/lookup");
        Assert.Contains(lookup!, x => x.Id == id);
    }
}
