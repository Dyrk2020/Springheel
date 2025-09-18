using System;
using System.Collections.Generic;
using System.IO;

namespace BrainCloud.JsonFx.Json;

public class DataWriterProvider : IDataWriterProvider
{
	private readonly IDataWriter DefaultWriter;

	private readonly IDictionary<string, IDataWriter> WritersByExt = new Dictionary<string, IDataWriter>(StringComparer.OrdinalIgnoreCase);

	private readonly IDictionary<string, IDataWriter> WritersByMime = new Dictionary<string, IDataWriter>(StringComparer.OrdinalIgnoreCase);

	public IDataWriter DefaultDataWriter => DefaultWriter;

	public DataWriterProvider(IEnumerable<IDataWriter> writers)
	{
		if (writers == null)
		{
			return;
		}
		foreach (IDataWriter writer in writers)
		{
			if (DefaultWriter == null)
			{
				DefaultWriter = writer;
			}
			if (!string.IsNullOrEmpty(writer.ContentType))
			{
				WritersByMime[writer.ContentType] = writer;
			}
			if (!string.IsNullOrEmpty(writer.ContentType))
			{
				string key = NormalizeExtension(writer.FileExtension);
				WritersByExt[key] = writer;
			}
		}
	}

	public IDataWriter Find(string extension)
	{
		extension = NormalizeExtension(extension);
		if (WritersByExt.ContainsKey(extension))
		{
			return WritersByExt[extension];
		}
		return null;
	}

	public IDataWriter Find(string acceptHeader, string contentTypeHeader)
	{
		foreach (string item in ParseHeaders(acceptHeader, contentTypeHeader))
		{
			if (WritersByMime.ContainsKey(item))
			{
				return WritersByMime[item];
			}
		}
		return null;
	}

	public static IEnumerable<string> ParseHeaders(string accept, string contentType)
	{
		string text;
		foreach (string item in SplitTrim(accept, ','))
		{
			text = ParseMediaType(item);
			if (!string.IsNullOrEmpty(text))
			{
				yield return text;
			}
		}
		text = ParseMediaType(contentType);
		if (!string.IsNullOrEmpty(text))
		{
			yield return text;
		}
	}

	public static string ParseMediaType(string type)
	{
		using (IEnumerator<string> enumerator = SplitTrim(type, ';').GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return string.Empty;
	}

	private static IEnumerable<string> SplitTrim(string source, char ch)
	{
		if (string.IsNullOrEmpty(source))
		{
			yield break;
		}
		int length = source.Length;
		int num = 0;
		int next = 0;
		while (num < length && next >= 0)
		{
			next = source.IndexOf(ch, num);
			if (next < 0)
			{
				next = length;
			}
			string text = source.Substring(num, next - num).Trim();
			if (text.Length > 0)
			{
				yield return text;
			}
			num = next + 1;
		}
	}

	private static string NormalizeExtension(string extension)
	{
		if (string.IsNullOrEmpty(extension))
		{
			return string.Empty;
		}
		return Path.GetExtension(extension);
	}
}
