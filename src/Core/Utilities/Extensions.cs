using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MDIP.Core.Domain.Enums;

namespace MDIP.Core.Utilities;

public static class Extensions
{
    public static bool IsRegularNote(this NoteType noteType)
    {
        var value = (int)noteType;
        return value is >= (int)NoteType.Monster and <= (int)NoteType.Mul;
    }

    public static Color ToColor(this string color)
        => ColorUtility.TryParseHtmlString(color, out var result) ? result : Color.white;

    public static string Colored(this string text, string color) => $"<color={color}>{text}</color>";

    public static string TruncateByWidth(this string input, int maxWidth)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var stripped = Regex.Replace(input, "<.*?>", string.Empty);

        var totalWidth = stripped.CalculateWidth();
        if (totalWidth <= maxWidth) return input;

        var breakSymbols = new[] { ' ', '-', '—', '》', '，', '。', '、', '：', '；', ')', '）', '》', '」', '』', '!', '！', '.', ',', '…' };

        var currentStripped = stripped;

        while (totalWidth > maxWidth)
        {
            var foundBreakPoint = false;

            for (var i = currentStripped.Length - 1; i >= 0; i--)
            {
                if (!breakSymbols.Contains(currentStripped[i]))
                    continue;

                currentStripped = currentStripped[..i].TrimEnd();
                totalWidth = currentStripped.CalculateWidth();
                foundBreakPoint = true;
                break;
            }

            if (!foundBreakPoint)
                break;
        }

        if (totalWidth > maxWidth)
        {
            var width = 0;
            var charCount = 0;
            foreach (var charWidth in currentStripped.Select(c => IsFullWidth(c) ? 2 : 1).TakeWhile(charWidth => width + charWidth <= maxWidth))
            {
                width += charWidth;
                charCount++;
            }
            currentStripped = currentStripped[..charCount];
        }

        var visibleCount = 0;
        var j = 0;
        while (j < input.Length && visibleCount < currentStripped.Length)
        {
            if (input[j] == '<')
            {
                var end = input.IndexOf('>', j);
                if (end == -1) break;
                j = end + 1;
                continue;
            }

            visibleCount++;
            j++;
        }

        return input[..j];
    }

    public static string EscapeReturn(this string text) => text.Replace("\n", "\\n");

    public static string UnEscapeReturn(this string text) => text.Replace("\\n", "\n");

    public static int CalculateWidth(this string input) => input.Sum(c => IsFullWidth(c) ? 2 : 1);

    public static bool IsFullWidth(this char c)
        => c >= 0x1100 && (
            c <= 0x115F ||
            c == 0x2329 ||
            c == 0x232A ||
            c >= 0x2E80 && c <= 0xA4CF && c != 0x303F ||
            c >= 0xAC00 && c <= 0xD7A3 ||
            c >= 0xF900 && c <= 0xFAFF ||
            c >= 0xFE10 && c <= 0xFE19 ||
            c >= 0xFE30 && c <= 0xFE6F ||
            c >= 0xFF00 && c <= 0xFF60 ||
            c >= 0xFFE0 && c <= 0xFFE6);

    public static string ToSafeFileName(this string input, char replacement = '_')
    {
        if (string.IsNullOrEmpty(input))
            return "InfoPlus";

        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(input.Length);

        foreach (var ch in input)
            builder.Append(invalid.Contains(ch) ? replacement : ch);

        return builder.ToString().Trim();
    }

    public static string GetConsistentHash(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

    public static string SafeFormatDateTime(this DateTime time, string format, string culture)
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