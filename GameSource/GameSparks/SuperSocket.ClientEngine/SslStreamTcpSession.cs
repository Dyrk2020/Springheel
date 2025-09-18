using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using GameSparks.Core;
using GameSparks.RT;
using Org.BouncyCastle.Crypto.Tls;

namespace SuperSocket.ClientEngine;

public class SslStreamTcpSession : TcpClientSession
{
	private class SslAsyncState
	{
		public Stream SslStream { get; set; }

		public Socket Client { get; set; }

		public PosList<ArraySegment<byte>> SendingItems { get; set; }
	}

	private Stream m_SslStream;

	public bool AllowUnstrustedCertificate { get; set; }

	public SslStreamTcpSession(EndPoint remoteEndPoint)
		: base(remoteEndPoint)
	{
	}

	public SslStreamTcpSession(EndPoint remoteEndPoint, int receiveBufferSize)
		: base(remoteEndPoint, receiveBufferSize)
	{
	}

	protected override void SocketEventArgsCompleted(object sender, SocketAsyncEventArgs e)
	{
		ProcessConnect(sender as Socket, null, e);
	}

	protected override void OnGetSocket(SocketAsyncEventArgs e)
	{
		try
		{
			m_SslStream = GSTlsClient.WrapStream(new NetworkStream(base.Client), base.HostName);
			OnConnected();
			if (base.Buffer.Array == null)
			{
				base.Buffer = new ArraySegment<byte>(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);
			}
			BeginRead();
		}
		catch (Exception ex)
		{
			if (!IsIgnorableException(ex) || ex is TlsFatalAlert)
			{
				OnError(ex);
			}
		}
	}

	private void OnDataRead(IAsyncResult result)
	{
		if (result.AsyncState is SslAsyncState { SslStream: not null, SslStream: var sslStream } sslAsyncState)
		{
			int num = 0;
			try
			{
				num = sslStream.EndRead(result);
			}
			catch (Exception e)
			{
				if (!IsIgnorableException(e) && m_SslStream != null)
				{
					OnError(e);
				}
				if (EnsureSocketClosed(sslAsyncState.Client))
				{
					OnClosed();
				}
				return;
			}
			if (num == 0)
			{
				if (EnsureSocketClosed(sslAsyncState.Client))
				{
					OnClosed();
				}
			}
			else
			{
				OnDataReceived(base.Buffer.Array, base.Buffer.Offset, num);
				BeginRead();
			}
		}
		else
		{
			OnError(new NullReferenceException("Null state or stream."));
		}
	}

	private void BeginRead()
	{
		Socket client = base.Client;
		if (client == null || m_SslStream == null)
		{
			return;
		}
		try
		{
			m_SslStream.BeginRead(base.Buffer.Array, base.Buffer.Offset, base.Buffer.Count, OnDataRead, new SslAsyncState
			{
				SslStream = m_SslStream,
				Client = client
			});
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
		}
	}

	protected override bool IsIgnorableException(Exception e)
	{
		if (base.IsIgnorableException(e))
		{
			return true;
		}
		if (e is IOException)
		{
			if (e.InnerException is ObjectDisposedException)
			{
				return true;
			}
			if (e.InnerException is IOException && e.InnerException.InnerException is ObjectDisposedException)
			{
				return true;
			}
		}
		return false;
	}

	protected override void SendInternal(PosList<ArraySegment<byte>> items)
	{
		Socket client = base.Client;
		try
		{
			ArraySegment<byte> arraySegment = items[items.Position];
			GameSparksUtil.Log("SendInternal items.length=" + items.Count);
			GameSparksUtil.Log(m_SslStream.ToString());
			m_SslStream.BeginWrite(arraySegment.Array, arraySegment.Offset, arraySegment.Count, OnWriteComplete, new SslAsyncState
			{
				SslStream = m_SslStream,
				Client = client,
				SendingItems = items
			});
			GameSparksUtil.Log("SendInternal, done");
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
		}
	}

	private void OnWriteComplete(IAsyncResult result)
	{
		GameSparksUtil.Log("OnWriteComplete");
		if (result.AsyncState is SslAsyncState { SslStream: not null, SslStream: var sslStream } sslAsyncState)
		{
			try
			{
				sslStream.EndWrite(result);
			}
			catch (Exception e)
			{
				if (!IsIgnorableException(e))
				{
					OnError(e);
				}
				if (EnsureSocketClosed(sslAsyncState.Client))
				{
					OnClosed();
				}
				return;
			}
			PosList<ArraySegment<byte>> sendingItems = sslAsyncState.SendingItems;
			int num = sendingItems.Position + 1;
			if (num < sendingItems.Count)
			{
				sendingItems.Position = num;
				SendInternal(sendingItems);
			}
			else
			{
				OnSendingCompleted();
			}
		}
		else
		{
			OnError(new NullReferenceException("State of Ssl stream is null."));
		}
	}

	public override void Close()
	{
		Stream sslStream = m_SslStream;
		if (sslStream != null)
		{
			sslStream.Close();
			sslStream.Dispose();
			m_SslStream = null;
		}
		base.Close();
	}
}
