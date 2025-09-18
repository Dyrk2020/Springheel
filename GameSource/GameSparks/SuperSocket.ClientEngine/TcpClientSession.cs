using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SuperSocket.ClientEngine;

public abstract class TcpClientSession : ClientSession
{
	private bool m_InConnecting;

	private IBatchQueue<ArraySegment<byte>> m_SendingQueue;

	private PosList<ArraySegment<byte>> m_SendingItems;

	private int m_IsSending;

	protected string HostName { get; private set; }

	public override int ReceiveBufferSize
	{
		get
		{
			return base.ReceiveBufferSize;
		}
		set
		{
			if (base.Buffer.Array != null)
			{
				throw new Exception("ReceiveBufferSize cannot be set after the socket has been connected!");
			}
			base.ReceiveBufferSize = value;
		}
	}

	protected bool IsSending => m_IsSending == 1;

	public TcpClientSession(EndPoint remoteEndPoint)
		: this(remoteEndPoint, 1024)
	{
	}

	public TcpClientSession(EndPoint remoteEndPoint, int receiveBufferSize)
		: base(remoteEndPoint)
	{
		ReceiveBufferSize = receiveBufferSize;
		if (remoteEndPoint is DnsEndPoint2 dnsEndPoint)
		{
			HostName = dnsEndPoint.Host;
		}
		else if (remoteEndPoint is IPEndPoint iPEndPoint)
		{
			HostName = iPEndPoint.Address.ToString();
		}
	}

	protected virtual bool IsIgnorableException(Exception e)
	{
		if (e is ObjectDisposedException)
		{
			return true;
		}
		if (e is NullReferenceException)
		{
			return true;
		}
		return false;
	}

	protected bool IsIgnorableSocketError(int errorCode)
	{
		if (errorCode == 10058 || errorCode == 10053 || errorCode == 10054 || errorCode == 995)
		{
			return true;
		}
		return false;
	}

	protected abstract void SocketEventArgsCompleted(object sender, SocketAsyncEventArgs e);

	public override void Connect()
	{
		if (m_InConnecting)
		{
			throw new Exception("The socket is connecting, cannot connect again!");
		}
		if (base.Client != null)
		{
			throw new Exception("The socket is connected, you neednt' connect again!");
		}
		if (base.Proxy != null)
		{
			base.Proxy.Completed += Proxy_Completed;
			base.Proxy.Connect(base.RemoteEndPoint);
			m_InConnecting = true;
		}
		else
		{
			m_InConnecting = true;
			base.RemoteEndPoint.ConnectAsync(ProcessConnect, null);
		}
	}

	private void Proxy_Completed(object sender, ProxyEventArgs e)
	{
		base.Proxy.Completed -= Proxy_Completed;
		if (e.Connected)
		{
			ProcessConnect(e.Socket, null, null);
			return;
		}
		OnError(new Exception("proxy error", e.Exception));
		m_InConnecting = false;
	}

	protected void ProcessConnect(Socket socket, object state, SocketAsyncEventArgs e)
	{
		if (e != null && e.SocketError != SocketError.Success)
		{
			e.Dispose();
			m_InConnecting = false;
			OnError(new SocketException((int)e.SocketError));
			return;
		}
		if (socket == null)
		{
			m_InConnecting = false;
			OnError(new SocketException(10053));
			return;
		}
		if (!socket.Connected)
		{
			m_InConnecting = false;
			SocketError errorCode = (SocketError)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error);
			OnError(new SocketException((int)errorCode));
			return;
		}
		if (e == null)
		{
			e = new SocketAsyncEventArgs();
		}
		e.Completed += SocketEventArgsCompleted;
		base.Client = socket;
		m_InConnecting = false;
		try
		{
			base.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
		}
		catch (Exception e2)
		{
			OnError(e2);
		}
		OnGetSocket(e);
	}

	protected abstract void OnGetSocket(SocketAsyncEventArgs e);

	protected bool EnsureSocketClosed()
	{
		return EnsureSocketClosed(null);
	}

	protected bool EnsureSocketClosed(Socket prevClient)
	{
		Socket socket = base.Client;
		if (socket == null)
		{
			return false;
		}
		bool result = true;
		if (prevClient != null && prevClient != socket)
		{
			socket = prevClient;
			result = false;
		}
		else
		{
			base.Client = null;
			m_IsSending = 0;
		}
		try
		{
			socket.Shutdown(SocketShutdown.Both);
		}
		catch
		{
		}
		finally
		{
			try
			{
				socket.Close();
			}
			catch
			{
			}
		}
		return result;
	}

	private bool DetectConnected()
	{
		if (base.Client != null)
		{
			return true;
		}
		OnError(new SocketException(10057));
		return false;
	}

	private IBatchQueue<ArraySegment<byte>> GetSendingQueue()
	{
		if (m_SendingQueue != null)
		{
			return m_SendingQueue;
		}
		lock (this)
		{
			if (m_SendingQueue != null)
			{
				return m_SendingQueue;
			}
			m_SendingQueue = new ConcurrentBatchQueue<ArraySegment<byte>>(Math.Max(base.SendingQueueSize, 3), (ArraySegment<byte> t) => t.Array == null);
			return m_SendingQueue;
		}
	}

	private PosList<ArraySegment<byte>> GetSendingItems()
	{
		if (m_SendingItems == null)
		{
			m_SendingItems = new PosList<ArraySegment<byte>>();
		}
		return m_SendingItems;
	}

	public override bool TrySend(ArraySegment<byte> segment)
	{
		if (!DetectConnected())
		{
			return true;
		}
		if (!GetSendingQueue().Enqueue(segment))
		{
			return false;
		}
		if (Interlocked.CompareExchange(ref m_IsSending, 1, 0) != 0)
		{
			return true;
		}
		DequeueSend();
		return true;
	}

	public override bool TrySend(IList<ArraySegment<byte>> segments)
	{
		if (!DetectConnected())
		{
			return true;
		}
		if (!GetSendingQueue().Enqueue(segments))
		{
			return false;
		}
		if (Interlocked.CompareExchange(ref m_IsSending, 1, 0) != 0)
		{
			return true;
		}
		DequeueSend();
		return true;
	}

	private void DequeueSend()
	{
		PosList<ArraySegment<byte>> sendingItems = GetSendingItems();
		if (!m_SendingQueue.TryDequeue(sendingItems))
		{
			m_IsSending = 0;
		}
		else
		{
			SendInternal(sendingItems);
		}
	}

	protected abstract void SendInternal(PosList<ArraySegment<byte>> items);

	protected void OnSendingCompleted()
	{
		PosList<ArraySegment<byte>> sendingItems = GetSendingItems();
		sendingItems.Clear();
		sendingItems.Position = 0;
		if (!m_SendingQueue.TryDequeue(sendingItems))
		{
			m_IsSending = 0;
		}
		else
		{
			SendInternal(sendingItems);
		}
	}

	public override void Close()
	{
		if (EnsureSocketClosed())
		{
			OnClosed();
		}
	}
}
