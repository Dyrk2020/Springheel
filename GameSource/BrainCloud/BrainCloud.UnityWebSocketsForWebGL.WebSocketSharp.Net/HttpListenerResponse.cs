using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net;

public sealed class HttpListenerResponse : IDisposable
{
	private bool _closeConnection;

	private Encoding _contentEncoding;

	private long _contentLength;

	private string _contentType;

	private HttpListenerContext _context;

	private CookieCollection _cookies;

	private bool _disposed;

	private WebHeaderCollection _headers;

	private bool _headersSent;

	private bool _keepAlive;

	private string _location;

	private ResponseStream _outputStream;

	private bool _sendChunked;

	private int _statusCode;

	private string _statusDescription;

	private System.Version _version;

	internal bool CloseConnection
	{
		get
		{
			return _closeConnection;
		}
		set
		{
			_closeConnection = value;
		}
	}

	internal WebHeaderCollection FullHeaders
	{
		get
		{
			WebHeaderCollection webHeaderCollection = new WebHeaderCollection(HttpHeaderType.Response, internallyUsed: true);
			if (_headers != null)
			{
				webHeaderCollection.Add(_headers);
			}
			if (_contentType != null)
			{
				webHeaderCollection.InternalSet("Content-Type", createContentTypeHeaderText(_contentType, _contentEncoding), response: true);
			}
			if (webHeaderCollection["Server"] == null)
			{
				webHeaderCollection.InternalSet("Server", "websocket-sharp/1.0", response: true);
			}
			if (webHeaderCollection["Date"] == null)
			{
				webHeaderCollection.InternalSet("Date", DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture), response: true);
			}
			if (_sendChunked)
			{
				webHeaderCollection.InternalSet("Transfer-Encoding", "chunked", response: true);
			}
			else
			{
				webHeaderCollection.InternalSet("Content-Length", _contentLength.ToString(CultureInfo.InvariantCulture), response: true);
			}
			bool num = !_context.Request.KeepAlive || !_keepAlive || _statusCode == 400 || _statusCode == 408 || _statusCode == 411 || _statusCode == 413 || _statusCode == 414 || _statusCode == 500 || _statusCode == 503;
			int reuses = _context.Connection.Reuses;
			if (num || reuses >= 100)
			{
				webHeaderCollection.InternalSet("Connection", "close", response: true);
			}
			else
			{
				webHeaderCollection.InternalSet("Keep-Alive", $"timeout=15,max={100 - reuses}", response: true);
				if (_context.Request.ProtocolVersion < HttpVersion.Version11)
				{
					webHeaderCollection.InternalSet("Connection", "keep-alive", response: true);
				}
			}
			if (_location != null)
			{
				webHeaderCollection.InternalSet("Location", _location, response: true);
			}
			if (_cookies != null)
			{
				foreach (Cookie cookie in _cookies)
				{
					webHeaderCollection.InternalSet("Set-Cookie", cookie.ToResponseString(), response: true);
				}
			}
			return webHeaderCollection;
		}
	}

	internal bool HeadersSent
	{
		get
		{
			return _headersSent;
		}
		set
		{
			_headersSent = value;
		}
	}

	internal string StatusLine => $"HTTP/{_version} {_statusCode} {_statusDescription}\r\n";

	public Encoding ContentEncoding
	{
		get
		{
			return _contentEncoding;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			_contentEncoding = value;
		}
	}

	public long ContentLength64
	{
		get
		{
			return _contentLength;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (_headersSent)
			{
				throw new InvalidOperationException("The response is already being sent.");
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("Less than zero.", "value");
			}
			_contentLength = value;
		}
	}

	public string ContentType
	{
		get
		{
			return _contentType;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (value == null)
			{
				_contentType = null;
				return;
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("An empty string.", "value");
			}
			_contentType = value;
		}
	}

	public CookieCollection Cookies
	{
		get
		{
			if (_cookies == null)
			{
				_cookies = new CookieCollection();
			}
			return _cookies;
		}
		set
		{
			_cookies = value;
		}
	}

	public WebHeaderCollection Headers
	{
		get
		{
			if (_headers == null)
			{
				_headers = new WebHeaderCollection(HttpHeaderType.Response, internallyUsed: false);
			}
			return _headers;
		}
		set
		{
			if (value == null)
			{
				_headers = null;
				return;
			}
			if (value.State != HttpHeaderType.Response)
			{
				throw new InvalidOperationException("The value is not valid for a response.");
			}
			_headers = value;
		}
	}

	public bool KeepAlive
	{
		get
		{
			return _keepAlive;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (_headersSent)
			{
				throw new InvalidOperationException("The response is already being sent.");
			}
			_keepAlive = value;
		}
	}

	public Stream OutputStream
	{
		get
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (_outputStream == null)
			{
				_outputStream = _context.Connection.GetResponseStream();
			}
			return _outputStream;
		}
	}

	public System.Version ProtocolVersion
	{
		get
		{
			return _version;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (_headersSent)
			{
				throw new InvalidOperationException("The response is already being sent.");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Major != 1)
			{
				throw new ArgumentException("Its Major property is not 1.", "value");
			}
			if (value.Minor < 0 || value.Minor > 1)
			{
				throw new ArgumentException("Its Minor property is not 0 or 1.", "value");
			}
			_version = value;
		}
	}

	public string RedirectLocation
	{
		get
		{
			return _location;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (value == null)
			{
				_location = null;
				return;
			}
			if (!value.MaybeUri())
			{
				throw new ArgumentException("Not an absolute URL.", "value");
			}
			if (!Uri.TryCreate(value, UriKind.Absolute, out var _))
			{
				throw new ArgumentException("Not an absolute URL.", "value");
			}
			_location = value;
		}
	}

	public bool SendChunked
	{
		get
		{
			return _sendChunked;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (_headersSent)
			{
				throw new InvalidOperationException("The response is already being sent.");
			}
			_sendChunked = value;
		}
	}

	public int StatusCode
	{
		get
		{
			return _statusCode;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (_headersSent)
			{
				throw new InvalidOperationException("The response is already being sent.");
			}
			if (value < 100 || value > 999)
			{
				throw new ProtocolViolationException("A value is not between 100 and 999 inclusive.");
			}
			_statusCode = value;
			_statusDescription = value.GetStatusDescription();
		}
	}

	public string StatusDescription
	{
		get
		{
			return _statusDescription;
		}
		set
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (_headersSent)
			{
				throw new InvalidOperationException("The response is already being sent.");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				_statusDescription = _statusCode.GetStatusDescription();
				return;
			}
			if (!value.IsText())
			{
				throw new ArgumentException("It contains an invalid character.", "value");
			}
			if (value.IndexOfAny(new char[2] { '\r', '\n' }) > -1)
			{
				throw new ArgumentException("It contains an invalid character.", "value");
			}
			_statusDescription = value;
		}
	}

	internal HttpListenerResponse(HttpListenerContext context)
	{
		_context = context;
		_keepAlive = true;
		_statusCode = 200;
		_statusDescription = "OK";
		_version = HttpVersion.Version11;
	}

	private bool canSetCookie(Cookie cookie)
	{
		List<Cookie> list = findCookie(cookie).ToList();
		if (list.Count == 0)
		{
			return true;
		}
		int version = cookie.Version;
		foreach (Cookie item in list)
		{
			if (item.Version == version)
			{
				return true;
			}
		}
		return false;
	}

	private void close(bool force)
	{
		_disposed = true;
		_context.Connection.Close(force);
	}

	private static string createContentTypeHeaderText(string value, Encoding encoding)
	{
		if (value.IndexOf("charset=", StringComparison.Ordinal) > -1)
		{
			return value;
		}
		if (encoding == null)
		{
			return value;
		}
		return $"{value}; charset={encoding.WebName}";
	}

	private IEnumerable<Cookie> findCookie(Cookie cookie)
	{
		if (_cookies == null || _cookies.Count == 0)
		{
			yield break;
		}
		foreach (Cookie cookie2 in _cookies)
		{
			if (cookie2.EqualsWithoutValueAndVersion(cookie))
			{
				yield return cookie2;
			}
		}
	}

	public void Abort()
	{
		if (!_disposed)
		{
			close(force: true);
		}
	}

	public void AddHeader(string name, string value)
	{
		Headers.Set(name, value);
	}

	public void AppendCookie(Cookie cookie)
	{
		Cookies.Add(cookie);
	}

	public void AppendHeader(string name, string value)
	{
		Headers.Add(name, value);
	}

	public void Close()
	{
		if (!_disposed)
		{
			close(force: false);
		}
	}

	public void Close(byte[] responseEntity, bool willBlock)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(GetType().ToString());
		}
		if (responseEntity == null)
		{
			throw new ArgumentNullException("responseEntity");
		}
		int count = responseEntity.Length;
		Stream output = OutputStream;
		if (willBlock)
		{
			output.Write(responseEntity, 0, count);
			close(force: false);
			return;
		}
		output.BeginWrite(responseEntity, 0, count, delegate(IAsyncResult ar)
		{
			output.EndWrite(ar);
			close(force: false);
		}, null);
	}

	public void CopyFrom(HttpListenerResponse templateResponse)
	{
		if (templateResponse == null)
		{
			throw new ArgumentNullException("templateResponse");
		}
		WebHeaderCollection headers = templateResponse._headers;
		if (headers != null)
		{
			if (_headers != null)
			{
				_headers.Clear();
			}
			Headers.Add(headers);
		}
		else
		{
			_headers = null;
		}
		_contentLength = templateResponse._contentLength;
		_statusCode = templateResponse._statusCode;
		_statusDescription = templateResponse._statusDescription;
		_keepAlive = templateResponse._keepAlive;
		_version = templateResponse._version;
	}

	public void Redirect(string url)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(GetType().ToString());
		}
		if (_headersSent)
		{
			throw new InvalidOperationException("The response is already being sent.");
		}
		if (url == null)
		{
			throw new ArgumentNullException("url");
		}
		if (!url.MaybeUri())
		{
			throw new ArgumentException("Not an absolute URL.", "url");
		}
		if (!Uri.TryCreate(url, UriKind.Absolute, out var _))
		{
			throw new ArgumentException("Not an absolute URL.", "url");
		}
		_location = url;
		_statusCode = 302;
		_statusDescription = "Found";
	}

	public void SetCookie(Cookie cookie)
	{
		if (cookie == null)
		{
			throw new ArgumentNullException("cookie");
		}
		if (!canSetCookie(cookie))
		{
			throw new ArgumentException("It cannot be updated.", "cookie");
		}
		Cookies.Add(cookie);
	}

	void IDisposable.Dispose()
	{
		if (!_disposed)
		{
			close(force: true);
		}
	}
}
