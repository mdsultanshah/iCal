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
}
