using System;
using System.Net;
using System.Net.Sockets;
using GameSparks.Core;

namespace SuperSocket.ClientEngine;

public static class ConnectAsyncExtension
{
	private class ConnectToken
	{
		public object State { get; set; }

		public ConnectedCallback Callback { get; set; }
	}

	private class DnsConnectState
	{
		public IPAddress[] Addresses { get; set; }

		public int NextAddressIndex { get; set; }

		public int Port { get; set; }

		public Socket Socket4 { get; set; }

		public Socket Socket6 { get; set; }

		public object State { get; set; }

		public ConnectedCallback Callback { get; set; }
	}

	private static void SocketAsyncEventCompleted(object sender, SocketAsyncEventArgs e)
	{
		e.Completed -= SocketAsyncEventCompleted;
		ConnectToken connectToken = (ConnectToken)e.UserToken;
		e.UserToken = null;
		connectToken.Callback(sender as Socket, connectToken.State, e);
	}

	private static SocketAsyncEventArgs CreateSocketAsyncEventArgs(EndPoint remoteEndPoint, ConnectedCallback callback, object state)
	{
		SocketAsyncEventArgs e = new SocketAsyncEventArgs();
		e.UserToken = new ConnectToken
		{
			State = state,
			Callback = callback
		};
		e.RemoteEndPoint = remoteEndPoint;
		e.Completed += SocketAsyncEventCompleted;
		return e;
	}

	private static void ConnectAsyncInternal(this EndPoint remoteEndPoint, ConnectedCallback callback, object state)
	{
		if (remoteEndPoint is DnsEndPoint2)
		{
			DnsEndPoint2 dnsEndPoint = (DnsEndPoint2)remoteEndPoint;
			IAsyncResult asyncResult = Dns.BeginGetHostAddresses(dnsEndPoint.Host, OnGetHostAddresses, new DnsConnectState
			{
				Port = dnsEndPoint.Port,
				Callback = callback,
				State = state
			});
			if (asyncResult.CompletedSynchronously)
			{
				OnGetHostAddresses(asyncResult);
			}
		}
		else
		{
			SocketAsyncEventArgs e = CreateSocketAsyncEventArgs(remoteEndPoint, callback, state);
			Socket socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.ConnectAsync(e);
		}
	}

	private static IPAddress GetNextAddress(DnsConnectState state, out Socket attempSocket)
	{
		IPAddress iPAddress = null;
		attempSocket = null;
		int nextAddressIndex = state.NextAddressIndex;
		while (attempSocket == null && nextAddressIndex < state.Addresses.Length)
		{
			iPAddress = state.Addresses[nextAddressIndex++];
			if (iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
			{
				attempSocket = state.Socket6;
			}
		}
		if (attempSocket == null)
		{
			nextAddressIndex = state.NextAddressIndex;
			while (attempSocket == null)
			{
				if (nextAddressIndex >= state.Addresses.Length)
				{
					return null;
				}
				iPAddress = state.Addresses[nextAddressIndex++];
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					attempSocket = state.Socket4;
				}
			}
		}
		state.NextAddressIndex = nextAddressIndex;
		return iPAddress;
	}

	private static void OnGetHostAddresses(IAsyncResult result)
	{
		DnsConnectState dnsConnectState = result.AsyncState as DnsConnectState;
		IPAddress[] array;
		try
		{
			array = Dns.EndGetHostAddresses(result);
		}
		catch
		{
			dnsConnectState.Callback(null, dnsConnectState.State, null);
			return;
		}
		if (array == null || array.Length <= 0)
		{
			dnsConnectState.Callback(null, dnsConnectState.State, null);
			return;
		}
		dnsConnectState.Addresses = array;
		CreateAttempSocket(dnsConnectState);
		Socket attempSocket;
		IPAddress nextAddress = GetNextAddress(dnsConnectState, out attempSocket);
		if (nextAddress == null)
		{
			dnsConnectState.Callback(null, dnsConnectState.State, null);
			return;
		}
		SocketAsyncEventArgs e = new SocketAsyncEventArgs();
		e.Completed += SocketConnectCompleted;
		IPEndPoint remoteEndPoint = new IPEndPoint(nextAddress, dnsConnectState.Port);
		e.RemoteEndPoint = remoteEndPoint;
		e.UserToken = dnsConnectState;
		if (!attempSocket.ConnectAsync(e))
		{
			SocketConnectCompleted(attempSocket, e);
		}
	}

	private static void SocketConnectCompleted(object sender, SocketAsyncEventArgs e)
	{
		DnsConnectState dnsConnectState = e.UserToken as DnsConnectState;
		if (e.SocketError == SocketError.Success)
		{
			ClearSocketAsyncEventArgs(e);
			dnsConnectState.Callback((Socket)sender, dnsConnectState.State, e);
			return;
		}
		if (e.SocketError != SocketError.HostUnreachable && e.SocketError != SocketError.ConnectionRefused)
		{
			ClearSocketAsyncEventArgs(e);
			dnsConnectState.Callback(null, dnsConnectState.State, e);
			return;
		}
		Socket attempSocket;
		IPAddress nextAddress = GetNextAddress(dnsConnectState, out attempSocket);
		if (nextAddress == null)
		{
			ClearSocketAsyncEventArgs(e);
			e.SocketError = SocketError.HostUnreachable;
			dnsConnectState.Callback(null, dnsConnectState.State, e);
			return;
		}
		e.RemoteEndPoint = new IPEndPoint(nextAddress, dnsConnectState.Port);
		if (!attempSocket.ConnectAsync(e))
		{
			SocketConnectCompleted(attempSocket, e);
		}
	}

	private static void ClearSocketAsyncEventArgs(SocketAsyncEventArgs e)
	{
		e.Completed -= SocketConnectCompleted;
		e.UserToken = null;
	}

	public static void ConnectAsync(this EndPoint remoteEndPoint, ConnectedCallback callback, object state)
	{
		remoteEndPoint.ConnectAsyncInternal(callback, state);
	}

	private static void CreateAttempSocket(DnsConnectState connectState)
	{
		bool flag = false;
		try
		{
			if (Socket.OSSupportsIPv6)
			{
				flag = true;
			}
		}
		catch (Exception ex)
		{
			GameSparksUtil.LogError("Socket.OSSupportsIPv6: " + ex.ToString());
		}
		try
		{
			if (Socket.SupportsIPv6)
			{
				flag = true;
			}
		}
		catch (Exception ex2)
		{
			GameSparksUtil.LogError("Socket.SupportsIPv6: " + ex2.ToString());
		}
		if (flag)
		{
			try
			{
				connectState.Socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
				GameSparksUtil.Log("IPv6 on!");
			}
			catch (Exception ex3)
			{
				GameSparksUtil.LogError(ex3.ToString());
			}
		}
		connectState.Socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	}
}
