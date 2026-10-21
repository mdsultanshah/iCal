using System.Text.Json;
using BookingCalendar.Core;

namespace BookingCalendar.App.State;

public static class BookingArchive
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static string Serialize(IEnumerable<Booking> bookings)
    {
        var rows = bookings.Select(booking => new StoredBooking
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
        });

        return JsonSerializer.Serialize(rows, JsonOptions);
    }

    public static bool TryParse(string? json, out List<Booking> bookings)
    {
        bookings = [];
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            var rows = JsonSerializer.Deserialize<List<StoredBooking>>(json, JsonOptions);
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
        catch (JsonException)
        {
            bookings = [];
            return false;
        }
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

        var second = 0;
        if (parts.Length >= 3 && !int.TryParse(parts[2], out second))
            return false;

        try
        {
            time = new TimeOnly(hour, minute, second);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
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
