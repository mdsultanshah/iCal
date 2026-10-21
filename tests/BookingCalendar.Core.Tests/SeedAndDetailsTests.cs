using BookingCalendar.Core;

namespace BookingCalendar.Core.Tests;

public class SeedAndDetailsTests
{
    private static readonly DateOnly Wednesday = new(2026, 10, 21);

    [Fact]
    public void Seeds_stay_inside_opening_hours_and_leave_one_day_full()
    {
        var bookings = SeedBookings.Create(Wednesday);
        var fullDay = SeedBookings.FullDay(Wednesday) ?? throw new InvalidOperationException("Expected a full day.");
        Assert.Equal(new DateOnly(2026, 10, 23), fullDay);
        Assert.Equal(11, bookings.Count);
        Assert.Equal(bookings.Count, bookings.Select(booking => booking.BookingReference).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(bookings, booking =>
        {
            Assert.False(booking.IsCancelled);
            Assert.False(BookingRules.IsWeekend(booking.Date));
            Assert.True(BookingRules.FitsOpeningHours(booking.Start, booking.LengthMinutes));
            Assert.False(string.IsNullOrWhiteSpace(booking.CustomerName));
            Assert.Null(CustomerDetails.EmailError(booking.CustomerEmail));
        });

        foreach (var service in Catalog.Services)
            Assert.Empty(BookingRules.FreeSlots(service, fullDay, Wednesday, new TimeOnly(9, 0), bookings));

        Assert.NotEmpty(BookingRules.FreeSlots(Catalog.Services[1], new DateOnly(2026, 10, 22), Wednesday, new TimeOnly(9, 0), bookings));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("Avery Tan", false)]
    public void Name_is_required(string name, bool missing)
    {
        var error = CustomerDetails.NameError(name);
        if (missing)
            Assert.Equal(CustomerDetails.MissingNameMessage, error);
        else
            Assert.Null(error);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("ada", false)]
    [InlineData("ada@", false)]
    [InlineData("ada@studio", false)]
    [InlineData("ada@studio.", false)]
    [InlineData("ada @studio.sg", false)]
    [InlineData("avery@example.com", true)]
    public void Email_must_look_like_an_email(string email, bool valid)
    {
        var error = CustomerDetails.EmailError(email);
        if (valid)
            Assert.Null(error);
        else
            Assert.Equal(CustomerDetails.InvalidEmailMessage, error);
    }

    [Fact]
    public void Singapore_time_is_eight_hours_ahead_of_utc()
    {
        var utc = new DateTimeOffset(2026, 10, 20, 16, 30, 0, TimeSpan.Zero);

        Assert.Equal("Asia/Singapore", SingaporeTime.ZoneId);
        Assert.Equal(TimeSpan.FromHours(8), SingaporeTime.Offset);
        Assert.Equal(new DateOnly(2026, 10, 21), SingaporeTime.Today(utc));
        Assert.Equal(new TimeOnly(0, 30), SingaporeTime.TimeOfDay(utc));
    }

    [Fact]
    public void Prototype_options_read_reset_and_today()
    {
        var options = PrototypeOptions.Parse("http://127.0.0.1:5088/?today=2026-10-21&reset=1");

        Assert.True(options.Reset);
        Assert.Equal(new DateOnly(2026, 10, 21), options.Today);
        Assert.False(PrototypeOptions.Parse("http://127.0.0.1:5088/?reset=true").Reset);
        Assert.Null(PrototypeOptions.Parse("/?today=2026-02-31").Today);
        Assert.Null(PrototypeOptions.Parse("/?today=yesterday").Today);
        Assert.Equal(new DateOnly(2026, 11, 2), PrototypeOptions.Parse("/date?today=2026-11-02#slot").Today);
    }

    [Fact]
    public void Booking_reference_is_a_short_unused_code_and_cancel_link_uses_it()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ABCDEF" };
        var code = ReferenceCodes.Create(existing, new Random(3));

        Assert.Equal(ReferenceCodes.Length, code.Length);
        Assert.DoesNotContain(code, existing);
        Assert.Equal($"/cancel/{code}", CancelLink.For(code));
    }
}
