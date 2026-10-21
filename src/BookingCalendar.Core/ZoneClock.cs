namespace BookingCalendar.Core;

public static class ZoneClock
{
    public static TimeZoneInfo Singapore() => FindOrFixed(SingaporeTime.ZoneId, SingaporeTime.Offset);

    public static TimeZoneInfo FindOrFixed(string id, TimeSpan offset)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.CreateCustomTimeZone(id, offset, id, id);
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone(id, offset, id, id);
        }
    }

    public static DateTimeOffset BusinessInstant(DateOnly date, TimeOnly time, TimeZoneInfo businessZone)
    {
        var wall = date.ToDateTime(time);
        var offset = businessZone.GetUtcOffset(DateTime.SpecifyKind(wall, DateTimeKind.Unspecified));
        return new DateTimeOffset(wall, offset);
    }

    public static (DateOnly Date, TimeOnly Time) InZone(DateTimeOffset instant, TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTime(instant, zone);
        return (DateOnly.FromDateTime(local.DateTime), TimeOnly.FromTimeSpan(local.TimeOfDay));
    }

    public static DateTimeOffset At(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    {
        var wall = date.ToDateTime(time);
        var offset = zone.GetUtcOffset(DateTime.SpecifyKind(wall, DateTimeKind.Unspecified));
        return new DateTimeOffset(wall, offset);
    }
}
