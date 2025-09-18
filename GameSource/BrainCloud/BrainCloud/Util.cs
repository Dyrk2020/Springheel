using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrainCloud;

public class Util
{
	private static readonly DateTime s_unixEpoch;

	protected static Dictionary<SystemLanguage, string> s_langCodes;

	protected static SystemLanguage s_defaultLang;

	protected static string _usersLocale;

	public static DateTime BcTimeToDateTime(long millis)
	{
		return s_unixEpoch.AddMilliseconds(millis);
	}

	public static double DateTimeToBcTimestamp(DateTime dateTime)
	{
		return (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalMilliseconds;
	}

	static Util()
	{
		s_unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		s_langCodes = new Dictionary<SystemLanguage, string>();
		_usersLocale = "";
		s_defaultLang = SystemLanguage.English;
		s_langCodes[SystemLanguage.Afrikaans] = "af";
		s_langCodes[SystemLanguage.Arabic] = "ar";
		s_langCodes[SystemLanguage.Basque] = "eu";
		s_langCodes[SystemLanguage.Belarusian] = "be";
		s_langCodes[SystemLanguage.Bulgarian] = "bg";
		s_langCodes[SystemLanguage.Catalan] = "ca";
		s_langCodes[SystemLanguage.Chinese] = "zh";
		s_langCodes[SystemLanguage.Czech] = "cs";
		s_langCodes[SystemLanguage.Danish] = "da";
		s_langCodes[SystemLanguage.Dutch] = "nl";
		s_langCodes[SystemLanguage.English] = "en";
		s_langCodes[SystemLanguage.Estonian] = "et";
		s_langCodes[SystemLanguage.Faroese] = "fo";
		s_langCodes[SystemLanguage.Finnish] = "fi";
		s_langCodes[SystemLanguage.French] = "fr";
		s_langCodes[SystemLanguage.German] = "de";
		s_langCodes[SystemLanguage.Greek] = "el";
		s_langCodes[SystemLanguage.Hebrew] = "he";
		s_langCodes[SystemLanguage.Icelandic] = "is";
		s_langCodes[SystemLanguage.Indonesian] = "id";
		s_langCodes[SystemLanguage.Italian] = "it";
		s_langCodes[SystemLanguage.Japanese] = "ja";
		s_langCodes[SystemLanguage.Korean] = "ko";
		s_langCodes[SystemLanguage.Latvian] = "lv";
		s_langCodes[SystemLanguage.Lithuanian] = "lt";
		s_langCodes[SystemLanguage.Norwegian] = "no";
		s_langCodes[SystemLanguage.Polish] = "pl";
		s_langCodes[SystemLanguage.Portuguese] = "pt";
		s_langCodes[SystemLanguage.Romanian] = "ro";
		s_langCodes[SystemLanguage.Russian] = "ru";
		s_langCodes[SystemLanguage.SerboCroatian] = "hr";
		s_langCodes[SystemLanguage.Slovak] = "sk";
		s_langCodes[SystemLanguage.Slovenian] = "sl";
		s_langCodes[SystemLanguage.Spanish] = "es";
		s_langCodes[SystemLanguage.Swedish] = "sv";
		s_langCodes[SystemLanguage.Thai] = "th";
		s_langCodes[SystemLanguage.Turkish] = "tr";
		s_langCodes[SystemLanguage.Ukrainian] = "uk";
		s_langCodes[SystemLanguage.Vietnamese] = "vi";
		s_langCodes[SystemLanguage.Hungarian] = "hu";
	}

	public static string GetIsoCodeForLanguage(SystemLanguage lang)
	{
		if (!s_langCodes.TryGetValue(lang, out var value))
		{
			return "en";
		}
		return value;
	}

	public static SystemLanguage GetLanguageForIsoCode(string isoCode)
	{
		foreach (SystemLanguage key in s_langCodes.Keys)
		{
			if (s_langCodes[key].Equals(isoCode))
			{
				return key;
			}
		}
		return s_defaultLang;
	}

	public static string GetIsoCodeForCurrentLanguage()
	{
		return GetIsoCodeForLanguage(Application.systemLanguage);
	}

	public static double GetUTCOffsetForCurrentTimeZone()
	{
		double result = 0.0;
		try
		{
			DateTime time = default(DateTime);
			TimeZone currentTimeZone = TimeZone.CurrentTimeZone;
			DateTime time2 = currentTimeZone.ToLocalTime(time);
			result = currentTimeZone.GetUtcOffset(time2).TotalHours;
		}
		catch (Exception)
		{
		}
		return result;
	}

	public static void SetCurrentCountryCode(string isoCode)
	{
		_usersLocale = isoCode;
	}

	public static string GetCurrentCountryCode()
	{
		return _usersLocale;
	}

	public static bool IsOptionalParameterValid(string s)
	{
		if (s != null)
		{
			return s.Length > 0;
		}
		return false;
	}

	public static long DateTimeToUnixTimestamp(DateTime dateTime)
	{
		return (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
	}
}
