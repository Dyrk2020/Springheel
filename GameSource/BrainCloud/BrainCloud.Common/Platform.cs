using System.Collections.Generic;

namespace BrainCloud.Common;

public sealed class Platform
{
	private readonly string value;

	public static readonly Platform AppleTVOS = new Platform("APPLE_TV_OS");

	public static readonly Platform BlackBerry = new Platform("BB");

	public static readonly Platform Facebook = new Platform("FB");

	public static readonly Platform Oculus = new Platform("Oculus");

	public static readonly Platform GooglePlayAndroid = new Platform("ANG");

	public static readonly Platform iOS = new Platform("IOS");

	public static readonly Platform Linux = new Platform("LINUX");

	public static readonly Platform Mac = new Platform("MAC");

	public static readonly Platform PS3 = new Platform("PS3");

	public static readonly Platform PS4 = new Platform("PS4");

	public static readonly Platform PSVita = new Platform("PS_VITA");

	public static readonly Platform Roku = new Platform("ROKU");

	public static readonly Platform Tizen = new Platform("TIZEN");

	public static readonly Platform Unknown = new Platform("UNKNOWN");

	public static readonly Platform WatchOS = new Platform("WATCH_OS");

	public static readonly Platform Web = new Platform("WEB");

	public static readonly Platform Wii = new Platform("WII");

	public static readonly Platform WindowsPhone = new Platform("WINP");

	public static readonly Platform Windows = new Platform("WINDOWS");

	public static readonly Platform Xbox360 = new Platform("XBOX_360");

	public static readonly Platform XboxOne = new Platform("XBOX_ONE");

	public static readonly Platform Amazon = new Platform("AMAZON");

	public static readonly Platform Nintendo = new Platform("NINTENDO");

	private static readonly Dictionary<string, Platform> _platformsForString = new Dictionary<string, Platform>
	{
		{ AppleTVOS.value, AppleTVOS },
		{ Amazon.value, Amazon },
		{ BlackBerry.value, BlackBerry },
		{ Facebook.value, Facebook },
		{ Oculus.value, Oculus },
		{ GooglePlayAndroid.value, GooglePlayAndroid },
		{ iOS.value, iOS },
		{ Linux.value, Linux },
		{ Mac.value, Mac },
		{ PS3.value, PS3 },
		{ PS4.value, PS4 },
		{ PSVita.value, PSVita },
		{ Roku.value, Roku },
		{ Tizen.value, Tizen },
		{ Unknown.value, Unknown },
		{ WatchOS.value, WatchOS },
		{ Web.value, Web },
		{ Wii.value, Wii },
		{ WindowsPhone.value, WindowsPhone },
		{ Windows.value, Windows },
		{ Xbox360.value, Xbox360 },
		{ XboxOne.value, XboxOne },
		{ Nintendo.value, Nintendo }
	};

	private Platform(string value)
	{
		this.value = value;
	}

	public override string ToString()
	{
		return value;
	}

	public static Platform FromString(string s)
	{
		if (!_platformsForString.TryGetValue(s, out var result))
		{
			return Unknown;
		}
		return result;
	}

	public static Platform FromUnityRuntime()
	{
		return Windows;
	}
}
