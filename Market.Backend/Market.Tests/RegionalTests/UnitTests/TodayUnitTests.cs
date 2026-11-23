using Market.Application.Common;
using Market.Infrastructure.Common;
using Microsoft.Extensions.Time.Testing;

namespace Market.Tests.RegionalTests.UnitTests;

/// <summary>
/// Polica (#5): "today" is the calendar day in Sarajevo, not in UTC
/// (TimeProvider.GetToday() in Market.Application/Common/DateTimeExtensions.cs).
/// FakeTimeProvider fixes the UTC time and the local time zone, so the tests do not depend on
/// when or on which machine they run.
/// </summary>
public class TodayUnitTests
{
    private static readonly TimeZoneInfo Sarajevo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Sarajevo");

    [Theory]
    // summer time (UTC+2): 23:30 UTC is already 01:30 the next day in Sarajevo
    [InlineData("2026-10-14T23:30:00Z", "2026-10-15")]
    // winter time (UTC+1): 23:30 UTC is 00:30 the next day in Sarajevo
    [InlineData("2027-01-15T23:30:00Z", "2027-01-16")]
    // winter time: 22:59 UTC is 23:59 in Sarajevo, still the same day
    [InlineData("2027-01-15T22:59:00Z", "2027-01-15")]
    public void GetToday_ReturnsCalendarDayInSarajevo(string utcNow, string expectedToday)
    {
        // Arrange
        var time = new FakeTimeProvider(DateTimeOffset.Parse(utcNow));
        time.SetLocalTimeZone(Sarajevo);

        // Act
        var today = time.GetToday();

        // Assert
        Assert.Equal(DateOnly.Parse(expectedToday), today);
    }

    [Fact]
    public void AppTimeProvider_UsesConfiguredTimeZone()
    {
        // Arrange: the same id as "Regional:TimeZoneId" in appsettings.json
        var time = new AppTimeProvider("Europe/Sarajevo");
        var winterNoonUtc = new DateTime(2027, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var offset = time.LocalTimeZone.GetUtcOffset(winterNoonUtc);

        // Assert: Central European Time, one hour ahead of UTC in winter
        Assert.Equal(TimeSpan.FromHours(1), offset);
    }
}
