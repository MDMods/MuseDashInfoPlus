using Il2CppGameLogic;
using System.Linq;
using System;
using UnityEngine;

using MDIP.Modules;

namespace MDIP.Utils;

public static class Extensions
{
    public static string Colored(this string text, string color) => $"<color={color}>{text}</color>";

    public static string TruncateByWidth(this string input, int maxWidth)
    {
        if (string.IsNullOrEmpty(input)) return input;

        int totalWidth = CalculateWidth(input);

        if (totalWidth <= maxWidth) return input;

        var breakSymbols = new[] { ' ', '-', '—', '》', '，', '。', '、', '：', '；', ')', '）', '》', '」', '』', '!', '！', '.', ',', '…' };

        string currentString = input;

        while (totalWidth > maxWidth)
        {
            bool foundBreakPoint = false;

            for (int i = currentString.Length - 1; i >= 0; i--)
            {
                if (breakSymbols.Contains(currentString[i]))
                {
                    currentString = currentString.Substring(0, i).TrimEnd();
                    totalWidth = CalculateWidth(currentString);
                    foundBreakPoint = true;
                    break;
                }
            }

            if (!foundBreakPoint)
                break;
        }

        if (totalWidth > maxWidth)
        {
            int width = 0;
            int charCount = 0;

            foreach (char c in currentString)
            {
                int charWidth = IsFullWidth(c) ? 2 : 1;
                if (width + charWidth > maxWidth)
                    break;

                width += charWidth;
                charCount++;
            }

            currentString = currentString[..charCount];
        }

        return currentString;
    }

    public static string EscapeReturn(this string text) => text.Replace("\n", "\\n");

    public static string UnEscapeReturn(this string text) => text.Replace("\\n", "\n");

    private static int CalculateWidth(this string input) => input.Sum(c => IsFullWidth(c) ? 2 : 1);

    private static bool IsFullWidth(this char c)
        => (c >= 0x4E00 && c <= 0x9FFF) ||    // CJK统一汉字
               (c >= 0x3000 && c <= 0x303F) ||    // CJK标点符号
               (c >= 0xFF00 && c <= 0xFFEF);      // 全角ASCII、全角标点

    public static bool IsRegularNote(this NoteType noteType) => Helper.IsRegularNote((uint)noteType);

    public static Color ToColor(this string color)
    {
        if (ColorUtility.TryParseHtmlString(color, out Color result))
            return result;
        return Color.white;
    }

    public static int Count(this Il2CppSystem.Collections.Generic.List<MusicData> noteList, Func<MusicData, bool> predicate)
    {
        int count = 0;
        foreach (var note in noteList)
        {
            if (predicate(note))
                count++;
        }
        return count;
    }
}
