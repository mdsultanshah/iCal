namespace BookingCalendar.Core;

public static class ReferenceCodes
{
    public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const int Length = 6;

    public static string Create(IReadOnlySet<string> existing, Random random)
    {
        for (var attempt = 0; attempt < 32; attempt++)
        {
            var chars = new char[Length];
            for (var i = 0; i < chars.Length; i++)
                chars[i] = Alphabet[random.Next(Alphabet.Length)];

            var code = new string(chars);
            if (!existing.Contains(code))
                return code;
        }

        throw new InvalidOperationException("Could not create a booking reference.");
    }
}
