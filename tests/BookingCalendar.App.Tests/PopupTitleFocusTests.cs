using System.Reflection;
using BookingCalendar.App.Components;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BookingCalendar.App.Tests;

/// <summary>
/// G1. The day pop-up focuses the title input as soon as it first renders,
/// before any other script runs, so keystrokes cannot land on the day cell.
/// </summary>
public class PopupTitleFocusTests : BunitContext
{
    public PopupTitleFocusTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Day_popup_focuses_the_title_when_it_first_renders()
    {
        Action<ComponentParameterCollectionBuilder<QuickAdd>> parameters = p =>
        {
            p.Add(x => x.Date, new DateOnly(2026, 10, 21));
            p.Add(x => x.Start, "10:00");
        };
        var popup = Render(parameters);

        var focus = JSInterop.VerifyFocusAsyncInvoke();
        var title = (ElementReference)typeof(QuickAdd)
            .GetField("_titleInput", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(popup.Instance)!;
        var focused = Assert.IsType<ElementReference>(focus.Arguments[0]);
        Assert.Equal(title.Id, focused.Id);
        Assert.False(string.IsNullOrEmpty(title.Id));

        var first = JSInterop.Invocations.First();
        Assert.Equal(focus.Identifier, first.Identifier);
    }
}
