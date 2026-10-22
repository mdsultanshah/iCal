namespace BookingCalendar.Core;

/// <summary>
/// Test-only URL options. <c>?reset=1</c> clears saved appointments and restores the
/// three examples. <c>?today=yyyy-mm-dd</c> fixes what "today" is.
/// <c>?preview=empty</c> shows a week with nothing on it.
/// <c>?view=month</c>, <c>?view=week</c>, or <c>?view=day</c> forces the layout.
/// A <c>?today=</c> value that is not a real yyyy-mm-dd date is ignored.
/// </summary>
public sealed record PrototypeOptions(bool Reset, DateOnly? Today, bool PreviewEmpty, string? View)
{
    public static PrototypeOptions Parse(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return new PrototypeOptions(false, null, false, null);

        var queryIndex = url.IndexOf('?');
        if (queryIndex < 0 || queryIndex == url.Length - 1)
            return new PrototypeOptions(false, null, false, null);

        var query = url[(queryIndex + 1)..];
        var hash = query.IndexOf('#');
        if (hash >= 0)
            query = query[..hash];

        var reset = false;
        var previewEmpty = false;
        DateOnly? today = null;
        string? view = null;

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var eq = part.IndexOf('=');
            var rawKey = eq >= 0 ? part[..eq] : part;
            var rawValue = eq >= 0 ? part[(eq + 1)..] : "";
            var key = Uri.UnescapeDataString(rawKey);
            var value = Uri.UnescapeDataString(rawValue);

            if (key == "reset" && value == "1")
                reset = true;

            if (key == "today" && TryParseDate(value, out var date))
                today = date;

            if (key == "preview" && value == "empty")
                previewEmpty = true;

            if (key == "view" && value is "day" or "week" or "month")
                view = value;
        }

        return new PrototypeOptions(reset, today, previewEmpty, view);
    }

    public static bool TryParseDate(string? value, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrEmpty(value))
            return false;

        var bits = value.Split('-');
        if (bits.Length != 3)
            return false;

        if (bits[0].Length != 4 || bits[1].Length != 2 || bits[2].Length != 2)
            return false;

        if (!int.TryParse(bits[0], out var year) ||
            !int.TryParse(bits[1], out var month) ||
            !int.TryParse(bits[2], out var day))
            return false;

        try
        {
            date = new DateOnly(year, month, day);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
