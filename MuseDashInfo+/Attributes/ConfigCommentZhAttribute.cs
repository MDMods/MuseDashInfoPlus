using System;

namespace MDIP.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigCommentZhAttribute : Attribute
{
    public string Comment { get; }

    public ConfigCommentZhAttribute(string comment)
    {
        Comment = comment;
    }
}