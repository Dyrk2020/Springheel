using System;
using System.Collections.Generic;
using System.Net;

namespace BrainCloud.ModernHttpClient;

public class NativeCookieHandler
{
	private const string wrongVersion = "You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version";

	public List<Cookie> Cookies
	{
		get
		{
			throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
		}
	}

	public void SetCookies(IEnumerable<Cookie> cookies)
	{
		throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
	}

	public void DeleteCookies()
	{
		throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
	}

	public void SetCookie(Cookie cookie)
	{
		throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
	}

	public void DeleteCookie(Cookie cookie)
	{
		throw new Exception("You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version");
	}
}
