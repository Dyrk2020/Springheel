using System;
using BrainCloud.UnityWebSocketsForWebGL.WebSocketSharp;

namespace BrainCloud.Internal;

public class BrainCloudWebSocket
{
	public delegate void OnOpenHandler(BrainCloudWebSocket accepted);

	public delegate void OnMessageHandler(BrainCloudWebSocket sender, byte[] data);

	public delegate void OnErrorHandler(BrainCloudWebSocket sender, string message);

	public delegate void OnCloseHandler(BrainCloudWebSocket sender, int code, string reason);

	private WebSocket WebSocket;

	public event OnOpenHandler OnOpen;

	public event OnMessageHandler OnMessage;

	public event OnErrorHandler OnError;

	public event OnCloseHandler OnClose;

	public BrainCloudWebSocket(string url)
	{
		WebSocket = new WebSocket(url);
		WebSocket.ConnectAsync();
		WebSocket.OnOpen += WebSocket_OnOpen;
		WebSocket.OnMessage += WebSocket_OnMessage;
		WebSocket.OnError += WebSocket_OnError;
		WebSocket.OnClose += WebSocket_OnClose;
	}

	public void Close()
	{
		if (WebSocket != null)
		{
			WebSocket.CloseAsync();
			WebSocket.OnOpen -= WebSocket_OnOpen;
			WebSocket.OnMessage -= WebSocket_OnMessage;
			WebSocket.OnError -= WebSocket_OnError;
			WebSocket.OnClose -= WebSocket_OnClose;
			WebSocket = null;
		}
	}

	private void WebSocket_OnOpen(object sender, EventArgs e)
	{
		WebSocket.TCPClient.NoDelay = true;
		WebSocket.TCPClient.Client.NoDelay = true;
		if (this.OnOpen != null)
		{
			this.OnOpen(this);
		}
	}

	private void WebSocket_OnMessage(object sender, MessageEventArgs e)
	{
		if (this.OnMessage != null)
		{
			this.OnMessage(this, e.RawData);
		}
	}

	private void WebSocket_OnError(object sender, ErrorEventArgs e)
	{
		if (this.OnError != null)
		{
			this.OnError(this, e.Message);
		}
	}

	private void WebSocket_OnClose(object sender, CloseEventArgs e)
	{
		if (this.OnClose != null)
		{
			this.OnClose(this, e.Code, e.Reason);
		}
	}

	public void SendAsync(byte[] packet)
	{
		WebSocket.SendAsync(packet, null);
	}

	public void Send(byte[] packet)
	{
		WebSocket.Send(packet);
	}
}
