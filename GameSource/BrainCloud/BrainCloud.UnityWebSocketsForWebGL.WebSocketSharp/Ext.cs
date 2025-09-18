using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net;

namespace BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp;

public static class Ext
{
	private static readonly byte[] _last = new byte[1];

	private static readonly int _retry = 5;

	private const string _tspecials = "()<>@,;:\\\"/[]?={} \t";

	private static byte[] compress(this byte[] data)
	{
		if (data.LongLength == 0L)
		{
			return data;
		}
		using MemoryStream stream = new MemoryStream(data);
		return stream.compressToArray();
	}

	private static MemoryStream compress(this Stream stream)
	{
		MemoryStream memoryStream = new MemoryStream();
		if (stream.Length == 0L)
		{
			return memoryStream;
		}
		stream.Position = 0L;
		using DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, leaveOpen: true);
		stream.CopyTo(deflateStream, 1024);
		deflateStream.Close();
		memoryStream.Write(_last, 0, 1);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private static byte[] compressToArray(this Stream stream)
	{
		using MemoryStream memoryStream = stream.compress();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	private static byte[] decompress(this byte[] data)
	{
		if (data.LongLength == 0L)
		{
			return data;
		}
		using MemoryStream stream = new MemoryStream(data);
		return stream.decompressToArray();
	}

	private static MemoryStream decompress(this Stream stream)
	{
		MemoryStream memoryStream = new MemoryStream();
		if (stream.Length == 0L)
		{
			return memoryStream;
		}
		stream.Position = 0L;
		using DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true);
		deflateStream.CopyTo(memoryStream, 1024);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private static byte[] decompressToArray(this Stream stream)
	{
		using MemoryStream memoryStream = stream.decompress();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	private static bool isHttpMethod(this string value)
	{
		switch (value)
		{
		default:
			return value == "TRACE";
		case "GET":
		case "HEAD":
		case "POST":
		case "PUT":
		case "DELETE":
		case "CONNECT":
		case "OPTIONS":
			return true;
		}
	}

	private static bool isHttpMethod10(this string value)
	{
		if (!(value == "GET") && !(value == "HEAD"))
		{
			return value == "POST";
		}
		return true;
	}

	internal static byte[] Append(this ushort code, string reason)
	{
		byte[] array = code.InternalToByteArray(ByteOrder.Big);
		if (reason == null || reason.Length == 0)
		{
			return array;
		}
		List<byte> list = new List<byte>(array);
		list.AddRange(Encoding.UTF8.GetBytes(reason));
		return list.ToArray();
	}

	internal static void Close(this BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpListenerResponse response, BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpStatusCode code)
	{
		response.StatusCode = (int)code;
		response.OutputStream.Close();
	}

	internal static void CloseWithAuthChallenge(this BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpListenerResponse response, string challenge)
	{
		response.Headers.InternalSet("WWW-Authenticate", challenge, response: true);
		response.Close(BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpStatusCode.Unauthorized);
	}

	internal static byte[] Compress(this byte[] data, CompressionMethod method)
	{
		if (method != CompressionMethod.Deflate)
		{
			return data;
		}
		return data.compress();
	}

	internal static Stream Compress(this Stream stream, CompressionMethod method)
	{
		if (method != CompressionMethod.Deflate)
		{
			return stream;
		}
		return stream.compress();
	}

	internal static byte[] CompressToArray(this Stream stream, CompressionMethod method)
	{
		if (method != CompressionMethod.Deflate)
		{
			return stream.ToByteArray();
		}
		return stream.compressToArray();
	}

	internal static bool Contains(this string value, params char[] anyOf)
	{
		if (anyOf == null || anyOf.Length == 0)
		{
			return false;
		}
		return value.IndexOfAny(anyOf) > -1;
	}

	internal static bool Contains(this NameValueCollection collection, string name)
	{
		return collection[name] != null;
	}

	internal static bool Contains(this NameValueCollection collection, string name, string value, StringComparison comparisonTypeForValue)
	{
		string text = collection[name];
		if (text == null)
		{
			return false;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Trim().Equals(value, comparisonTypeForValue))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> condition)
	{
		foreach (T item in source)
		{
			if (condition(item))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool ContainsTwice(this string[] values)
	{
		int len = values.Length;
		int end = len - 1;
		Func<int, bool> seek = null;
		seek = delegate(int idx)
		{
			if (idx == end)
			{
				return false;
			}
			string text = values[idx];
			for (int i = idx + 1; i < len; i++)
			{
				if (values[i] == text)
				{
					return true;
				}
			}
			return seek(++idx);
		};
		return seek(0);
	}

	internal static T[] Copy<T>(this T[] source, int length)
	{
		T[] array = new T[length];
		Array.Copy(source, 0, array, 0, length);
		return array;
	}

	internal static T[] Copy<T>(this T[] source, long length)
	{
		T[] array = new T[length];
		Array.Copy(source, 0L, array, 0L, length);
		return array;
	}

	internal static void CopyTo(this Stream source, Stream destination, int bufferLength)
	{
		byte[] buffer = new byte[bufferLength];
		int num = 0;
		while (true)
		{
			num = source.Read(buffer, 0, bufferLength);
			if (num > 0)
			{
				destination.Write(buffer, 0, num);
				continue;
			}
			break;
		}
	}

	internal static void CopyToAsync(this Stream source, Stream destination, int bufferLength, Action completed, Action<Exception> error)
	{
		byte[] buff = new byte[bufferLength];
		AsyncCallback callback = null;
		callback = delegate(IAsyncResult ar)
		{
			try
			{
				int num = source.EndRead(ar);
				if (num <= 0)
				{
					if (completed != null)
					{
						completed();
					}
				}
				else
				{
					destination.Write(buff, 0, num);
					source.BeginRead(buff, 0, bufferLength, callback, null);
				}
			}
			catch (Exception obj2)
			{
				if (error != null)
				{
					error(obj2);
				}
			}
		};
		try
		{
			source.BeginRead(buff, 0, bufferLength, callback, null);
		}
		catch (Exception obj)
		{
			if (error != null)
			{
				error(obj);
			}
		}
	}

	internal static byte[] Decompress(this byte[] data, CompressionMethod method)
	{
		if (method != CompressionMethod.Deflate)
		{
			return data;
		}
		return data.decompress();
	}

	internal static Stream Decompress(this Stream stream, CompressionMethod method)
	{
		if (method != CompressionMethod.Deflate)
		{
			return stream;
		}
		return stream.decompress();
	}

	internal static byte[] DecompressToArray(this Stream stream, CompressionMethod method)
	{
		if (method != CompressionMethod.Deflate)
		{
			return stream.ToByteArray();
		}
		return stream.decompressToArray();
	}

	internal static bool EqualsWith(this int value, char c, Action<int> action)
	{
		action(value);
		return value == c;
	}

	internal static string GetAbsolutePath(this Uri uri)
	{
		if (uri.IsAbsoluteUri)
		{
			return uri.AbsolutePath;
		}
		string originalString = uri.OriginalString;
		if (originalString[0] != '/')
		{
			return null;
		}
		int num = originalString.IndexOfAny(new char[2] { '?', '#' });
		if (num <= 0)
		{
			return originalString;
		}
		return originalString.Substring(0, num);
	}

	internal static BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.CookieCollection GetCookies(this NameValueCollection headers, bool response)
	{
		string text = headers[response ? "Set-Cookie" : "Cookie"];
		if (text == null)
		{
			return new BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.CookieCollection();
		}
		return BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.CookieCollection.Parse(text, response);
	}

	internal static string GetDnsSafeHost(this Uri uri, bool bracketIPv6)
	{
		if (!bracketIPv6 || uri.HostNameType != UriHostNameType.IPv6)
		{
			return uri.DnsSafeHost;
		}
		return uri.Host;
	}

	internal static string GetMessage(this CloseStatusCode code)
	{
		return code switch
		{
			CloseStatusCode.TlsHandshakeFailure => "An error has occurred during a TLS handshake.", 
			CloseStatusCode.ServerError => "WebSocket server got an internal error.", 
			CloseStatusCode.MandatoryExtension => "WebSocket client didn't receive expected extension(s).", 
			CloseStatusCode.TooBig => "A too big message has been received.", 
			CloseStatusCode.PolicyViolation => "A policy violation has occurred.", 
			CloseStatusCode.InvalidData => "Invalid data has been received.", 
			CloseStatusCode.Abnormal => "An exception has occurred.", 
			CloseStatusCode.UnsupportedData => "Unsupported data has been received.", 
			CloseStatusCode.ProtocolError => "A WebSocket protocol error has occurred.", 
			_ => string.Empty, 
		};
	}

	internal static string GetName(this string nameAndValue, char separator)
	{
		int num = nameAndValue.IndexOf(separator);
		if (num <= 0)
		{
			return null;
		}
		return nameAndValue.Substring(0, num).Trim();
	}

	internal static string GetUTF8DecodedString(this byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}

	internal static byte[] GetUTF8EncodedBytes(this string s)
	{
		return Encoding.UTF8.GetBytes(s);
	}

	internal static string GetValue(this string nameAndValue, char separator)
	{
		return nameAndValue.GetValue(separator, unquote: false);
	}

	internal static string GetValue(this string nameAndValue, char separator, bool unquote)
	{
		int num = nameAndValue.IndexOf(separator);
		if (num < 0 || num == nameAndValue.Length - 1)
		{
			return null;
		}
		string text = nameAndValue.Substring(num + 1).Trim();
		if (!unquote)
		{
			return text;
		}
		return text.Unquote();
	}

	internal static byte[] InternalToByteArray(this ushort value, ByteOrder order)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (!order.IsHostOrder())
		{
			Array.Reverse(bytes);
		}
		return bytes;
	}

	internal static byte[] InternalToByteArray(this ulong value, ByteOrder order)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (!order.IsHostOrder())
		{
			Array.Reverse(bytes);
		}
		return bytes;
	}

	internal static bool IsCompressionExtension(this string value, CompressionMethod method)
	{
		return value.StartsWith(method.ToExtensionString());
	}

	internal static bool IsControl(this byte opcode)
	{
		if (opcode > 7)
		{
			return opcode < 16;
		}
		return false;
	}

	internal static bool IsControl(this Opcode opcode)
	{
		return (int)opcode >= 8;
	}

	internal static bool IsData(this byte opcode)
	{
		if (opcode != 1)
		{
			return opcode == 2;
		}
		return true;
	}

	internal static bool IsData(this Opcode opcode)
	{
		if (opcode != Opcode.Text)
		{
			return opcode == Opcode.Binary;
		}
		return true;
	}

	internal static bool IsHttpMethod(this string value, System.Version version)
	{
		if (!(version == BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpVersion.Version10))
		{
			return value.isHttpMethod();
		}
		return value.isHttpMethod10();
	}

	internal static bool IsPortNumber(this int value)
	{
		if (value > 0)
		{
			return value < 65536;
		}
		return false;
	}

	internal static bool IsReserved(this ushort code)
	{
		if (code != 1004 && code != 1005 && code != 1006)
		{
			return code == 1015;
		}
		return true;
	}

	internal static bool IsReserved(this CloseStatusCode code)
	{
		if (code != CloseStatusCode.Undefined && code != CloseStatusCode.NoStatus && code != CloseStatusCode.Abnormal)
		{
			return code == CloseStatusCode.TlsHandshakeFailure;
		}
		return true;
	}

	internal static bool IsSupported(this byte opcode)
	{
		return Enum.IsDefined(typeof(Opcode), opcode);
	}

	internal static bool IsText(this string value)
	{
		int length = value.Length;
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			if (c < ' ')
			{
				if ("\r\n\t".IndexOf(c) == -1)
				{
					return false;
				}
				if (c == '\n')
				{
					i++;
					if (i == length)
					{
						break;
					}
					c = value[i];
					if (" \t".IndexOf(c) == -1)
					{
						return false;
					}
				}
			}
			else if (c == '\u007f')
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsToken(this string value)
	{
		foreach (char c in value)
		{
			if (c < ' ')
			{
				return false;
			}
			if (c > '~')
			{
				return false;
			}
			if ("()<>@,;:\\\"/[]?={} \t".IndexOf(c) > -1)
			{
				return false;
			}
		}
		return true;
	}

	internal static bool KeepsAlive(this NameValueCollection headers, System.Version version)
	{
		StringComparison comparisonTypeForValue = StringComparison.OrdinalIgnoreCase;
		if (!(version < BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpVersion.Version11))
		{
			return !headers.Contains("Connection", "close", comparisonTypeForValue);
		}
		return headers.Contains("Connection", "keep-alive", comparisonTypeForValue);
	}

	internal static string Quote(this string value)
	{
		return string.Format("\"{0}\"", value.Replace("\"", "\\\""));
	}

	internal static byte[] ReadBytes(this Stream stream, int length)
	{
		byte[] array = new byte[length];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (length > 0)
		{
			num3 = stream.Read(array, num, length);
			if (num3 <= 0)
			{
				if (num2 >= _retry)
				{
					return array.SubArray(0, num);
				}
				num2++;
			}
			else
			{
				num2 = 0;
				num += num3;
				length -= num3;
			}
		}
		return array;
	}

	internal static byte[] ReadBytes(this Stream stream, long length, int bufferLength)
	{
		using MemoryStream memoryStream = new MemoryStream();
		byte[] buffer = new byte[bufferLength];
		int num = 0;
		int num2 = 0;
		while (length > 0)
		{
			if (length < bufferLength)
			{
				bufferLength = (int)length;
			}
			num2 = stream.Read(buffer, 0, bufferLength);
			if (num2 <= 0)
			{
				if (num >= _retry)
				{
					break;
				}
				num++;
			}
			else
			{
				num = 0;
				memoryStream.Write(buffer, 0, num2);
				length -= num2;
			}
		}
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	internal static void ReadBytesAsync(this Stream stream, int length, Action<byte[]> completed, Action<Exception> error)
	{
		byte[] buff = new byte[length];
		int offset = 0;
		int retry = 0;
		AsyncCallback callback = null;
		callback = delegate(IAsyncResult ar)
		{
			try
			{
				int num = stream.EndRead(ar);
				if (num <= 0)
				{
					if (retry < _retry)
					{
						retry++;
						stream.BeginRead(buff, offset, length, callback, null);
					}
					else if (completed != null)
					{
						completed(buff.SubArray(0, offset));
					}
				}
				else if (num == length)
				{
					if (completed != null)
					{
						completed(buff);
					}
				}
				else
				{
					retry = 0;
					offset += num;
					length -= num;
					stream.BeginRead(buff, offset, length, callback, null);
				}
			}
			catch (Exception obj2)
			{
				if (error != null)
				{
					error(obj2);
				}
			}
		};
		try
		{
			stream.BeginRead(buff, offset, length, callback, null);
		}
		catch (Exception obj)
		{
			if (error != null)
			{
				error(obj);
			}
		}
	}

	internal static void ReadBytesAsync(this Stream stream, long length, int bufferLength, Action<byte[]> completed, Action<Exception> error)
	{
		MemoryStream dest = new MemoryStream();
		byte[] buff = new byte[bufferLength];
		int retry = 0;
		Action<long> read = null;
		read = delegate(long len)
		{
			if (len < bufferLength)
			{
				bufferLength = (int)len;
			}
			stream.BeginRead(buff, 0, bufferLength, delegate(IAsyncResult ar)
			{
				try
				{
					int num = stream.EndRead(ar);
					if (num <= 0)
					{
						if (retry < _retry)
						{
							int num2 = retry;
							retry = num2 + 1;
							read(len);
						}
						else
						{
							if (completed != null)
							{
								dest.Close();
								completed(dest.ToArray());
							}
							dest.Dispose();
						}
					}
					else
					{
						dest.Write(buff, 0, num);
						if (num == len)
						{
							if (completed != null)
							{
								dest.Close();
								completed(dest.ToArray());
							}
							dest.Dispose();
						}
						else
						{
							retry = 0;
							read(len - num);
						}
					}
				}
				catch (Exception obj2)
				{
					dest.Dispose();
					if (error != null)
					{
						error(obj2);
					}
				}
			}, null);
		};
		try
		{
			read(length);
		}
		catch (Exception obj)
		{
			dest.Dispose();
			if (error != null)
			{
				error(obj);
			}
		}
	}

	internal static T[] Reverse<T>(this T[] array)
	{
		int num = array.Length;
		T[] array2 = new T[num];
		int num2 = num - 1;
		for (int i = 0; i <= num2; i++)
		{
			array2[i] = array[num2 - i];
		}
		return array2;
	}

	internal static IEnumerable<string> SplitHeaderValue(this string value, params char[] separators)
	{
		int length = value.Length;
		StringBuilder buff = new StringBuilder(32);
		int end = length - 1;
		bool escaped = false;
		bool quoted = false;
		for (int i = 0; i <= end; i++)
		{
			char c = value[i];
			buff.Append(c);
			switch (c)
			{
			case '"':
				if (escaped)
				{
					escaped = false;
				}
				else
				{
					quoted = !quoted;
				}
				continue;
			case '\\':
				if (i != end)
				{
					if (value[i + 1] == '"')
					{
						escaped = true;
					}
					continue;
				}
				break;
			default:
				if (Array.IndexOf(separators, c) > -1 && !quoted)
				{
					buff.Length--;
					yield return buff.ToString();
					buff.Length = 0;
				}
				continue;
			}
			break;
		}
		yield return buff.ToString();
	}

	internal static byte[] ToByteArray(this Stream stream)
	{
		using MemoryStream memoryStream = new MemoryStream();
		stream.Position = 0L;
		stream.CopyTo(memoryStream, 1024);
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	internal static CompressionMethod ToCompressionMethod(this string value)
	{
		foreach (CompressionMethod value2 in Enum.GetValues(typeof(CompressionMethod)))
		{
			if (value2.ToExtensionString() == value)
			{
				return value2;
			}
		}
		return CompressionMethod.None;
	}

	internal static string ToExtensionString(this CompressionMethod method, params string[] parameters)
	{
		if (method == CompressionMethod.None)
		{
			return string.Empty;
		}
		string text = $"permessage-{method.ToString().ToLower()}";
		if (parameters == null || parameters.Length == 0)
		{
			return text;
		}
		return string.Format("{0}; {1}", text, parameters.ToString("; "));
	}

	internal static IPAddress ToIPAddress(this string value)
	{
		if (value == null || value.Length == 0)
		{
			return null;
		}
		if (IPAddress.TryParse(value, out var address))
		{
			return address;
		}
		try
		{
			return Dns.GetHostAddresses(value)[0];
		}
		catch
		{
			return null;
		}
	}

	internal static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
	{
		return new List<TSource>(source);
	}

	internal static string ToString(this IPAddress address, bool bracketIPv6)
	{
		if (!bracketIPv6 || address.AddressFamily != AddressFamily.InterNetworkV6)
		{
			return address.ToString();
		}
		return $"[{address.ToString()}]";
	}

	internal static ushort ToUInt16(this byte[] source, ByteOrder sourceOrder)
	{
		return BitConverter.ToUInt16(source.ToHostOrder(sourceOrder), 0);
	}

	internal static ulong ToUInt64(this byte[] source, ByteOrder sourceOrder)
	{
		return BitConverter.ToUInt64(source.ToHostOrder(sourceOrder), 0);
	}

	internal static IEnumerable<string> Trim(this IEnumerable<string> source)
	{
		foreach (string item in source)
		{
			yield return item.Trim();
		}
	}

	internal static string TrimSlashFromEnd(this string value)
	{
		string text = value.TrimEnd('/');
		if (text.Length <= 0)
		{
			return "/";
		}
		return text;
	}

	internal static string TrimSlashOrBackslashFromEnd(this string value)
	{
		string text = value.TrimEnd('/', '\\');
		if (text.Length <= 0)
		{
			return value[0].ToString();
		}
		return text;
	}

	internal static bool TryCreateVersion(this string versionString, out System.Version result)
	{
		result = null;
		try
		{
			result = new System.Version(versionString);
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static bool TryCreateWebSocketUri(this string uriString, out Uri result, out string message)
	{
		result = null;
		message = null;
		Uri uri = uriString.ToUri();
		if (uri == null)
		{
			message = "An invalid URI string.";
			return false;
		}
		if (!uri.IsAbsoluteUri)
		{
			message = "A relative URI.";
			return false;
		}
		string scheme = uri.Scheme;
		if (!(scheme == "ws") && !(scheme == "wss"))
		{
			message = "The scheme part is not 'ws' or 'wss'.";
			return false;
		}
		int port = uri.Port;
		if (port == 0)
		{
			message = "The port part is zero.";
			return false;
		}
		if (uri.Fragment.Length > 0)
		{
			message = "It includes the fragment component.";
			return false;
		}
		result = ((port != -1) ? uri : new Uri(string.Format("{0}://{1}:{2}{3}", scheme, uri.Host, (scheme == "ws") ? 80 : 443, uri.PathAndQuery)));
		return true;
	}

	internal static bool TryGetUTF8DecodedString(this byte[] bytes, out string s)
	{
		s = null;
		try
		{
			s = Encoding.UTF8.GetString(bytes);
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static bool TryGetUTF8EncodedBytes(this string s, out byte[] bytes)
	{
		bytes = null;
		try
		{
			bytes = Encoding.UTF8.GetBytes(s);
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static bool TryOpenRead(this FileInfo fileInfo, out FileStream fileStream)
	{
		fileStream = null;
		try
		{
			fileStream = fileInfo.OpenRead();
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static string Unquote(this string value)
	{
		int num = value.IndexOf('"');
		if (num == -1)
		{
			return value;
		}
		int num2 = value.LastIndexOf('"');
		if (num2 == num)
		{
			return value;
		}
		int num3 = num2 - num - 1;
		if (num3 <= 0)
		{
			return string.Empty;
		}
		return value.Substring(num + 1, num3).Replace("\\\"", "\"");
	}

	internal static bool Upgrades(this NameValueCollection headers, string protocol)
	{
		StringComparison comparisonTypeForValue = StringComparison.OrdinalIgnoreCase;
		if (headers.Contains("Upgrade", protocol, comparisonTypeForValue))
		{
			return headers.Contains("Connection", "Upgrade", comparisonTypeForValue);
		}
		return false;
	}

	internal static string UrlDecode(this string value, Encoding encoding)
	{
		return HttpUtility.UrlDecode(value, encoding);
	}

	internal static string UrlEncode(this string value, Encoding encoding)
	{
		return HttpUtility.UrlEncode(value, encoding);
	}

	internal static void WriteBytes(this Stream stream, byte[] bytes, int bufferLength)
	{
		using MemoryStream memoryStream = new MemoryStream(bytes);
		memoryStream.CopyTo(stream, bufferLength);
	}

	internal static void WriteBytesAsync(this Stream stream, byte[] bytes, int bufferLength, Action completed, Action<Exception> error)
	{
		MemoryStream src = new MemoryStream(bytes);
		src.CopyToAsync(stream, bufferLength, delegate
		{
			if (completed != null)
			{
				completed();
			}
			src.Dispose();
		}, delegate(Exception ex)
		{
			src.Dispose();
			if (error != null)
			{
				error(ex);
			}
		});
	}

	public static void Emit(this EventHandler eventHandler, object sender, EventArgs e)
	{
		eventHandler?.Invoke(sender, e);
	}

	public static void Emit<TEventArgs>(this EventHandler<TEventArgs> eventHandler, object sender, TEventArgs e) where TEventArgs : EventArgs
	{
		eventHandler?.Invoke(sender, e);
	}

	public static string GetDescription(this BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpStatusCode code)
	{
		return ((int)code).GetStatusDescription();
	}

	public static string GetStatusDescription(this int code)
	{
		return code switch
		{
			100 => "Continue", 
			101 => "Switching Protocols", 
			102 => "Processing", 
			200 => "OK", 
			201 => "Created", 
			202 => "Accepted", 
			203 => "Non-Authoritative Information", 
			204 => "No Content", 
			205 => "Reset Content", 
			206 => "Partial Content", 
			207 => "Multi-Status", 
			300 => "Multiple Choices", 
			301 => "Moved Permanently", 
			302 => "Found", 
			303 => "See Other", 
			304 => "Not Modified", 
			305 => "Use Proxy", 
			307 => "Temporary Redirect", 
			400 => "Bad Request", 
			401 => "Unauthorized", 
			402 => "Payment Required", 
			403 => "Forbidden", 
			404 => "Not Found", 
			405 => "Method Not Allowed", 
			406 => "Not Acceptable", 
			407 => "Proxy Authentication Required", 
			408 => "Request Timeout", 
			409 => "Conflict", 
			410 => "Gone", 
			411 => "Length Required", 
			412 => "Precondition Failed", 
			413 => "Request Entity Too Large", 
			414 => "Request-Uri Too Long", 
			415 => "Unsupported Media Type", 
			416 => "Requested Range Not Satisfiable", 
			417 => "Expectation Failed", 
			422 => "Unprocessable Entity", 
			423 => "Locked", 
			424 => "Failed Dependency", 
			500 => "Internal Server Error", 
			501 => "Not Implemented", 
			502 => "Bad Gateway", 
			503 => "Service Unavailable", 
			504 => "Gateway Timeout", 
			505 => "Http Version Not Supported", 
			507 => "Insufficient Storage", 
			_ => string.Empty, 
		};
	}

	public static bool IsCloseStatusCode(this ushort value)
	{
		if (value > 999)
		{
			return value < 5000;
		}
		return false;
	}

	public static bool IsEnclosedIn(this string value, char c)
	{
		if (value == null)
		{
			return false;
		}
		int length = value.Length;
		if (length < 2)
		{
			return false;
		}
		if (value[0] == c)
		{
			return value[length - 1] == c;
		}
		return false;
	}

	public static bool IsHostOrder(this ByteOrder order)
	{
		return BitConverter.IsLittleEndian == (order == ByteOrder.Little);
	}

	public static bool IsLocal(this IPAddress address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (address.Equals(IPAddress.Any))
		{
			return true;
		}
		if (address.Equals(IPAddress.Loopback))
		{
			return true;
		}
		if (Socket.OSSupportsIPv6)
		{
			if (address.Equals(IPAddress.IPv6Any))
			{
				return true;
			}
			if (address.Equals(IPAddress.IPv6Loopback))
			{
				return true;
			}
		}
		IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
		foreach (IPAddress obj in hostAddresses)
		{
			if (address.Equals(obj))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNullOrEmpty(this string value)
	{
		if (value != null)
		{
			return value.Length == 0;
		}
		return true;
	}

	public static bool IsPredefinedScheme(this string value)
	{
		if (value == null || value.Length < 2)
		{
			return false;
		}
		switch (value[0])
		{
		case 'h':
			if (!(value == "http"))
			{
				return value == "https";
			}
			return true;
		case 'w':
			if (!(value == "ws"))
			{
				return value == "wss";
			}
			return true;
		case 'f':
			if (!(value == "file"))
			{
				return value == "ftp";
			}
			return true;
		case 'g':
			return value == "gopher";
		case 'm':
			return value == "mailto";
		case 'n':
		{
			char c = value[1];
			if (c != 'e')
			{
				return value == "nntp";
			}
			if (!(value == "news") && !(value == "net.pipe"))
			{
				return value == "net.tcp";
			}
			return true;
		}
		default:
			return false;
		}
	}

	public static bool MaybeUri(this string value)
	{
		if (value == null)
		{
			return false;
		}
		if (value.Length == 0)
		{
			return false;
		}
		int num = value.IndexOf(':');
		if (num == -1)
		{
			return false;
		}
		if (num >= 10)
		{
			return false;
		}
		return value.Substring(0, num).IsPredefinedScheme();
	}

	public static T[] SubArray<T>(this T[] array, int startIndex, int length)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = array.Length;
		if (num == 0)
		{
			if (startIndex != 0)
			{
				throw new ArgumentOutOfRangeException("startIndex");
			}
			if (length != 0)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			return array;
		}
		if (startIndex < 0 || startIndex >= num)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (length < 0 || length > num - startIndex)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (length == 0)
		{
			return new T[0];
		}
		if (length == num)
		{
			return array;
		}
		T[] array2 = new T[length];
		Array.Copy(array, startIndex, array2, 0, length);
		return array2;
	}

	public static T[] SubArray<T>(this T[] array, long startIndex, long length)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		long num = array.LongLength;
		if (num == 0L)
		{
			if (startIndex != 0L)
			{
				throw new ArgumentOutOfRangeException("startIndex");
			}
			if (length != 0L)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			return array;
		}
		if (startIndex < 0 || startIndex >= num)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (length < 0 || length > num - startIndex)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (length == 0L)
		{
			return new T[0];
		}
		if (length == num)
		{
			return array;
		}
		T[] array2 = new T[length];
		Array.Copy(array, startIndex, array2, 0L, length);
		return array2;
	}

	public static void Times(this int n, Action action)
	{
		if (n > 0 && action != null)
		{
			for (int i = 0; i < n; i++)
			{
				action();
			}
		}
	}

	public static void Times(this long n, Action action)
	{
		if (n > 0 && action != null)
		{
			for (long num = 0L; num < n; num++)
			{
				action();
			}
		}
	}

	public static void Times(this uint n, Action action)
	{
		if (n != 0 && action != null)
		{
			for (uint num = 0u; num < n; num++)
			{
				action();
			}
		}
	}

	public static void Times(this ulong n, Action action)
	{
		if (n != 0L && action != null)
		{
			for (ulong num = 0uL; num < n; num++)
			{
				action();
			}
		}
	}

	public static void Times(this int n, Action<int> action)
	{
		if (n > 0 && action != null)
		{
			for (int i = 0; i < n; i++)
			{
				action(i);
			}
		}
	}

	public static void Times(this long n, Action<long> action)
	{
		if (n > 0 && action != null)
		{
			for (long num = 0L; num < n; num++)
			{
				action(num);
			}
		}
	}

	public static void Times(this uint n, Action<uint> action)
	{
		if (n != 0 && action != null)
		{
			for (uint num = 0u; num < n; num++)
			{
				action(num);
			}
		}
	}

	public static void Times(this ulong n, Action<ulong> action)
	{
		if (n != 0L && action != null)
		{
			for (ulong num = 0uL; num < n; num++)
			{
				action(num);
			}
		}
	}

	[Obsolete("This method will be removed.")]
	public static T To<T>(this byte[] source, ByteOrder sourceOrder) where T : struct
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.Length == 0)
		{
			return default(T);
		}
		Type typeFromHandle = typeof(T);
		byte[] value = source.ToHostOrder(sourceOrder);
		if (!(typeFromHandle == typeof(bool)))
		{
			if (!(typeFromHandle == typeof(char)))
			{
				if (!(typeFromHandle == typeof(double)))
				{
					if (!(typeFromHandle == typeof(short)))
					{
						if (!(typeFromHandle == typeof(int)))
						{
							if (!(typeFromHandle == typeof(long)))
							{
								if (!(typeFromHandle == typeof(float)))
								{
									if (!(typeFromHandle == typeof(ushort)))
									{
										if (!(typeFromHandle == typeof(uint)))
										{
											if (!(typeFromHandle == typeof(ulong)))
											{
												return default(T);
											}
											return (T)(object)BitConverter.ToUInt64(value, 0);
										}
										return (T)(object)BitConverter.ToUInt32(value, 0);
									}
									return (T)(object)BitConverter.ToUInt16(value, 0);
								}
								return (T)(object)BitConverter.ToSingle(value, 0);
							}
							return (T)(object)BitConverter.ToInt64(value, 0);
						}
						return (T)(object)BitConverter.ToInt32(value, 0);
					}
					return (T)(object)BitConverter.ToInt16(value, 0);
				}
				return (T)(object)BitConverter.ToDouble(value, 0);
			}
			return (T)(object)BitConverter.ToChar(value, 0);
		}
		return (T)(object)BitConverter.ToBoolean(value, 0);
	}

	[Obsolete("This method will be removed.")]
	public static byte[] ToByteArray<T>(this T value, ByteOrder order) where T : struct
	{
		Type typeFromHandle = typeof(T);
		byte[] array = ((typeFromHandle == typeof(bool)) ? BitConverter.GetBytes((bool)(object)value) : ((!(typeFromHandle == typeof(byte))) ? ((typeFromHandle == typeof(char)) ? BitConverter.GetBytes((char)(object)value) : ((typeFromHandle == typeof(double)) ? BitConverter.GetBytes((double)(object)value) : ((typeFromHandle == typeof(short)) ? BitConverter.GetBytes((short)(object)value) : ((typeFromHandle == typeof(int)) ? BitConverter.GetBytes((int)(object)value) : ((typeFromHandle == typeof(long)) ? BitConverter.GetBytes((long)(object)value) : ((typeFromHandle == typeof(float)) ? BitConverter.GetBytes((float)(object)value) : ((typeFromHandle == typeof(ushort)) ? BitConverter.GetBytes((ushort)(object)value) : ((typeFromHandle == typeof(uint)) ? BitConverter.GetBytes((uint)(object)value) : ((typeFromHandle == typeof(ulong)) ? BitConverter.GetBytes((ulong)(object)value) : WebSocket.EmptyBytes))))))))) : new byte[1] { (byte)(object)value }));
		if (array.Length > 1 && !order.IsHostOrder())
		{
			Array.Reverse(array);
		}
		return array;
	}

	public static byte[] ToHostOrder(this byte[] source, ByteOrder sourceOrder)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.Length < 2)
		{
			return source;
		}
		if (sourceOrder.IsHostOrder())
		{
			return source;
		}
		return source.Reverse();
	}

	public static string ToString<T>(this T[] array, string separator)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = array.Length;
		if (num == 0)
		{
			return string.Empty;
		}
		if (separator == null)
		{
			separator = string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(64);
		int num2 = num - 1;
		for (int i = 0; i < num2; i++)
		{
			stringBuilder.AppendFormat("{0}{1}", array[i], separator);
		}
		stringBuilder.Append(array[num2].ToString());
		return stringBuilder.ToString();
	}

	public static Uri ToUri(this string value)
	{
		Uri.TryCreate(value, value.MaybeUri() ? UriKind.Absolute : UriKind.Relative, out var result);
		return result;
	}

	[Obsolete("This method will be removed.")]
	public static void WriteContent(this BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.HttpListenerResponse response, byte[] content)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		long num = content.LongLength;
		if (num == 0L)
		{
			response.Close();
			return;
		}
		response.ContentLength64 = num;
		Stream outputStream = response.OutputStream;
		if (num <= int.MaxValue)
		{
			outputStream.Write(content, 0, (int)num);
		}
		else
		{
			outputStream.WriteBytes(content, 1024);
		}
		outputStream.Close();
	}
}
