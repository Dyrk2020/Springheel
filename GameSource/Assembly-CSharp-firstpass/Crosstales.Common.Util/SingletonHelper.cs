using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crosstales.Common.Util;

public class SingletonHelper
{
	private static bool isInitialized;

	public static bool isQuitting { get; set; }

	static SingletonHelper()
	{
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log($"{Time.realtimeSinceStartup} - Constructor!");
		}
		initialize();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void initialize()
	{
		if (isInitialized)
		{
			if (BaseConstants.DEV_DEBUG)
			{
				Debug.Log($"{Time.realtimeSinceStartup} - already initialized!");
			}
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log($"{Time.realtimeSinceStartup} - initialize: {isQuitting}");
		}
		isInitialized = true;
		Application.quitting += onQuitting;
		SceneManager.sceneLoaded += onSceneLoaded;
	}

	private static void onSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log($"{Time.realtimeSinceStartup} - onSceneLoaded");
		}
		isQuitting = false;
		BaseHelper.ApplicationIsPlaying = true;
	}

	private static void onQuitting()
	{
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log($"{Time.realtimeSinceStartup} - onQuitting");
		}
		isQuitting = true;
	}
}
