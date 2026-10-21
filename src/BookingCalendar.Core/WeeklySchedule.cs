namespace BookingCalendar.Core;

public sealed record DayHours(DayOfWeek Day, bool IsOpen, TimeOnly Opens, TimeOnly Closes);

/// <summary>Weekly opening hours in the business zone. Closed days offer no times.</summary>
public sealed class WeeklySchedule
{
    public WeeklySchedule(IEnumerable<DayHours> days) =>
        Days = days.ToList();

    public IReadOnlyList<DayHours> Days { get; }

    public DayHours For(DayOfWeek day) =>
        Days.FirstOrDefault(item => item.Day == day)
        ?? new DayHours(day, false, Catalog.Opens, Catalog.Closes);

    public WeeklySchedule Set(DayOfWeek day, bool isOpen, TimeOnly opens, TimeOnly closes) =>
        new(Days.Select(item => item.Day == day ? new DayHours(day, isOpen, opens, closes) : item));

    public static WeeklySchedule Default()
    {
        DayOfWeek[] order =
        [
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday,
        ];

        return new WeeklySchedule(order.Select(day =>
        {
            var open = day is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
            return new DayHours(day, open, Catalog.Opens, Catalog.Closes);
        }));
    }

    public static bool IsValid(WeeklySchedule schedule)
    {
        if (schedule.Days.Select(day => day.Day).Distinct().Count() != 7)
            return false;

        if (!schedule.Days.Any(day => day.IsOpen))
            return false;

        return schedule.Days.Where(day => day.IsOpen).All(day => day.Closes > day.Opens);
    }
}
