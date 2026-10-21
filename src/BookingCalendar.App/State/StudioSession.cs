using BookingCalendar.Core;
using Microsoft.JSInterop;

namespace BookingCalendar.App.State;

public sealed class StudioSession
{
    public static readonly TimeSpan SaveDelay = TimeSpan.FromMilliseconds(1200);

    private readonly IJSRuntime _js;
    private readonly List<Booking> _bookings = [];
    private bool _todayFixed;

    public StudioSession(IJSRuntime js) => _js = js;

    public bool IsReady { get; private set; }
    public DateOnly Today { get; private set; }
    public TimeOnly Now { get; private set; }
    public IReadOnlyList<Booking> Bookings => _bookings;

    public string? ServiceId { get; private set; }
    public DateOnly? Date { get; private set; }
    public TimeOnly? Start { get; private set; }
    public string Name { get; private set; } = "";
    public string Email { get; private set; } = "";

    public Service? SelectedService => Catalog.Find(ServiceId);

    public async Task InitializeAsync(string absoluteUri)
    {
        if (IsReady)
            return;

        var options = PrototypeOptions.Parse(absoluteUri);
        var utcNow = DateTimeOffset.UtcNow;
        _todayFixed = options.Today is not null;
        Today = options.Today ?? SingaporeTime.Today(utcNow);
        Now = SingaporeTime.TimeOfDay(utcNow);

        if (options.Reset)
            await _js.InvokeVoidAsync("northQuay.clear");

        var json = options.Reset ? null : await _js.InvokeAsync<string?>("northQuay.load");
        if (!BookingArchive.TryParse(json, out var loaded))
        {
            _bookings.Clear();
            _bookings.AddRange(SeedBookings.Create(Today));
            await SaveAsync();
        }
        else
        {
            _bookings.AddRange(loaded);
        }

        IsReady = true;
    }

    public void RefreshClock()
    {
        var utcNow = DateTimeOffset.UtcNow;
        Now = SingaporeTime.TimeOfDay(utcNow);
        if (!_todayFixed)
            Today = SingaporeTime.Today(utcNow);
    }

    public void SelectService(Service service)
    {
        if (ServiceId != service.Id)
        {
            Date = null;
            Start = null;
        }

        ServiceId = service.Id;
    }

    public void SelectDate(DateOnly date)
    {
        if (Date != date)
            Start = null;

        Date = date;
    }

    public void SelectSlot(TimeOnly start) => Start = start;

    public void SetDetails(string name, string email)
    {
        Name = name.Trim();
        Email = email.Trim();
    }

    public Booking? TryConfirm()
    {
        RefreshClock();
        if (SelectedService is not { } service || Date is not { } date || Start is not { } start)
            return null;

        var booking = BookingRules.TryConfirm(
            _bookings,
            service,
            date,
            start,
            Name,
            Email,
            Today,
            Now,
            taken => ReferenceCodes.Create(taken, Random.Shared));

        if (booking is null)
            return null;

        _bookings.Add(booking);
        return booking;
    }

    public Booking? FindActive(string? bookingReference)
    {
        if (string.IsNullOrWhiteSpace(bookingReference))
            return null;

        return _bookings.FirstOrDefault(booking =>
            !booking.IsCancelled &&
            string.Equals(booking.BookingReference, bookingReference, StringComparison.OrdinalIgnoreCase));
    }

    public Booking? FindAny(string? bookingReference)
    {
        if (string.IsNullOrWhiteSpace(bookingReference))
            return null;

        return _bookings.FirstOrDefault(booking =>
            string.Equals(booking.BookingReference, bookingReference, StringComparison.OrdinalIgnoreCase));
    }

    public bool Cancel(string bookingReference)
    {
        var index = _bookings.FindIndex(booking =>
            string.Equals(booking.BookingReference, bookingReference, StringComparison.OrdinalIgnoreCase));
        if (index < 0 || _bookings[index].IsCancelled)
            return false;

        _bookings[index] = _bookings[index] with { IsCancelled = true };
        return true;
    }

    public Task SaveAsync() =>
        _js.InvokeVoidAsync("northQuay.save", BookingArchive.Serialize(_bookings)).AsTask();
}
