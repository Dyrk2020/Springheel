using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Timekeeper
{
	private struct TimeSource
	{
		public float scale;

		public float slowDuration;

		public float startTime;

		public bool paused;

		public TimeSource(float scale, float slowDuration)
		{
			this.scale = scale;
			this.slowDuration = slowDuration;
			startTime = Time.unscaledTime;
			paused = false;
		}
	}

	private static bool slowing;

	private static bool ramping;

	private static Dictionary<GameObject, TimeSource> timeSources;

	private static List<GameObject> sourcesToRemove;

	private static float slowRampTime;

	private static bool listeningForScene;

	public static bool Slowing
	{
		get
		{
			if (!slowing)
			{
				return ramping;
			}
			return true;
		}
	}

	private static bool AnySourcePaused
	{
		get
		{
			foreach (KeyValuePair<GameObject, TimeSource> timeSource in timeSources)
			{
				if (timeSource.Value.paused)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static void AddSlowMoSource(MonoBehaviour source, float scale, float duration, float rampTime = 0f)
	{
		AddSlowMoSource(source, scale, duration, rampTime, AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
	}

	public static void AddSlowMoSource(MonoBehaviour source, float scale, float duration, float rampTime, AnimationCurve slowRamp)
	{
		if (timeSources == null)
		{
			timeSources = new Dictionary<GameObject, TimeSource>();
		}
		TimeSource value = new TimeSource(scale, duration);
		if (!timeSources.ContainsKey(source.gameObject))
		{
			timeSources.Add(source.gameObject, value);
		}
		else
		{
			timeSources[source.gameObject] = value;
		}
		LobbyManager.instance.CurrentGameController.StartCoroutine(slowTime(rampTime, slowRamp));
	}

	public static void RemoveSlowMoSource(MonoBehaviour source)
	{
		if (timeSources != null && timeSources.ContainsKey(source.gameObject))
		{
			timeSources.Remove(source.gameObject);
		}
	}

	public static void PauseSlowMoSource(MonoBehaviour source)
	{
		if (timeSources == null || !timeSources.ContainsKey(source.gameObject))
		{
			return;
		}
		TimeSource value = timeSources[source.gameObject];
		value.paused = true;
		if (value.slowDuration >= 0f)
		{
			value.slowDuration -= Time.unscaledTime - value.startTime;
			if (value.slowDuration < 0f)
			{
				value.slowDuration = 0f;
			}
		}
		timeSources[source.gameObject] = value;
	}

	public static void UnpauseSlowMoSource(MonoBehaviour source)
	{
		if (timeSources != null && timeSources.ContainsKey(source.gameObject))
		{
			TimeSource value = timeSources[source.gameObject];
			value.paused = false;
			if (value.slowDuration >= 0f)
			{
				value.startTime = Time.unscaledTime;
			}
			timeSources[source.gameObject] = value;
		}
	}

	public static bool HasSource(MonoBehaviour source)
	{
		if (timeSources != null)
		{
			return timeSources.ContainsKey(source.gameObject);
		}
		return false;
	}

	private static IEnumerator slowTime(float rampTime, AnimationCurve slowRamp)
	{
		if (!listeningForScene)
		{
			SceneManager.activeSceneChanged += onSceneChanged;
			listeningForScene = true;
		}
		if (slowing)
		{
			yield break;
		}
		slowing = true;
		AkSoundEngine.PostEvent("SFX_Pieces_Stop_Watch_Start", GameState.GetInstance().gameObject);
		slowRampTime = 0f;
		while (timeSources.Count > 0 && slowing && slowRampTime < rampTime)
		{
			while (AnySourcePaused)
			{
				yield return null;
			}
			ramping = true;
			slowRampTime += Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Lerp(minScale(), Modifiers.GetInstance().GameSpeed, slowRamp.Evaluate(slowRampTime / rampTime));
			timeoutSources();
			yield return null;
		}
		ramping = false;
		while (AnySourcePaused)
		{
			yield return null;
		}
		if (slowing && timeSources.Count > 0)
		{
			slowRampTime = rampTime;
			Time.timeScale = minScale();
		}
		float scale;
		while (timeSources.Count > 0 && slowing)
		{
			while (AnySourcePaused)
			{
				yield return null;
			}
			scale = minScale();
			float num = Time.timeScale;
			if (num > scale)
			{
				num = Mathf.Lerp(Modifiers.GetInstance().GameSpeed, scale, slowRamp.Evaluate(Time.timeScale / scale + Time.unscaledDeltaTime / rampTime));
			}
			Time.timeScale = num;
			timeoutSources();
			yield return null;
		}
		slowing = false;
		scale = Time.timeScale;
		AkSoundEngine.PostEvent("SFX_Pieces_Stop_Watch_Stop", GameState.GetInstance().gameObject);
		ramping = true;
		while (slowRampTime > 0f && !slowing)
		{
			while (AnySourcePaused)
			{
				yield return null;
			}
			slowRampTime -= Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Lerp(scale, Modifiers.GetInstance().GameSpeed, slowRamp.Evaluate(slowRampTime / rampTime));
			yield return null;
		}
		ramping = false;
	}

	private static void onSceneChanged(Scene scene, Scene newScene)
	{
		if (slowing || timeSources.Count > 0)
		{
			timeSources.Clear();
			slowing = false;
			Time.timeScale = Modifiers.GetInstance().GameSpeed;
			AkSoundEngine.PostEvent("SFX_Pieces_Stop_Watch_Stop", GameState.GetInstance().gameObject);
		}
	}

	private static float minScale()
	{
		float num = Modifiers.GetInstance().GameSpeed;
		foreach (TimeSource value in timeSources.Values)
		{
			if (!value.paused && value.scale < num)
			{
				num = value.scale;
			}
		}
		return num;
	}

	private static void timeoutSources()
	{
		if (sourcesToRemove == null)
		{
			sourcesToRemove = new List<GameObject>();
		}
		float unscaledTime = Time.unscaledTime;
		foreach (KeyValuePair<GameObject, TimeSource> timeSource in timeSources)
		{
			if ((timeSource.Value.slowDuration >= 0f && !timeSource.Value.paused && unscaledTime >= timeSource.Value.startTime + timeSource.Value.slowDuration) || timeSource.Key == null)
			{
				sourcesToRemove.Add(timeSource.Key);
			}
		}
		foreach (GameObject item in sourcesToRemove)
		{
			timeSources.Remove(item);
		}
		sourcesToRemove.Clear();
	}
}
