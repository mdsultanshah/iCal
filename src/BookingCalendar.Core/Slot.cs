namespace BookingCalendar.Core;

/// <summary>A slot is a start time that is free for the chosen service.</summary>
public readonly record struct Slot(TimeOnly Start, TimeOnly End);
