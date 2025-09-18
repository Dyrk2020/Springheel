using System;
using System.Net;

namespace BrainCloud.ModernHttpClient;

public class CaptiveNetworkException : WebException
{
	private const string DefaultCaptiveNetworkErrorMessage = "Hostnames don't match, you are probably on a captive network";

	public Uri SourceUri { get; private set; }

	public Uri DestinationUri { get; private set; }

	public CaptiveNetworkException(Uri sourceUri, Uri destinationUri)
		: base("Hostnames don't match, you are probably on a captive network")
	{
		SourceUri = sourceUri;
		DestinationUri = destinationUri;
	}
}
