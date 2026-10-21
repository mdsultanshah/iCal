using System.Text.Json;
using BookingCalendar.Core;

namespace BookingCalendar.App.State;

public sealed class StudioFile
{
    public required IReadOnlyList<Booking> Bookings { get; init; }
    public required WeeklySchedule Schedule { get; init; }
    public required IReadOnlyList<string> ServiceIds { get; init; }
}

public static class BookingArchive
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static string Serialize(StudioFile file)
    {
        var document = new StoredStudio
        {
            Bookings = file.Bookings.Select(Stored).ToList(),
            ServiceIds = file.ServiceIds.ToList(),
            Days = file.Schedule.Days.Select(day => new StoredDay
            {
                Day = day.Day.ToString(),
                IsOpen = day.IsOpen,
                Opens = DiaryText.Time(day.Opens),
                Closes = DiaryText.Time(day.Closes),
            }).ToList(),
        };

        return JsonSerializer.Serialize(document, JsonOptions);
    }

    public static bool TryRead(string? json, out StudioFile file)
    {
        file = new StudioFile
        {
            Bookings = [],
            Schedule = WeeklySchedule.Default(),
            ServiceIds = ServiceSelection.DefaultIds(),
        };

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            var trimmed = json.TrimStart();
            if (trimmed.StartsWith('['))
            {
                if (!TryBookings(trimmed, out var legacy))
                    return false;

                file = new StudioFile
                {
                    Bookings = legacy,
                    Schedule = WeeklySchedule.Default(),
                    ServiceIds = ServiceSelection.DefaultIds(),
                };
                return true;
            }

            var stored = JsonSerializer.Deserialize<StoredStudio>(json, JsonOptions);
            if (stored is null || !TryBookings(stored.Bookings, out var bookings))
                return false;

            var schedule = ReadSchedule(stored.Days) ?? WeeklySchedule.Default();
            var serviceIds = stored.ServiceIds is { Count: > 0 } && ServiceSelection.IsValid(stored.ServiceIds)
                ? stored.ServiceIds
                : ServiceSelection.DefaultIds();

            file = new StudioFile
            {
                Bookings = bookings,
                Schedule = schedule,
                ServiceIds = serviceIds,
            };
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static StoredBooking Stored(Booking booking) => new()
    {
        BookingReference = booking.BookingReference,
        ServiceId = booking.ServiceId,
        ServiceName = booking.ServiceName,
        LengthMinutes = booking.LengthMinutes,
        Date = DiaryText.Iso(booking.Date),
        Start = DiaryText.Time(booking.Start),
        CustomerName = booking.CustomerName,
        CustomerEmail = booking.CustomerEmail,
        IsCancelled = booking.IsCancelled,
    };

    private static bool TryBookings(string json, out List<Booking> bookings)
    {
        var rows = JsonSerializer.Deserialize<List<StoredBooking>>(json, JsonOptions);
        return TryBookings(rows, out bookings);
    }

    private static bool TryBookings(List<StoredBooking>? rows, out List<Booking> bookings)
    {
        bookings = [];
        if (rows is null)
            return false;

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.BookingReference) ||
                string.IsNullOrWhiteSpace(row.ServiceId) ||
                row.LengthMinutes <= 0 ||
                !PrototypeOptions.TryParseDate(row.Date, out var date) ||
                !TryParseTime(row.Start, out var start))
                return false;

            bookings.Add(new Booking(
                row.BookingReference,
                row.ServiceId,
                row.ServiceName ?? row.ServiceId,
                row.LengthMinutes,
                date,
                start,
                row.CustomerName ?? "",
                row.CustomerEmail ?? "",
                row.IsCancelled));
        }

        return true;
    }

    private static WeeklySchedule? ReadSchedule(List<StoredDay>? days)
    {
        if (days is null || days.Count == 0)
            return null;

        var parsed = new List<DayHours>();
        foreach (var day in days)
        {
            if (!Enum.TryParse<DayOfWeek>(day.Day, out var weekday) ||
                !TryParseTime(day.Opens, out var opens) ||
                !TryParseTime(day.Closes, out var closes))
                return null;

            parsed.Add(new DayHours(weekday, day.IsOpen, opens, closes));
        }

        var schedule = new WeeklySchedule(parsed);
        return WeeklySchedule.IsValid(schedule) ? schedule : null;
    }

    private static bool TryParseTime(string? value, out TimeOnly time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var parts = value.Split(':');
        if (parts.Length < 2)
            return false;

        if (!int.TryParse(parts[0], out var hour) || !int.TryParse(parts[1], out var minute))
            return false;

        try
        {
            time = new TimeOnly(hour, minute);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private sealed class StoredStudio
    {
        public List<StoredBooking> Bookings { get; set; } = [];
        public List<string> ServiceIds { get; set; } = [];
        public List<StoredDay> Days { get; set; } = [];
    }

    private sealed class StoredDay
    {
        public string Day { get; set; } = "";
        public bool IsOpen { get; set; }
        public string Opens { get; set; } = "";
        public string Closes { get; set; } = "";
    }

    private sealed class StoredBooking
    {
        public string BookingReference { get; set; } = "";
        public string ServiceId { get; set; } = "";
        public string? ServiceName { get; set; }
        public int LengthMinutes { get; set; }
        public string Date { get; set; } = "";
        public string Start { get; set; } = "";
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public bool IsCancelled { get; set; }
    }
}
