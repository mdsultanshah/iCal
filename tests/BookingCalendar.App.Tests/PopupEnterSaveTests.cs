using BookingCalendar.App.Components;
using BookingCalendar.App.State;
using Bunit;

namespace BookingCalendar.App.Tests;

/// <summary>
/// G2. Enter in the title box saves the typed title without the field losing focus first.
/// </summary>
public class PopupEnterSaveTests : BunitContext
{
    public PopupEnterSaveTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Enter_in_the_title_saves_without_the_field_losing_focus()
    {
        QuickSave? saved = null;
        Action<ComponentParameterCollectionBuilder<QuickAdd>> parameters = p =>
        {
            p.Add(x => x.Date, new DateOnly(2026, 10, 21));
            p.Add(x => x.Start, "10:00");
            p.Add(x => x.OnSave, (QuickSave save) => saved = save);
        };
        var popup = Render(parameters);

        popup.Find("#quick-title").Input("Golf Hotel 4");
        popup.Find("#quick-start").Input("16:00");
        popup.Find("form").Submit();

        Assert.DoesNotContain("Enter a title.", popup.Markup);
        Assert.NotNull(saved);
        Assert.Equal("Golf Hotel 4", saved.Title);
        Assert.Equal(new TimeOnly(16, 0), saved.Start);
    }
}
