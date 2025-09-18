using UnityEngine;

namespace nn;

public static class Pctl
{
	private static float lastCheck = 0f;

	private static float checkFrequency = 10f;

	private static bool lastResult = false;

	private static bool internal_CheckFreeCommunicationPermission(bool shouldShowUi)
	{
		return true;
	}

	public static bool ReCheckFreeCommunicationPermission(bool shouldShowUI)
	{
		float unscaledTime = Time.unscaledTime;
		lastResult = internal_CheckFreeCommunicationPermission(shouldShowUI);
		lastCheck = unscaledTime;
		return lastResult;
	}

	public static bool ReCheckFreeCommunicationPermission(bool shouldShowUI, float checkTime)
	{
		lastResult = internal_CheckFreeCommunicationPermission(shouldShowUI);
		lastCheck = checkTime;
		return lastResult;
	}

	public static bool CheckFreeCommunicationPermission(bool shouldShowUI)
	{
		if (shouldShowUI || lastCheck == 0f)
		{
			lastResult = internal_CheckFreeCommunicationPermission(shouldShowUI);
			lastCheck = Time.unscaledTime;
		}
		return lastResult;
	}

	public static bool CheckFreeCommunicationPermission(bool shouldShowUI, float checkTime)
	{
		if (shouldShowUI || lastCheck == 0f)
		{
			lastResult = internal_CheckFreeCommunicationPermission(shouldShowUI);
			lastCheck = checkTime;
		}
		return lastResult;
	}
}
