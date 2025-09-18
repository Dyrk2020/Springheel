using System;
using System.Collections.Generic;
using System.Net;
using GameSparks.Core;
using SuperSocket.ClientEngine;
using SuperSocket.ClientEngine.Proxy;
using WebSocket4Net;

namespace GameSparks;

public class GameSparksWebSocket : IGameSparksWebSocket
{
	private Action<string> onMessage;

	private Action<byte[]> onBinaryMessage;

	private Action onClose;

	private Action onOpen;

	private Action<string> onError;

	protected WebSocket ws;

	public static EndPoint Proxy { get; set; }

	public GameSparksWebSocketState State
	{
		get
		{
			try
			{
				switch (ws.State)
				{
				case WebSocketState.Closed:
					return GameSparksWebSocketState.Closed;
				case WebSocketState.Closing:
					return GameSparksWebSocketState.Closing;
				case WebSocketState.Connecting:
					return GameSparksWebSocketState.Connecting;
				case WebSocketState.Open:
					return GameSparksWebSocketState.Open;
				}
			}
			catch
			{
			}
			return GameSparksWebSocketState.None;
		}
	}

	private void Initialize(string url, Action onClose, Action onOpen, Action<string> onError)
	{
		this.onOpen = onOpen;
		this.onError = onError;
		this.onClose = onClose;
		ws = new WebSocket(url);
		if (Proxy != null)
		{
			ws.Proxy = new HttpConnectProxy(Proxy);
		}
		ws.NoDelay = true;
		ws.Opened += webSocketClient_Opened;
		ws.Closed += webSocketClient_Closed;
		ws.Error += webSocketClient_Error;
		ws.EnableAutoSendPing = true;
		ws.AutoSendPingInterval = 30;
	}

	public void Initialize(string url, Action<string> onMessage, Action onClose, Action onOpen, Action<string> onError)
	{
		Initialize(url, onClose, onOpen, onError);
		this.onMessage = onMessage;
		ws.MessageReceived += webSocketClient_MessageReceived;
	}

	public void Initialize(string url, Action<byte[]> onBinaryMessage, Action onClose, Action onOpen, Action<string> onError)
	{
		Initialize(url, onClose, onOpen, onError);
		this.onBinaryMessage = onBinaryMessage;
		ws.DataReceived += webSocketClient_BinaryMessageReceived;
	}

	public void Open()
	{
		GameSparksUtil.Log("Opening Websocket");
		try
		{
			ws.Open();
		}
		catch (Exception e)
		{
			GameSparksUtil.LogException(e);
		}
	}

	public void Close()
	{
		Terminate();
		GameSparksUtil.Log("Closing Websocket");
		try
		{
			ws.Close();
		}
		catch (Exception e)
		{
			GameSparksUtil.LogException(e);
		}
	}

	public void Terminate()
	{
		GameSparksUtil.Log("Closing Websocket");
		try
		{
			ws.Opened -= webSocketClient_Opened;
			ws.Closed -= webSocketClient_Closed;
			ws.Error -= webSocketClient_Error;
			ws.MessageReceived -= webSocketClient_MessageReceived;
			ws.DataReceived -= webSocketClient_BinaryMessageReceived;
			ws.CloseWithoutHandshake();
			if (onClose != null)
			{
				onClose();
			}
		}
		catch (Exception e)
		{
			GameSparksUtil.LogException(e);
		}
	}

	public void Send(string request)
	{
		try
		{
			ws.Send(request);
		}
		catch (Exception e)
		{
			GameSparksUtil.LogException(e);
		}
	}

	public void SendBinary(byte[] request, int offset, int length)
	{
		try
		{
			List<ArraySegment<byte>> list = new List<ArraySegment<byte>>();
			list.Add(new ArraySegment<byte>(request, offset, length));
			ws.Send(list);
		}
		catch (Exception e)
		{
			GameSparksUtil.LogException(e);
		}
	}

	private void webSocketClient_Error(object sender, ErrorEventArgs e)
	{
		if (onError != null)
		{
			onError(e.Exception.Message);
		}
	}

	private void webSocketClient_Opened(object sender, EventArgs e)
	{
		if (onOpen != null)
		{
			onOpen();
		}
	}

	private void webSocketClient_Closed(object sender, EventArgs e)
	{
		if (onClose != null)
		{
			onClose();
		}
	}

	private void webSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
	{
		if (onMessage != null)
		{
			onMessage(e.Message);
		}
	}

	private void webSocketClient_BinaryMessageReceived(object sender, DataReceivedEventArgs e)
	{
		if (onBinaryMessage != null)
		{
			onBinaryMessage(e.Data);
		}
	}
}
