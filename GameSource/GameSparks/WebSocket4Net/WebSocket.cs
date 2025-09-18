using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using SuperSocket.ClientEngine;
using SuperSocket.ClientEngine.Protocol;
using WebSocket4Net.Command;
using WebSocket4Net.Protocol;

namespace WebSocket4Net;

public class WebSocket : IDisposable
{
	protected const string UserAgentKey = "UserAgent";

	private const string m_UriScheme = "ws";

	private const string m_UriPrefix = "ws://";

	private const string m_SecureUriScheme = "wss";

	private const int m_SecurePort = 443;

	private const string m_SecureUriPrefix = "wss://";

	private const string m_NotOpenSendingMessage = "You must send data by websocket after websocket is opened!";

	private int m_StateCode;

	private Dictionary<string, ICommand<WebSocket, WebSocketCommandInfo>> m_CommandDict = new Dictionary<string, ICommand<WebSocket, WebSocketCommandInfo>>(StringComparer.OrdinalIgnoreCase);

	private static ProtocolProcessorFactory m_ProtocolProcessorFactory;

	private Timer m_WebSocketTimer;

	private string m_LastPingRequest;

	private EventHandler m_Opened;

	private EventHandler<MessageReceivedEventArgs> m_MessageReceived;

	private EventHandler<DataReceivedEventArgs> m_DataReceived;

	private EventHandler m_Closed;

	private EventHandler<ErrorEventArgs> m_Error;

	private bool m_AllowUnstrustedCertificate;

	internal TcpClientSession Client { get; private set; }

	public WebSocketVersion Version { get; private set; }

	public DateTime LastActiveTime { get; internal set; }

	public bool EnableAutoSendPing { get; set; }

	public int AutoSendPingInterval { get; set; }

	internal IProtocolProcessor ProtocolProcessor { get; private set; }

	public bool SupportBinary => ProtocolProcessor.SupportBinary;

	internal Uri TargetUri { get; private set; }

	internal string SubProtocol { get; private set; }

	internal IDictionary<string, object> Items { get; private set; }

	internal List<KeyValuePair<string, string>> Cookies { get; private set; }

	internal List<KeyValuePair<string, string>> CustomHeaderItems { get; private set; }

	internal int StateCode => m_StateCode;

	public WebSocketState State => (WebSocketState)m_StateCode;

	public bool Handshaked { get; private set; }

	public IProxyConnector Proxy { get; set; }

	protected IClientCommandReader<WebSocketCommandInfo> CommandReader { get; private set; }

	internal bool NotSpecifiedVersion { get; private set; }

	internal string LastPongResponse { get; set; }

	internal string HandshakeHost { get; private set; }

	internal string Origin { get; private set; }

	public bool NoDelay { get; set; }

	public int ReceiveBufferSize
	{
		get
		{
			return Client.ReceiveBufferSize;
		}
		set
		{
			Client.ReceiveBufferSize = value;
		}
	}

	public bool AllowUnstrustedCertificate
	{
		get
		{
			return m_AllowUnstrustedCertificate;
		}
		set
		{
			m_AllowUnstrustedCertificate = value;
			if (Client is SslStreamTcpSession sslStreamTcpSession)
			{
				sslStreamTcpSession.AllowUnstrustedCertificate = m_AllowUnstrustedCertificate;
			}
		}
	}

	public event EventHandler Opened
	{
		add
		{
			m_Opened = (EventHandler)Delegate.Combine(m_Opened, value);
		}
		remove
		{
			m_Opened = (EventHandler)Delegate.Remove(m_Opened, value);
		}
	}

	public event EventHandler<MessageReceivedEventArgs> MessageReceived
	{
		add
		{
			m_MessageReceived = (EventHandler<MessageReceivedEventArgs>)Delegate.Combine(m_MessageReceived, value);
		}
		remove
		{
			m_MessageReceived = (EventHandler<MessageReceivedEventArgs>)Delegate.Remove(m_MessageReceived, value);
		}
	}

	public event EventHandler<DataReceivedEventArgs> DataReceived
	{
		add
		{
			m_DataReceived = (EventHandler<DataReceivedEventArgs>)Delegate.Combine(m_DataReceived, value);
		}
		remove
		{
			m_DataReceived = (EventHandler<DataReceivedEventArgs>)Delegate.Remove(m_DataReceived, value);
		}
	}

	public event EventHandler Closed
	{
		add
		{
			m_Closed = (EventHandler)Delegate.Combine(m_Closed, value);
		}
		remove
		{
			m_Closed = (EventHandler)Delegate.Remove(m_Closed, value);
		}
	}

	public event EventHandler<ErrorEventArgs> Error
	{
		add
		{
			m_Error = (EventHandler<ErrorEventArgs>)Delegate.Combine(m_Error, value);
		}
		remove
		{
			m_Error = (EventHandler<ErrorEventArgs>)Delegate.Remove(m_Error, value);
		}
	}

	static WebSocket()
	{
		m_ProtocolProcessorFactory = new ProtocolProcessorFactory(new Rfc6455Processor(), new DraftHybi10Processor(), new DraftHybi00Processor());
	}

	private EndPoint ResolveUri(string uri, int defaultPort, out int port)
	{
		TargetUri = new Uri(uri);
		port = TargetUri.Port;
		if (port <= 0)
		{
			port = defaultPort;
		}
		if (IPAddress.TryParse(TargetUri.Host, out var address))
		{
			return new IPEndPoint(address, port);
		}
		return new DnsEndPoint2(TargetUri.Host, port);
	}

	private TcpClientSession CreateClient(string uri)
	{
		int port;
		EndPoint remoteEndPoint = ResolveUri(uri, 80, out port);
		if (port == 80)
		{
			HandshakeHost = TargetUri.Host;
		}
		else
		{
			HandshakeHost = TargetUri.Host + ":" + port;
		}
		return new AsyncTcpSession(remoteEndPoint);
	}

	private TcpClientSession CreateSecureClient(string uri)
	{
		int num = uri.IndexOf('/', "wss://".Length);
		if (num < 0)
		{
			num = uri.IndexOf(':', "wss://".Length, uri.Length - "wss://".Length);
			uri = ((num >= 0) ? (uri + "/") : (uri + ":" + 443 + "/"));
		}
		else
		{
			if (num == "wss://".Length)
			{
				throw new ArgumentException("Invalid uri", "uri");
			}
			int num2 = uri.IndexOf(':', "wss://".Length, num - "wss://".Length);
			if (num2 < 0)
			{
				uri = uri.Substring(0, num) + ":" + 443 + uri.Substring(num);
			}
		}
		int port;
		EndPoint remoteEndPoint = ResolveUri(uri, 443, out port);
		if (port == 443)
		{
			HandshakeHost = TargetUri.Host;
		}
		else
		{
			HandshakeHost = TargetUri.Host + ":" + port;
		}
		return new SslStreamTcpSession(remoteEndPoint);
	}

	private void Initialize(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, string origin, WebSocketVersion version)
	{
		if (version == WebSocketVersion.None)
		{
			NotSpecifiedVersion = true;
			version = WebSocketVersion.Rfc6455;
		}
		Version = version;
		ProtocolProcessor = GetProtocolProcessor(version);
		Cookies = cookies;
		Origin = origin;
		if (!string.IsNullOrEmpty(userAgent))
		{
			if (customHeaderItems == null)
			{
				customHeaderItems = new List<KeyValuePair<string, string>>();
			}
			customHeaderItems.Add(new KeyValuePair<string, string>("UserAgent", userAgent));
		}
		if (customHeaderItems != null && customHeaderItems.Count > 0)
		{
			CustomHeaderItems = customHeaderItems;
		}
		Handshake handshake = new Handshake();
		m_CommandDict.Add(handshake.Name, handshake);
		Text text = new Text();
		m_CommandDict.Add(text.Name, text);
		Binary binary = new Binary();
		m_CommandDict.Add(binary.Name, binary);
		Close close = new Close();
		m_CommandDict.Add(close.Name, close);
		Ping ping = new Ping();
		m_CommandDict.Add(ping.Name, ping);
		Pong pong = new Pong();
		m_CommandDict.Add(pong.Name, pong);
		BadRequest badRequest = new BadRequest();
		m_CommandDict.Add(badRequest.Name, badRequest);
		m_StateCode = -1;
		SubProtocol = subProtocol;
		Items = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		TcpClientSession tcpClientSession;
		if (uri.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
		{
			tcpClientSession = CreateClient(uri);
		}
		else
		{
			if (!uri.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("Invalid uri", "uri");
			}
			tcpClientSession = CreateSecureClient(uri);
		}
		tcpClientSession.Connected += client_Connected;
		tcpClientSession.Closed += client_Closed;
		tcpClientSession.Error += client_Error;
		tcpClientSession.DataReceived += client_DataReceived;
		Client = tcpClientSession;
		EnableAutoSendPing = true;
	}

	private void client_DataReceived(object sender, DataEventArgs e)
	{
		OnDataReceived(e.Data, e.Offset, e.Length);
	}

	private void client_Error(object sender, ErrorEventArgs e)
	{
		OnError(e);
		if (m_StateCode == 0)
		{
			m_StateCode = 2;
			OnClosed();
		}
	}

	private void client_Closed(object sender, EventArgs e)
	{
		OnClosed();
	}

	private void client_Connected(object sender, EventArgs e)
	{
		OnConnected();
	}

	internal bool GetAvailableProcessor(int[] availableVersions)
	{
		IProtocolProcessor preferedProcessorFromAvialable = m_ProtocolProcessorFactory.GetPreferedProcessorFromAvialable(availableVersions);
		if (preferedProcessorFromAvialable == null)
		{
			return false;
		}
		ProtocolProcessor = preferedProcessorFromAvialable;
		return true;
	}

	public void Open()
	{
		m_StateCode = 0;
		if (Proxy != null)
		{
			Client.Proxy = Proxy;
		}
		Client.NoDeplay = NoDelay;
		Client.Connect();
	}

	private static IProtocolProcessor GetProtocolProcessor(WebSocketVersion version)
	{
		IProtocolProcessor processorByVersion = m_ProtocolProcessorFactory.GetProcessorByVersion(version);
		if (processorByVersion == null)
		{
			throw new ArgumentException("Invalid websocket version");
		}
		return processorByVersion;
	}

	private void OnConnected()
	{
		CommandReader = ProtocolProcessor.CreateHandshakeReader(this);
		if (Items.Count > 0)
		{
			Items.Clear();
		}
		ProtocolProcessor.SendHandshake(this);
	}

	protected internal virtual void OnHandshaked()
	{
		m_StateCode = 1;
		Handshaked = true;
		if (m_Opened == null)
		{
			return;
		}
		m_Opened(this, EventArgs.Empty);
		if (EnableAutoSendPing && ProtocolProcessor.SupportPingPong)
		{
			if (AutoSendPingInterval <= 0)
			{
				AutoSendPingInterval = 60;
			}
			m_WebSocketTimer = new Timer(OnPingTimerCallback, ProtocolProcessor, AutoSendPingInterval * 1000, AutoSendPingInterval * 1000);
		}
	}

	private void OnPingTimerCallback(object state)
	{
		if (!string.IsNullOrEmpty(m_LastPingRequest) && !m_LastPingRequest.Equals(LastPongResponse))
		{
			Close();
			return;
		}
		IProtocolProcessor protocolProcessor = state as IProtocolProcessor;
		m_LastPingRequest = DateTime.Now.ToString();
		try
		{
			protocolProcessor.SendPing(this, m_LastPingRequest);
		}
		catch (Exception e)
		{
			OnError(e);
		}
	}

	internal void FireMessageReceived(string message)
	{
		if (m_MessageReceived != null)
		{
			m_MessageReceived(this, new MessageReceivedEventArgs(message));
		}
	}

	internal void FireDataReceived(byte[] data)
	{
		if (m_DataReceived != null)
		{
			m_DataReceived(this, new DataReceivedEventArgs(data));
		}
	}

	private bool EnsureWebSocketOpen()
	{
		if (!Handshaked)
		{
			OnError(new Exception("You must send data by websocket after websocket is opened!"));
			return false;
		}
		return true;
	}

	public void Send(string message)
	{
		if (EnsureWebSocketOpen())
		{
			ProtocolProcessor.SendMessage(this, message);
		}
	}

	public void Send(byte[] data, int offset, int length)
	{
		if (EnsureWebSocketOpen())
		{
			ProtocolProcessor.SendData(this, data, offset, length);
		}
	}

	public void Send(IList<ArraySegment<byte>> segments)
	{
		if (EnsureWebSocketOpen())
		{
			ProtocolProcessor.SendData(this, segments);
		}
	}

	private void OnClosed()
	{
		bool flag = false;
		if (m_StateCode == 2 || m_StateCode == 1)
		{
			flag = true;
		}
		m_StateCode = 3;
		if (flag)
		{
			FireClosed();
		}
	}

	public void Close()
	{
		Close(string.Empty);
	}

	public void Close(string reason)
	{
		Close(ProtocolProcessor.CloseStatusCode.NormalClosure, reason);
	}

	public void Close(int statusCode, string reason)
	{
		if (Interlocked.CompareExchange(ref m_StateCode, 3, -1) == -1)
		{
			OnClosed();
		}
		else if (Interlocked.CompareExchange(ref m_StateCode, 2, 0) == 0)
		{
			TcpClientSession client = Client;
			if (client != null && client.IsConnected)
			{
				client.Close();
			}
			else
			{
				OnClosed();
			}
		}
		else
		{
			m_StateCode = 2;
			ClearTimer();
			m_WebSocketTimer = new Timer(CheckCloseHandshake, null, 5000, -1);
			ProtocolProcessor.SendCloseHandshake(this, statusCode, reason);
		}
	}

	private void CheckCloseHandshake(object state)
	{
		if (m_StateCode == 3)
		{
			return;
		}
		try
		{
			CloseWithoutHandshake();
		}
		catch (Exception e)
		{
			OnError(e);
		}
	}

	internal void CloseWithoutHandshake()
	{
		Client.Close();
	}

	protected void ExecuteCommand(WebSocketCommandInfo commandInfo)
	{
		if (m_CommandDict.TryGetValue(commandInfo.Key, out var value))
		{
			value.ExecuteCommand(this, commandInfo);
		}
	}

	private void OnDataReceived(byte[] data, int offset, int length)
	{
		while (true)
		{
			int left;
			WebSocketCommandInfo commandInfo = CommandReader.GetCommandInfo(data, offset, length, out left);
			if (CommandReader.NextCommandReader != null)
			{
				CommandReader = CommandReader.NextCommandReader;
			}
			if (commandInfo == null)
			{
				break;
			}
			ExecuteCommand(commandInfo);
			if (left <= 0)
			{
				break;
			}
			offset = offset + length - left;
			length = left;
		}
	}

	internal void FireError(Exception error)
	{
		OnError(error);
	}

	private void ClearTimer()
	{
		if (m_WebSocketTimer != null)
		{
			m_WebSocketTimer.Change(-1, -1);
			m_WebSocketTimer.Dispose();
			m_WebSocketTimer = null;
		}
	}

	private void FireClosed()
	{
		ClearTimer();
		m_Closed?.Invoke(this, EventArgs.Empty);
	}

	private void OnError(ErrorEventArgs e)
	{
		m_Error?.Invoke(this, e);
	}

	private void OnError(Exception e)
	{
		OnError(new ErrorEventArgs(e));
	}

	void IDisposable.Dispose()
	{
		TcpClientSession client = Client;
		if (client != null)
		{
			if (client.IsConnected)
			{
				client.Close();
			}
			Client = null;
		}
	}

	public WebSocket(string uri)
		: this(uri, string.Empty)
	{
	}

	public WebSocket(string uri, WebSocketVersion version)
		: this(uri, string.Empty, null, version)
	{
	}

	public WebSocket(string uri, string subProtocol)
		: this(uri, subProtocol, null, WebSocketVersion.None)
	{
	}

	public WebSocket(string uri, List<KeyValuePair<string, string>> cookies)
		: this(uri, string.Empty, cookies, WebSocketVersion.None)
	{
	}

	public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies)
		: this(uri, subProtocol, cookies, WebSocketVersion.None)
	{
	}

	public WebSocket(string uri, string subProtocol, WebSocketVersion version)
		: this(uri, subProtocol, null, version)
	{
	}

	public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, WebSocketVersion version)
		: this(uri, subProtocol, cookies, new List<KeyValuePair<string, string>>(), null, version)
	{
	}

	public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, string userAgent, WebSocketVersion version)
		: this(uri, subProtocol, cookies, null, userAgent, version)
	{
	}

	public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, WebSocketVersion version)
		: this(uri, subProtocol, cookies, customHeaderItems, userAgent, string.Empty, version)
	{
	}

	public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, string origin, WebSocketVersion version)
	{
		Initialize(uri, subProtocol, cookies, customHeaderItems, userAgent, origin, version);
	}
}
