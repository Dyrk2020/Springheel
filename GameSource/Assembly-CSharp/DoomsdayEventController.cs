using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class DoomsdayEventController : MonoBehaviour, IGameEventListener
{
	private bool paused;

	public DoomsdayMeteor[] meteorPrefabs;

	private UnityEngine.Random.State meteorRNG;

	private bool meteorsOn;

	private List<DoomsdayMeteor> meteors = new List<DoomsdayMeteor>();

	private int meteorFrameCounter;

	private IEnumerator meteorAnim;

	public DoomsdayLava lavaPrefab;

	private DoomsdayLava lavaInstance;

	private bool lavaOn;

	private int lavaFrameCounter;

	private IEnumerator lavaAnim;

	private void Start()
	{
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<LevelResetEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<RoundCompleteEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent) && (e as StartPhaseEvent).Phase == GameControl.GamePhase.PLAY && GameSettings.GetInstance().GameMode != GameState.GameMode.CHALLENGE)
		{
			if (GameState.GetInstance().UsingHotSeat)
			{
				ResetDoomsdayStuff();
			}
			else
			{
				ResetDoomsdayStuff();
				StartDoomsdayStuff();
			}
		}
		if (type == typeof(LevelResetEvent))
		{
			switch (GameSettings.GetInstance().GameMode)
			{
			case GameState.GameMode.CHALLENGE:
				StartDoomsdayStuff();
				break;
			case GameState.GameMode.FREEPLAY:
				ResetDoomsdayStuff();
				StartDoomsdayStuff();
				break;
			case GameState.GameMode.CREATIVE:
				if (GameState.GetInstance().UsingHotSeat)
				{
					StartDoomsdayStuff();
				}
				break;
			}
		}
		if (type == typeof(EndPhaseEvent) && (e as EndPhaseEvent).Phase == GameControl.GamePhase.PLAY)
		{
			ResetDoomsdayStuff();
		}
		if (type == typeof(RoundCompleteEvent))
		{
			ResetDoomsdayStuff();
		}
		if (type == typeof(PauseEvent))
		{
			PauseEvent pauseEvent = e as PauseEvent;
			paused = pauseEvent.Paused;
		}
	}

	private void StartDoomsdayStuff()
	{
		if (Modifiers.GetInstance().DoomsdayMeteorsEnabled)
		{
			StartMeteors();
		}
		if (Modifiers.GetInstance().DoomsdayLavaEnabled)
		{
			StartLava();
		}
	}

	private void ResetDoomsdayStuff()
	{
		StopMeteors();
		StopLava();
	}

	private void FixedUpdate()
	{
		if (!paused)
		{
			if (meteorAnim != null && !meteorAnim.MoveNext())
			{
				meteorAnim = null;
			}
			meteorFrameCounter++;
			if (lavaAnim != null && !lavaAnim.MoveNext())
			{
				lavaAnim = null;
			}
			lavaFrameCounter++;
		}
	}

	private void StartMeteors()
	{
		meteorsOn = true;
		meteorFrameCounter = 0;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		int num = 123456789;
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		if ((uint)(gameMode - 1) <= 1u)
		{
			GameControl currentGameController = LobbyManager.instance.CurrentGameController;
			if (currentGameController != null)
			{
				num += currentGameController.RoundNumber;
			}
			else
			{
				Debug.LogError("Could not find game controller.");
			}
		}
		UnityEngine.Random.InitState(num);
		meteorRNG = UnityEngine.Random.state;
		UnityEngine.Random.state = state;
		meteorAnim = MeteorGenerator();
	}

	private void StartLava()
	{
		lavaOn = true;
		lavaFrameCounter = 0;
		lavaAnim = LavaAnimator();
	}

	private void StopMeteors()
	{
		foreach (DoomsdayMeteor meteor in meteors)
		{
			if (meteor != null)
			{
				UnityEngine.Object.Destroy(meteor.gameObject);
			}
		}
		meteors.Clear();
		meteorsOn = false;
		meteorAnim = null;
	}

	private void StopLava()
	{
		if (lavaInstance != null)
		{
			if (lavaInstance.OverStartZone)
			{
				GameEventManager.SendEvent(new HoldRespawnEvent(hold: false));
				Debug.Log("Respawns are allowed again.");
			}
			UnityEngine.Object.Destroy(lavaInstance.gameObject);
			lavaInstance = null;
		}
		lavaOn = false;
		lavaAnim = null;
	}

	private int GetRandomMeteorInt(int rangeStart, int rangeEnd)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.state = meteorRNG;
		int result = UnityEngine.Random.Range(rangeStart, rangeEnd);
		meteorRNG = UnityEngine.Random.state;
		UnityEngine.Random.state = state;
		return result;
	}

	private DoomsdayMeteor GetRandomMeteorPrefab()
	{
		return meteorPrefabs[GetRandomMeteorInt(0, meteorPrefabs.Length)];
	}

	private float GetRandomMeteorFloat()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.state = meteorRNG;
		float value = UnityEngine.Random.value;
		meteorRNG = UnityEngine.Random.state;
		UnityEngine.Random.state = state;
		return value;
	}

	private IEnumerator LavaAnimator()
	{
		Modifiers instance = Modifiers.GetInstance();
		Debug.Log("Lava will start bubbling in " + instance.DoomsdayLavaDelay + " seconds");
		int waitFrames = Mathf.CeilToInt((float)instance.DoomsdayLavaDelay / Time.fixedDeltaTime);
		int i = 0;
		while (i < waitFrames)
		{
			yield return null;
			int num = i + 1;
			i = num;
		}
		lavaInstance = UnityEngine.Object.Instantiate(lavaPrefab);
		Modifiers.PerLevelLavaSettings perLevelLavaSettings = Modifiers.GetInstance().FindLavaSettingsForLevel(GameState.GetInstance().SelectedLevel);
		if (perLevelLavaSettings != null)
		{
			lavaInstance.lavaDirection = perLevelLavaSettings.lavaDirection;
			lavaInstance.extraPadding = perLevelLavaSettings.extraPadding;
			lavaInstance.lavaSpeedMultiplier = perLevelLavaSettings.speedMultiplier;
		}
		lavaInstance.Initialize();
	}

	private IEnumerator MeteorGenerator()
	{
		Modifiers mods = Modifiers.GetInstance();
		int FixedUpdateFPS = Mathf.CeilToInt(1f / Time.fixedDeltaTime);
		float minMeteorAngle = 115f;
		float maxMeteorAngle = 155f;
		Level levelLayout = LobbyManager.instance.CurrentGameController.LevelLayout;
		Bounds camBounds = ((levelLayout.thisLevelis == GameState.LevelName.BLANKLEVEL) ? levelLayout.GetBlankLevelBounds() : levelLayout.GetCameraBounds());
		float num = 2f;
		Vector3 A = new Vector3(camBounds.min.x, camBounds.max.y + num, 0f);
		Vector3 B = new Vector3(camBounds.max.x, camBounds.max.y + num, 0f);
		Debug.Log("Meteors will start falling in " + mods.DoomsdayMeteorsDelay + " seconds");
		int meteorwaitFrames = Mathf.CeilToInt((float)mods.DoomsdayMeteorsDelay / Time.fixedDeltaTime);
		int i = 0;
		while (i < meteorwaitFrames)
		{
			yield return null;
			int num2 = i + 1;
			i = num2;
		}
		Debug.Log("A strange smell of sulfur fills the air...");
		int nextMeteor = 0;
		while (meteorsOn)
		{
			for (int num3 = meteors.Count - 1; num3 >= 0; num3--)
			{
				if (meteors[num3] == null)
				{
					meteors.SwapRemove(num3);
				}
			}
			if (meteors.Count < 30)
			{
				i = 0;
				while (i < nextMeteor)
				{
					yield return null;
					int num2 = i + 1;
					i = num2;
				}
				float num4 = Mathf.Lerp(minMeteorAngle, maxMeteorAngle, GetRandomMeteorFloat());
				float num5 = Mathf.Abs(camBounds.max.y - camBounds.min.y);
				Vector3 vector = Quaternion.AngleAxis(num4, Vector3.forward) * Vector3.up * (num5 / Mathf.Cos(MathF.PI / 180f * num4));
				vector.x *= 0.8f;
				vector.y = 0f;
				DoomsdayMeteor doomsdayMeteor = UnityEngine.Object.Instantiate(GetRandomMeteorPrefab());
				float randomMeteorFloat = GetRandomMeteorFloat();
				doomsdayMeteor.speed = Mathf.Lerp(17f, 10f, randomMeteorFloat);
				doomsdayMeteor.transform.localScale = Vector3.one * Mathf.Lerp(1f, 10f, randomMeteorFloat);
				doomsdayMeteor.size = (DoomsdayMeteor.MeteorSizes)Mathf.CeilToInt(Mathf.Lerp(0.1f, 5f, randomMeteorFloat));
				doomsdayMeteor.StartMeteorSound();
				doomsdayMeteor.yMin = camBounds.min.y - 10f;
				doomsdayMeteor.transform.position = Vector3.Lerp(A + vector, B + vector, GetRandomMeteorFloat());
				doomsdayMeteor.transform.rotation = Quaternion.AngleAxis(num4, Vector3.forward);
				meteors.Add(doomsdayMeteor);
				float t = (float)meteorFrameCounter / (float)(FixedUpdateFPS * mods.DoomsdayMeteorsFullRampUpSeconds);
				int rangeStart = (int)Mathf.Lerp(FixedUpdateFPS, 1f, t);
				int rangeEnd = (int)Mathf.Lerp(FixedUpdateFPS * 3, FixedUpdateFPS, t);
				nextMeteor = GetRandomMeteorInt(rangeStart, rangeEnd);
				yield return null;
			}
			else
			{
				yield return null;
			}
		}
	}
}
