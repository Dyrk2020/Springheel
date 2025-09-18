using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Crosstales.Common.Util;

public abstract class BaseConstants
{
	public const string COMMON_VERSION = "2023.3.1";

	public const int COMMON_BUILD = 20231120;

	public static readonly DateTime COMMON_CHANGED = new DateTime(2023, 11, 20);

	public const string ASSET_AUTHOR = "crosstales LLC";

	public const string ASSET_AUTHOR_URL = "https://www.crosstales.com";

	public const string ASSET_CT_URL = "https://assetstore.unity.com/lists/crosstales-42213?aid=1011lNGT";

	public const string ASSET_SOCIAL_DISCORD = "https://discord.gg/ZbZ2sh4";

	public const string ASSET_SOCIAL_FACEBOOK = "https://www.facebook.com/crosstales/";

	public const string ASSET_SOCIAL_TWITTER = "https://twitter.com/crosstales";

	public const string ASSET_SOCIAL_YOUTUBE = "https://www.youtube.com/c/Crosstales";

	public const string ASSET_SOCIAL_LINKEDIN = "https://www.linkedin.com/company/crosstales";

	public const string ASSET_3P_PLAYMAKER = "https://assetstore.unity.com/packages/slug/368?aid=1011lNGT";

	public const string ASSET_3P_VOLUMETRIC_AUDIO = "https://assetstore.unity.com/packages/slug/17125?aid=1011lNGT";

	public const string ASSET_3P_ROCKTOMATE = "https://assetstore.unity.com/packages/slug/156311?aid=1011lNGT";

	public const string ASSET_3P_RTFB = "https://assetstore.unity.com/packages/slug/113006?aid=1011lNGT";

	public const string ASSET_BWF = "https://assetstore.unity.com/packages/slug/26255?aid=1011lNGT";

	public const string ASSET_DJ = "https://assetstore.unity.com/packages/slug/41993?aid=1011lNGT";

	public const string ASSET_FB = "https://assetstore.unity.com/packages/slug/98713?aid=1011lNGT";

	public const string ASSET_OC = "https://assetstore.unity.com/packages/slug/74688?aid=1011lNGT";

	public const string ASSET_RADIO = "https://assetstore.unity.com/packages/slug/32034?aid=1011lNGT";

	public const string ASSET_RTV = "https://assetstore.unity.com/packages/slug/41068?aid=1011lNGT";

	public const string ASSET_TB = "https://assetstore.unity.com/packages/slug/98711?aid=1011lNGT";

	public const string ASSET_TPB = "https://assetstore.unity.com/packages/slug/98714?aid=1011lNGT";

	public const string ASSET_TPS = "https://assetstore.unity.com/packages/slug/60040?aid=1011lNGT";

	public const string ASSET_TR = "https://assetstore.unity.com/packages/slug/61617?aid=1011lNGT";

	public const int FACTOR_KB = 1024;

	public const int FACTOR_MB = 1048576;

	public const int FACTOR_GB = 1073741824;

	public const float FLOAT_32768 = 32768f;

	public const float FLOAT_TOLERANCE = 0.0001f;

	public const string FORMAT_TWO_DECIMAL_PLACES = "0.00";

	public const string FORMAT_NO_DECIMAL_PLACES = "0";

	public const string FORMAT_PERCENT = "0%";

	public const bool DEFAULT_DEBUG = false;

	public const string PATH_DELIMITER_WINDOWS = "\\";

	public const string PATH_DELIMITER_UNIX = "/";

	private static Regex _regexLineEndings;

	private static Regex _regexEmail;

	private static Regex _regexCreditCard;

	private static Regex _regexUrlWeb;

	private static Regex _regexIPAddress;

	private static Regex _regexInvalidChars;

	private static Regex _regexAlpha;

	private static Regex _regexCleanSpace;

	private static Regex _regexCleanTags;

	private static Regex _regexDriveLetters;

	private static Regex _regexFile;

	public const string ALPHABET_LATIN_UPPERCASE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public const string ALPHABET_LATIN_LOWERCASE = "abcdefghijklmnopqrstuvwxyz";

	public const string ALPHABET_EXT_UPPERCASE = "ÀÂÄÆÇÈÉÊËÎÏÔŒÙÛÜ";

	public const string ALPHABET_EXT_LOWERCASE = "àâäæçèéêëîïôœùûü";

	public static readonly string ALPHABET_LATIN = "ABCDEFGHIJKLMNOPQRSTUVWXYZÀÂÄÆÇÈÉÊËÎÏÔŒÙÛÜabcdefghijklmnopqrstuvwxyzàâäæçèéêëîïôœùûü";

	public const string NUMBERS = "0123456789";

	public static readonly string SIGNS = ALPHABET_LATIN + "0123456789";

	public static bool DEV_DEBUG = false;

	public static string TEXT_TOSTRING_START = " {";

	public static string TEXT_TOSTRING_END = "}";

	public static string TEXT_TOSTRING_DELIMITER = "', ";

	public static string TEXT_TOSTRING_DELIMITER_END = "'";

	public const string PREFIX_HTTP = "http://";

	public const string PREFIX_HTTPS = "https://";

	public static int PROCESS_KILL_TIME = 5000;

	public static string CMD_WINDOWS_PATH = "C:\\Windows\\system32\\cmd.exe";

	public static bool SHOW_BWF_BANNER = true;

	public static bool SHOW_DJ_BANNER = true;

	public static bool SHOW_FB_BANNER = true;

	public static bool SHOW_OC_BANNER = true;

	public static bool SHOW_RADIO_BANNER = true;

	public static bool SHOW_RTV_BANNER = true;

	public static bool SHOW_TB_BANNER = true;

	public static bool SHOW_TPB_BANNER = true;

	public static bool SHOW_TPS_BANNER = true;

	public static bool SHOW_TR_BANNER = true;

	public static Regex REGEX_LINEENDINGS => _regexLineEndings ?? (_regexLineEndings = new Regex("[\\u000A\\u000B\\u000C\\u000D\\u2028\\u2029\\u0085]+"));

	public static Regex REGEX_EMAIL => _regexEmail ?? (_regexEmail = new Regex("^(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-zA-Z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,6}))$"));

	public static Regex REGEX_CREDITCARD => _regexCreditCard ?? (_regexCreditCard = new Regex("^((\\d{4}[- ]?){3}\\d{4})$"));

	public static Regex REGEX_URL_WEB => _regexUrlWeb ?? (_regexUrlWeb = new Regex("^(ht|f)tp(s?)\\:\\/\\/[0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*(:(0-9)*)*(\\/?)([a-zA-Z0-9\\-\\.\\?\\,\\'\\/\\\\\\+&amp;%\\$#_]*)?$"));

	public static Regex REGEX_IP_ADDRESS => _regexIPAddress ?? (_regexIPAddress = new Regex("^([0-9]{1,3}\\.){3}[0-9]{1,3}$"));

	public static Regex REGEX_INVALID_CHARS => _regexInvalidChars ?? (_regexInvalidChars = new Regex("[^\\w\\.@-]"));

	public static Regex REGEX_ALPHANUMERIC => _regexAlpha ?? (_regexAlpha = new Regex("([A-Za-z0-9_]+)"));

	public static Regex REGEX_CLEAN_SPACES => _regexCleanSpace ?? (_regexCleanSpace = new Regex("\\s+"));

	public static Regex REGEX_CLEAN_TAGS => _regexCleanTags ?? (_regexCleanTags = new Regex("<.*?>"));

	public static Regex REGEX_DRIVE_LETTERS => _regexDriveLetters ?? (_regexDriveLetters = new Regex("^[a-zA-Z]:"));

	public static Regex REGEX_FILE => _regexFile ?? (_regexFile = new Regex("^.*\\.[\\w]+$"));

	public static string PREFIX_FILE
	{
		get
		{
			if ((BaseHelper.isWindowsBasedPlatform || BaseHelper.isWindowsEditor) && !BaseHelper.isMacOSEditor && !BaseHelper.isLinuxEditor)
			{
				return "file:///";
			}
			return "file://";
		}
	}

	public static string APPLICATION_PATH => FileHelper.ValidatePath(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/') + 1));
}
