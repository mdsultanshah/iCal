namespace BookingCalendar.App.State;

/// <summary>An empty hour the user chose on the grid.</summary>
public readonly record struct SlotChoice(DateOnly Date, int Hour);
