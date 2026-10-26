using System.Reflection;
using BookingCalendar.App.State;
using BookingCalendar.Core;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BookingCalendar.App.Tests;

internal static class CalendarHarness
{
    public static AppointmentStore Store(IJSRuntime js, bool narrow)
    {
        var store = new AppointmentStore(js);
        var today = new DateOnly(2026, 10, 21);
        Set(store, nameof(AppointmentStore.Today), today);
        Set(store, nameof(AppointmentStore.Cursor), today);
        Set(store, nameof(AppointmentStore.Appointments), ExampleAppointments.Create(today));
        Set(store, nameof(AppointmentStore.Narrow), narrow);
        Set(store, nameof(AppointmentStore.Use24Hour), true);
        Set(store, nameof(AppointmentStore.Dark), false);
        Set(store, nameof(AppointmentStore.IsReady), true);
        return store;
    }

    public static IRenderedComponent<Pages.CalendarPage> Page(BunitContext context, bool narrow)
    {
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton(Store(context.JSInterop.JSRuntime, narrow));
        Action<ComponentParameterCollectionBuilder<Pages.CalendarPage>> parameters = _ => { };
        return context.Render(parameters);
    }

    private static void Set(AppointmentStore store, string name, object? value) =>
        typeof(AppointmentStore).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(store, value);
}
