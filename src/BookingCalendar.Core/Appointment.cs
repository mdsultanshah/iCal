namespace BookingCalendar.Core;

/// <summary>One appointment on a single day. Times are in the viewer's zone.</summary>
public sealed record Appointment(
    string Id,
    string Title,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string Note,
    bool IsExample);
