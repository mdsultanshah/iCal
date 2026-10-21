namespace BookingCalendar.Core;

/// <summary>A booking is a confirmed slot with a name and email.</summary>
public sealed record Booking(
    string BookingReference,
    string ServiceId,
    string ServiceName,
    int LengthMinutes,
    DateOnly Date,
    TimeOnly Start,
    string CustomerName,
    string CustomerEmail,
    bool IsCancelled);
