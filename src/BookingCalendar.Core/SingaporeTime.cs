namespace BookingCalendar.Core;

/// <summary>
/// Singapore (Asia/Singapore) is UTC+8 all year. Slots are computed in this
/// zone regardless of the browser's zone.
/// </summary>
public static class SingaporeTime
{
    public const string ZoneId = Catalog.ZoneId;
    public static readonly TimeSpan Offset = TimeSpan.FromHours(8);

    public static DateTimeOffset ToSingapore(DateTimeOffset instant) =>
        instant.ToOffset(Offset);

    public static DateOnly Today(DateTimeOffset utcNow) =>
        DateOnly.FromDateTime(ToSingapore(utcNow).DateTime);

    public static TimeOnly TimeOfDay(DateTimeOffset utcNow) =>
        TimeOnly.FromTimeSpan(ToSingapore(utcNow).TimeOfDay);
}
