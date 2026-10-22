using System.Globalization;

namespace BookingCalendar.Core;

/// <summary>12-hour and 24-hour time text. The stored time is always a time of day.</summary>
public static class ClockFormat
{
    public static string Format(TimeOnly time, bool twentyFourHour)
    {
        if (twentyFourHour)
            return time.ToString("HH:mm", CultureInfo.InvariantCulture);

        var hour = time.Hour % 12;
        if (hour == 0)
            hour = 12;

        var suffix = time.Hour < 12 ? "AM" : "PM";
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{hour}:{time.Minute:00} {suffix}");
    }

    public static string Range(TimeOnly start, TimeOnly end, bool twentyFourHour) =>
        $"{Format(start, twentyFourHour)}–{Format(end, twentyFourHour)}";

    public const string HourMinuteError = "Enter a time as HH:mm.";

    /// <summary>Accepts a 24-hour <c>HH:mm</c> value. Anything else is rejected.</summary>
    public static bool TryParseHourMinute(string? value, out TimeOnly time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        if (text.Length != 5 || text[2] != ':')
            return false;

        return TimeOnly.TryParseExact(
            text,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }

    /// <summary>
    /// The next full hour. An exact hour stays put. After 23:00 the day has no later hour, so the result stays 23:00.
    /// </summary>
    public static TimeOnly NextFullHour(TimeOnly time)
    {
        if (time is { Minute: 0, Second: 0, Millisecond: 0 })
            return new TimeOnly(time.Hour, 0);

        if (time.Hour >= 23)
            return new TimeOnly(23, 0);

        return new TimeOnly(time.Hour + 1, 0);
    }
}
