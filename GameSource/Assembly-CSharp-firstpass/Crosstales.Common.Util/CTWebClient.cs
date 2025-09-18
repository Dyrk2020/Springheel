using System;
using System.Net;

namespace Crosstales.Common.Util;

public class CTWebClient : WebClient
{
	public int Timeout { get; set; }

	public int ConnectionLimit { get; set; }

	public CTWebClient()
		: this(5000)
	{
	}

	public CTWebClient(int timeout, int connectionLimit = 20)
	{
		Timeout = timeout;
		ConnectionLimit = connectionLimit;
	}

	public WebRequest CTGetWebRequest(string uri)
	{
		return GetWebRequest(new Uri(uri));
	}

	protected override WebRequest GetWebRequest(Uri uri)
	{
		WebRequest webRequest = base.GetWebRequest(uri);
		if (webRequest != null && webRequest.GetType() == typeof(HttpWebRequest))
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)base.GetWebRequest(uri);
			if (httpWebRequest != null)
			{
				httpWebRequest.ServicePoint.ConnectionLimit = ConnectionLimit;
				httpWebRequest.Timeout = Timeout;
				return httpWebRequest;
			}
		}
		return webRequest;
	}
}
