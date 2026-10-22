namespace BookingCalendar.Core;

/// <summary>Exactly three sample appointments, always inside the week that contains today.</summary>
public static class ExampleAppointments
{
    public static IReadOnlyList<Appointment> Create(DateOnly today)
    {
        var week = CalendarRange.Week(today);
        var first = week[3];
        var second = week[4];
        return
        [
            new("ex-colour", "Colour plan", first, new TimeOnly(10, 0), new TimeOnly(11, 0), "Talk through a colour change.", true),
            new("ex-trim", "Trim with Hana", first, new TimeOnly(10, 30), new TimeOnly(11, 30), "A short trim.", true),
            new("ex-tidy", "Studio tidy", second, new TimeOnly(14, 0), new TimeOnly(15, 30), "Reset the room.", true),
        ];
    }
}
