using Market.Application.Modules.Auth.Commands.ChangePassword;
using Market.Application.Modules.Auth.Commands.Login;
using Market.Application.Modules.Auth.Queries.GetMe;
using Market.Domain.Entities.Identity;
using Market.Shared.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Market.Tests.AuthTests.IntegrationTests;

/// <summary>
/// Forgot/reset password, profile (me) and change password.
/// Every test creates its own user (random email), so the seeded accounts and their cached tokens
/// in <see cref="CustomWebApplicationFactory{TProgram}"/> are never logged out by a password change.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class AccountIntegrationTests
{
    private const string Password = "Lozinka123";

    private readonly CustomWebApplicationFactory<Program> _factory;

    public AccountIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static string NewEmail() => $"user-{Guid.NewGuid():N}@test.local";

    private HttpClient CreateClient(string? accessToken = null)
    {
        var client = _factory.CreateClient();
        if (accessToken is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    /// <summary>
    /// Creates a new staff account with <see cref="Password"/> and logs it in; returns its email and tokens.
    /// </summary>
    /// <remarks>
    /// Polica (#4): the starter called POST api/auth/register here. Self-registration was removed,
    /// because in Polica only the administrator creates accounts (#8). The test therefore writes the
    /// user straight into the test database, the same way the administrator's create command will:
    /// <list type="number">
    /// <item>open a DI scope on the test server (DatabaseContext is a scoped service),</item>
    /// <item>hash the password with the same <see cref="IPasswordHasher{TUser}"/> the login handler uses to verify it,</item>
    /// <item>save the user, then log in through the real endpoint, so the tokens are genuine.</item>
    /// </list>
    /// The account is a pharmacist (IsEmployee), one of the three roles that exist in Polica.
    /// </remarks>
    private async Task<(string Email, LoginCommandDto Tokens)> CreateUserAndLoginAsync()
    {
        var email = NewEmail();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<MarketUserEntity>>();

            var user = new MarketUserEntity
            {
                Firstname = "Lejla",
                Lastname = "Kovač",
                Email = email,
                Address = "Titova 10",
                City = "Sarajevo",
                IsEmployee = true,
                IsEnabled = true
            };
            user.PasswordHash = hasher.HashPassword(user, Password);

            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var response = await CreateClient().PostAsJsonAsync("api/auth/login", new { Email = email, Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokens = await response.Content.ReadFromJsonAsync<LoginCommandDto>();
        return (email, tokens!);
    }

    private static async Task<FieldErrorDto[]> ReadFieldErrorsAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        return error!.Errors!.ToArray();
    }

    [Fact]
    public async Task Post_ForgotPassword_UnknownEmail_Returns204AndSendsNoEmail()
    {
        // Arrange
        var email = NewEmail();

        // Act
        var response = await CreateClient().PostAsJsonAsync("api/auth/forgot-password", new { Email = email });

        // Assert: the same response as for an existing account
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_factory.Emails.SentTo(email));
    }

    [Fact]
    public async Task Post_ForgotAndResetPassword_SetsNewPasswordOnce_AndLogsOutEverywhere()
    {
        // Arrange
        var (email, oldTokens) = await CreateUserAndLoginAsync();
        var anonymous = CreateClient();
        const string newPassword = "NovaLozinka1";

        // Act 1: forgot password → email with the link
        var forgot = await anonymous.PostAsJsonAsync("api/auth/forgot-password", new { Email = email });
        var link = ExtractLink(Assert.Single(_factory.Emails.SentTo(email)).HtmlBody);
        var query = HttpUtility.ParseQueryString(new Uri(link).Query);

        // Act 2: the frontend sends email + token from the link (twice: the second use must fail)
        var resetRequest = new { Email = query["email"], Token = query["token"], NewPassword = newPassword };
        var reset = await anonymous.PostAsJsonAsync("api/auth/reset-password", resetRequest);
        var secondReset = await anonymous.PostAsJsonAsync("api/auth/reset-password", resetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, forgot.StatusCode);
        Assert.StartsWith($"{CustomWebApplicationFactory<Program>.FrontendBaseUrl}/auth/reset-password?", link);
        Assert.Equal(email, query["email"]);
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        Assert.Equal("token", Assert.Single(await ReadFieldErrorsAsync(secondReset)).Field);

        var loginNew = await anonymous.PostAsJsonAsync("api/auth/login", new { Email = email, Password = newPassword });
        var loginOld = await anonymous.PostAsJsonAsync("api/auth/login", new { Email = email, Password });
        var refreshOld = await anonymous.PostAsJsonAsync("api/auth/refresh", new { oldTokens.RefreshToken });
        var meWithOldAccessToken = await CreateClient(oldTokens.AccessToken).GetAsync("api/auth/me");

        Assert.Equal(HttpStatusCode.OK, loginNew.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, loginOld.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshOld.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, meWithOldAccessToken.StatusCode);
    }

    [Fact]
    public async Task Put_Me_ChangesProfile_ButNotEmail()
    {
        // Arrange
        var (email, tokens) = await CreateUserAndLoginAsync();
        var client = CreateClient(tokens.AccessToken);

        // Act: "Email" is not part of the request and is ignored
        var put = await client.PutAsJsonAsync("api/auth/me", new
        {
            Firstname = "  Amra ",
            Lastname = "Hodžić",
            Address = "Ferhadija 1",
            City = "",
            Email = "drugi@test.local"
        });
        var me = await client.GetFromJsonAsync<GetMeQueryDto>("api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        Assert.Equal("Amra", me!.Firstname);
        Assert.Equal("Hodžić", me.Lastname);
        Assert.Equal("Ferhadija 1", me.Address);
        Assert.Null(me.City); // an empty field is stored as "no value"
        Assert.Equal(email, me.Email);
    }

    [Fact]
    public async Task Get_Me_Anonymous_Returns401()
    {
        // Act
        var response = await CreateClient().GetAsync("api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_ChangePassword_WrongCurrentPassword_ShouldReturnFieldErrorOnCurrentPassword()
    {
        // Arrange
        var (_, tokens) = await CreateUserAndLoginAsync();

        // Act
        var response = await CreateClient(tokens.AccessToken).PostAsJsonAsync("api/auth/change-password", new
        {
            CurrentPassword = "PogresnaLozinka1",
            NewPassword = "Promijenjena2"
        });

        // Assert
        Assert.Equal("currentPassword", Assert.Single(await ReadFieldErrorsAsync(response)).Field);
    }

    [Fact]
    public async Task Post_ChangePassword_ReturnsNewTokens_AndRevokesOldSession()
    {
        // Arrange
        var (email, oldTokens) = await CreateUserAndLoginAsync();
        var anonymous = CreateClient();
        const string newPassword = "Promijenjena2";

        // Act
        var response = await CreateClient(oldTokens.AccessToken).PostAsJsonAsync("api/auth/change-password", new
        {
            CurrentPassword = Password,
            NewPassword = newPassword
        });
        var newTokens = await response.Content.ReadFromJsonAsync<ChangePasswordCommandDto>();

        // Assert: old tokens no longer work, this client continues with the new ones
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(oldTokens.RefreshToken, newTokens!.RefreshToken);

        var refreshOld = await anonymous.PostAsJsonAsync("api/auth/refresh", new { oldTokens.RefreshToken });
        var meWithOldAccessToken = await CreateClient(oldTokens.AccessToken).GetAsync("api/auth/me");
        var meWithNewAccessToken = await CreateClient(newTokens.AccessToken).GetAsync("api/auth/me");
        var refreshNew = await anonymous.PostAsJsonAsync("api/auth/refresh", new { newTokens.RefreshToken });
        var loginNew = await anonymous.PostAsJsonAsync("api/auth/login", new { Email = email, Password = newPassword });

        Assert.Equal(HttpStatusCode.Unauthorized, refreshOld.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, meWithOldAccessToken.StatusCode);
        Assert.Equal(HttpStatusCode.OK, meWithNewAccessToken.StatusCode);
        Assert.Equal(HttpStatusCode.OK, refreshNew.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginNew.StatusCode);
    }

    /// <summary>
    /// The reset link from the email body (href of the link, HTML-decoded: "&amp;amp;" → "&amp;").
    /// </summary>
    private static string ExtractLink(string htmlBody)
    {
        var match = Regex.Match(htmlBody, "href=\"([^\"]+)\"");
        Assert.True(match.Success, "The email contains no link.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
