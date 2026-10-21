namespace BookingCalendar.Core;

/// <summary>The result of trying to save an appointment.</summary>
public sealed record SaveOutcome(
    IReadOnlyList<Appointment> Appointments,
    string? TitleError,
    string? TimeError)
{
    public bool Ok => TitleError is null && TimeError is null;
}

/// <summary>Validation and the save, delete, and clear-examples operations.</summary>
public static class AppointmentRules
{
    public static string? TitleError(string? title) =>
        string.IsNullOrWhiteSpace(title) ? "Enter a title." : null;

    public static string? TimeOrderError(TimeOnly start, TimeOnly end) =>
        end > start ? null : "End must be after the start.";

    public static SaveOutcome Save(
        IReadOnlyList<Appointment> existing,
        string? id,
        string? title,
        DateOnly date,
        TimeOnly start,
        TimeOnly end,
        string? note,
        Func<string> nextId)
    {
        var titleError = TitleError(title);
        var timeError = TimeOrderError(start, end);
        if (titleError is not null || timeError is not null)
            return new SaveOutcome(existing, titleError, timeError);

        var kept = id is not null
            ? existing.FirstOrDefault(item => item.Id == id)
            : null;
        var appointment = new Appointment(
            kept?.Id ?? nextId(),
            title!.Trim(),
            date,
            start,
            end,
            (note ?? "").Trim(),
            kept?.IsExample ?? false);

        var list = existing.Where(item => item.Id != appointment.Id).Append(appointment).ToList();
        return new SaveOutcome(list, null, null);
    }

    public static IReadOnlyList<Appointment> Delete(IReadOnlyList<Appointment> existing, string id) =>
        existing.Where(item => item.Id != id).ToList();

    public static IReadOnlyList<Appointment> ClearExamples(IReadOnlyList<Appointment> existing) =>
        existing.Where(item => !item.IsExample).ToList();
}
