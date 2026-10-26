using BookingCalendar.Core;
using Microsoft.JSInterop;

namespace BookingCalendar.App.State;

/// <summary>Appointments for this browser, plus the day and view the calendar is showing.</summary>
public sealed class AppointmentStore : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<AppointmentStore>? _self;
    private int _toastToken;

    public AppointmentStore(IJSRuntime js) => _js = js;

    public bool IsReady { get; private set; }
    public bool Narrow { get; private set; }
    public string ZoneLabel { get; private set; } = "";
    public DateOnly Today { get; private set; }
    public DateOnly Cursor { get; private set; }
    public IReadOnlyList<Appointment> Appointments { get; private set; } = [];
    public string? Toast { get; private set; }
    public bool Use24Hour { get; private set; } = true;
    public bool Dark { get; private set; }

    public bool HasExamples => Appointments.Any(item => item.IsExample);

    public event Action? Changed;

    public async Task InitializeAsync(string uri)
    {
        var options = PrototypeOptions.Parse(uri);

        var zone = await _js.InvokeAsync<ZoneInfo>("calendar.zone");
        ZoneLabel = string.IsNullOrWhiteSpace(zone.TimeZone) ? "UTC" : zone.TimeZone;

        Today = options.Today ?? DateOnly.FromDateTime(DateTime.Now);
        Cursor = Today;

        var theme = await _js.InvokeAsync<string?>("calendar.loadTheme");
        var prefersDark = await _js.InvokeAsync<bool>("calendar.prefersDark");
        Dark = theme == "dark" || (theme != "light" && prefersDark);
        await _js.InvokeVoidAsync("calendar.applyTheme", Dark ? "dark" : "light");

        var clock = await _js.InvokeAsync<string?>("calendar.loadClock");
        Use24Hour = clock != "12";

        if (options.Reset)
            await _js.InvokeVoidAsync("calendar.clear");

        if (options.PreviewEmpty)
        {
            Appointments = [];
        }
        else
        {
            var json = await _js.InvokeAsync<string?>("calendar.load");
            if (!AppointmentArchive.TryRead(json, out var loaded))
            {
                Appointments = ExampleAppointments.Create(Today);
                await PersistAsync();
            }
            else
            {
                Appointments = loaded;
            }
        }

        Narrow = await _js.InvokeAsync<bool>("calendar.isNarrow");
        _self = DotNetObjectReference.Create(this);
        await _js.InvokeVoidAsync("calendar.watchNarrow", _self);
        IsReady = true;
        Changed?.Invoke();
    }

    [JSInvokable]
    public void SetNarrow(bool narrow)
    {
        if (Narrow == narrow)
            return;

        Narrow = narrow;
        Changed?.Invoke();
    }

    public void Select(DateOnly date)
    {
        if (Cursor == date)
            return;

        Cursor = date;
        Changed?.Invoke();
    }

    public void ShowToday()
    {
        if (Cursor == Today)
            return;

        Cursor = Today;
        Changed?.Invoke();
    }

    public void Previous() => Move(-1);

    public void Next() => Move(1);

    public async Task ToggleThemeAsync()
    {
        Dark = !Dark;
        var name = Dark ? "dark" : "light";
        await _js.InvokeVoidAsync("calendar.saveTheme", name);
        await _js.InvokeVoidAsync("calendar.applyTheme", name);
        Changed?.Invoke();
    }

    public async Task SetClockAsync(bool twentyFourHour)
    {
        if (Use24Hour == twentyFourHour)
            return;

        Use24Hour = twentyFourHour;
        await _js.InvokeVoidAsync("calendar.saveClock", twentyFourHour ? "24" : "12");
        Changed?.Invoke();
    }

    public async Task<SaveOutcome> SaveAsync(
        string? id,
        string? title,
        DateOnly date,
        TimeOnly start,
        TimeOnly end,
        string? note)
    {
        var outcome = AppointmentRules.Save(Appointments, id, title, date, start, end, note, NextId);
        if (!outcome.Ok)
            return outcome;

        Appointments = outcome.Appointments;
        await PersistAsync();
        ShowToast("Appointment saved");
        return outcome;
    }

    public async Task DeleteAsync(string id)
    {
        Appointments = AppointmentRules.Delete(Appointments, id);
        await PersistAsync();
        ShowToast("Appointment deleted");
    }

    public async Task ClearExamplesAsync()
    {
        Appointments = AppointmentRules.ClearExamples(Appointments);
        await PersistAsync();
        Changed?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("calendar.unwatchNarrow");
        }
        catch (JSException)
        {
            // The page is going away.
        }

        _self?.Dispose();
    }

    private void Move(int direction)
    {
        Cursor = CalendarRange.ShiftMonth(Cursor, direction);
        Changed?.Invoke();
    }

    private void ShowToast(string message)
    {
        Toast = message;
        var token = ++_toastToken;
        _ = DismissToast(token);
        Changed?.Invoke();
    }

    private async Task DismissToast(int token)
    {
        await Task.Delay(TimeSpan.FromSeconds(4));
        if (token != _toastToken)
            return;

        Toast = null;
        Changed?.Invoke();
    }

    private async Task PersistAsync()
    {
        await _js.InvokeVoidAsync("calendar.save", AppointmentArchive.Write(Appointments));
    }

    private static string NextId() => Guid.NewGuid().ToString("N")[..8];

    private sealed record ZoneInfo(string TimeZone, int OffsetMinutes);
}
