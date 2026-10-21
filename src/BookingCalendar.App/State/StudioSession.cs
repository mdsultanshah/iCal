using BookingCalendar.Core;
using Microsoft.JSInterop;

namespace BookingCalendar.App.State;

public sealed class StudioSession
{
    public static readonly TimeSpan SaveDelay = TimeSpan.FromMilliseconds(1200);

    private readonly IJSRuntime _js;
    private readonly List<Booking> _bookings = [];
    private List<string> _serviceIds = ServiceSelection.DefaultIds().ToList();
    private bool _todayFixed;

    public StudioSession(IJSRuntime js) => _js = js;

    public bool IsReady { get; private set; }
    public bool PreviewEmpty { get; private set; }
    public WeeklySchedule Schedule { get; private set; } = WeeklySchedule.Default();
    public TimeZoneInfo BusinessZone { get; private set; } = ZoneClock.Singapore();
    public TimeZoneInfo CustomerZone { get; private set; } = ZoneClock.Singapore();
    public string CustomerZoneLabel { get; private set; } = SingaporeTime.ZoneId;
    public DateOnly Today { get; private set; }
    public DateTimeOffset NowInstant { get; private set; }
    public IReadOnlyList<Booking> Bookings => _bookings;

    public string? ServiceId { get; private set; }
    public DateOnly? CustomerDate { get; private set; }
    public DateOnly? BusinessDate { get; private set; }
    public TimeOnly? BusinessStart { get; private set; }
    public TimeOnly? CustomerStart { get; private set; }
    public TimeOnly? CustomerEnd { get; private set; }
    public string Name { get; private set; } = "";
    public string Email { get; private set; } = "";

    public IReadOnlyList<Service> Offered =>
        Catalog.Services.Where(service => _serviceIds.Contains(service.Id)).ToList();

    public Service? SelectedService =>
        Catalog.Find(ServiceId) is { } service && _serviceIds.Contains(service.Id) ? service : null;

    public async Task InitializeAsync(string absoluteUri)
    {
        if (IsReady)
            return;

        var options = PrototypeOptions.Parse(absoluteUri);
        PreviewEmpty = options.PreviewEmpty;
        var zone = await _js.InvokeAsync<ZoneSnapshot>("northQuay.zone");
        CustomerZoneLabel = string.IsNullOrWhiteSpace(zone.TimeZone) ? "UTC" : zone.TimeZone;
        CustomerZone = ZoneClock.FindOrFixed(CustomerZoneLabel, TimeSpan.FromMinutes(zone.OffsetMinutes));
        BusinessZone = ZoneClock.Singapore();
        ApplyClock(options.Today);
        _todayFixed = options.Today is not null;

        if (options.Reset)
            await _js.InvokeVoidAsync("northQuay.clear");

        var json = options.Reset ? null : await _js.InvokeAsync<string?>("northQuay.load");
        if (!BookingArchive.TryRead(json, out var file))
        {
            Schedule = WeeklySchedule.Default();
            _serviceIds = ServiceSelection.DefaultIds().ToList();
            _bookings.Clear();
            _bookings.AddRange(SeedBookings.Create(BusinessToday()));
            await SaveAsync();
        }
        else
        {
            Schedule = file.Schedule;
            _serviceIds = file.ServiceIds.ToList();
            _bookings.AddRange(file.Bookings);
        }

        IsReady = true;
    }

    public void RefreshClock() => ApplyClock(_todayFixed ? Today : null);

    public void SelectService(Service service)
    {
        if (ServiceId != service.Id)
            ClearTime();

        ServiceId = service.Id;
    }

    public void SelectDate(DateOnly customerDate)
    {
        if (CustomerDate != customerDate)
            ClearTime();

        CustomerDate = customerDate;
    }

    public void SelectTime(CustomerSlot slot)
    {
        CustomerDate = slot.CustomerDate;
        BusinessDate = slot.BusinessDate;
        BusinessStart = slot.BusinessStart;
        CustomerStart = slot.CustomerStart;
        CustomerEnd = slot.CustomerEnd;
    }

    public void SetDetails(string name, string email)
    {
        Name = name.Trim();
        Email = email.Trim();
    }

    public void SetSchedule(WeeklySchedule schedule) => Schedule = schedule;

    public async Task SaveSetupAsync(WeeklySchedule schedule, IReadOnlyList<string> serviceIds)
    {
        Schedule = schedule;
        _serviceIds = serviceIds.ToList();
        if (SelectedService is null)
            ServiceId = null;

        await SaveAsync();
    }

    public void OpenEmptyPreview()
    {
        if (!PreviewEmpty)
            return;

        PreviewEmpty = false;
        var full = SeedBookings.FullDay(BusinessToday());
        if (full is not DateOnly day)
            return;

        var service = Offered.FirstOrDefault(item => item.Id == "cut-style") ?? Offered.FirstOrDefault();
        if (service is not null)
            ServiceId = service.Id;

        CustomerDate = day;
        ClearTime();
    }

    public IReadOnlyList<CustomerSlot> FreeTimes(DateOnly customerDate, string? ignoreReference = null)
    {
        var lengthService = serviceFor(ignoreReference) ?? SelectedService;
        if (lengthService is null)
            return [];

        return Availability.Free(
            lengthService,
            customerDate,
            NowInstant,
            CustomerZone,
            BusinessZone,
            Schedule,
            _bookings,
            ignoreReference);
    }

    public DayState StateFor(DateOnly customerDate, Service? service = null, string? ignoreReference = null)
    {
        var chosen = service ?? SelectedService ?? serviceFor(ignoreReference);
        if (chosen is null)
            return DayState.Closed;

        return Availability.State(
            chosen,
            customerDate,
            NowInstant,
            CustomerZone,
            BusinessZone,
            Schedule,
            _bookings,
            ignoreReference);
    }

    public DateOnly? NextOpen(DateOnly after, string? ignoreReference = null)
    {
        if (SelectedService is not { } service)
            return null;

        return Availability.NextOpenDay(
            service,
            after,
            NowInstant,
            CustomerZone,
            BusinessZone,
            Schedule,
            _bookings);
    }

    public Booking? TryConfirm()
    {
        RefreshClock();
        if (SelectedService is not { } service || BusinessDate is not { } date || BusinessStart is not { } start)
            return null;

        var booking = Availability.TryConfirm(
            _bookings,
            service,
            date,
            start,
            Name,
            Email,
            NowInstant,
            CustomerZone,
            BusinessZone,
            Schedule,
            taken => ReferenceCodes.Create(taken, Random.Shared));

        if (booking is null)
            return null;

        _bookings.Add(booking);
        return booking;
    }

    public Booking? TryReschedule(string bookingReference, CustomerSlot slot)
    {
        RefreshClock();
        var updated = Availability.TryReschedule(
            _bookings,
            bookingReference,
            slot.BusinessDate,
            slot.BusinessStart,
            NowInstant,
            CustomerZone,
            BusinessZone,
            Schedule);

        if (updated is null)
            return null;

        var index = _bookings.FindIndex(booking =>
            string.Equals(booking.BookingReference, bookingReference, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return null;

        _bookings[index] = updated;
        return updated;
    }

    public (DateOnly Date, TimeOnly Start, TimeOnly End) Display(Booking booking)
    {
        var start = ZoneClock.BusinessInstant(booking.Date, booking.Start, BusinessZone);
        var shownStart = ZoneClock.InZone(start, CustomerZone);
        var shownEnd = ZoneClock.InZone(start.AddMinutes(booking.LengthMinutes), CustomerZone);
        return (shownStart.Date, shownStart.Time, shownEnd.Time);
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

    public void ClearDraft()
    {
        ServiceId = null;
        CustomerDate = null;
        Name = "";
        Email = "";
        ClearTime();
    }

    public Task SaveAsync() =>
        _js.InvokeVoidAsync("northQuay.save", BookingArchive.Serialize(new StudioFile
        {
            Bookings = _bookings.ToList(),
            Schedule = Schedule,
            ServiceIds = _serviceIds,
        })).AsTask();

    private DateOnly BusinessToday() => ZoneClock.InZone(NowInstant, BusinessZone).Date;

    private void ApplyClock(DateOnly? fixedToday)
    {
        var utc = DateTimeOffset.UtcNow;
        var clock = ZoneClock.InZone(utc, CustomerZone);
        if (fixedToday is DateOnly day)
        {
            Today = day;
            NowInstant = ZoneClock.At(day, clock.Time, CustomerZone);
            return;
        }

        Today = clock.Date;
        NowInstant = utc;
    }

    private void ClearTime()
    {
        BusinessDate = null;
        BusinessStart = null;
        CustomerStart = null;
        CustomerEnd = null;
    }

    private Service? serviceFor(string? reference)
    {
        if (reference is null)
            return null;

        var booking = FindAny(reference);
        return booking is null
            ? null
            : new Service(booking.ServiceId, booking.ServiceName, booking.LengthMinutes, "");
    }

    private sealed class ZoneSnapshot
    {
        public string TimeZone { get; set; } = "UTC";
        public int OffsetMinutes { get; set; }
    }
}
