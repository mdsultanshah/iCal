namespace BookingCalendar.Core;

/// <summary>How a day's appointments appear in a month cell: three lines, then "+N more".</summary>
public static class DayAgenda
{
    public const int ChipLimit = 3;

    public static IReadOnlyList<Appointment> Ordered(IEnumerable<Appointment> items) =>
        items.OrderBy(item => item.Start).ThenBy(item => item.Id, StringComparer.Ordinal).ToList();

    public static IReadOnlyList<Appointment> Chips(IEnumerable<Appointment> items) =>
        Ordered(items).Take(ChipLimit).ToList();

    public static int MoreCount(int total) => total > ChipLimit ? total - ChipLimit : 0;

    public static string? MoreLabel(int total)
    {
        var more = MoreCount(total);
        return more == 0 ? null : $"+{more} more";
    }

    public static string ChipText(Appointment item, bool twentyFourHour) =>
        $"{ClockFormat.Format(item.Start, twentyFourHour)} {item.Title}";
}
