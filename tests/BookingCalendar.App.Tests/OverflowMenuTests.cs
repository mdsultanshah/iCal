using Bunit;

namespace BookingCalendar.App.Tests;

/// <summary>C1. At 390px the header actions sit in an overflow menu.</summary>
public class OverflowMenuTests : BunitContext
{
    [Fact]
    public void Narrow_header_puts_actions_in_an_overflow_menu()
    {
        var page = CalendarHarness.Page(this, narrow: true);

        Assert.Empty(page.FindAll("header .clock"));
        var more = page.Find("#header-more");
        more.Click();

        Assert.NotEmpty(page.FindAll("#header-menu .clock"));
        Assert.NotEmpty(page.FindAll("#header-menu .theme"));
    }
}
