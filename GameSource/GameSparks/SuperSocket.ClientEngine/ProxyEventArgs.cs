using System;
using System.Net.Sockets;

namespace SuperSocket.ClientEngine;

public class ProxyEventArgs : EventArgs
{
	public bool Connected { get; private set; }

	public Socket Socket { get; private set; }

	public Exception Exception { get; private set; }

	public ProxyEventArgs(Socket socket)
		: this(connected: true, socket, null)
	{
	}

	public ProxyEventArgs(Exception exception)
		: this(connected: false, null, exception)
	{
	}

	public ProxyEventArgs(bool connected, Socket socket, Exception exception)
	{
		Connected = connected;
		Socket = socket;
		Exception = exception;
	}
}
