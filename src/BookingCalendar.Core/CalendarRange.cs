namespace BookingCalendar.Core;

/// <summary>Week and day ranges. Weeks start on Sunday.</summary>
public static class CalendarRange
{
    public const int HoursInDay = 24;

    public static DateOnly StartOfWeek(DateOnly date) =>
        date.AddDays(-(int)date.DayOfWeek);

    public static IReadOnlyList<DateOnly> Week(DateOnly date)
    {
        var start = StartOfWeek(date);
        var days = new List<DateOnly>(7);
        for (var offset = 0; offset < 7; offset++)
            days.Add(start.AddDays(offset));

        return days;
    }

    public static bool InWeek(DateOnly date, DateOnly today)
    {
        var start = StartOfWeek(today);
        return date >= start && date < start.AddDays(7);
    }

    public static DateOnly Shift(DateOnly date, int days) => date.AddDays(days);

    public static IReadOnlyList<TimeOnly> Hours()
    {
        var hours = new List<TimeOnly>(HoursInDay);
        for (var hour = 0; hour < HoursInDay; hour++)
            hours.Add(new TimeOnly(hour, 0));

        return hours;
    }

    /// <summary>The end time pre-filled when a grid hour is chosen. The last hour ends at 23:59.</summary>
    public static TimeOnly DefaultEnd(TimeOnly start)
    {
        if (start.Hour >= 23)
            return new TimeOnly(23, 59);

        return start.AddHours(1);
    }
}
