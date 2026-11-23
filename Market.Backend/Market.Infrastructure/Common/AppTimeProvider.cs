namespace Market.Infrastructure.Common;

/// <summary>
/// Polica (#5): the application clock. It is the system clock (the base <see cref="TimeProvider"/> already
/// reads the real UTC time), with one change: its local time zone is the one from configuration
/// ("Regional:TimeZoneId" = Europe/Sarajevo) instead of the time zone of the machine the API runs on.
///
/// Why: handlers ask for "today" (TimeProvider.GetToday(), Market.Application/Common/DateTimeExtensions.cs)
/// to check expiry dates, shelf life on receipt and the expected delivery date. A pharmacy's day is the
/// day in Sarajevo. On a server whose clock is set to UTC, TimeZoneInfo.Local would still give yesterday
/// from midnight to 01:00 Sarajevo time (to 02:00 in summer), because UTC is one (two) hours behind.
///
/// Registered once, as a singleton, in Market.Infrastructure/DependencyInjection.cs. Every class that
/// injects TimeProvider (handlers, DatabaseContext, JwtTokenService) gets this instance. Unit tests use
/// FakeTimeProvider instead, which can set both the time and the local time zone.
/// </summary>
public sealed class AppTimeProvider : TimeProvider
{
    private readonly TimeZoneInfo _localTimeZone;

    /// <param name="timeZoneId">IANA id such as "Europe/Sarajevo" (also works on Windows).</param>
    /// <exception cref="TimeZoneNotFoundException">The id is not known on this machine; the API does not start.</exception>
    public AppTimeProvider(string timeZoneId)
    {
        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    /// <summary>
    /// Used by GetLocalNow(): GetLocalNow() = GetUtcNow() converted to this zone.
    /// GetUtcNow() itself is not overridden: timestamps stay in UTC everywhere (database, JWT, API).
    /// </summary>
    public override TimeZoneInfo LocalTimeZone => _localTimeZone;
}
