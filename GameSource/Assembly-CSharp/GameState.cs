using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class GameState : MonoBehaviour
{
	public enum GameMode
	{
		FREEPLAY,
		CREATIVE,
		PARTY,
		CHALLENGE
	}

	public enum LevelName
	{
		FARM = 0,
		ROOFTOPS = 1,
		OLDMANSION = 2,
		WATERFALL = 3,
		PYRAMID = 4,
		WINDMILL = 5,
		METALPLANT = 6,
		ICEBERG = 7,
		DANCEPARTY = 8,
		PIER = 9,
		BLANKLEVEL = 10,
		JUNGLETEMPLE = 11,
		VOLCANO = 12,
		CRUMBLINGBRIDGE = 13,
		NUCLEARPLANT = 14,
		TRONLEVEL = 15,
		SPACELEVEL = 16,
		BALLROOM = 17,
		ROLLERCOASTER = 18,
		METRO = 19,
		WATERTOWER = 20,
		RAFT = 21,
		PICTUREFRAME = 22,
		SPACESTATION = 23,
		RANDOM = 100,
		PROTOTYPE1 = 101,
		PROTOTYPE2 = 102,
		PROTOTYPE3 = 103,
		PROTOTYPE4 = 104,
		PROTOTYPE5 = 105,
		PROTOTYPE6 = 106,
		PROTOTYPE7 = 107,
		PROTOTYPE8 = 108
	}

	public enum PortalID
	{
		FARM = 0,
		ROOFTOPS = 1,
		OLDMANSION = 2,
		WATERFALL = 3,
		PYRAMID = 4,
		WINDMILL = 5,
		METALPLANT = 6,
		ICEBERG = 7,
		DANCEPARTY = 8,
		PIER = 9,
		BLANKLEVEL = 10,
		JUNGLETEMPLE = 11,
		VOLCANO = 12,
		CRUMBLINGBRIDGE = 13,
		NUCLEARPLANT = 14,
		TRONLEVEL = 15,
		CUSTOMA = 16,
		CUSTOMB = 17,
		CUSTOMC = 18,
		CUSTOMD = 19,
		SPACELEVEL = 20,
		BALLROOM = 21,
		ROLLERCOASTER = 22,
		METRO = 23,
		WATERTOWER = 24,
		RAFT = 25,
		PICTUREFRAME = 26,
		SPACESTATION = 27,
		RANDOM = 100,
		PROTOTYPE1 = 101,
		PROTOTYPE2 = 102,
		PROTOTYPE3 = 103,
		PROTOTYPE4 = 104,
		PROTOTYPE5 = 105,
		PROTOTYPE6 = 106,
		PROTOTYPE7 = 107,
		PROTOTYPE8 = 108
	}

	private static GameMode[] _modeList;

	protected static GameState instance;

	public static bool wasDestroyed = false;

	private string[] onscreenPrefixes = new string[1] { "[Net]" };

	public KeyboardInput Keyboard;

	[SerializeField]
	public List<Controller> Controllers = new List<Controller>();

	public int[] PlayerScores = new int[4];

	public LevelName SelectedLevel;

	public Dictionary<LobbyPlayer, UnLockInfo> nextUnlocks = new Dictionary<LobbyPlayer, UnLockInfo>();

	public Dictionary<LobbyPlayer, bool> guaranteedUnlocks = new Dictionary<LobbyPlayer, bool>();

	public bool UsingHotSeat;

	protected bool started;

	public static bool DebugMode;

	public static bool DebugModeUI;

	public static bool OnlineDebugMode;

	public static bool ExtraDataDebugMode;

	public static bool FlagsDontWin = false;

	public bool Paused;

	public float TimeStarted;

	public float TimeElapsed;

	protected Dictionary<string, int> pieceCount;

	public ControlTipData controlTips = new ControlTipData();

	protected int currentSortOrderNumber;

	public static ChatDisplay ChatSystem;

	public bool PreservePlayers;

	public int LastLobbyScore;

	public LevelSelectController.PlayedSnapshotInfo currentSnapshotInfo;

	public string lastLevelPlayed = "";

	public bool autoStartHosting;

	public bool autoJoinInvitedGame;

	private int debugTextRow = 1;

	private static int numberOfTextRows = 0;

	private int Textheight = 20;

	private static List<string> debugStringList = new List<string>();

	private int opacityCutoffHeight = 20;

	private int distanceFromPresent;

	private GUIStyle debugStyle = new GUIStyle();

	private GUIStyle debugStyleBg = new GUIStyle();

	private GUIStyle betaMessage = new GUIStyle();

	private GUIStyle debugExtraDataStyle = new GUIStyle();

	private Rect versionRect = new Rect(5f, 0f, 200f, 20f);

	private static GameMode[] ModeList
	{
		get
		{
			if (_modeList == null)
			{
				_modeList = new GameMode[4]
				{
					GameMode.PARTY,
					GameMode.CREATIVE,
					GameMode.CHALLENGE,
					GameMode.FREEPLAY
				};
			}
			return _modeList;
		}
	}

	public int CurrentSortOrderNumber
	{
		get
		{
			currentSortOrderNumber += 100;
			if (currentSortOrderNumber >= 13000 || currentSortOrderNumber <= -30000)
			{
				currentSortOrderNumber = 0;
			}
			return currentSortOrderNumber;
		}
		set
		{
			if (value >= 13000 || value <= -30000)
			{
				currentSortOrderNumber = 0;
			}
			else
			{
				currentSortOrderNumber = value;
			}
		}
	}

	public static GameMode NextMode(GameMode current)
	{
		GameMode[] modeList = ModeList;
		int num = -1;
		for (int i = 0; i < modeList.Length; i++)
		{
			if (modeList[i] == current)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return modeList[0];
		}
		return modeList[(num + 1) % modeList.Length];
	}

	public static GameMode PreviousMode(GameMode current)
	{
		GameMode[] modeList = ModeList;
		int num = -1;
		for (int i = 0; i < modeList.Length; i++)
		{
			if (modeList[i] == current)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return modeList[0];
		}
		return modeList[(num - 1 + modeList.Length) % modeList.Length];
	}

	public static bool ModeIsAllowed(GameMode mode)
	{
		return ModeList.Contains(mode);
	}

	public static string GetLevelSceneName(LevelName level)
	{
		return level switch
		{
			LevelName.FARM => "Farm", 
			LevelName.ROOFTOPS => "Rooftops", 
			LevelName.OLDMANSION => "RicketyHouse", 
			LevelName.WATERFALL => "Waterfall", 
			LevelName.PYRAMID => "Pyramid", 
			LevelName.WINDMILL => "WindMill", 
			LevelName.METALPLANT => "MetalPlant", 
			LevelName.ICEBERG => "Iceberg", 
			LevelName.DANCEPARTY => "DanceParty", 
			LevelName.PIER => "Pier", 
			LevelName.VOLCANO => "Volcano", 
			LevelName.BLANKLEVEL => "BlankLevel", 
			LevelName.JUNGLETEMPLE => "JungleTemple", 
			LevelName.CRUMBLINGBRIDGE => "CrumblingBridge", 
			LevelName.NUCLEARPLANT => "NuclearPlant", 
			LevelName.TRONLEVEL => "TronLevel", 
			LevelName.SPACELEVEL => "SpaceLevel", 
			LevelName.BALLROOM => "Ballroom", 
			LevelName.ROLLERCOASTER => "RollerCoaster", 
			LevelName.METRO => "Metro", 
			LevelName.WATERTOWER => "WaterTower", 
			LevelName.RAFT => "Raft", 
			LevelName.PICTUREFRAME => "PictureFrame", 
			LevelName.SPACESTATION => "SpaceStation", 
			LevelName.PROTOTYPE1 => "Prototype1", 
			LevelName.PROTOTYPE2 => "Prototype2", 
			LevelName.PROTOTYPE3 => "Prototype3", 
			LevelName.PROTOTYPE4 => "Prototype4", 
			LevelName.PROTOTYPE5 => "Prototype5", 
			LevelName.PROTOTYPE6 => "Prototype6", 
			LevelName.PROTOTYPE7 => "Prototype7", 
			LevelName.PROTOTYPE8 => "Prototype8", 
			_ => "TreeHouseLobby", 
		};
	}

	public static string GetLevelLocalizedName(LevelName level)
	{
		return level switch
		{
			LevelName.FARM => LocalizationManager.GetTranslation("LevelNames/The Farm"), 
			LevelName.ROOFTOPS => LocalizationManager.GetTranslation("LevelNames/Rooftops"), 
			LevelName.OLDMANSION => LocalizationManager.GetTranslation("LevelNames/Old House"), 
			LevelName.WATERFALL => LocalizationManager.GetTranslation("LevelNames/Waterfall"), 
			LevelName.PYRAMID => LocalizationManager.GetTranslation("LevelNames/Desert"), 
			LevelName.WINDMILL => LocalizationManager.GetTranslation("LevelNames/Windmill"), 
			LevelName.METALPLANT => LocalizationManager.GetTranslation("LevelNames/Metal Plant"), 
			LevelName.ICEBERG => LocalizationManager.GetTranslation("LevelNames/Iceberg"), 
			LevelName.DANCEPARTY => LocalizationManager.GetTranslation("LevelNames/Dance Party"), 
			LevelName.PIER => LocalizationManager.GetTranslation("LevelNames/The Pier"), 
			LevelName.VOLCANO => LocalizationManager.GetTranslation("LevelNames/Volcano"), 
			LevelName.BLANKLEVEL => LocalizationManager.GetTranslation("LevelNames/Blank Level"), 
			LevelName.JUNGLETEMPLE => LocalizationManager.GetTranslation("LevelNames/Jungle"), 
			LevelName.CRUMBLINGBRIDGE => LocalizationManager.GetTranslation("LevelNames/CrumblingBridge"), 
			LevelName.NUCLEARPLANT => LocalizationManager.GetTranslation("LevelNames/NuclearPlant"), 
			LevelName.TRONLEVEL => LocalizationManager.GetTranslation("LevelNames/TronLevel"), 
			LevelName.SPACELEVEL => LocalizationManager.GetTranslation("LevelNames/SpaceLevel"), 
			LevelName.BALLROOM => LocalizationManager.GetTranslation("LevelNames/TheBallroom"), 
			LevelName.ROLLERCOASTER => LocalizationManager.GetTranslation("LevelNames/RollerCoaster"), 
			LevelName.METRO => LocalizationManager.GetTranslation("LevelNames/Metro"), 
			LevelName.WATERTOWER => LocalizationManager.GetTranslation("LevelNames/WaterTower"), 
			LevelName.RAFT => LocalizationManager.GetTranslation("LevelNames/Raft"), 
			LevelName.PICTUREFRAME => LocalizationManager.GetTranslation("LevelNames/PictureFrame"), 
			LevelName.SPACESTATION => LocalizationManager.GetTranslation("LevelNames/SpaceStation"), 
			LevelName.PROTOTYPE1 => "Prototype1", 
			LevelName.PROTOTYPE2 => "Prototype2", 
			LevelName.PROTOTYPE3 => "Prototype3", 
			LevelName.PROTOTYPE4 => "Prototype4", 
			LevelName.PROTOTYPE5 => "Prototype5", 
			LevelName.PROTOTYPE6 => "Prototype6", 
			LevelName.PROTOTYPE7 => "Prototype7", 
			LevelName.PROTOTYPE8 => "Prototype8", 
			_ => "default Level name error", 
		};
	}

	public static string GetLevelLocalizationTerm(LevelName level)
	{
		return level switch
		{
			LevelName.FARM => "LevelNames/The Farm", 
			LevelName.ROOFTOPS => "LevelNames/Rooftops", 
			LevelName.OLDMANSION => "LevelNames/Old House", 
			LevelName.WATERFALL => "LevelNames/Waterfall", 
			LevelName.PYRAMID => "LevelNames/Desert", 
			LevelName.WINDMILL => "LevelNames/Windmill", 
			LevelName.METALPLANT => "LevelNames/Metal Plant", 
			LevelName.ICEBERG => "LevelNames/Iceberg", 
			LevelName.DANCEPARTY => "LevelNames/Dance Party", 
			LevelName.PIER => "LevelNames/The Pier", 
			LevelName.VOLCANO => "LevelNames/Volcano", 
			LevelName.BLANKLEVEL => "LevelNames/Blank Level", 
			LevelName.JUNGLETEMPLE => "LevelNames/Jungle", 
			LevelName.CRUMBLINGBRIDGE => "LevelNames/CrumblingBridge", 
			LevelName.NUCLEARPLANT => "LevelNames/NuclearPlant", 
			LevelName.TRONLEVEL => "LevelNames/TronLevel", 
			LevelName.SPACELEVEL => "LevelNames/SpaceLevel", 
			LevelName.BALLROOM => "LevelNames/TheBallroom", 
			LevelName.ROLLERCOASTER => "LevelNames/RollerCoaster", 
			LevelName.METRO => "LevelNames/Metro", 
			LevelName.WATERTOWER => "LevelNames/WaterTower", 
			LevelName.RAFT => "LevelNames/Raft", 
			LevelName.PICTUREFRAME => "LevelNames/PictureFrame", 
			LevelName.SPACESTATION => "LevelNames/SpaceStation", 
			_ => "", 
		};
	}

	public static string GetLocalizationVersionNumber()
	{
		return LocalizationManager.GetTranslation("CurrentVersion");
	}

	public static string GetLevelMusString(LevelName level)
	{
		switch (level)
		{
		case LevelName.FARM:
			return "MUS_Level_Farm";
		case LevelName.ROOFTOPS:
			return "MUS_Level_Rooftop";
		case LevelName.OLDMANSION:
			return "MUS_Level_Oldhouse";
		case LevelName.WATERFALL:
			return "MUS_Level_WaterFall";
		case LevelName.PYRAMID:
			return "MUS_Level_Pyramid";
		case LevelName.WINDMILL:
			return "MUS_Level_Windmill";
		case LevelName.METALPLANT:
			return "MUS_Level_Metal_Plant";
		case LevelName.ICEBERG:
			return "MUS_Level_Iceberg";
		case LevelName.DANCEPARTY:
			return "MUS_Level_DanceParty";
		case LevelName.PIER:
			return "MUS_Level_Pier";
		case LevelName.BLANKLEVEL:
			return "MUS_Menu_Start";
		case LevelName.JUNGLETEMPLE:
			return "MUS_Level_JungleTemple";
		case LevelName.VOLCANO:
			return "MUS_Level_Volcano";
		case LevelName.CRUMBLINGBRIDGE:
			return "MUS_Crumbling_Bridge";
		case LevelName.TRONLEVEL:
			return "MUS_Light_Bridge";
		case LevelName.NUCLEARPLANT:
			return "MUS_Nuclear_Plant";
		case LevelName.BALLROOM:
			return "Mus_Level_Ballroom";
		case LevelName.SPACELEVEL:
			return "Mus_Level_Space";
		case LevelName.ROLLERCOASTER:
			return "MUS_Level_RollerCoaster";
		case LevelName.METRO:
			return "MUS_Level_Subway";
		case LevelName.WATERTOWER:
			return "MUS_Level_ToxicTower";
		case LevelName.RAFT:
			return "MUS_Level_Islands";
		case LevelName.PICTUREFRAME:
			return "MUS_Level_PictureFrame";
		case LevelName.SPACESTATION:
			return "MUS_Level_SpaceStation";
		default:
			Debug.Log("Trying to get invalid level Music string. Potential Problem");
			return "MUS_Level_Rooftop";
		}
	}

	public static string GetLevelAmbienceString(LevelName level)
	{
		switch (level)
		{
		case LevelName.FARM:
			return "SFX_Level_Farm_Ambiance";
		case LevelName.ROOFTOPS:
			return "SFX_Level_Rooftops";
		case LevelName.OLDMANSION:
			return "SFX_Level_OldHouse";
		case LevelName.WATERFALL:
			return "SFX_Level_Waterfall";
		case LevelName.PYRAMID:
			return "SFX_Level_Pyramid";
		case LevelName.WINDMILL:
			return "SFX_Level_Windmill";
		case LevelName.METALPLANT:
			return "SFX_Level_Metal_Plant";
		case LevelName.ICEBERG:
			return "SFX_Level_Iceberg";
		case LevelName.DANCEPARTY:
			return "SFX_Level_DanceParty";
		case LevelName.PIER:
			return "SFX_Level_Pier";
		case LevelName.BLANKLEVEL:
			return "SFX_Level_BlankLevel";
		case LevelName.JUNGLETEMPLE:
			return "SFX_Level_Jungle_Temple";
		case LevelName.VOLCANO:
			return "SFX_Level_Volcano";
		case LevelName.CRUMBLINGBRIDGE:
			return "SFX_Level_Crumbling_Bridge";
		case LevelName.TRONLEVEL:
			return "SFX_Level_Light_Bridge";
		case LevelName.NUCLEARPLANT:
			return "SFX_Level_Nuclear_Plant";
		case LevelName.BALLROOM:
			return "SFX_Level_Ballroom";
		case LevelName.SPACELEVEL:
			return "SFX_Level_Space";
		case LevelName.ROLLERCOASTER:
			return "SFX_Level_RollerCoaster";
		case LevelName.METRO:
			return "SFX_Level_Subway";
		case LevelName.WATERTOWER:
			return "SFX_Level_ToxicTower";
		case LevelName.RAFT:
			return "SFX_Level_Islands";
		case LevelName.PICTUREFRAME:
			return "SFX_Level_Pictureframe";
		case LevelName.SPACESTATION:
			return "SFX_Level_SpaceStation";
		default:
			Debug.Log("Trying to get invalid level Ambience string. Potential Problem");
			return "SFX_Level_BlankLevel";
		}
	}

	public static GameState GetInstance()
	{
		if (instance == null)
		{
			GameObject gameObject = new GameObject("Game State", typeof(GameState));
			instance = gameObject.GetComponent<GameState>();
			gameObject.AddComponent<TimeController>();
			if (!Application.isEditor && Debug.isDebugBuild)
			{
				gameObject.AddComponent<Debugger>();
			}
			UnityEngine.Object.DontDestroyOnLoad(instance);
			GameObject gameObject2 = new GameObject("Keyboard", typeof(KeyboardInput));
			instance.Keyboard = gameObject2.GetComponent<KeyboardInput>();
			gameObject2.transform.parent = instance.transform;
			CheatListener cheatListener = UnityEngine.Object.Instantiate(Resources.Load<CheatListener>("CheaterTemplate"));
			ChatSystem = UnityEngine.Object.Instantiate(Resources.Load<ChatDisplay>("ChatSystemPrefab"));
			ChatSystem.keyboard = instance.Keyboard;
			Controller.AddGlobalReceiver(ChatSystem);
			cheatListener.name = "Cheater";
			cheatListener.transform.parent = instance.Keyboard.transform;
			instance.Keyboard.AddReceiver(cheatListener);
			instance.pieceCount = new Dictionary<string, int>();
			GameObject[] array = PlaceableMetadataList.Instance.allBlockPrefabs.Concat(PlaceableMetadataList.Instance.extraBlocks).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Placeable component = array[i].GetComponent<Placeable>();
				if (!instance.pieceCount.ContainsKey(component.Name))
				{
					instance.pieceCount.Add(component.Name, 0);
				}
				else
				{
					Debug.LogError("ERROR! " + component.Name + " is already in GameState.pieceCount");
				}
			}
			string versionNumber = GameSettings.GetInstance().VersionNumber;
			Debug.Log("Ultimate Chicken Horse - version " + versionNumber);
			Debug.Log("Date UTC: " + DateTime.UtcNow);
			if (!instance.started)
			{
				instance.Start();
			}
		}
		return instance;
	}

	private async void Start()
	{
		SetGuiStyle();
		Application.targetFrameRate = 300;
		UnityEngine.Cursor.visible = false;
		UnityEngine.Cursor.lockState = CursorLockMode.Confined;
		if (started)
		{
			return;
		}
		if (InputManager.Devices != null)
		{
			for (int i = 0; i != InputManager.Devices.Count; i++)
			{
				AttachDeviceToController(InputManager.Devices[i]);
			}
		}
		else
		{
			Debug.LogWarning("InputManager.Devices is null -- not sure why. Remove this warning if everything works.");
		}
		InputManager.OnDeviceAttached += AttachDeviceToController;
		InputManager.OnDeviceDetached += RemoveDeviceFromController;
		started = true;
		AkSoundEngine.LoadBank("New_SoundBank.bnk", out var _);
		Matchmaker.InstantiateNow();
	}

	public static void ToggleDebugMode()
	{
		DebugMode = !DebugMode;
		DebugModeUI = DebugMode;
		UserMessageManager.Instance.UserMessage("Debug Mode  " + (DebugMode ? "Enabled" : "Disabled"), 1f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
	}

	public static void ToggleDebugModeNoSpecialUI()
	{
		DebugMode = !DebugMode;
		DebugModeUI = false;
		UserMessageManager.Instance.UserMessage("Debug Mode No Special UI " + (DebugMode ? "Enabled" : "Disabled"), 1f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
	}

	private void Update()
	{
		if (!Paused)
		{
			TimeElapsed += Time.unscaledDeltaTime;
		}
		if (Application.isEditor && Input.GetKeyDown(KeyCode.F5) && Input.GetKey(KeyCode.LeftShift))
		{
			ToggleDebugMode();
		}
		if (Input.GetKeyDown(KeyCode.PageUp))
		{
			distanceFromPresent++;
		}
		if (Input.GetKeyDown(KeyCode.PageDown))
		{
			distanceFromPresent--;
			if (distanceFromPresent < 0)
			{
				distanceFromPresent = 0;
			}
		}
		if (GameSparksManager.Instance != null && GameSparksManager.Instance.Connected && GameSparksManager.Instance.MainUserIsAdmin && Input.GetKeyDown(KeyCode.F8) && Input.GetKey(KeyCode.LeftShift))
		{
			GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
			gameSparksQuery.SendSimpleRequest("SetNextLoginPermission", new Dictionary<string, object> { { "overridePermissionLevel", 0 } }, returnScriptData: true);
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
			{
				if (q.HasError)
				{
					Debug.LogError("Could not set temp permission level. " + q.Error);
					UserMessageManager.Instance.UserMessage("Could not set temp permission level.", 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				}
				else
				{
					UserMessageManager.Instance.UserMessage("Permission level will be 0 next login.", 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				}
			});
		}
		int num = 0;
		if ((bool)LobbyManager.instance && (bool)LobbyManager.instance.PlayerTracker)
		{
			foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
			{
				if (allGameNetID == 0)
				{
					continue;
				}
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
				if (gameObject != null)
				{
					GamePlayer component = gameObject.GetComponent<GamePlayer>();
					if (component.CharacterInstance != null && component.CharacterInstance.Enabled && !component.CharacterInstance.Dead)
					{
						num++;
					}
				}
			}
		}
		AkSoundEngine.SetRTPCValue("NumberOfActiveCharacters", num);
		if (WordFilter.PlatformHasWordFilter)
		{
			WordFilter.ProcessBatchedWordFilterQueue();
		}
	}

	public void OnEnable()
	{
		Application.logMessageReceived += HandleOnscreenLog;
	}

	public void OnDisable()
	{
		Application.logMessageReceived -= HandleOnscreenLog;
	}

	private void OnDestroy()
	{
		CleanupBeforeQuit();
	}

	public void CleanupBeforeQuit()
	{
		SendEndAnalytics();
		if (!SteamManager.Destroyed && !SteamManager.DestroyedByEditor && SteamManager.EverInitialized && SteamManager.Initialized)
		{
			if (Matchmaker.IsInstantiated)
			{
				Matchmaker.Instance.LeaveLobby();
			}
			SteamManager.PostDestroy();
		}
		wasDestroyed = true;
	}

	private void AttachDeviceToController(InputDevice d)
	{
		foreach (Controller controller in Controllers)
		{
			InControlController inControlController = controller as InControlController;
			InputDevice inputDevice = inControlController.GetInputDevice();
			if (inputDevice != null && inputDevice == d)
			{
				Debug.Log("Controller with input mask " + inControlController.GetControlMask() + " and device " + d.Name + " Re-Plugged in");
				inControlController.SetInputDevice(d);
				return;
			}
		}
		foreach (Controller controller2 in Controllers)
		{
			InControlController inControlController2 = controller2 as InControlController;
			InputDevice inputDevice2 = inControlController2.GetInputDevice();
			if (inputDevice2 != null && !inputDevice2.IsAttached && inputDevice2.Name == d.Name)
			{
				Debug.Log("Couldn't find matching existing device: " + d.Name + ". Assigning to controller missing device with input mask " + inControlController2.GetControlMask());
				inControlController2.SetInputDevice(d);
				return;
			}
		}
		GameObject obj = new GameObject(d.Name, typeof(X360Controller));
		obj.transform.parent = base.transform;
		Controller component = obj.GetComponent<Controller>();
		Controllers.Add(component);
		((InControlController)component).SetInputDevice(d);
		CheatListener cheatListener = UnityEngine.Object.Instantiate(Resources.Load<CheatListener>("CheaterTemplate"));
		cheatListener.name = "Cheater";
		cheatListener.transform.parent = component.transform;
		component.AddReceiver(cheatListener);
		Debug.Log("New device " + d.Name + " plugged in with input mask " + component.GetControlMask());
	}

	private void RemoveDeviceFromController(InputDevice d)
	{
		Debug.Log(d.Name + " Unplugged" + d.IsAttached);
	}

	public void ResetPieceCount()
	{
		foreach (string item in new List<string>(pieceCount.Keys))
		{
			pieceCount[item] = 0;
		}
	}

	public void DecrementPieceCount(string name)
	{
		if (pieceCount.ContainsKey(name))
		{
			pieceCount[name]--;
		}
	}

	public void IncrementPieceCount(string name)
	{
		if (pieceCount.ContainsKey(name))
		{
			pieceCount[name]++;
		}
	}

	public void SetPieceCount(string name, int value)
	{
		if (pieceCount.ContainsKey(name))
		{
			pieceCount[name] = value;
		}
	}

	public int GetPieceCount(string name)
	{
		if (pieceCount.ContainsKey(name))
		{
			return pieceCount[name];
		}
		return 0;
	}

	public void SendStartAnalytics()
	{
		AnalyticEvent.GameStartEvent(LocalizationManager.CurrentLanguage);
	}

	public void SendEndAnalytics()
	{
		AnalyticEvent.GameQuitEvent(Time.time);
	}

	private void SetGuiStyle()
	{
		debugStyle.fontSize = Textheight;
		debugStyleBg.fontSize = Textheight;
		debugStyle.normal.textColor = Color.white;
		debugStyleBg.normal.textColor = Color.black;
		betaMessage.fontSize = 15;
		betaMessage.normal.textColor = new Color(1f, 1f, 1f, 0.3f);
		betaMessage.alignment = TextAnchor.LowerLeft;
		betaMessage.font = GameSettings.GetInstance().onlineBetaMessageFont;
		debugExtraDataStyle.alignment = TextAnchor.MiddleCenter;
		debugExtraDataStyle.normal.textColor = Color.black;
	}

	private void SetGuiStyleOpacity(float opacity)
	{
		debugStyle.normal.textColor = new Color(debugStyle.normal.textColor.r, debugStyle.normal.textColor.g, debugStyle.normal.textColor.b, opacity);
		debugStyleBg.normal.textColor = new Color(debugStyleBg.normal.textColor.r, debugStyleBg.normal.textColor.g, debugStyleBg.normal.textColor.b, opacity);
	}

	protected void DrawDebugText(string text)
	{
		DrawDebugText(text, Color.white, Color.white);
	}

	protected void DrawDebugText(string text, Color color, Color bg)
	{
		int textheight = Textheight;
		if (debugTextRow > opacityCutoffHeight)
		{
			SetGuiStyleOpacity(0f);
		}
		else
		{
			SetGuiStyleOpacity(((float)opacityCutoffHeight - (float)debugTextRow) / (float)opacityCutoffHeight);
		}
		GUI.Label(new Rect(2f, Screen.height - textheight * debugTextRow, 1024f, textheight), text, debugStyleBg);
		GUI.Label(new Rect(2f, Screen.height - textheight * debugTextRow + 2, 1024f, textheight), text, debugStyleBg);
		GUI.Label(new Rect(0f, Screen.height - textheight * debugTextRow, 1024f, textheight), text, debugStyleBg);
		GUI.Label(new Rect(0f, Screen.height - textheight * debugTextRow + 2, 1024f, textheight), text, debugStyleBg);
		GUI.Label(new Rect(0f, Screen.height - textheight * debugTextRow, 1024f, textheight), text, debugStyle);
		debugTextRow--;
	}

	protected virtual void DrawDebug()
	{
		int num = debugStringList.Count - 1 - distanceFromPresent;
		if (num < 0)
		{
			num = 0;
		}
		if (num > debugStringList.Count - 1)
		{
			num = debugStringList.Count - 1;
		}
		int num2 = num - numberOfTextRows;
		if (num2 < 0)
		{
			num2 = 0;
		}
		List<string> range = debugStringList.GetRange(num2, num - num2 + 1);
		debugTextRow = range.Count;
		foreach (string item in range)
		{
			DrawDebugText(item);
		}
	}

	public void OnGUI()
	{
		if (OnlineDebugMode)
		{
			numberOfTextRows = Screen.height / 2 / Textheight;
			debugTextRow = debugStringList.Count;
			DrawDebug();
		}
		if (ControllerMonitor.Instance.IsMainControllerSet && PlayerManager.GetInstance().FirstUserLoggedIn && !StatTracker.Instance.GetSaveFileDataForMainUser().HideVersion)
		{
			GUI.Label(versionRect, GameSettings.GetInstance().VersionNumber, betaMessage);
		}
	}

	private static void writeOnline(string newline)
	{
		debugStringList.Add(newline);
	}

	public void DrawExtraDataDebug(string text, Vector2 screenPosition, Color color)
	{
		debugExtraDataStyle.normal.textColor = color;
		GUI.Label(new Rect(screenPosition.x, (float)Screen.height - screenPosition.y, 50f, 50f), text, debugExtraDataStyle);
	}

	private void HandleOnscreenLog(string logString, string stackTrace, LogType type)
	{
		for (int i = 0; i != onscreenPrefixes.Length; i++)
		{
			if (logString.StartsWith(onscreenPrefixes[i]))
			{
				writeOnline(logString);
			}
		}
	}

	public void OnApplicationFocus(bool focus)
	{
		if (!Application.isEditor)
		{
			if (focus)
			{
				UnityEngine.Cursor.visible = false;
				UnityEngine.Cursor.lockState = CursorLockMode.Confined;
				WwiseSuspender.UnmuteAudio();
			}
			else
			{
				UnityEngine.Cursor.visible = true;
				UnityEngine.Cursor.lockState = CursorLockMode.None;
				WwiseSuspender.MuteAudio();
			}
		}
	}

	private IEnumerator OpenRedditUrlInASecond()
	{
		yield return new WaitForSeconds(2f);
		OpenURLWrapper.Open(ScriptLocalization.RedditLevelShareMegaThread);
	}

	public static void OnSceneChange()
	{
		if (ChatSystem != null)
		{
			ChatSystem.UnshiftPosition();
		}
	}

	public static string GetLocalizedGameModeName(GameMode gameMode)
	{
		return gameMode switch
		{
			GameMode.FREEPLAY => ScriptLocalization.RuleBook.FreePlayText, 
			GameMode.CREATIVE => ScriptLocalization.RuleBook.CreativeModeText, 
			GameMode.PARTY => ScriptLocalization.RuleBook.PartyModeText, 
			GameMode.CHALLENGE => ScriptLocalization.RuleBook.ChallengeModeText, 
			_ => null, 
		};
	}
}
