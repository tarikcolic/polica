using System.ComponentModel.DataAnnotations;

namespace Market.Shared.Options;

/// <summary>
/// Polica (#5): "Regional" section of appsettings.json, the regional settings of the whole backend.
/// <list type="bullet">
/// <item><see cref="Culture"/> is used by UseRequestLocalization in Program.cs: every HTTP request runs with
/// this culture (number and date formatting in messages, language of FluentValidation's built-in messages).</item>
/// <item><see cref="TimeZoneId"/> is used by AppTimeProvider (Market.Infrastructure/Common): it decides which
/// calendar day "today" is (TimeProvider.GetToday() in Market.Application/Common/DateTimeExtensions.cs).</item>
/// </list>
/// These are infrastructure settings, so they live in appsettings.json. Business settings that the
/// administrator changes at runtime (expiry thresholds...) live in the database (#11).
/// </summary>
public sealed class RegionalOptions
{
    public const string SectionName = "Regional";

    /// <summary>.NET culture name, e.g. "bs-BA": decimal comma, Bosnian month names.</summary>
    [Required] public string Culture { get; init; } = default!;

    /// <summary>
    /// IANA time zone id, e.g. "Europe/Sarajevo". .NET also accepts it on Windows
    /// (it is mapped to the Windows id "Central European Standard Time").
    /// </summary>
    [Required] public string TimeZoneId { get; init; } = default!;
}
