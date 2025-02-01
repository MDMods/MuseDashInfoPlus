using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MDIP.Modules.Configs;

namespace MDIP.Utils;

public class YamlParser
{
	public string Serialize(object obj)
	{
		var builder = new StringBuilder();
		SerializeObject(obj, builder, 0);
		return builder.ToString();
	}

	private void SerializeObject(object obj, StringBuilder builder, int indent)
	{
		if (obj == null) return;

		var type = obj.GetType();
		var properties = type.GetProperties()
			.Where(p => p.CanRead && p.CanWrite)
			.OrderBy(p => p.DeclaringType == typeof(ConfigBase))
			.ThenBy(p => p.MetadataToken);

		foreach (var prop in properties)
		{
			var value = prop.GetValue(obj);
			if (value == null) continue;

			var name = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
			var indentStr = new string(' ', indent * 2);

			if (value is string strValue)
				builder.AppendLine($"{indentStr}{name}: {EscapeString(strValue)}");
			else if (value is DateTime dateValue)
				builder.AppendLine($"{indentStr}{name}: {dateValue:yyyy-MM-dd HH:mm:ss}");
			else if (value.GetType().IsPrimitive || value is decimal)
				builder.AppendLine($"{indentStr}{name}: {value}");
			else if (value is IEnumerable<object> collection)
			{
				builder.AppendLine($"{indentStr}{name}:");
				foreach (var item in collection) builder.AppendLine($"{indentStr}- {item}");
			}
			else
			{
				builder.AppendLine($"{indentStr}{name}:");
				SerializeObject(value, builder, indent + 1);
			}
		}
	}

	public T Deserialize<T>(string yaml) where T : new()
	{
		var lines = yaml.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		var result = new T();
		var currentObject = result;
		var currentPath = new Stack<string>();
		var indentLevel = 0;

		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line)) continue;

			var currentIndent = line.TakeWhile(c => c == ' ').Count() / 2;
			var actualLine = line.Trim();

			if (currentIndent < indentLevel)
			{
				while (currentIndent < indentLevel && currentPath.Count > 0)
				{
					currentPath.Pop();
					indentLevel--;
				}
			}

			var parts = actualLine.Split(new[] { ':' }, 2);
			if (parts.Length != 2) continue;

			var propertyName = parts[0].Trim();
			var value = parts[1].Trim();

			var property = GetProperty(currentObject.GetType(), propertyName);
			if (property == null) continue;

			if (value.Length > 0)
				SetValue(currentObject, property, value);
			else
			{
				currentPath.Push(propertyName);
				indentLevel = currentIndent + 1;
			}
		}

		return result;
	}

	private PropertyInfo GetProperty(Type type, string name) =>
		type.GetProperty(
			char.ToUpperInvariant(name[0]) + name.Substring(1),
			BindingFlags.Public | BindingFlags.Instance);

	private void SetValue(object obj, PropertyInfo property, string value)
	{
		var type = property.PropertyType;

		if (type == typeof(string))
			property.SetValue(obj, UnescapeString(value));
		else if (type == typeof(DateTime))
		{
			if (DateTime.TryParse(value, out var dateValue)) property.SetValue(obj, dateValue);
		}
		else if (type == typeof(bool))
		{
			if (bool.TryParse(value, out var boolValue)) property.SetValue(obj, boolValue);
		}
		else if (type == typeof(int))
		{
			if (int.TryParse(value, out var intValue)) property.SetValue(obj, intValue);
		}
		else if (type == typeof(float))
		{
			if (float.TryParse(value, out var floatValue)) property.SetValue(obj, floatValue);
		}
		else if (type == typeof(double))
		{
			if (double.TryParse(value, out var doubleValue))
				property.SetValue(obj, doubleValue);
		}
	}

	private string EscapeString(string value)
	{
		if (value.Contains('\n') || value.Contains(':') || value.Contains('#')) return $"\"{value.Replace("\"", "\\\"")}\"";
		return value;
	}

	private string UnescapeString(string value)
	{
		if (value.StartsWith("\"") && value.EndsWith("\"")) return value[1..^1].Replace("\\\"", "\"");
		return value;
	}
}