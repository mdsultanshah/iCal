using System.Globalization;
using System.Text.Json;
using BookingCalendar.Core;

namespace BookingCalendar.App.State;

/// <summary>Reads and writes appointments as JSON for local storage.</summary>
public static class AppointmentArchive
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Write(IReadOnlyList<Appointment> appointments)
    {
        var rows = appointments.Select(item => new StoredAppointment(
            item.Id,
            item.Title,
            item.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            item.Start.ToString("HH:mm", CultureInfo.InvariantCulture),
            item.End.ToString("HH:mm", CultureInfo.InvariantCulture),
            item.Note,
            item.IsExample));
        return JsonSerializer.Serialize(rows, Options);
    }

    /// <summary>
    /// Returns false when the stored value is missing or not an array.
    /// An empty array is a real "nothing saved" list and returns true.
    /// </summary>
    public static bool TryRead(string? json, out IReadOnlyList<Appointment> appointments)
    {
        appointments = [];
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return false;

            var list = new List<Appointment>();
            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (ReadOne(item) is { } appointment)
                    list.Add(appointment);
            }

            appointments = list;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static Appointment? ReadOne(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
            return null;

        if (!TryString(item, "id", out var id) || string.IsNullOrWhiteSpace(id))
            return null;
        if (!TryString(item, "title", out var title) || string.IsNullOrWhiteSpace(title))
            return null;
        if (!TryString(item, "date", out var dateText) || !PrototypeOptions.TryParseDate(dateText, out var date))
            return null;
        if (!TryString(item, "start", out var startText) || !CalendarText.TryParseTime(startText, out var start))
            return null;
        if (!TryString(item, "end", out var endText) || !CalendarText.TryParseTime(endText, out var end))
            return null;
        if (end <= start)
            return null;

        var note = TryString(item, "note", out var noteText) ? noteText : "";
        var isExample = item.TryGetProperty("isExample", out var flag) && flag.ValueKind == JsonValueKind.True;

        return new Appointment(id, title.Trim(), date, start, end, note.Trim(), isExample);
    }

    private static bool TryString(JsonElement item, string name, out string value)
    {
        value = "";
        if (!item.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
            return false;

        value = property.GetString() ?? "";
        return true;
    }

    private sealed record StoredAppointment(
        string Id,
        string Title,
        string Date,
        string Start,
        string End,
        string Note,
        bool IsExample);
}
