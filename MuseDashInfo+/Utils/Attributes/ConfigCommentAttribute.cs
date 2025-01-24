using System;

namespace MDIP.Utils.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigCommentAttribute : Attribute
{
    public string ChineseComment { get; }
    public string EnglishComment { get; }

    public ConfigCommentAttribute(string chineseComment, string englishComment)
    {
        ChineseComment = chineseComment;
        EnglishComment = englishComment;
    }
}