using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;

namespace GameSparks.RT;

internal class GSTlsClient : DefaultTlsClient
{
	private string hostName;

	public static Action<string> logger;

	internal static Stream WrapStream(Stream stream, string hostName)
	{
		if (logger != null)
		{
			logger("Wrapping");
		}
		TlsClientProtocol tlsClientProtocol = new TlsClientProtocol(stream, new SecureRandom());
		tlsClientProtocol.Connect(new GSTlsClient(hostName));
		return new DuplexTlsStream(tlsClientProtocol.Stream);
	}

	private GSTlsClient(string hostName)
	{
		this.hostName = hostName;
	}

	public override TlsAuthentication GetAuthentication()
	{
		return new GSTlsAuthentication(hostName);
	}

	public override int[] GetCipherSuites()
	{
		return new int[9] { 49170, 49191, 60, 49171, 47, 49160, 49170, 49155, 49165 };
	}

	public override byte[] GetCompressionMethods()
	{
		return new byte[1];
	}

	public override IDictionary GetClientExtensions()
	{
		IList list = new ArrayList();
		list.Add(new ServerName(0, hostName));
		ServerNameList serverNameList = new ServerNameList(list);
		IDictionary clientExtensions = base.GetClientExtensions();
		TlsExtensionsUtilities.AddServerNameExtension(clientExtensions, serverNameList);
		return clientExtensions;
	}
}
