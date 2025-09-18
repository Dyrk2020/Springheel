using System;
using System.Net;
using System.Net.Http;

namespace BrainCloud.ModernHttpClient;

public class NativeMessageHandler : HttpClientHandler
{
	private const string wrongVersion = "You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version";

	public bool DisableCaching { get; set; }

	public TimeSpan? Timeout
	{
		get
		{
			throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
		}
		set
		{
			throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
		}
	}

	public NativeMessageHandler()
	{
	}

	public NativeMessageHandler(bool throwOnCaptiveNetwork, TLSConfig tLSConfig, NativeCookieHandler cookieHandler = null, IWebProxy proxy = null)
	{
	}

	public void RegisterForProgress(HttpRequestMessage request, ProgressDelegate callback)
	{
		throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
	}
}
