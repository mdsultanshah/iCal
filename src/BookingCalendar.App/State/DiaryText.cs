using BookingCalendar.Core;

namespace BookingCalendar.App.State;

public static class DiaryText
{
    private static readonly string[] Weekdays =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly string[] ShortDays =
        ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    private static readonly string[] Months =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    ];

    public static string Weekday(DateOnly date) => Weekdays[(int)date.DayOfWeek];

    public static string Month(DateOnly date) => Months[date.Month - 1];

    public static string Date(DateOnly date) =>
        $"{Weekday(date)} {date.Day} {Month(date)}";

    public static string ShortDate(DateOnly date) =>
        $"{ShortDays[(int)date.DayOfWeek]} {date.Day} {Month(date)[..3]}";

    public static string MonthTitle(IReadOnlyList<DateOnly> days)
    {
        if (days.Count == 0)
            return "";

        var first = days[0];
        var last = days[^1];
        if (first.Month == last.Month && first.Year == last.Year)
            return $"{Month(first)} {first.Year}";

        if (first.Year == last.Year)
            return $"{Month(first)} – {Month(last)} {first.Year}";

        return $"{Month(first)} {first.Year} – {Month(last)} {last.Year}";
    }

    public static string Iso(DateOnly date) =>
        $"{date.Year:0000}-{date.Month:00}-{date.Day:00}";

    public static string Time(TimeOnly time) => $"{time.Hour:00}:{time.Minute:00}";

    public static string Range(TimeOnly start, TimeOnly end) => $"{Time(start)}–{Time(end)}";

    public static string Hours(WeeklySchedule schedule)
    {
        var open = schedule.Days.Where(day => day.IsOpen).ToList();
        var weekdays = open.All(day => day.Day is >= DayOfWeek.Monday and <= DayOfWeek.Friday)
            && open.Count == 5
            && open.All(day => day.Opens == new TimeOnly(9, 0) && day.Closes == new TimeOnly(17, 0));
        if (weekdays)
            return "Mon–Fri, 9:00–17:00";

        return string.Join(" · ", open.Select(day =>
            $"{ShortDays[(int)day.Day]} {Time(day.Opens)}–{Time(day.Closes)}"));
    }

    public static IReadOnlyList<TimeOnly> HalfHours()
    {
        var times = new List<TimeOnly>(48);
        for (var minutes = 0; minutes < 24 * 60; minutes += 30)
            times.Add(BookingRules.FromMinutes(minutes));

        return times;
    }
}
