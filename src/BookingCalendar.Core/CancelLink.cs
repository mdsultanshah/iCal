namespace BookingCalendar.Core;

/// <summary>The cancel link is the link that cancels one booking.</summary>
public static class CancelLink
{
    public static string For(string bookingReference) => $"/cancel/{bookingReference}";
}
