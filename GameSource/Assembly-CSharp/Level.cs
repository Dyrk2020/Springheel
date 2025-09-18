using System;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
using UnityEngine;

public class Level : MonoBehaviour, IGameEventListener
{
	public GameState.LevelName thisLevelis;

	public Transform StartPoint;

	public Transform StartPoint2;

	public Transform Goal;

	public Transform[] LargeCharacterSpawns;

	public Transform[] SpectatorStart;

	public Transform[] SpectatorGoal;

	public Transform SpectatorStartParent;

	public Transform SpectatorGoalParent;

	public Transform CursorSpawnPoint;

	public float CursorSpawnRadius;

	public Collider2D CameraBounds;

	public Collider2D ThumbnailBounds;

	public Collider2D CursorBounds;

	public float ExtraCursorBounds = 5f;

	public int StartingBlocks;

	public int MaxStartingBlocks;

	public float TotalArea;

	public GameObject[] DeletePreUnload;

	private HashSet<Transform> ZoomedOutCameraTargets = new HashSet<Transform>();

	public float MinimumCharacterPosition;

	public float MaxDensity;

	public float MinDensity;

	public Transform[] UnlockSpawnLocations;

	public List<GoalBlock> goalBlocks = new List<GoalBlock>();

	public BackgroundType DefaultCustombackground;

	public CustomBackground currentCustomBackground;

	public GameState.LevelName currentCustomMusic = GameState.LevelName.BLANKLEVEL;

	public GameState.LevelName currentCustomAmbience = GameState.LevelName.BLANKLEVEL;

	public float LevelUnitTopBuffer;

	public float LevelUnitBottomBuffer;

	public float LevelUnitLeftBuffer;

	public float LevelUnitRightBuffer;

	public Transform Top;

	public Transform Bottom;

	public Transform Left;

	public Transform Right;

	public float ComputedTotalArea
	{
		get
		{
			if (BlankLevelBoundsAllowed())
			{
				return GetBlankLevelBounds().size.x * GetBlankLevelBounds().size.y;
			}
			return TotalArea;
		}
	}

	private void Awake()
	{
		if (CursorBounds == null)
		{
			CursorBounds = CameraBounds;
		}
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<SetCustomBackgroundEvent>(this, adding);
		GameEventManager.ChangeListener<SetCustomMusicEvent>(this, adding);
		GameEventManager.ChangeListener<SetCustomAmbienceEvent>(this, adding);
	}

	private void Start()
	{
		if (CursorSpawnPoint == null)
		{
			if (StartPoint != null)
			{
				CursorSpawnPoint = StartPoint.transform;
			}
			else
			{
				CursorSpawnPoint = base.transform;
			}
		}
		if (DefaultCustombackground != BackgroundType.None && currentCustomBackground == null)
		{
			SetBackground(DefaultCustombackground);
		}
		CameraBounds.gameObject.layer = LayerMask.NameToLayer("Intangible");
		ThumbnailBounds.gameObject.layer = LayerMask.NameToLayer("Intangible");
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(PiecePlacedEvent))
		{
			GoalBlock goalBlock = (e as PiecePlacedEvent).PlacedBlock as GoalBlock;
			if (goalBlock != null && !goalBlocks.Contains(goalBlock))
			{
				goalBlocks.Add(goalBlock);
			}
		}
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PieceDestroyed)
			{
				MsgPieceDestroyed msgPieceDestroyed = (MsgPieceDestroyed)networkMessageReceivedEvent.ReadMessage;
				for (int i = 0; i < goalBlocks.Count; i++)
				{
					if (goalBlocks[i].ID == msgPieceDestroyed.BlockID)
					{
						goalBlocks.RemoveAt(i);
						break;
					}
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetCustomBackground)
			{
				MsgSetCustomBackground msgSetCustomBackground = (MsgSetCustomBackground)networkMessageReceivedEvent.ReadMessage;
				SetBackground(msgSetCustomBackground.newBackground);
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetCustomMusic)
			{
				MsgSetCustomMusic msgSetCustomMusic = (MsgSetCustomMusic)networkMessageReceivedEvent.ReadMessage;
				SetMusic(msgSetCustomMusic.newLevelMusic);
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetCustomAmbience)
			{
				MsgSetCustomAmbience msgSetCustomAmbience = (MsgSetCustomAmbience)networkMessageReceivedEvent.ReadMessage;
				SetAmbience(msgSetCustomAmbience.newLevelAmbiance);
			}
		}
		if (type == typeof(SetCustomBackgroundEvent))
		{
			SetCustomBackgroundEvent setCustomBackgroundEvent = e as SetCustomBackgroundEvent;
			MsgSetCustomBackground msgSetCustomBackground2 = new MsgSetCustomBackground();
			msgSetCustomBackground2.newBackground = setCustomBackgroundEvent.NewBackground;
			LobbyManager.instance.client.Send(NetMsgTypes.SetCustomBackground, msgSetCustomBackground2);
		}
		if (type == typeof(SetCustomMusicEvent))
		{
			SetCustomMusicEvent setCustomMusicEvent = e as SetCustomMusicEvent;
			MsgSetCustomMusic msgSetCustomMusic2 = new MsgSetCustomMusic();
			msgSetCustomMusic2.newLevelMusic = setCustomMusicEvent.NewLevelMusic;
			LobbyManager.instance.client.Send(NetMsgTypes.SetCustomMusic, msgSetCustomMusic2);
		}
		if (type == typeof(SetCustomAmbienceEvent))
		{
			SetCustomAmbienceEvent setCustomAmbienceEvent = e as SetCustomAmbienceEvent;
			MsgSetCustomAmbience msgSetCustomAmbience2 = new MsgSetCustomAmbience();
			msgSetCustomAmbience2.newLevelAmbiance = setCustomAmbienceEvent.NewLevelAmbience;
			LobbyManager.instance.client.Send(NetMsgTypes.SetCustomAmbience, msgSetCustomAmbience2);
		}
	}

	public void AddZoomedOutCameraTarget(ZoomCamera camera, Transform target)
	{
		camera.AddTarget(target);
		ZoomedOutCameraTargets.Add(target);
	}

	public void AddStartAndGoalsToCameraTargets(ZoomCamera camera)
	{
		AddZoomedOutCameraTarget(camera, StartPoint);
		if (Goal != null)
		{
			AddZoomedOutCameraTarget(camera, Goal);
		}
		foreach (GoalBlock goalBlock in goalBlocks)
		{
			if (goalBlock != null)
			{
				AddZoomedOutCameraTarget(camera, goalBlock.transform);
			}
		}
	}

	public void RemoveStartAndGoalsFromCameraTargets(ZoomCamera camera)
	{
		foreach (Transform zoomedOutCameraTarget in ZoomedOutCameraTargets)
		{
			camera.RemoveTarget(zoomedOutCameraTarget);
		}
		ZoomedOutCameraTargets.Clear();
	}

	public GoalBlock GetGoalBlockByID(int id)
	{
		return goalBlocks.FirstOrDefault((GoalBlock goalBlock) => goalBlock.ID == id);
	}

	public void SetBackground(BackgroundType newBackground)
	{
		if (currentCustomBackground != null)
		{
			if (currentCustomBackground.background == newBackground)
			{
				return;
			}
			UnityEngine.Object.Destroy(currentCustomBackground.gameObject);
		}
		CustomBackground background = BackgroundLibrary.Instance.GetBackground(newBackground);
		if (background != null)
		{
			currentCustomBackground = UnityEngine.Object.Instantiate(background);
		}
	}

	public void EnablePlacementBounds(bool enable)
	{
		CursorBounds.enabled = enable;
	}

	public void SetMusic(GameState.LevelName newMusicFromLevel)
	{
		currentCustomMusic = newMusicFromLevel;
		if (newMusicFromLevel == GameState.LevelName.BLANKLEVEL)
		{
			string in_pszState = "Lobby_Free_Play";
			switch (GameSettings.GetInstance().GameMode)
			{
			case GameState.GameMode.PARTY:
				in_pszState = "Lobby_Party_Mode";
				break;
			case GameState.GameMode.CREATIVE:
				in_pszState = "Lobby_Normal";
				break;
			case GameState.GameMode.CHALLENGE:
				in_pszState = "Lobby_Challenge_Mode";
				break;
			}
			AkSoundEngine.SetState("Menus", in_pszState);
		}
		AkSoundEngine.PostEvent(GameState.GetLevelMusString(newMusicFromLevel), base.gameObject);
	}

	public void SetAmbience(GameState.LevelName newAmbienceFromLevel)
	{
		currentCustomAmbience = newAmbienceFromLevel;
		string levelAmbienceString = GameState.GetLevelAmbienceString(newAmbienceFromLevel);
		if (!string.IsNullOrEmpty(levelAmbienceString))
		{
			AkSoundEngine.PostEvent(levelAmbienceString, base.gameObject);
		}
	}

	public bool BlankLevelBoundsAllowed()
	{
		if (Left != null && Right != null && Bottom != null)
		{
			return Top != null;
		}
		return false;
	}

	public Bounds GetBlankLevelBounds()
	{
		Vector3 center = new Vector3((Left.position.x + Right.position.x) / 2f, (Top.position.y + Bottom.position.y) / 2f, 0f);
		Vector3 size = new Vector3(Right.position.x - Left.position.x, Top.position.y - Bottom.position.y, 0f);
		return new Bounds(center, size);
	}

	public Bounds GetCameraBounds()
	{
		if (thisLevelis == GameState.LevelName.BLANKLEVEL && GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
		{
			Vector3 center = new Vector3((Left.position.x + Right.position.x) / 2f, (Top.position.y + Bottom.position.y) / 2f, 0f);
			Vector3 size = new Vector3(Right.position.x - Left.position.x + 10f, Top.position.y - Bottom.position.y + 10f, 0f);
			return new Bounds(center, size);
		}
		return CameraBounds.bounds;
	}

	public Bounds GetThumbnailBounds()
	{
		if (thisLevelis == GameState.LevelName.BLANKLEVEL)
		{
			Vector3 center = new Vector3((Left.position.x + Right.position.x) / 2f, (Top.position.y + Bottom.position.y) / 2f, 0f);
			Vector3 size = new Vector3(Right.position.x - Left.position.x, Top.position.y - Bottom.position.y, 0f);
			return new Bounds(center, size);
		}
		return ThumbnailBounds.bounds;
	}

	public Bounds GetCursorBounds()
	{
		Bounds result;
		if (thisLevelis == GameState.LevelName.BLANKLEVEL && GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
		{
			Vector3 center = new Vector3((Left.position.x + Right.position.x) / 2f, (Top.position.y + Bottom.position.y) / 2f, 0f);
			Vector3 size = new Vector3(Right.position.x - Left.position.x, Top.position.y - Bottom.position.y, 0f);
			result = new Bounds(center, size);
		}
		else
		{
			result = CursorBounds.bounds;
		}
		result.Expand(ExtraCursorBounds);
		return result;
	}

	public static Vector3 GetSpawnFeetOffset()
	{
		float num = -0.3f * Modifiers.GetInstance().CharacterScale - -0.3f * Modifiers.GetInstance().CharacterScales[0] - 0.43f;
		return new Vector3(0f, 0f - num, 0f);
	}

	public Vector3 GetSpawnPosition(float desiredPosition)
	{
		Vector3 spawnFeetOffset = GetSpawnFeetOffset();
		if (!StartPoint2)
		{
			return StartPoint.position + spawnFeetOffset;
		}
		return Vector3.Lerp(StartPoint.position, StartPoint2.position, desiredPosition) + spawnFeetOffset;
	}

	public Vector3 GetLargeCharacterSpawnPosition(int position, int totalPlayerCount)
	{
		if (LargeCharacterSpawns != null && LargeCharacterSpawns.Length + 1 >= totalPlayerCount && LargeCharacterSpawns[position] != null)
		{
			return LargeCharacterSpawns[position].position;
		}
		Debug.Log("Large Spawnpoints not setup for this level, using default fallback system");
		if (position < 2)
		{
			return GetSpawnPosition(position);
		}
		return GetSpawnPosition(position - 2) + Vector3.up * Modifiers.GetInstance().CharacterScale * 0.66f + Vector3.right * 0.1f;
	}

	public void SpawnCharacter(Character c, float desiredPosition)
	{
		SpawnCharacter(c, GetSpawnPosition(desiredPosition));
	}

	public void SpawnCharacter(Character c, Vector3 worldPosition)
	{
		c.StartInvincibleTimer(0.3f);
		c.PositionCharacter(worldPosition);
	}
}
