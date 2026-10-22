namespace BookingCalendar.Core;

/// <summary>Where an appointment sits when overlaps share a day side by side.</summary>
public sealed record PlacedAppointment(Appointment Appointment, int Column, int ColumnCount);

/// <summary>
/// Packs overlapping appointments into columns. Touching at an endpoint does not overlap.
/// </summary>
public static class OverlapLayout
{
    public static bool Overlaps(Appointment left, Appointment right)
    {
        if (left.Date != right.Date)
            return false;

        return left.Start < right.End && right.Start < left.End;
    }

    public static IReadOnlyList<PlacedAppointment> Place(IEnumerable<Appointment> appointments)
    {
        var ordered = appointments
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Start)
            .ThenBy(item => item.End)
            .ThenBy(item => item.Id, StringComparer.Ordinal)
            .ToList();

        var placed = new List<PlacedAppointment>(ordered.Count);
        var index = 0;
        while (index < ordered.Count)
        {
            var cluster = new List<Appointment> { ordered[index] };
            var clusterEnd = ordered[index].End;
            var date = ordered[index].Date;
            index++;
            while (index < ordered.Count && ordered[index].Date == date && ordered[index].Start < clusterEnd)
            {
                if (ordered[index].End > clusterEnd)
                    clusterEnd = ordered[index].End;

                cluster.Add(ordered[index]);
                index++;
            }

            placed.AddRange(PlaceCluster(cluster));
        }

        return placed;
    }

    private static IEnumerable<PlacedAppointment> PlaceCluster(List<Appointment> cluster)
    {
        var columnEnds = new List<TimeOnly>();
        var assigned = new List<(Appointment Appointment, int Column)>();
        foreach (var item in cluster)
        {
            var column = columnEnds.FindIndex(end => end <= item.Start);
            if (column < 0)
            {
                column = columnEnds.Count;
                columnEnds.Add(item.End);
            }
            else
            {
                columnEnds[column] = item.End;
            }

            assigned.Add((item, column));
        }

        var count = Math.Max(1, columnEnds.Count);
        return assigned.Select(pair => new PlacedAppointment(pair.Appointment, pair.Column, count));
    }
}
