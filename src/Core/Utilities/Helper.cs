using System.Globalization;

namespace MDIP.Core.Utilities;

public static class Helper
{
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
        catch
        {
            return time.ToString(defaultFormat, CultureInfo.InvariantCulture);
        }
    }
}