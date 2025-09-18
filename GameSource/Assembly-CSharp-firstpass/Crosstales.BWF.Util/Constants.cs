using System;
using Crosstales.Common.Util;

namespace Crosstales.BWF.Util;

public abstract class Constants : BaseConstants
{
	public const string ASSET_NAME = "Bad Word Filter PRO";

	public const string ASSET_NAME_SHORT = "BWF PRO";

	public const string ASSET_VERSION = "2023.2.4";

	public const int ASSET_BUILD = 20231120;

	public static readonly DateTime ASSET_CREATED = new DateTime(2015, 1, 3);

	public static readonly DateTime ASSET_CHANGED = new DateTime(2023, 11, 20);

	public const string ASSET_PRO_URL = "https://assetstore.unity.com/packages/slug/26255?aid=1011lNGT";

	public const string ASSET_UPDATE_CHECK_URL = "https://www.crosstales.com/media/assets/bwf_versions.txt";

	public const string ASSET_CONTACT = "bwf@crosstales.com";

	public const string ASSET_MANUAL_URL = "https://www.crosstales.com/media/data/assets/badwordfilter/BadWordFilter-doc.pdf";

	public const string ASSET_API_URL = "https://www.crosstales.com/en/assets/badwordfilter/api";

	public const string ASSET_FORUM_URL = "https://forum.unity.com/threads/bad-word-filter-pro-solution-against-profanity-and-obscenity.289960/";

	public const string ASSET_WEB_URL = "https://www.crosstales.com/en/portfolio//badwordfilter/";

	public const string ASSET_VIDEO_PROMO = "https://youtu.be/pXICeRKaRPM?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S";

	public const string ASSET_VIDEO_TUTORIAL = "https://youtu.be/W8FxFlIObWM?list=PLgtonIOr6Tb41XTMeeZ836tjHlKgOO84S";

	public const string KEY_PREFIX = "BWF_CFG_";

	public const string KEY_DEBUG = "BWF_CFG_DEBUG";

	public const string KEY_DEBUG_BADWORDS = "BWF_CFG_DEBUG_BADWORDS";

	public const string KEY_DEBUG_DOMAINS = "BWF_CFG_DEBUG_DOMAINS";

	public const bool DEFAULT_DEBUG_BADWORDS = false;

	public const bool DEFAULT_DEBUG_DOMAINS = false;

	public const string MANAGER_SCENE_OBJECT_NAME = "BWF";

	public static int WWW_TIMEOUT = 5;
}
