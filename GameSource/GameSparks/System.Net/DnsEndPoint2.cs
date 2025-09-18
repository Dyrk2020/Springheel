namespace System.Net;

public class DnsEndPoint2 : EndPoint
{
	public string Host { get; private set; }

	public int Port { get; private set; }

	public DnsEndPoint2(string host, int port)
	{
		Host = host;
		Port = port;
	}
}
