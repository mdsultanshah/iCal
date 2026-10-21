namespace BookingCalendar.Core;

/// <summary>A service is what you book, with a fixed length.</summary>
public sealed record Service(string Id, string Name, int LengthMinutes, string Summary);
