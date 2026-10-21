namespace BookingCalendar.Core;

/// <summary>
/// Weekly hours, customer time zones, and reschedule rules.
/// Times are generated in the business zone and shown in the customer zone.
/// </summary>
public static class Availability
{
    public static bool CanPick(DayState state) => state == DayState.Open;

    public static IReadOnlyList<DateOnly> CalendarDays(DateOnly today, int horizonDays = Catalog.HorizonDays)
    {
        var last = today.AddDays(horizonDays - 1);
        var start = today.AddDays(-(int)today.DayOfWeek);
        var end = last.AddDays(6 - (int)last.DayOfWeek);
        var days = new List<DateOnly>();
        for (var day = start; day <= end; day = day.AddDays(1))
            days.Add(day);

        return days;
    }

    public static IReadOnlyList<CustomerSlot> Possible(
        Service service,
        DateOnly customerDate,
        TimeZoneInfo customerZone,
        TimeZoneInfo businessZone,
        WeeklySchedule schedule)
    {
        var results = new List<CustomerSlot>();
        for (var shift = -1; shift <= 1; shift++)
        {
            var businessDate = customerDate.AddDays(shift);
            var hours = schedule.For(businessDate.DayOfWeek);
            if (!hours.IsOpen)
                continue;

            foreach (var start in BookingRules.StartsBetween(hours.Opens, hours.Closes, service.LengthMinutes))
            {
                var instant = ZoneClock.BusinessInstant(businessDate, start, businessZone);
                var shown = ZoneClock.InZone(instant, customerZone);
                if (shown.Date != customerDate)
                    continue;

                var endInstant = instant.AddMinutes(service.LengthMinutes);
                var shownEnd = ZoneClock.InZone(endInstant, customerZone);
                var businessEnd = BookingRules.FromMinutes(BookingRules.ToMinutes(start) + service.LengthMinutes);
                results.Add(new CustomerSlot(
                    businessDate,
                    start,
                    businessEnd,
                    customerDate,
                    shown.Time,
                    shownEnd.Time));
            }
        }

        return results;
    }

    public static IReadOnlyList<CustomerSlot> Free(
        Service service,
        DateOnly customerDate,
        DateTimeOffset now,
        TimeZoneInfo customerZone,
        TimeZoneInfo businessZone,
        WeeklySchedule schedule,
        IEnumerable<Booking> bookings,
        string? ignoreReference = null)
    {
        var today = ZoneClock.InZone(now, customerZone).Date;
        if (customerDate < today || customerDate > today.AddDays(Catalog.HorizonDays - 1))
            return [];

        var active = bookings.Where(booking =>
            !booking.IsCancelled &&
            !string.Equals(booking.BookingReference, ignoreReference, StringComparison.OrdinalIgnoreCase)).ToList();

        var free = new List<CustomerSlot>();
        foreach (var slot in Possible(service, customerDate, customerZone, businessZone, schedule))
        {
            var instant = ZoneClock.BusinessInstant(slot.BusinessDate, slot.BusinessStart, businessZone);
            if (instant < now)
                continue;

            if (active.Any(booking =>
                    booking.Date == slot.BusinessDate &&
                    BookingRules.Overlaps(slot.BusinessStart, service.LengthMinutes, booking.Start, booking.LengthMinutes)))
                continue;

            free.Add(slot);
        }

        return free;
    }

    public static DayState State(
        Service service,
        DateOnly customerDate,
        DateTimeOffset now,
        TimeZoneInfo customerZone,
        TimeZoneInfo businessZone,
        WeeklySchedule schedule,
        IEnumerable<Booking> bookings,
        string? ignoreReference = null)
    {
        var today = ZoneClock.InZone(now, customerZone).Date;
        if (customerDate < today)
            return DayState.Past;

        if (customerDate > today.AddDays(Catalog.HorizonDays - 1))
            return DayState.Outside;

        if (Possible(service, customerDate, customerZone, businessZone, schedule).Count == 0)
            return DayState.Closed;

        if (Free(service, customerDate, now, customerZone, businessZone, schedule, bookings, ignoreReference).Count == 0)
            return DayState.Full;

        return DayState.Open;
    }

    public static DateOnly? NextOpenDay(
        Service service,
        DateOnly after,
        DateTimeOffset now,
        TimeZoneInfo customerZone,
        TimeZoneInfo businessZone,
        WeeklySchedule schedule,
        IEnumerable<Booking> bookings)
    {
        var today = ZoneClock.InZone(now, customerZone).Date;
        foreach (var date in BookingRules.NextDays(today))
        {
            if (date <= after)
                continue;

            if (State(service, date, now, customerZone, businessZone, schedule, bookings) == DayState.Open)
                return date;
        }

        return null;
    }

    public static Booking? TryConfirm(
        IReadOnlyList<Booking> existing,
        Service service,
        DateOnly businessDate,
        TimeOnly businessStart,
        string name,
        string email,
        DateTimeOffset now,
        TimeZoneInfo customerZone,
        TimeZoneInfo businessZone,
        WeeklySchedule schedule,
        Func<IReadOnlySet<string>, string> nextReference)
    {
        if (CustomerDetails.NameError(name) is not null || CustomerDetails.EmailError(email) is not null)
            return null;

        var customerDate = ZoneClock.InZone(
            ZoneClock.BusinessInstant(businessDate, businessStart, businessZone),
            customerZone).Date;
        var free = Free(service, customerDate, now, customerZone, businessZone, schedule, existing);
        if (!free.Any(slot => slot.BusinessDate == businessDate && slot.BusinessStart == businessStart))
            return null;

        var taken = existing
            .Select(booking => booking.BookingReference)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new Booking(
            nextReference(taken),
            service.Id,
            service.Name,
            service.LengthMinutes,
            businessDate,
            businessStart,
            name.Trim(),
            email.Trim(),
            false);
    }

    public static Booking? TryReschedule(
        IReadOnlyList<Booking> existing,
        string bookingReference,
        DateOnly businessDate,
        TimeOnly businessStart,
        DateTimeOffset now,
        TimeZoneInfo customerZone,
        TimeZoneInfo businessZone,
        WeeklySchedule schedule)
    {
        var current = existing.FirstOrDefault(booking =>
            string.Equals(booking.BookingReference, bookingReference, StringComparison.OrdinalIgnoreCase));
        if (current is null || current.IsCancelled)
            return null;

        var service = new Service(current.ServiceId, current.ServiceName, current.LengthMinutes, "");
        var customerDate = ZoneClock.InZone(
            ZoneClock.BusinessInstant(businessDate, businessStart, businessZone),
            customerZone).Date;
        var free = Free(
            service,
            customerDate,
            now,
            customerZone,
            businessZone,
            schedule,
            existing,
            current.BookingReference);
        if (!free.Any(slot => slot.BusinessDate == businessDate && slot.BusinessStart == businessStart))
            return null;

        return current with { Date = businessDate, Start = businessStart };
    }
}
