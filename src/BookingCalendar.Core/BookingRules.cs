namespace BookingCalendar.Core;

/// <summary>
/// Slot generation and the booking rules: a service must fit inside opening
/// hours, past slots are not offered, and a provider cannot be double booked.
/// </summary>
public static class BookingRules
{
    public static bool IsWeekend(DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool IsPastDay(DateOnly date, DateOnly today) => date < today;

    public static bool CanSelectDate(DateOnly date, DateOnly today)
    {
        if (date < today || date > today.AddDays(Catalog.HorizonDays - 1))
            return false;

        return !IsWeekend(date);
    }

    public static IReadOnlyList<DateOnly> NextDays(DateOnly today, int count = Catalog.HorizonDays)
    {
        var days = new List<DateOnly>(count);
        for (var i = 0; i < count; i++)
            days.Add(today.AddDays(i));

        return days;
    }

    public static int ToMinutes(TimeOnly time) => time.Hour * 60 + time.Minute;

    public static TimeOnly FromMinutes(int minutes) => new(minutes / 60, minutes % 60);

    public static bool FitsOpeningHours(TimeOnly start, int lengthMinutes)
    {
        if (lengthMinutes <= 0)
            return false;

        var startMinutes = ToMinutes(start);
        var open = ToMinutes(Catalog.Opens);
        var close = ToMinutes(Catalog.Closes);
        if (startMinutes < open)
            return false;

        return startMinutes + lengthMinutes <= close;
    }

    public static IReadOnlyList<TimeOnly> CandidateStarts(int lengthMinutes) =>
        StartsBetween(Catalog.Opens, Catalog.Closes, lengthMinutes);

    public static IReadOnlyList<TimeOnly> StartsBetween(TimeOnly opens, TimeOnly closes, int lengthMinutes)
    {
        var starts = new List<TimeOnly>();
        if (lengthMinutes <= 0 || closes <= opens)
            return starts;

        var cursor = ToMinutes(opens);
        var close = ToMinutes(closes);
        while (cursor + lengthMinutes <= close)
        {
            starts.Add(FromMinutes(cursor));
            cursor += Catalog.SlotStepMinutes;
        }

        return starts;
    }

    public static bool Overlaps(TimeOnly start, int lengthMinutes, TimeOnly otherStart, int otherLength)
    {
        var a0 = ToMinutes(start);
        var a1 = a0 + lengthMinutes;
        var b0 = ToMinutes(otherStart);
        var b1 = b0 + otherLength;
        return a0 < b1 && b0 < a1;
    }

    public static bool IsPastSlot(DateOnly date, TimeOnly start, DateOnly today, TimeOnly now)
    {
        if (date < today)
            return true;

        if (date > today)
            return false;

        return start < now;
    }

    public static IReadOnlyList<Slot> FreeSlots(
        Service service,
        DateOnly date,
        DateOnly today,
        TimeOnly now,
        IEnumerable<Booking> bookings)
    {
        if (!CanSelectDate(date, today))
            return [];

        var active = bookings.Where(booking => !booking.IsCancelled && booking.Date == date).ToList();
        var slots = new List<Slot>();
        foreach (var start in CandidateStarts(service.LengthMinutes))
        {
            if (IsPastSlot(date, start, today, now))
                continue;

            if (active.Any(booking => Overlaps(start, service.LengthMinutes, booking.Start, booking.LengthMinutes)))
                continue;

            slots.Add(new Slot(start, FromMinutes(ToMinutes(start) + service.LengthMinutes)));
        }

        return slots;
    }

    public static DateOnly? NextDayWithFreeSlots(
        Service service,
        DateOnly after,
        DateOnly today,
        TimeOnly now,
        IEnumerable<Booking> bookings)
    {
        foreach (var date in NextDays(today))
        {
            if (date <= after)
                continue;

            if (FreeSlots(service, date, today, now, bookings).Count > 0)
                return date;
        }

        return null;
    }

    public static Booking? TryConfirm(
        IReadOnlyList<Booking> existing,
        Service service,
        DateOnly date,
        TimeOnly start,
        string name,
        string email,
        DateOnly today,
        TimeOnly now,
        Func<IReadOnlySet<string>, string> nextReference)
    {
        if (NameError(name) is not null || EmailError(email) is not null)
            return null;

        if (!FreeSlots(service, date, today, now, existing).Any(slot => slot.Start == start))
            return null;

        var taken = existing
            .Select(booking => booking.BookingReference)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var bookingReference = nextReference(taken);

        return new Booking(
            bookingReference,
            service.Id,
            service.Name,
            service.LengthMinutes,
            date,
            start,
            name.Trim(),
            email.Trim(),
            false);
    }

    private static string? NameError(string name) => CustomerDetails.NameError(name);

    private static string? EmailError(string email) => CustomerDetails.EmailError(email);
}
