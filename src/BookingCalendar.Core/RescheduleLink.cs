namespace BookingCalendar.Core;

/// <summary>The reschedule link moves one booking to a new time.</summary>
public static class RescheduleLink
{
    public static string For(string bookingReference) => $"/reschedule/{bookingReference}";
}
