using BookingCalendar.App.Components;
using BookingCalendar.App.State;
using Bunit;

namespace BookingCalendar.App.Tests;

/// <summary>C9. The pop-up saves and cancels, and panel focus moves in and back to the day.</summary>
public class PopupAndFocusTests : BunitContext
{
    public PopupAndFocusTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Save_adds_the_typed_appointment()
    {
        QuickSave? saved = null;
        var popup = Open(save => saved = save, close: null);
        popup.Find("#quick-title").Input("Fitting");
        popup.Find("form").Submit();

        Assert.NotNull(saved);
        Assert.Equal("Fitting", saved.Title);
        Assert.Equal(new TimeOnly(10, 0), saved.Start);
    }

    [Fact]
    public void Close_does_not_add_the_appointment()
    {
        var saved = 0;
        var closed = false;
        var popup = Open(_ => saved++, () => closed = true);
        popup.Find("#quick-title").Input("Fitting");
        popup.Find("button.ghost").Click();

        Assert.True(closed);
        Assert.Equal(0, saved);
    }

    [Fact]
    public void Opening_the_panel_moves_focus_inside_it_and_closing_returns_to_the_day()
    {
        var page = CalendarHarness.Page(this, narrow: false);
        page.Find("[data-appointment='ex-colour']").Click();

        var intoPanel = LastFocus("Opening the panel did not move focus into it.");
        Assert.Equal("#panel-close", intoPanel.Arguments[0]?.ToString());

        page.Find("#panel-close").Click();
        var back = LastFocus("Closing the panel did not move focus back to the day.");
        Assert.Equal("[data-month-grid] [data-date='2026-10-22']", back.Arguments[0]?.ToString());
    }

    private IRenderedComponent<QuickAdd> Open(Action<QuickSave> save, Action? close)
    {
        Action<ComponentParameterCollectionBuilder<QuickAdd>> parameters = p =>
        {
            p.Add(x => x.Date, new DateOnly(2026, 10, 21));
            p.Add(x => x.Start, "10:00");
            p.Add(x => x.OnSave, save);
            if (close is not null)
                p.Add(x => x.OnClose, close);
        };
        return Render(parameters);
    }

    private JSRuntimeInvocation LastFocus(string because)
    {
        var matches = JSInterop.Invocations
            .Where(call => call.Identifier.EndsWith("focus", StringComparison.Ordinal))
            .ToList();
        Assert.True(matches.Count > 0, because);
        return matches[^1];
    }
}
