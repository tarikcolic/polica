namespace Market.Application.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// Returns the value as UTC. All dates in this application are UTC, so a value without time zone
    /// information (<see cref="DateTimeKind.Unspecified"/>, e.g. "2026-10-01" sent without "Z") is treated as UTC,
    /// and a local time is converted to UTC.
    /// </summary>
    public static DateTime AsUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    /// <summary>
    /// Polica (#5): today's calendar date in the application's time zone (Europe/Sarajevo), for comparing
    /// with DateOnly columns such as ExpiryDate and ManufactureDate (#13), "expired" in FEFO (#15) and
    /// days left in expiry alerts (#16).
    /// <para>
    /// GetLocalNow() is the current UTC time converted to <see cref="TimeProvider.LocalTimeZone"/>: in the
    /// running API that is AppTimeProvider's Sarajevo zone, in unit tests the zone set on FakeTimeProvider.
    /// Example: 14.10.2026. 23:30 UTC is 15.10.2026. 01:30 in Sarajevo, so today is 15.10.2026.
    /// DateTime.UtcNow.Date would return 14.10.2026. and DateTime.Today depends on the server's settings.
    /// </para>
    /// Usage in a handler that injects TimeProvider: <c>var today = timeProvider.GetToday();</c>
    /// </summary>
    public static DateOnly GetToday(this TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
}
