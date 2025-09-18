using System.Collections.Specialized;
using System.Text;

namespace BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net;

internal sealed class QueryStringCollection : NameValueCollection
{
	public QueryStringCollection()
	{
	}

	public QueryStringCollection(int capacity)
		: base(capacity)
	{
	}

	private static string urlDecode(string s, Encoding encoding)
	{
		if (s.IndexOfAny(new char[2] { '%', '+' }) <= -1)
		{
			return s;
		}
		return HttpUtility.UrlDecode(s, encoding);
	}

	public static QueryStringCollection Parse(string query)
	{
		return Parse(query, Encoding.UTF8);
	}

	public static QueryStringCollection Parse(string query, Encoding encoding)
	{
		if (query == null)
		{
			return new QueryStringCollection(1);
		}
		if (query.Length == 0)
		{
			return new QueryStringCollection(1);
		}
		if (query == "?")
		{
			return new QueryStringCollection(1);
		}
		if (query[0] == '?')
		{
			query = query.Substring(1);
		}
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		QueryStringCollection queryStringCollection = new QueryStringCollection();
		string[] array = query.Split('&');
		foreach (string text in array)
		{
			int length = text.Length;
			if (length != 0 && !(text == "="))
			{
				int num = text.IndexOf('=');
				if (num < 0)
				{
					queryStringCollection.Add(null, urlDecode(text, encoding));
					continue;
				}
				if (num == 0)
				{
					queryStringCollection.Add(null, urlDecode(text.Substring(1), encoding));
					continue;
				}
				string name = urlDecode(text.Substring(0, num), encoding);
				int num2 = num + 1;
				string value = ((num2 < length) ? urlDecode(text.Substring(num2), encoding) : string.Empty);
				queryStringCollection.Add(name, value);
			}
		}
		return queryStringCollection;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] allKeys = AllKeys;
		foreach (string text in allKeys)
		{
			stringBuilder.AppendFormat("{0}={1}&", text, base[text]);
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Length--;
		}
		return stringBuilder.ToString();
	}
}
