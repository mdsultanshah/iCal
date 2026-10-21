namespace BookingCalendar.Core;

/// <summary>
/// A free start, stored in business time and shown in the customer's zone.
/// Customers see the customer start as a time, not as a slot.
/// </summary>
public readonly record struct CustomerSlot(
    DateOnly BusinessDate,
    TimeOnly BusinessStart,
    TimeOnly BusinessEnd,
    DateOnly CustomerDate,
    TimeOnly CustomerStart,
    TimeOnly CustomerEnd);
