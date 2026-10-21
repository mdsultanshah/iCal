using System.Globalization;
using BookingCalendar.Core;

namespace BookingCalendar.App.State;

/// <summary>Display text for dates and times. Weeks are titled from Sunday.</summary>
public static class CalendarText
{
    public static bool TryParseTime(string? value, out TimeOnly time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return TimeOnly.TryParseExact(
            value.Trim(),
            ["HH:mm", "HH:mm:ss", "H:mm"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }

    public static string Time(TimeOnly time) =>
        time.ToString("HH:mm", CultureInfo.InvariantCulture);

    public static string DateValue(DateOnly date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string Range(TimeOnly start, TimeOnly end) =>
        $"{Time(start)}–{Time(end)}";

    public static string Weekday(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Sunday => "Sunday",
        DayOfWeek.Monday => "Monday",
        DayOfWeek.Tuesday => "Tuesday",
        DayOfWeek.Wednesday => "Wednesday",
        DayOfWeek.Thursday => "Thursday",
        DayOfWeek.Friday => "Friday",
        DayOfWeek.Saturday => "Saturday",
        _ => "",
    };

    public static string ShortWeekday(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Sunday => "Sun",
        DayOfWeek.Monday => "Mon",
        DayOfWeek.Tuesday => "Tue",
        DayOfWeek.Wednesday => "Wed",
        DayOfWeek.Thursday => "Thu",
        DayOfWeek.Friday => "Fri",
        DayOfWeek.Saturday => "Sat",
        _ => "",
    };

    public static string Month(DateOnly date) => date.Month switch
    {
        1 => "January",
        2 => "February",
        3 => "March",
        4 => "April",
        5 => "May",
        6 => "June",
        7 => "July",
        8 => "August",
        9 => "September",
        10 => "October",
        11 => "November",
        _ => "December",
    };

    public static string DayTitle(DateOnly date) =>
        $"{Weekday(date)} {date.Day} {Month(date)}";

    public static string WeekTitle(IReadOnlyList<DateOnly> days)
    {
        var first = days[0];
        var last = days[^1];
        if (first.Year != last.Year)
            return $"{first.Day} {Month(first)} {first.Year} – {last.Day} {Month(last)} {last.Year}";
        if (first.Month != last.Month)
            return $"{first.Day} {Month(first)} – {last.Day} {Month(last)} {last.Year}";
        return $"{first.Day}–{last.Day} {Month(last)} {last.Year}";
    }
}
