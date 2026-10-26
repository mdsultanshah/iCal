namespace BookingCalendar.App.State;

/// <summary>Field values from the appointment panel, before they are checked.</summary>
public sealed record AppointmentDraft(
    string? Id,
    string Title,
    string Date,
    string Start,
    string End,
    string Note);
