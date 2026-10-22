namespace BookingCalendar.App.State;

public sealed record QuickDraft(string Title, string StartText);

public sealed record QuickSave(string Title, TimeOnly Start);
