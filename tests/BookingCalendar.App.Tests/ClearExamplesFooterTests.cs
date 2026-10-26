using Bunit;

namespace BookingCalendar.App.Tests;

/// <summary>C7. Clear examples lives in the footer and still asks before removing examples.</summary>
public class ClearExamplesFooterTests : BunitContext
{
    [Fact]
    public void Clear_examples_sits_in_the_footer_and_confirms_first()
    {
        var page = CalendarHarness.Page(this, narrow: false);

        Assert.DoesNotContain("Clear examples", page.Find("header").TextContent, StringComparison.Ordinal);
        var footer = page.Find("footer");
        Assert.Contains("Clear examples", footer.TextContent, StringComparison.Ordinal);

        footer.QuerySelector("button")!.Click();
        Assert.Contains("Clear example appointments?", page.Markup, StringComparison.Ordinal);
    }
}
