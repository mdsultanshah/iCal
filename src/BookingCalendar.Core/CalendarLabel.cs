namespace BookingCalendar.Core;

/// <summary>Accessible names built from a date value, never from raw query text.</summary>
public static class CalendarLabel
{
    public static string DayAccessName(DateOnly date, int count)
    {
        var noun = count == 1 ? "appointment" : "appointments";
        return $"{Weekday(date)} {date.Day} {Month(date)} {date.Year}, {count} {noun}";
    }

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
}
