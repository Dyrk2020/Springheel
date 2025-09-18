using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Principal;

namespace BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp.Net.WebSockets;

public class HttpListenerWebSocketContext : WebSocketContext
{
	private HttpListenerContext _context;

	private WebSocket _websocket;

	internal Logger Log => _context.Listener.Log;

	internal Stream Stream => _context.Connection.Stream;

	public override CookieCollection CookieCollection => _context.Request.Cookies;

	public override NameValueCollection Headers => _context.Request.Headers;

	public override string Host => _context.Request.UserHostName;

	public override bool IsAuthenticated => _context.Request.IsAuthenticated;

	public override bool IsLocal => _context.Request.IsLocal;

	public override bool IsSecureConnection => _context.Request.IsSecureConnection;

	public override bool IsWebSocketRequest => _context.Request.IsWebSocketRequest;

	public override string Origin => _context.Request.Headers["Origin"];

	public override NameValueCollection QueryString => _context.Request.QueryString;

	public override Uri RequestUri => _context.Request.Url;

	public override string SecWebSocketKey => _context.Request.Headers["Sec-WebSocket-Key"];

	public override IEnumerable<string> SecWebSocketProtocols
	{
		get
		{
			string text = _context.Request.Headers["Sec-WebSocket-Protocol"];
			if (text == null || text.Length == 0)
			{
				yield break;
			}
			string[] array = text.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i].Trim();
				if (text2.Length != 0)
				{
					yield return text2;
				}
			}
		}
	}

	public override string SecWebSocketVersion => _context.Request.Headers["Sec-WebSocket-Version"];

	public override IPEndPoint ServerEndPoint => _context.Request.LocalEndPoint;

	public override IPrincipal User => _context.User;

	public override IPEndPoint UserEndPoint => _context.Request.RemoteEndPoint;

	public override WebSocket WebSocket => _websocket;

	internal HttpListenerWebSocketContext(HttpListenerContext context, string protocol)
	{
		_context = context;
		_websocket = new WebSocket(this, protocol);
	}

	internal void Close()
	{
		_context.Connection.Close(force: true);
	}

	internal void Close(HttpStatusCode code)
	{
		_context.Response.Close(code);
	}

	public override string ToString()
	{
		return _context.Request.ToString();
	}
}
