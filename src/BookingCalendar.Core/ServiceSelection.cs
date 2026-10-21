namespace BookingCalendar.Core;

public static class ServiceSelection
{
    public static bool IsValid(IReadOnlyCollection<string> serviceIds)
    {
        if (serviceIds.Count is < 1 or > 3)
            return false;

        if (serviceIds.Distinct(StringComparer.Ordinal).Count() != serviceIds.Count)
            return false;

        var known = Catalog.Services.Select(service => service.Id).ToHashSet(StringComparer.Ordinal);
        return serviceIds.All(known.Contains);
    }

    public static IReadOnlyList<string> DefaultIds() =>
        Catalog.Services.Select(service => service.Id).ToList();
}
