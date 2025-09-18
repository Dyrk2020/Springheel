using System;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.ClientEngine;

public class AsyncTcpSession : TcpClientSession
{
	private SocketAsyncEventArgs m_SocketEventArgs;

	private SocketAsyncEventArgs m_SocketEventArgsSend;

	public AsyncTcpSession(EndPoint remoteEndPoint)
		: base(remoteEndPoint)
	{
	}

	public AsyncTcpSession(EndPoint remoteEndPoint, int receiveBufferSize)
		: base(remoteEndPoint, receiveBufferSize)
	{
	}

	protected override void SocketEventArgsCompleted(object sender, SocketAsyncEventArgs e)
	{
		if (e.LastOperation == SocketAsyncOperation.Connect)
		{
			ProcessConnect(sender as Socket, null, e);
		}
		else
		{
			ProcessReceive(e);
		}
	}

	protected override void SetBuffer(ArraySegment<byte> bufferSegment)
	{
		base.SetBuffer(bufferSegment);
		if (m_SocketEventArgs != null)
		{
			m_SocketEventArgs.SetBuffer(bufferSegment.Array, bufferSegment.Offset, bufferSegment.Count);
		}
	}

	protected override void OnGetSocket(SocketAsyncEventArgs e)
	{
		if (base.Buffer.Array == null)
		{
			base.Buffer = new ArraySegment<byte>(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);
		}
		e.SetBuffer(base.Buffer.Array, base.Buffer.Offset, base.Buffer.Count);
		m_SocketEventArgs = e;
		OnConnected();
		StartReceive();
	}

	private void BeginReceive()
	{
		if (!base.Client.ReceiveAsync(m_SocketEventArgs))
		{
			ProcessReceive(m_SocketEventArgs);
		}
	}

	private void ProcessReceive(SocketAsyncEventArgs e)
	{
		if (e.SocketError != SocketError.Success)
		{
			if (EnsureSocketClosed())
			{
				OnClosed();
			}
			if (!IsIgnorableSocketError((int)e.SocketError))
			{
				OnError(new SocketException((int)e.SocketError));
			}
		}
		else if (e.BytesTransferred == 0)
		{
			if (EnsureSocketClosed())
			{
				OnClosed();
			}
		}
		else
		{
			OnDataReceived(e.Buffer, e.Offset, e.BytesTransferred);
			StartReceive();
		}
	}

	private void StartReceive()
	{
		Socket client = base.Client;
		if (client == null)
		{
			return;
		}
		bool flag;
		try
		{
			flag = client.ReceiveAsync(m_SocketEventArgs);
		}
		catch (SocketException ex)
		{
			if (!IsIgnorableSocketError(ex.ErrorCode))
			{
				OnError(ex);
			}
			if (EnsureSocketClosed(client))
			{
				OnClosed();
			}
			return;
		}
		catch (Exception e)
		{
			if (!IsIgnorableException(e))
			{
				OnError(e);
			}
			if (EnsureSocketClosed(client))
			{
				OnClosed();
			}
			return;
		}
		if (!flag)
		{
			ProcessReceive(m_SocketEventArgs);
		}
	}

	protected override void SendInternal(PosList<ArraySegment<byte>> items)
	{
		if (m_SocketEventArgsSend == null)
		{
			m_SocketEventArgsSend = new SocketAsyncEventArgs();
			m_SocketEventArgsSend.Completed += Sending_Completed;
		}
		bool flag;
		try
		{
			if (items.Count > 1)
			{
				if (m_SocketEventArgsSend.Buffer != null)
				{
					m_SocketEventArgsSend.SetBuffer(null, 0, 0);
				}
				m_SocketEventArgsSend.BufferList = items;
			}
			else
			{
				ArraySegment<byte> arraySegment = items[0];
				try
				{
					if (m_SocketEventArgsSend.BufferList != null)
					{
						m_SocketEventArgsSend.BufferList = null;
					}
				}
				catch
				{
				}
				m_SocketEventArgsSend.SetBuffer(arraySegment.Array, 0, arraySegment.Count);
			}
			flag = base.Client.SendAsync(m_SocketEventArgsSend);
		}
		catch (SocketException ex)
		{
			if (EnsureSocketClosed() && !IsIgnorableSocketError(ex.ErrorCode))
			{
				OnError(ex);
			}
			return;
		}
		catch (Exception e)
		{
			if (EnsureSocketClosed() && IsIgnorableException(e))
			{
				OnError(e);
			}
			return;
		}
		if (!flag)
		{
			Sending_Completed(base.Client, m_SocketEventArgsSend);
		}
	}

	private void Sending_Completed(object sender, SocketAsyncEventArgs e)
	{
		if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
		{
			if (EnsureSocketClosed())
			{
				OnClosed();
			}
			if (e.SocketError != SocketError.Success && !IsIgnorableSocketError((int)e.SocketError))
			{
				OnError(new SocketException((int)e.SocketError));
			}
		}
		else
		{
			OnSendingCompleted();
		}
	}

	protected override void OnClosed()
	{
		if (m_SocketEventArgsSend != null)
		{
			m_SocketEventArgsSend.Dispose();
			m_SocketEventArgsSend = null;
		}
		if (m_SocketEventArgs != null)
		{
			m_SocketEventArgs.Dispose();
			m_SocketEventArgs = null;
		}
		base.OnClosed();
	}
}
