using BookingCalendar.Core;

namespace BookingCalendar.Core.Tests;

public class BookingRulesTests
{
    private static readonly DateOnly Wednesday = new(2026, 10, 21);
    private static readonly TimeOnly Morning = new(9, 0);
    private static readonly Service Trim = Catalog.Services[0];
    private static readonly Service Cut = Catalog.Services[1];
    private static readonly Service Colour = Catalog.Services[2];

    [Fact]
    public void Horizon_is_the_next_14_days_including_today()
    {
        var days = BookingRules.NextDays(Wednesday);

        Assert.Equal(14, days.Count);
        Assert.Equal(Wednesday, days[0]);
        Assert.Equal(Wednesday.AddDays(13), days[^1]);
    }

    [Theory]
    [InlineData(30, 31, 16, 30)]
    [InlineData(45, 30, 16, 15)]
    [InlineData(60, 29, 16, 0)]
    public void Candidate_starts_fit_the_service_inside_opening_hours(int length, int count, int lastHour, int lastMinute)
    {
        var starts = BookingRules.CandidateStarts(length);

        Assert.Equal(count, starts.Count);
        Assert.Equal(Catalog.Opens, starts[0]);
        Assert.Equal(new TimeOnly(lastHour, lastMinute), starts[^1]);
        Assert.All(starts, start => Assert.True(BookingRules.FitsOpeningHours(start, length)));
    }

    [Theory]
    [InlineData(16, 30, 30, true)]
    [InlineData(16, 45, 30, false)]
    [InlineData(16, 15, 45, true)]
    [InlineData(16, 30, 45, false)]
    [InlineData(16, 0, 60, true)]
    [InlineData(16, 15, 60, false)]
    [InlineData(8, 45, 30, false)]
    public void A_slot_must_finish_by_closing(int hour, int minute, int length, bool fits)
    {
        Assert.Equal(fits, BookingRules.FitsOpeningHours(new TimeOnly(hour, minute), length));
    }

    [Fact]
    public void Weekends_and_past_days_cannot_be_selected()
    {
        Assert.False(BookingRules.CanSelectDate(new DateOnly(2026, 10, 24), Wednesday));
        Assert.False(BookingRules.CanSelectDate(new DateOnly(2026, 10, 25), Wednesday));
        Assert.False(BookingRules.CanSelectDate(Wednesday.AddDays(-1), Wednesday));
        Assert.False(BookingRules.CanSelectDate(Wednesday.AddDays(14), Wednesday));
        Assert.True(BookingRules.CanSelectDate(Wednesday, Wednesday));
        Assert.Empty(BookingRules.FreeSlots(Trim, new DateOnly(2026, 10, 24), Wednesday, Morning, []));
        Assert.Empty(BookingRules.FreeSlots(Trim, Wednesday.AddDays(-1), Wednesday, Morning, []));
    }

    [Fact]
    public void Past_slots_on_today_are_not_offered()
    {
        var slots = BookingRules.FreeSlots(Trim, Wednesday, Wednesday, new TimeOnly(10, 0), []);

        Assert.DoesNotContain(slots, slot => slot.Start < new TimeOnly(10, 0));
        Assert.Contains(slots, slot => slot.Start == new TimeOnly(10, 0));
        Assert.Equal(new TimeOnly(10, 30), slots.First(slot => slot.Start == new TimeOnly(10, 0)).End);
    }

    [Fact]
    public void A_later_day_does_not_hide_morning_slots()
    {
        var slots = BookingRules.FreeSlots(Trim, Wednesday.AddDays(1), Wednesday, new TimeOnly(16, 0), []);

        Assert.Equal(new TimeOnly(9, 0), slots[0].Start);
    }

    [Fact]
    public void An_active_booking_blocks_overlapping_starts_for_every_service()
    {
        var existing = new Booking(
            "K7NP2Q",
            Colour.Id,
            Colour.Name,
            Colour.LengthMinutes,
            Wednesday.AddDays(1),
            new TimeOnly(10, 0),
            "Priya Nair",
            "priya@example.com",
            false);

        var slots = BookingRules.FreeSlots(Trim, Wednesday.AddDays(1), Wednesday, Morning, [existing]);

        Assert.DoesNotContain(slots, slot => slot.Start == new TimeOnly(10, 0));
        Assert.DoesNotContain(slots, slot => slot.Start == new TimeOnly(10, 30));
        Assert.Contains(slots, slot => slot.Start == new TimeOnly(9, 30));
        Assert.Contains(slots, slot => slot.Start == new TimeOnly(11, 0));
    }

    [Fact]
    public void A_cancelled_booking_frees_the_slot()
    {
        var cancelled = new Booking(
            "K7NP2Q",
            Colour.Id,
            Colour.Name,
            60,
            Wednesday.AddDays(1),
            new TimeOnly(10, 0),
            "Priya Nair",
            "priya@example.com",
            true);

        var slots = BookingRules.FreeSlots(Cut, Wednesday.AddDays(1), Wednesday, Morning, [cancelled]);

        Assert.Contains(slots, slot => slot.Start == new TimeOnly(10, 0));
    }

    [Fact]
    public void Adjacent_bookings_do_not_count_as_a_double_booking()
    {
        Assert.False(BookingRules.Overlaps(new TimeOnly(9, 0), 60, new TimeOnly(10, 0), 30));
        Assert.True(BookingRules.Overlaps(new TimeOnly(9, 30), 45, new TimeOnly(10, 0), 30));
    }

    [Fact]
    public void TryConfirm_rejects_a_second_booking_on_the_same_slot()
    {
        var existing = new List<Booking>();
        var random = new Random(4);
        string Next(IReadOnlySet<string> taken) => ReferenceCodes.Create(taken, random);

        var first = BookingRules.TryConfirm(
            existing, Cut, Wednesday.AddDays(1), new TimeOnly(11, 0),
            "Avery Tan", "avery@example.com", Wednesday, Morning, Next);

        Assert.NotNull(first);
        existing.Add(first);

        var second = BookingRules.TryConfirm(
            existing, Trim, Wednesday.AddDays(1), new TimeOnly(11, 15),
            "Owen Tan", "owen@example.com", Wednesday, Morning, Next);

        Assert.Null(second);
    }

    [Fact]
    public void TryConfirm_rejects_a_start_that_does_not_fit_or_a_bad_email()
    {
        string Next(IReadOnlySet<string> taken) => ReferenceCodes.Create(taken, new Random(1));

        Assert.Null(BookingRules.TryConfirm(
            [], Cut, Wednesday.AddDays(1), new TimeOnly(16, 45),
            "Avery Tan", "avery@example.com", Wednesday, Morning, Next));

        Assert.Null(BookingRules.TryConfirm(
            [], Cut, Wednesday.AddDays(1), new TimeOnly(11, 0),
            " ", "not-an-email", Wednesday, Morning, Next));
    }

    [Fact]
    public void Next_free_day_skips_a_full_day_and_the_weekend()
    {
        var bookings = SeedBookings.Create(Wednesday);
        var friday = new DateOnly(2026, 10, 23);

        Assert.Empty(BookingRules.FreeSlots(Trim, friday, Wednesday, Morning, bookings));
        Assert.Equal(
            new DateOnly(2026, 10, 26),
            BookingRules.NextDayWithFreeSlots(Trim, friday, Wednesday, Morning, bookings));
    }
}
