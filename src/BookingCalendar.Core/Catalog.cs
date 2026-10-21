namespace BookingCalendar.Core;

/// <summary>
/// The one made-up business, its one provider, and the three services.
/// Opening hours are Monday to Friday, 9:00 to 17:00, Singapore time.
/// </summary>
public static class Catalog
{
    public const string BusinessName = "North Quay Studio";
    public const string ProviderName = "Hana Yusuf";
    public const string Neighbourhood = "Tiong Bahru, Singapore";
    public const string ZoneId = "Asia/Singapore";

    public static readonly TimeOnly Opens = new(9, 0);
    public static readonly TimeOnly Closes = new(17, 0);
    public const int SlotStepMinutes = 15;
    public const int HorizonDays = 14;

    public static readonly IReadOnlyList<Service> Services =
    [
        new("trim", "Trim", 30, "A neat cut around your current shape."),
        new("cut-style", "Cut and style", 45, "A full cut, washed and finished."),
        new("colour-consult", "Colour consult", 60, "A longer appointment to plan colour."),
    ];

    public static Service? Find(string? id) =>
        id is null ? null : Services.FirstOrDefault(service => service.Id == id);
}
