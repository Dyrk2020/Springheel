using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsWrapper
{
	public const int MAXPARAMS = 10;

	public static bool EnabledOnPlatform => true;

	public static void CustomEvent(string key, Dictionary<string, object> data)
	{
		if (!key.NullOrEmpty() && data != null)
		{
			if (data.Count > 10)
			{
				Debug.LogWarning("There are more than " + 10 + " parameters in the analytics event: " + key);
			}
			Analytics.CustomEvent(key, data);
		}
	}
}
