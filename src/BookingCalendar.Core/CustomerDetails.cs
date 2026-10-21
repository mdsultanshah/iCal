namespace BookingCalendar.Core;

public static class CustomerDetails
{
    public const string MissingNameMessage = "Enter your name.";
    public const string InvalidEmailMessage = "Enter a valid email.";

    public static string? NameError(string? name) =>
        string.IsNullOrWhiteSpace(name) ? MissingNameMessage : null;

    public static string? EmailError(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !IsEmail(email.Trim()))
            return InvalidEmailMessage;

        return null;
    }

    public static bool IsEmail(string email)
    {
        if (email.Length is < 3 or > 254)
            return false;

        var at = email.IndexOf('@');
        if (at <= 0 || at != email.LastIndexOf('@'))
            return false;

        var local = email[..at];
        var domain = email[(at + 1)..];
        if (local.Any(char.IsWhiteSpace) || domain.Any(char.IsWhiteSpace))
            return false;

        var dot = domain.LastIndexOf('.');
        if (dot <= 0 || dot >= domain.Length - 1)
            return false;

        return !domain.Contains("..", StringComparison.Ordinal);
    }
}
