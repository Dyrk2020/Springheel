using System;
using System.Net;

namespace SuperSocket.ClientEngine;

public interface IProxyConnector
{
	event EventHandler<ProxyEventArgs> Completed;

	void Connect(EndPoint remoteEndPoint);
}
