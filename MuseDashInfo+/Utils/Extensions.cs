using System.Security.Cryptography;
using System.Text;
using MDIP.Modules.Enums;

namespace MDIP.Utils;

public static class Extensions
{
    public static string Colored(this string text, string color) => $"<color={color}>{text}</color>";

    public static string TruncateByWidth(this string input, int maxWidth)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var totalWidth = CalculateWidth(input);

        if (totalWidth <= maxWidth) return input;

        var breakSymbols = new[] { ' ', '-', '—', '》', '，', '。', '、', '：', '；', ')', '）', '》', '」', '』', '!', '！', '.', ',', '…' };

        var currentString = input;

        while (totalWidth > maxWidth)
        {
            var foundBreakPoint = false;

            for (var i = currentString.Length - 1; i >= 0; i--)
            {
                if (!breakSymbols.Contains(currentString[i]))
                    continue;

                currentString = currentString.Substring(0, i).TrimEnd();
                totalWidth = CalculateWidth(currentString);
                foundBreakPoint = true;
                break;
            }

            if (!foundBreakPoint)
                break;
        }

        if (totalWidth <= maxWidth)
            return currentString;

        var width = 0;
        var charCount = 0;

        foreach (var charWidth in currentString.Select(c => IsFullWidth(c) ? 2 : 1).TakeWhile(charWidth => width + charWidth <= maxWidth))
        {
            width += charWidth;
            charCount++;
        }

        currentString = currentString[..charCount];

        return currentString;
    }

    public static string EscapeReturn(this string text) => text.Replace("\n", "\\n");

    public static string UnEscapeReturn(this string text) => text.Replace("\\n", "\n");

    public static string GetConsistentHash(this string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }

    private static int CalculateWidth(this string input) => input.Sum(c => IsFullWidth(c) ? 2 : 1);

    private static bool IsFullWidth(this char c)
        => c >= 0x4E00 && c <= 0x9FFF || // CJK统一汉字
           c >= 0x3000 && c <= 0x303F || // CJK标点符号
           c >= 0xFF00 && c <= 0xFFEF; // 全角ASCII、全角标点

    public static bool IsRegularNote(this NoteType noteType) => Helper.IsRegularNote((uint)noteType);

    public static Color ToColor(this string color)
        => ColorUtility.TryParseHtmlString(color, out var result) ? result : Color.white;
}