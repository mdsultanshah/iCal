namespace BookingCalendar.Core;

/// <summary>A few fake bookings so some times are already taken. One weekday is full.</summary>
public static class SeedBookings
{
    private static readonly (string Name, string Email)[] Guests =
    [
        ("Amira Hassan", "amira@example.com"),
        ("Ben Koh", "ben@example.com"),
        ("Chloe Ng", "chloe@example.com"),
        ("Daniel Wee", "daniel@example.com"),
        ("Farah Lim", "farah@example.com"),
        ("Gopal Krishnan", "gopal@example.com"),
        ("Hui Min Teo", "huimin@example.com"),
        ("Ivan Pereira", "ivan@example.com"),
        ("Priya Nair", "priya@example.com"),
        ("Owen Tan", "owen@example.com"),
        ("Lina Cho", "lina@example.com"),
    ];

    public static IReadOnlyList<Booking> Create(DateOnly today)
    {
        var random = new Random(20261021);
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var bookings = new List<Booking>();

        string NextReference()
        {
            var code = ReferenceCodes.Create(codes, random);
            codes.Add(code);
            return code;
        }

        var open = BookingRules.NextDays(today)
            .Where(date => BookingRules.CanSelectDate(date, today))
            .ToList();

        if (open.Count >= 2)
        {
            var day = open[1];
            bookings.Add(Make(NextReference(), Catalog.Services[2], day, new TimeOnly(10, 0), Guests[8]));
            bookings.Add(Make(NextReference(), Catalog.Services[1], day, new TimeOnly(14, 0), Guests[9]));
        }

        if (open.Count >= 3)
        {
            var day = open[2];
            var service = Catalog.Services[2];
            for (var index = 0; index < 8; index++)
            {
                var start = BookingRules.FromMinutes((9 * 60) + (index * 60));
                bookings.Add(Make(NextReference(), service, day, start, Guests[index]));
            }
        }

        if (open.Count >= 4)
            bookings.Add(Make(NextReference(), Catalog.Services[0], open[3], new TimeOnly(9, 30), Guests[10]));

        return bookings;
    }

    public static DateOnly? FullDay(DateOnly today)
    {
        var open = BookingRules.NextDays(today)
            .Where(date => BookingRules.CanSelectDate(date, today))
            .ToList();

        return open.Count >= 3 ? open[2] : null;
    }

    private static Booking Make(
        string bookingReference,
        Service service,
        DateOnly date,
        TimeOnly start,
        (string Name, string Email) guest) =>
        new(
            bookingReference,
            service.Id,
            service.Name,
            service.LengthMinutes,
            date,
            start,
            guest.Name,
            guest.Email,
            false);
}
