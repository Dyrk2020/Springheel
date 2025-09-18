using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebSocket4Net;

public static class Extensions
{
	private const string m_Tab = "\t";

	private const char m_Colon = ':';

	private const string m_Space = " ";

	private const string m_ValueSeparator = ", ";

	private static readonly char[] m_CrCf = new char[2] { '\r', '\n' };

	private static Type[] m_SimpleTypes = new Type[6]
	{
		typeof(string),
		typeof(decimal),
		typeof(DateTime),
		typeof(DateTimeOffset),
		typeof(TimeSpan),
		typeof(Guid)
	};

	public static void AppendFormatWithCrCf(this StringBuilder builder, string format, object arg)
	{
		builder.AppendFormat(format, arg);
		builder.Append(m_CrCf);
	}

	public static void AppendFormatWithCrCf(this StringBuilder builder, string format, params object[] args)
	{
		builder.AppendFormat(format, args);
		builder.Append(m_CrCf);
	}

	public static void AppendWithCrCf(this StringBuilder builder, string content)
	{
		builder.Append(content);
		builder.Append(m_CrCf);
	}

	public static void AppendWithCrCf(this StringBuilder builder)
	{
		builder.Append(m_CrCf);
	}

	public static bool ParseMimeHeader(this string source, IDictionary<string, object> valueContainer, out string verbLine)
	{
		verbLine = string.Empty;
		string text = string.Empty;
		StringReader stringReader = new StringReader(source);
		string text2;
		while (!string.IsNullOrEmpty(text2 = stringReader.ReadLine()))
		{
			if (string.IsNullOrEmpty(verbLine))
			{
				verbLine = text2;
				continue;
			}
			object value;
			if (text2.StartsWith("\t") && !string.IsNullOrEmpty(text))
			{
				if (!valueContainer.TryGetValue(text, out value))
				{
					return false;
				}
				valueContainer[text] = string.Concat(value, text2.Trim());
				continue;
			}
			int num = text2.IndexOf(':');
			string text3 = text2.Substring(0, num);
			if (!string.IsNullOrEmpty(text3))
			{
				text3 = text3.Trim();
			}
			string text4 = text2.Substring(num + 1);
			if (!string.IsNullOrEmpty(text4) && text4.StartsWith(" ") && text4.Length > 1)
			{
				text4 = text4.Substring(1);
			}
			if (!string.IsNullOrEmpty(text3))
			{
				if (!valueContainer.TryGetValue(text3, out value))
				{
					valueContainer.Add(text3, text4);
				}
				else
				{
					valueContainer[text3] = string.Concat(value, ", ", text4);
				}
				text = text3;
			}
		}
		return true;
	}

	public static TValue GetValue<TValue>(this IDictionary<string, object> valueContainer, string name)
	{
		return valueContainer.GetValue(name, default(TValue));
	}

	public static TValue GetValue<TValue>(this IDictionary<string, object> valueContainer, string name, TValue defaultValue)
	{
		if (!valueContainer.TryGetValue(name, out var value))
		{
			return defaultValue;
		}
		return (TValue)value;
	}

	internal static bool IsSimpleType(this Type type)
	{
		if (!type.IsValueType && !type.IsPrimitive && !m_SimpleTypes.Contains(type))
		{
			return Convert.GetTypeCode(type) != TypeCode.Object;
		}
		return true;
	}
}
