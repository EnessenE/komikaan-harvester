using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;
namespace komikaan.Harvester.Suppliers;

public class GtfsTimeConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            // Return default TimeSpan (00:00:00) if the field is empty.
            // Or throw an exception if time is always required.
            return default(TimeSpan);
        }

        var parts = text.Split(':');
        if (parts.Length != 3)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, "Invalid time format. Expected HH:mm:ss.");
        }

        try
        {
            int hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int seconds = int.Parse(parts[2], CultureInfo.InvariantCulture);

            return new TimeSpan(hours, minutes, seconds);
        }
        catch (Exception ex)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, "Error converting GTFS time.", ex);
        }
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is TimeSpan ts)
        {
            // Format back to HH:mm:ss, ensuring hours includes the total hours.
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
        return string.Empty;
    }
}
