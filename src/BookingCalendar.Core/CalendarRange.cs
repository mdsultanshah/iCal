namespace BookingCalendar.Core;

/// <summary>Week and month ranges. Weeks start on Monday.</summary>
public static class CalendarRange
{
    public const int HoursInDay = 24;

    public static int DaysFromMonday(DateOnly date) => ((int)date.DayOfWeek + 6) % 7;

    public static DateOnly StartOfWeek(DateOnly date) =>
        date.AddDays(-DaysFromMonday(date));

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

    public static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    /// <summary>
    /// Every cell in the month grid, including leading and trailing days.
    /// The list length is 35 (five weeks) or 42 (six weeks).
    /// </summary>
    public static IReadOnlyList<DateOnly> MonthGrid(DateOnly date)
    {
        var first = StartOfMonth(date);
        var start = StartOfWeek(first);
        var last = first.AddMonths(1).AddDays(-1);
        var end = StartOfWeek(last).AddDays(6);
        var days = new List<DateOnly>();
        for (var day = start; day <= end; day = day.AddDays(1))
            days.Add(day);

        return days;
    }

    public static bool InMonth(DateOnly date, DateOnly month) =>
        date.Year == month.Year && date.Month == month.Month;

    public static DateOnly Shift(DateOnly date, int days) => date.AddDays(days);

    public static DateOnly ShiftMonth(DateOnly date, int months) => date.AddMonths(months);

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
