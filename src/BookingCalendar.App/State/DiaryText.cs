namespace BookingCalendar.App.State;

public static class DiaryText
{
    private static readonly string[] Weekdays =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly string[] Months =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    ];

    public static string Weekday(DateOnly date) => Weekdays[(int)date.DayOfWeek];

    public static string Month(DateOnly date) => Months[date.Month - 1];

    public static string Date(DateOnly date) =>
        $"{Weekday(date)} {date.Day} {Month(date)}";

    public static string Iso(DateOnly date) =>
        $"{date.Year:0000}-{date.Month:00}-{date.Day:00}";

    public static string Time(TimeOnly time) => $"{time.Hour:00}:{time.Minute:00}";

    public static string Slot(TimeOnly start, TimeOnly end) => $"{Time(start)}–{Time(end)}";
}
