using Bunit;

namespace BookingCalendar.App.Tests;

/// <summary>C3. Busy-day dots show on the phone and stay hidden where titles are shown.</summary>
public class BusyDotTests : BunitContext
{
    [Fact]
    public void Desktop_month_hides_busy_day_dots()
    {
        var page = CalendarHarness.Page(this, narrow: false);
        var dots = page.FindAll(".busy-dot");
        Assert.True(dots.Count == 0, $"Expected no busy-day dots when titles are shown, found {dots.Count}.");
    }

    [Fact]
    public void Phone_month_shows_a_dot_on_a_busy_day()
    {
        var page = CalendarHarness.Page(this, narrow: true);
        Assert.NotEmpty(page.FindAll(".busy-dot.is-on"));
    }
}
