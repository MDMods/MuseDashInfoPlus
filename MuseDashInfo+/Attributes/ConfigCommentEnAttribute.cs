using System;

namespace MDIP.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigCommentEnAttribute : Attribute
{
	public string Comment { get; }

	public ConfigCommentEnAttribute(string comment) => Comment = comment;
}