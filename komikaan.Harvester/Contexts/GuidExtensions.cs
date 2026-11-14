namespace komikaan.Harvester.Contexts;


static class GuidExtensions
{
    // Byte order of Guid.ToByteArray() from least to most significant when compared
    // the way PostgreSQL compares uuid values (memcmp on the textual representation)
    private static readonly int[] _guidByteOrder =
        new[] { 15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3 };

    /// <summary>
    /// Returns the smallest Guid that compares greater than <paramref name="guid"/> in PostgreSQL uuid ordering.
    /// Used to build the upper bound of a stop_times partition range.
    /// </summary>
    public static Guid Increment(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        if (bytes.All(b => b == 255))
        {
            throw new OverflowException("Cannot increment a Guid that is all 0xFF");
        }
        bool carry = true;
        for (int i = 0; i < _guidByteOrder.Length && carry; i++)
        {
            int index = _guidByteOrder[i];
            byte oldValue = bytes[index]++;
            carry = oldValue > bytes[index];
        }
        return new Guid(bytes);
    }
}