using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Crosstales.Common.Model.Enum;
using UnityEngine;

namespace Crosstales.Common.Util;

public abstract class BaseHelper
{
	public static bool ApplicationIsPlaying = Application.isPlaying;

	protected static readonly System.Random _rnd = new System.Random();

	private static string[] _args = null;

	private static int _androidAPILevel = 0;

	public static CultureInfo BaseCulture
	{
		get
		{
			try
			{
				return new CultureInfo(LanguageToISO639(Application.systemLanguage));
			}
			catch
			{
				return CultureInfo.CurrentCulture;
			}
		}
	}

	public static bool isEditorMode
	{
		get
		{
			if (isEditor)
			{
				return !ApplicationIsPlaying;
			}
			return false;
		}
	}

	public static bool isIL2CPP => false;

	public static Platform CurrentPlatform
	{
		get
		{
			if (isWindowsPlatform)
			{
				return Platform.Windows;
			}
			if (isMacOSPlatform)
			{
				return Platform.OSX;
			}
			if (isLinuxPlatform)
			{
				return Platform.Linux;
			}
			if (isAndroidPlatform)
			{
				return Platform.Android;
			}
			if (isIOSBasedPlatform)
			{
				return Platform.IOS;
			}
			if (isWSABasedPlatform)
			{
				return Platform.WSA;
			}
			if (!isWebPlatform)
			{
				return Platform.Unsupported;
			}
			return Platform.Web;
		}
	}

	public static int AndroidAPILevel => _androidAPILevel;

	public static bool isWindowsPlatform => true;

	public static bool isMacOSPlatform => false;

	public static bool isLinuxPlatform => false;

	public static bool isStandalonePlatform
	{
		get
		{
			if (!isWindowsPlatform && !isMacOSPlatform)
			{
				return isLinuxPlatform;
			}
			return true;
		}
	}

	public static bool isAndroidPlatform => false;

	public static bool isIOSPlatform => false;

	public static bool isTvOSPlatform => false;

	public static bool isWSAPlatform => false;

	public static bool isXboxOnePlatform => false;

	public static bool isPS4Platform => false;

	public static bool isWebGLPlatform => false;

	public static bool isWebPlatform => isWebGLPlatform;

	public static bool isWindowsBasedPlatform
	{
		get
		{
			if (!isWindowsPlatform && !isWSAPlatform)
			{
				return isXboxOnePlatform;
			}
			return true;
		}
	}

	public static bool isWSABasedPlatform
	{
		get
		{
			if (!isWSAPlatform)
			{
				return isXboxOnePlatform;
			}
			return true;
		}
	}

	public static bool isAppleBasedPlatform
	{
		get
		{
			if (!isMacOSPlatform && !isIOSPlatform)
			{
				return isTvOSPlatform;
			}
			return true;
		}
	}

	public static bool isIOSBasedPlatform
	{
		get
		{
			if (!isIOSPlatform)
			{
				return isTvOSPlatform;
			}
			return true;
		}
	}

	public static bool isMobilePlatform
	{
		get
		{
			if (!isAndroidPlatform)
			{
				return isIOSBasedPlatform;
			}
			return true;
		}
	}

	public static bool isEditor
	{
		get
		{
			if (!isWindowsEditor && !isMacOSEditor)
			{
				return isLinuxEditor;
			}
			return true;
		}
	}

	public static bool isWindowsEditor => false;

	public static bool isMacOSEditor => false;

	public static bool isLinuxEditor => false;

	[RuntimeInitializeOnLoadMethod]
	private static void initialize()
	{
		ApplicationIsPlaying = Application.isPlaying;
	}

	public static string CreateString(string generateChars, int stringLength)
	{
		if (generateChars != null)
		{
			if (generateChars.Length > 1)
			{
				char[] array = new char[stringLength];
				for (int i = 0; i < stringLength; i++)
				{
					array[i] = generateChars[_rnd.Next(0, generateChars.Length)];
				}
				return new string(array);
			}
			if (generateChars.Length != 1)
			{
				return string.Empty;
			}
			return new string(generateChars[0], stringLength);
		}
		return string.Empty;
	}

	public static List<string> SplitStringToLines(string text, bool ignoreCommentedLines = true, int skipHeaderLines = 0, int skipFooterLines = 0)
	{
		List<string> list = new List<string>(100);
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogWarning("Parameter 'text' is null or empty => 'SplitStringToLines()' will return an empty string list.");
		}
		else
		{
			string[] array = BaseConstants.REGEX_LINEENDINGS.Split(text);
			for (int i = 0; i < array.Length; i++)
			{
				if (i + 1 <= skipHeaderLines || i >= array.Length - skipFooterLines || string.IsNullOrEmpty(array[i]))
				{
					continue;
				}
				if (ignoreCommentedLines)
				{
					if (!array[i].CTStartsWith("#"))
					{
						list.Add(array[i]);
					}
				}
				else
				{
					list.Add(array[i]);
				}
			}
		}
		return list;
	}

	public static string FormatBytesToHRF(long bytes, bool useSI = false)
	{
		int num = 0;
		if (useSI)
		{
			if (-1000 < bytes && bytes < 1000)
			{
				return bytes + " B";
			}
			while (bytes <= -999950 || bytes >= 999950)
			{
				bytes /= 1000;
				num++;
			}
			return string.Format("{0:N2} {1}B", (float)bytes / 1000f, "kMGTPE"[num]);
		}
		long num2 = ((bytes == long.MinValue) ? long.MaxValue : Math.Abs(bytes));
		if (num2 < 1024)
		{
			return bytes + " B";
		}
		long num3 = num2;
		int num4 = 40;
		while (num4 >= 0 && num2 > 1152865209611504844L >> num4)
		{
			num3 >>= 10;
			num++;
			num4 -= 10;
		}
		num3 *= Math.Sign(bytes);
		return string.Format("{0:N2} {1}iB", (float)num3 / 1024f, "kMGTPE"[num]);
	}

	public static string FormatSecondsToHRF(double seconds)
	{
		bool flag = seconds < 0.0;
		int num = Mathf.Abs((int)seconds);
		int value = num % 60;
		if (seconds >= 86400.0)
		{
			int num2 = num / 86400;
			int num3 = (num -= num2 * 86400) / 3600;
			int value2 = (num - num3 * 3600) / 60;
			return string.Format("{0}{1}d {2}:{3}:{4}", flag ? "-" : "", num2, num3, addLeadingZero(value2), addLeadingZero(value));
		}
		if (seconds >= 3600.0)
		{
			int num4 = num / 3600;
			int value3 = (num - num4 * 3600) / 60;
			return string.Format("{0}{1}:{2}:{3}", flag ? "-" : "", num4, addLeadingZero(value3), addLeadingZero(value));
		}
		int num5 = num / 60;
		return string.Format("{0}{1}:{2}", flag ? "-" : "", num5, addLeadingZero(value));
	}

	public static Color HSVToRGB(float h, float s, float v, float a = 1f)
	{
		if (s < 0.0001f)
		{
			return new Color(v, v, v, a);
		}
		float num = h / 60f;
		int num2 = Mathf.FloorToInt(num);
		float num3 = num - (float)num2;
		float num4 = v * (1f - s);
		float num5 = v * (1f - s * num3);
		float num6 = v * (1f - s * (1f - num3));
		return num2 switch
		{
			0 => new Color(v, num6, num4, a), 
			1 => new Color(num5, v, num4, a), 
			2 => new Color(num4, v, num6, a), 
			3 => new Color(num4, num5, v, a), 
			4 => new Color(num6, num4, v, a), 
			_ => new Color(v, num4, num5, a), 
		};
	}

	public static string GenerateLoremIpsum(int length, int minSentences = 1, int maxSentences = int.MaxValue, int minWords = 1, int maxWords = 15)
	{
		string[] array = new string[20]
		{
			"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing", "elit", "sed", "diam",
			"nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
		};
		int num = _rnd.Next(maxSentences - minSentences) + minSentences + 1;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < num; i++)
		{
			if (stringBuilder.Length > length)
			{
				break;
			}
			int num2 = _rnd.Next(maxWords - minWords) + minWords + 1;
			for (int j = 0; j < num2; j++)
			{
				if (stringBuilder.Length > length)
				{
					break;
				}
				if (j > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append((j == 0) ? array[_rnd.Next(array.Length)].CTToTitleCase() : array[_rnd.Next(array.Length)]);
			}
			stringBuilder.Append(". ");
		}
		string text = stringBuilder.ToString();
		if (length > 0 && text.Length > length)
		{
			text = text.Substring(0, length - 1) + ".";
		}
		return text;
	}

	public static string LanguageToISO639(SystemLanguage language)
	{
		switch (language)
		{
		case SystemLanguage.Afrikaans:
			return "af";
		case SystemLanguage.Arabic:
			return "ar";
		case SystemLanguage.Basque:
			return "eu";
		case SystemLanguage.Belarusian:
			return "be";
		case SystemLanguage.Bulgarian:
			return "bg";
		case SystemLanguage.Catalan:
			return "ca";
		case SystemLanguage.Chinese:
		case SystemLanguage.ChineseSimplified:
		case SystemLanguage.ChineseTraditional:
			return "zh";
		case SystemLanguage.Czech:
			return "cs";
		case SystemLanguage.Danish:
			return "da";
		case SystemLanguage.Dutch:
			return "nl";
		case SystemLanguage.English:
			return "en";
		case SystemLanguage.Estonian:
			return "et";
		case SystemLanguage.Faroese:
			return "fo";
		case SystemLanguage.Finnish:
			return "fi";
		case SystemLanguage.French:
			return "fr";
		case SystemLanguage.German:
			return "de";
		case SystemLanguage.Greek:
			return "el";
		case SystemLanguage.Hebrew:
			return "he";
		case SystemLanguage.Hungarian:
			return "hu";
		case SystemLanguage.Icelandic:
			return "is";
		case SystemLanguage.Indonesian:
			return "id";
		case SystemLanguage.Italian:
			return "it";
		case SystemLanguage.Japanese:
			return "ja";
		case SystemLanguage.Korean:
			return "ko";
		case SystemLanguage.Latvian:
			return "lv";
		case SystemLanguage.Lithuanian:
			return "lt";
		case SystemLanguage.Norwegian:
			return "nb";
		case SystemLanguage.Polish:
			return "pl";
		case SystemLanguage.Portuguese:
			return "pt";
		case SystemLanguage.Romanian:
			return "ro";
		case SystemLanguage.Russian:
			return "ru";
		case SystemLanguage.Slovak:
			return "sk";
		case SystemLanguage.Slovenian:
			return "sl";
		case SystemLanguage.Spanish:
			return "es";
		case SystemLanguage.Swedish:
			return "sv";
		case SystemLanguage.Thai:
			return "th";
		case SystemLanguage.Turkish:
			return "tr";
		case SystemLanguage.Ukrainian:
			return "uk";
		case SystemLanguage.Vietnamese:
			return "vi";
		default:
			return "en";
		}
	}

	public static SystemLanguage ISO639ToLanguage(string isoCode)
	{
		if (!string.IsNullOrEmpty(isoCode) && isoCode.Length >= 2)
		{
			switch (isoCode.Substring(0, 2).ToLower())
			{
			case "af":
				return SystemLanguage.Afrikaans;
			case "ar":
				return SystemLanguage.Arabic;
			case "eu":
				return SystemLanguage.Basque;
			case "be":
				return SystemLanguage.Belarusian;
			case "bg":
				return SystemLanguage.Bulgarian;
			case "ca":
				return SystemLanguage.Catalan;
			case "zh":
				return SystemLanguage.Chinese;
			case "cs":
				return SystemLanguage.Czech;
			case "da":
				return SystemLanguage.Danish;
			case "nl":
				return SystemLanguage.Dutch;
			case "en":
				return SystemLanguage.English;
			case "et":
				return SystemLanguage.Estonian;
			case "fo":
				return SystemLanguage.Faroese;
			case "fi":
				return SystemLanguage.Finnish;
			case "fr":
				return SystemLanguage.French;
			case "de":
				return SystemLanguage.German;
			case "el":
				return SystemLanguage.Greek;
			case "he":
				return SystemLanguage.Hebrew;
			case "hu":
				return SystemLanguage.Hungarian;
			case "is":
				return SystemLanguage.Icelandic;
			case "id":
				return SystemLanguage.Indonesian;
			case "it":
				return SystemLanguage.Italian;
			case "ja":
				return SystemLanguage.Japanese;
			case "ko":
				return SystemLanguage.Korean;
			case "lv":
				return SystemLanguage.Latvian;
			case "lt":
				return SystemLanguage.Lithuanian;
			case "no":
			case "nb":
				return SystemLanguage.Norwegian;
			case "pl":
				return SystemLanguage.Polish;
			case "pt":
				return SystemLanguage.Portuguese;
			case "ro":
				return SystemLanguage.Romanian;
			case "ru":
				return SystemLanguage.Russian;
			case "sk":
				return SystemLanguage.Slovak;
			case "sl":
				return SystemLanguage.Slovenian;
			case "es":
				return SystemLanguage.Spanish;
			case "sv":
				return SystemLanguage.Swedish;
			case "th":
				return SystemLanguage.Thai;
			case "tr":
				return SystemLanguage.Turkish;
			case "uk":
				return SystemLanguage.Ukrainian;
			case "vi":
				return SystemLanguage.Vietnamese;
			default:
				return SystemLanguage.English;
			}
		}
		return SystemLanguage.English;
	}

	public static object InvokeMethod(string className, string methodName, BindingFlags flags = BindingFlags.Static | BindingFlags.Public, params object[] parameters)
	{
		if (string.IsNullOrEmpty(className))
		{
			Debug.LogWarning("'className' is null or empty; can not execute.");
			return null;
		}
		if (string.IsNullOrEmpty(methodName))
		{
			Debug.LogWarning("'methodName' is null or empty; can not execute.");
			return null;
		}
		foreach (Type item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly assembly) => assembly.GetTypes()))
		{
			try
			{
				string fullName = item.FullName;
				if (fullName != null && fullName.Equals(className) && item.IsClass)
				{
					MethodInfo method = item.GetMethod(methodName, flags);
					if (method != null)
					{
						return method.Invoke(null, parameters);
					}
				}
			}
			catch (Exception arg)
			{
				Debug.LogWarning($"Could not execute method call '{methodName}' for '{className}': {arg}");
			}
		}
		Debug.LogWarning("Could not find class ' " + className + "' or method '" + methodName + "'");
		return null;
	}

	public static string GetArgument(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			string[] arguments = GetArguments();
			for (int i = 0; i < arguments.Length; i++)
			{
				if (name.CTEquals(arguments[i]) && arguments.Length > i + 1)
				{
					return arguments[i + 1];
				}
			}
		}
		return null;
	}

	public static string[] GetArguments()
	{
		if (_args == null)
		{
			_args = Environment.GetCommandLineArgs();
		}
		return _args;
	}

	public static Dictionary<string, List<string>> ParseJSON(string json)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		if (string.IsNullOrEmpty(json))
		{
			return dictionary;
		}
		string text = json.Trim();
		int num = 0;
		if (text.StartsWith("{"))
		{
			num = 1;
		}
		else
		{
			if (!text.StartsWith("[{"))
			{
				return dictionary;
			}
			num = 2;
		}
		text = text.Substring(num, text.Length - 2 * num);
		string[] array = text.Split(',');
		foreach (string text2 in array)
		{
			if (text2.Contains(":"))
			{
				string[] array2 = text2.Split(':');
				string key = array2[0].Trim(' ', '"').Trim();
				string item = array2[1].Trim(' ', '"').Trim();
				List<string> list = new List<string>();
				if (dictionary.ContainsKey(key))
				{
					list = dictionary[key];
				}
				else
				{
					dictionary.Add(key, list);
				}
				list.Add(item);
			}
		}
		return dictionary;
	}

	private static string addLeadingZero(int value)
	{
		if (value >= 10)
		{
			return value.ToString();
		}
		return "0" + value;
	}
}
