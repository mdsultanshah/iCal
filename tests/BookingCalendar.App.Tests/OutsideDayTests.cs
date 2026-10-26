namespace BookingCalendar.App.Tests;

/// <summary>C4. Days from the neighbouring months are grey in both themes, and today keeps its marker.</summary>
public class OutsideDayTests
{
    [Fact]
    public void Outside_days_are_grey_in_both_themes_and_today_keeps_its_marker()
    {
        var css = ReadCss();
        var light = Block(css, ":root");
        var dark = Block(css, "html[data-theme=\"dark\"]");

        Assert.True(light.TryGetValue("outside", out var lightOutside), "Light theme has no --outside grey.");
        Assert.True(dark.TryGetValue("outside", out var darkOutside), "Dark theme has no --outside grey.");
        Assert.NotEqual(light["card"], lightOutside);
        Assert.NotEqual(dark["card"], darkOutside);
        Assert.Contains(".cell.is-outside", css, StringComparison.Ordinal);
        Assert.Contains("background: var(--outside)", css, StringComparison.Ordinal);
        Assert.Contains(":not(.is-today)", css, StringComparison.Ordinal);
    }

    private static string ReadCss()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "BookingCalendar.App", "wwwroot", "css", "app.css");
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);

            dir = dir.Parent;
        }

        throw new FileNotFoundException("app.css");
    }

    private static Dictionary<string, string> Block(string css, string selector)
    {
        var start = css.IndexOf(selector, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Missing CSS block {selector}.");
        var open = css.IndexOf('{', start);
        var close = css.IndexOf('}', open);
        var body = css[(open + 1)..close];
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in body.Split('\n'))
        {
            var parts = line.Split(':', 2);
            if (parts.Length != 2 || !parts[0].Trim().StartsWith("--", StringComparison.Ordinal))
                continue;

            values[parts[0].Trim()[2..]] = parts[1].Trim().TrimEnd(';').Trim();
        }

        return values;
    }
}
