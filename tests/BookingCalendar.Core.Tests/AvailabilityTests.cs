using BookingCalendar.Core;

namespace BookingCalendar.Core.Tests;

public class AvailabilityTests
{
    private static readonly DateOnly Wednesday = new(2026, 10, 21);
    private static readonly TimeZoneInfo Singapore = ZoneClock.Singapore();
    private static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    private static readonly Service Trim = Catalog.Services[0];
    private static readonly Service Cut = Catalog.Services[1];

    private static DateTimeOffset Morning() =>
        ZoneClock.At(Wednesday, new TimeOnly(9, 0), Singapore);

    [Fact]
    public void Default_schedule_matches_the_fixed_opening_hours()
    {
        var schedule = WeeklySchedule.Default();
        var now = Morning();
        var slots = Availability.Free(Cut, Wednesday.AddDays(1), now, Singapore, Singapore, schedule, []);
        var legacy = BookingRules.FreeSlots(Cut, Wednesday.AddDays(1), Wednesday, new TimeOnly(9, 0), []);

        Assert.Equal(legacy.Select(slot => slot.Start), slots.Select(slot => slot.CustomerStart));
        Assert.True(WeeklySchedule.IsValid(schedule));
        Assert.True(ServiceSelection.IsValid(ServiceSelection.DefaultIds()));
    }

    [Fact]
    public void A_full_day_cannot_be_picked()
    {
        var bookings = SeedBookings.Create(Wednesday);
        var friday = new DateOnly(2026, 10, 23);
        var state = Availability.State(Trim, friday, Morning(), Singapore, Singapore, WeeklySchedule.Default(), bookings);

        Assert.Equal(DayState.Full, state);
        Assert.False(Availability.CanPick(state));
        Assert.Equal(DayState.Closed, Availability.State(Trim, new DateOnly(2026, 10, 24), Morning(), Singapore, Singapore, WeeklySchedule.Default(), bookings));
    }

    [Fact]
    public void Custom_hours_limit_the_times_and_can_open_a_weekend()
    {
        var schedule = WeeklySchedule.Default()
            .Set(DayOfWeek.Monday, true, new TimeOnly(10, 0), new TimeOnly(12, 0))
            .Set(DayOfWeek.Tuesday, false, new TimeOnly(9, 0), new TimeOnly(17, 0))
            .Set(DayOfWeek.Wednesday, false, new TimeOnly(9, 0), new TimeOnly(17, 0))
            .Set(DayOfWeek.Thursday, false, new TimeOnly(9, 0), new TimeOnly(17, 0))
            .Set(DayOfWeek.Friday, false, new TimeOnly(9, 0), new TimeOnly(17, 0))
            .Set(DayOfWeek.Saturday, true, new TimeOnly(11, 0), new TimeOnly(13, 0));

        var monday = new DateOnly(2026, 10, 26);
        var slots = Availability.Free(Trim, monday, Morning(), Singapore, Singapore, schedule, []);

        Assert.Equal(new TimeOnly(10, 0), slots[0].CustomerStart);
        Assert.Equal(new TimeOnly(11, 30), slots[^1].CustomerStart);
        Assert.DoesNotContain(slots, slot => slot.CustomerStart < new TimeOnly(10, 0));
        Assert.Equal(DayState.Closed, Availability.State(Trim, new DateOnly(2026, 10, 22), Morning(), Singapore, Singapore, schedule, []));
        Assert.Equal(DayState.Open, Availability.State(Trim, new DateOnly(2026, 10, 24), Morning(), Singapore, Singapore, schedule, []));
        Assert.False(WeeklySchedule.IsValid(WeeklySchedule.Default().Set(DayOfWeek.Monday, true, new TimeOnly(12, 0), new TimeOnly(9, 0))));
    }

    [Fact]
    public void Singapore_morning_is_the_previous_evening_in_New_York()
    {
        var instant = ZoneClock.BusinessInstant(new DateOnly(2026, 10, 22), new TimeOnly(9, 0), Singapore);
        var shown = ZoneClock.InZone(instant, NewYork);

        Assert.Equal(new DateOnly(2026, 10, 21), shown.Date);
        Assert.Equal(new TimeOnly(21, 0), shown.Time);

        var slots = Availability.Possible(Trim, new DateOnly(2026, 10, 21), NewYork, Singapore, WeeklySchedule.Default());
        Assert.Contains(slots, slot => slot.BusinessStart == new TimeOnly(9, 0) && slot.CustomerStart == new TimeOnly(21, 0));
    }

    [Fact]
    public void Reschedule_moves_the_booking_and_frees_the_old_time()
    {
        var schedule = WeeklySchedule.Default();
        var now = Morning();
        var existing = new List<Booking>
        {
            new("K7NP2Q", Cut.Id, Cut.Name, Cut.LengthMinutes, new DateOnly(2026, 10, 22), new TimeOnly(11, 0), "Avery Tan", "avery@example.com", false),
            new("OTHER1", Trim.Id, Trim.Name, Trim.LengthMinutes, new DateOnly(2026, 10, 22), new TimeOnly(15, 0), "Owen Tan", "owen@example.com", false),
        };

        var moved = Availability.TryReschedule(existing, "K7NP2Q", new DateOnly(2026, 10, 22), new TimeOnly(13, 0), now, Singapore, Singapore, schedule);
        Assert.NotNull(moved);
        Assert.Equal(new TimeOnly(13, 0), moved.Start);
        existing[0] = moved;

        Assert.Null(Availability.TryReschedule(existing, "K7NP2Q", new DateOnly(2026, 10, 22), new TimeOnly(15, 0), now, Singapore, Singapore, schedule));
        Assert.Contains(
            Availability.Free(Cut, new DateOnly(2026, 10, 22), now, Singapore, Singapore, schedule, existing),
            slot => slot.BusinessStart == new TimeOnly(11, 0));
        Assert.Null(Availability.TryReschedule(existing, "MISSING", new DateOnly(2026, 10, 22), new TimeOnly(12, 0), now, Singapore, Singapore, schedule));

        existing[0] = existing[0] with { IsCancelled = true };
        Assert.Null(Availability.TryReschedule(existing, "K7NP2Q", new DateOnly(2026, 10, 22), new TimeOnly(12, 0), now, Singapore, Singapore, schedule));
    }

    [Fact]
    public void Confirm_uses_weekly_hours_and_rejects_a_second_booking()
    {
        var schedule = WeeklySchedule.Default();
        var now = Morning();
        string Next(IReadOnlySet<string> taken) => ReferenceCodes.Create(taken, new Random(2));
        var existing = new List<Booking>();

        var first = Availability.TryConfirm(
            existing, Cut, new DateOnly(2026, 10, 22), new TimeOnly(11, 0),
            "Avery Tan", "avery@example.com", now, Singapore, Singapore, schedule, Next);
        Assert.NotNull(first);
        existing.Add(first);

        Assert.Null(Availability.TryConfirm(
            existing, Trim, new DateOnly(2026, 10, 22), new TimeOnly(11, 15),
            "Owen Tan", "owen@example.com", now, Singapore, Singapore, schedule, Next));
        Assert.Null(Availability.TryConfirm(
            existing, Trim, new DateOnly(2026, 10, 22), new TimeOnly(8, 0),
            "Owen Tan", "owen@example.com", now, Singapore, Singapore, schedule, Next));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void One_to_three_services_can_be_offered(int count, bool valid)
    {
        var ids = Catalog.Services.Select(service => service.Id).Take(count).ToList();
        if (count == 4)
            ids.Add("extra");

        Assert.Equal(valid, ServiceSelection.IsValid(ids));
    }

    [Fact]
    public void Preview_empty_is_a_test_option()
    {
        var options = PrototypeOptions.Parse("/times?reset=1&today=2026-10-21&preview=empty");

        Assert.True(options.Reset);
        Assert.True(options.PreviewEmpty);
        Assert.Equal(new DateOnly(2026, 10, 21), options.Today);
        Assert.False(PrototypeOptions.Parse("/?today=2026-10-21&reset=1").PreviewEmpty);
        Assert.Equal("/reschedule/K7NP2Q", RescheduleLink.For("K7NP2Q"));
    }
}
