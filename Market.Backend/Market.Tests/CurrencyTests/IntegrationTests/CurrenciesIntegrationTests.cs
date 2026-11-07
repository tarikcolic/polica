using Market.Application.Modules.Finance.Currencies.Queries.Convert;
using Market.Application.Modules.Finance.Currencies.Queries.List;
using Market.Shared.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace Market.Tests.CurrencyTests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class CurrenciesIntegrationTests
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CurrenciesIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Currencies_Anonymous_ReturnsSeededCurrenciesWithBaseFirst()
    {
        // Arrange
        var client = _factory.CreateClient(); // no Bearer token

        // Act
        var currencies = await client.GetFromJsonAsync<List<ListCurrenciesQueryDto>>("/Currencies");

        // Assert
        Assert.NotNull(currencies);
        Assert.Equal(["BAM", "CHF", "EUR", "GBP", "RSD", "USD"], currencies.Select(c => c.Code));
        Assert.True(currencies[0].IsBase);
        Assert.Equal(1m, currencies[0].RateToBase);
        Assert.Equal(1.95583m, currencies.Single(c => c.Code == "EUR").RateToBase);
    }

    [Fact]
    public async Task Get_Convert_Anonymous_ConvertsWithFixedEurRate()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetFromJsonAsync<ConvertCurrencyQueryDto>("/Currencies/convert?amount=100&from=eur&to=BAM");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EUR", result.From);
        Assert.Equal(195.58m, result.ConvertedAmount);
    }

    [Fact]
    public async Task Get_Rates_Anonymous_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Currencies/rates");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_RefreshRates_ProviderNone_ShouldReturnConflict()
    {
        // Arrange: appsettings.json has "ExchangeRates:Provider": "None", so no network call is made
        var client = await _factory.GetAuthenticatedClientAsync(); // admin

        // Act
        var response = await client.PostAsync("/Currencies/rates/refresh", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Contains("ExchangeRates", error!.Message);
    }

    [Theory]
    // The validator checks only the format of the code (no database access); whether the currency exists
    // and is not the base currency needs the database, so CreateExchangeRateCommandHandler checks it.
    [InlineData("XYZ", HttpStatusCode.NotFound)] // unknown currency → MarketNotFoundException
    [InlineData("BAM", HttpStatusCode.Conflict)] // base currency → MarketBusinessRuleException
    [InlineData("US", HttpStatusCode.BadRequest)] // wrong format → validator
    public async Task Post_Rate_CurrencyCode_IsCheckedByValidatorOrHandler(string code, HttpStatusCode expected)
    {
        // Arrange
        var client = await _factory.GetAuthenticatedClientAsync(); // admin

        // Act
        var response = await client.PostAsJsonAsync("/Currencies/rates",
            new { CurrencyCode = code, RateToBase = 1.5m, ValidFromUtc = DateTime.UtcNow });

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }
}
