using System.Globalization;

namespace MDIP.Utils;

public static class Helper
{
    public static bool IsRegularNote(uint noteType)
        => noteType is >= 1 and <= 8;

    public static string SafeFormatDateTime(DateTime time, string format, string culture)
    {
        const string defaultFormat = "h:mm:ss tt";
        try
        {
            if (string.IsNullOrWhiteSpace(format))
                return time.ToString(defaultFormat, CultureInfo.InvariantCulture);

            var cultureInfo = string.IsNullOrWhiteSpace(culture)
                ? CultureInfo.InvariantCulture
                : new(culture);

            return time.ToString(format, cultureInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Warning] Invalid time format, using default. Error: {ex.Message}");
            return time.ToString(defaultFormat, CultureInfo.InvariantCulture);
        }
    }
}