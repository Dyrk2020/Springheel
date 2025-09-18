using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using MLAPI.Relay.Transports;
using Steamworks;
using Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LevelSelectController : NetworkBehaviour, InputReceiver, IGameEventListener
{
	public enum Status
	{
		INACTIVE,
		CURSOR,
		CHARACTER,
		READY,
		COUCH
	}

	public struct PlayedSnapshotInfo
	{
		public GameState.LevelName nextLevel;

		public string snapshotName;

		public string snapshotCode;

		public FeaturedQuickFilter.LevelTypes snapshotType;

		public string authorID;

		public LobbyPlayer.SocialPlatform authorPlatform;

		public string authorPlatformID;

		public string authorDisplayName;
	}

	public int MaxPlayers;

	public int MinPlayers;

	public float InitialCameraHeight;

	public float CameraHeight;

	public playerJoinIndicator[] PlayerJoinIndicators = new playerJoinIndicator[4];

	public LobbyCursor CursorPrefab;

	public Transform[] CursorSpawnPoint;

	public List<LobbyStartPoint> StartingPoints = new List<LobbyStartPoint>();

	public HotseatMessage HotseatPlayerMessage;

	public HotSeat HotSeatCouch;

	public float HotSeatMessageTime;

	public ButtonSlide PartyModeButton;

	public TreehouseGrow TreehouseGrower;

	public LevelUnlockCounter levelUnlockCounter;

	public Transform[] TreehouseGeneralArt;

	[SyncVar]
	private int treehouseState;

	[SyncVar]
	private int unlockedCharacters;

	[SyncVar]
	public bool HostIsLoaded;

	public Collider2D CameraBounds;

	public Collider2D CameraboundsWithCredits;

	public Collider2D CursorBounds;

	public ZoomCamera MainCamera;

	public Camera UICamera;

	public LobbyPlayer[] JoinedPlayers = new LobbyPlayer[4];

	public LevelPortal[] portals;

	public CustomLevelPortal[] snapshotPortals;

	public static CustomLevelPortal.SnapshotInfo[] snapshotPortalInfo;

	public static int lastLobbyRulesetIdx;

	public static GameRulePreset lastLobbyRulesetCopy;

	public Transform[] UndergroundCharacterPosition;

	public GameObject magicSmoke;

	private LoadingInterstitialSplash FadeOut;

	public Animator CountDown;

	public countDownStart CountDownStart;

	private UnLockInfo[] CharacterUnlocks;

	private UnLockInfo[] LevelUnlocks;

	private UnLockInfo[] OutfitUnlocks;

	public OutfitManager outfitManager;

	public GameState.LevelName UnlockInLevel;

	protected bool levelChosen;

	protected bool readyStarted;

	protected bool castingVotes;

	protected bool votingDone;

	protected List<LobbyPlayer> readyCountList = new List<LobbyPlayer>();

	public float ForceStartTime;

	public float ForceStartWarnTime;

	private float forceStartTimer;

	private int forcingStart;

	public float readyTime;

	protected float readyTimer;

	public float flashingTime;

	public float arrowRemovalTime;

	public Dictionary<VoteArrow, LevelPortal> ArrowsInContention = new Dictionary<VoteArrow, LevelPortal>();

	public float countDownCoolDown;

	protected float countDownCoolDownTimer;

	private bool shuttingDown;

	public Collider2D StartViewBounds;

	public InventoryBook GameRuleBookPrefab;

	public InventoryBook GameRuleBook;

	public ModsDisplayController modsDisplayControllerPrefab;

	public UndergroundComputer undergroundComputer;

	public TreeHouseRenderControl treehouseRenderControl;

	private Dictionary<uint, GameState.LevelName> unlockQuestionMarks = new Dictionary<uint, GameState.LevelName>();

	private HashSet<Controller> controllersRequestingJoinIn = new HashSet<Controller>();

	private bool transitioningToMainMenu;

	private bool requestTransitionToMainMenu;

	private bool freeFormCamEnabled;

	public static LevelSelectController lastInstance;

	private bool waitingForCouchPlayerCreation;

	private bool GameRuleBookInitialized;

	private Action onRuleBookInitialized;

	private bool connectedToMasterRelay;

	private static int kRpcRpcLockVotes;

	private static int kRpcRpcTurnOffArrow;

	private static int kRpcRpcRemoveCameraTargetLevelPortal;

	private static int kRpcRpcAddCameraTargetLevelPortal;

	private static int kRpcRpcClearCameraTransformTargets;

	private static int kRpcRpcMagicSmokePoof;

	private static int kCmdCmdCreateCursorForPlayer;

	private static int kRpcRpcResetCharacter;

	private static int kRpcRpcPlayerPickedCharacter;

	private static int kRpcRpcStartCountDown;

	private static int kRpcRpcCountDownHide;

	private static int kRpcRpcRemoveStartView;

	private static int kRpcRpcAddStartView;

	private static int kRpcRpcRemovePlayer;

	private static int kCmdCmdSetTreehouseGrowState;

	private static int kRpcRpcSetTreeHouseGrowState;

	private static int kRpcRpcPlayMusic;

	private static int kRpcRpcPlaySound;

	private static int kRpcRpcSetGameMode;

	private static int kRpcRpcSetNextLevel;

	public bool LevelAboutToStart => castingVotes;

	public float TimeInTreehouse { get; protected set; }

	private int playersLeft
	{
		get
		{
			int num = 0;
			for (int i = 0; i != JoinedPlayers.Length; i++)
			{
				if (JoinedPlayers[i] == null || JoinedPlayers[i].PlayerStatus == LobbyPlayer.Status.INACTIVE)
				{
					num++;
				}
			}
			return num;
		}
	}

	private bool AnyPlayerInMenuNotProp
	{
		get
		{
			NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
				if (lobbyPlayer != null && lobbyPlayer.CharacterInstance != null && lobbyPlayer.CharacterInstance.InMenu)
				{
					UsableProp useableProp = lobbyPlayer.CharacterInstance.GetUseableProp();
					if (!(useableProp != null) || !(useableProp is OutfitChangeProp) || !(useableProp.characterUsing == lobbyPlayer.CharacterInstance))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public int NetworktreehouseState
	{
		get
		{
			return treehouseState;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref treehouseState, 1u);
		}
	}

	public int NetworkunlockedCharacters
	{
		get
		{
			return unlockedCharacters;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref unlockedCharacters, 2u);
		}
	}

	public bool NetworkHostIsLoaded
	{
		get
		{
			return HostIsLoaded;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref HostIsLoaded, 4u);
		}
	}

	private void Awake()
	{
		int cchNameBufferSize = 256;
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Steam is not initialized!");
			return;
		}
		string pchName;
		bool currentBetaName = SteamApps.GetCurrentBetaName(out pchName, cchNameBufferSize);
		if (currentBetaName)
		{
			Debug.Log("Player is on the beta branch: " + pchName);
		}
		else
		{
			Debug.Log("Player is on the default (public) branch.");
		}
		if (currentBetaName)
		{
			List<LobbyStartPoint> startPointsToLockOut = new List<LobbyStartPoint>();
			foreach (LobbyStartPoint startingPoint in StartingPoints)
			{
				foreach (SteamLockoutStrings steamLockOutString in startingPoint.SteamLockOutStrings)
				{
					if (string.Compare(steamLockOutString.nameOfBranchToLockOut, pchName) == 0)
					{
						startPointsToLockOut.Add(startingPoint);
						Debug.Log(startingPoint.AssociatedCharacter.ToString() + " is being disabled by " + pchName);
						startingPoint.gameObject.SetActive(value: false);
					}
				}
			}
			StartingPoints.RemoveAll((LobbyStartPoint x) => startPointsToLockOut.Contains(x));
		}
		FadeOut = LoadingInterstitialSplash.Instance;
		if (FadeOut != null)
		{
			FadeOut.FadeOutAutomatically = false;
		}
		if (PickableButton.maskAll)
		{
			PickableButton.ResetMasks();
		}
		lastInstance = this;
		if (snapshotPortalInfo == null)
		{
			snapshotPortalInfo = new CustomLevelPortal.SnapshotInfo[snapshotPortals.Length];
		}
		OutfitUnlocks = new UnLockInfo[UnlockInfoLibrary.Instance.AllOutfitUnlocks.Length];
		UnlockInfoLibrary.Instance.AllOutfitUnlocks.CopyTo(OutfitUnlocks, 0);
		CharacterUnlocks = new UnLockInfo[UnlockInfoLibrary.Instance.AllCharacterUnlocks.Length];
		UnlockInfoLibrary.Instance.AllCharacterUnlocks.CopyTo(CharacterUnlocks, 0);
		LevelUnlocks = new UnLockInfo[UnlockInfoLibrary.Instance.AllLevelUnlocks.Length];
		UnlockInfoLibrary.Instance.AllLevelUnlocks.CopyTo(LevelUnlocks, 0);
		GetFreeFormCamEnabled();
	}

	private void OnServerAddressReport(IPEndPoint ipEndPoint)
	{
		connectedToMasterRelay = true;
	}

	private IEnumerator WaitForInitialization()
	{
		yield return null;
		if (!GameSettings.GetInstance().StartLocal && NetworkManager.activeTransport is UnetRelayTransport { IsServer: not false, Enabled: not false, RemoteEndpointReported: false })
		{
			((UnetRelayTransport)NetworkManager.activeTransport).OnRemoteEndpointReported += OnServerAddressReport;
			WaitForSeconds waiter = new WaitForSeconds(1f);
			for (int i = 0; i < 10; i++)
			{
				yield return waiter;
				if (connectedToMasterRelay)
				{
					break;
				}
			}
			((UnetRelayTransport)NetworkManager.activeTransport).OnRemoteEndpointReported -= OnServerAddressReport;
			if (!connectedToMasterRelay)
			{
				LobbyManagerManager.Instance.AbortGameInProgress(LocalizationManager.GetTermTranslation("Network/FailedToConnectUnet"));
				yield break;
			}
		}
		NetworkPlayerTracker playerTracker = LobbyManager.instance.PlayerTracker;
		if (playerTracker.WaitingForLobbyPlayerInit)
		{
			Debug.Log("LevelSelectController: Waiting for player init...");
			yield return null;
			while (playerTracker.WaitingForLobbyPlayerInit)
			{
				yield return null;
			}
		}
		LobbyPlayer firstLobbyPlayer = GetFirstLocalLobbyPlayer();
		if (firstLobbyPlayer == null)
		{
			Debug.Log("LevelSelectController: Waiting for first local lobby player...");
			yield return null;
			while (firstLobbyPlayer == null)
			{
				yield return null;
				firstLobbyPlayer = GetFirstLocalLobbyPlayer();
			}
		}
		GameSettings.GetInstance().OnTreehouseStart();
		if (base.hasAuthority)
		{
			setupLobby();
			yield return null;
			SpawnRuleBook();
			ExecuteOnRuleBookInitialized(delegate
			{
				GameRuleBook.ShowingOnHost = true;
				SetupLobbyAfterWait();
				GameEventManager.SendEvent(new NetworkHostLobbyLoadedEvent());
				NetworkHostIsLoaded = true;
			});
		}
		else
		{
			float timeWaited = 0f;
			if (!HostIsLoaded)
			{
				Debug.Log("Waiting for Host to Initialize");
			}
			while (!HostIsLoaded)
			{
				yield return null;
				timeWaited += Time.unscaledDeltaTime;
				if (timeWaited > 15f)
				{
					Debug.LogError("[Net] Waited more than 15 seconds for host to become ready.");
					Debug.LogError("[Net] If you see this message, you may have found a bug! Please send your output log to support@clevendeav.com!");
					Debug.LogError("[Net] Instructions available at: http://www.cleverendeavourgames.com/outputlog (or google \"UCH outputlog\")");
					NetworkHostIsLoaded = true;
					break;
				}
			}
			setupLobby();
			yield return null;
			SpawnRuleBook();
			ExecuteOnRuleBookInitialized(delegate
			{
				GameRuleBook.ShowingOnHost = false;
				MsgClientLoadedTreehouse msg = new MsgClientLoadedTreehouse
				{
					NetworkPlayerNumber = firstLobbyPlayer.networkNumber
				};
				LobbyManager.instance.client.Send(NetMsgTypes.ClientLoadedTreehouse, msg);
				SetupLobbyAfterWait();
			});
		}
		while (!GameRuleBookInitialized)
		{
			yield return null;
		}
		yield return WaitForStableFramerate();
		FadeOut.FadeOut();
	}

	public static IEnumerator WaitForStableFramerate()
	{
		int curIdx = -1;
		float[] lastFrames = new float[3] { 0.5f, 0.5f, 0.5f };
		float timer = 0f;
		while (timer < 3f)
		{
			curIdx = (curIdx + 1) % lastFrames.Length;
			lastFrames[curIdx] = Time.unscaledDeltaTime;
			timer += Time.unscaledDeltaTime;
			bool flag = false;
			for (int i = 0; i < lastFrames.Length; i++)
			{
				if (lastFrames[i] > 0.05f)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private void SpawnRuleBook()
	{
		PickableRuleButton.levelSelectController = this;
		StartCoroutine(DoSpawnRuleBook());
	}

	private IEnumerator DoSpawnRuleBook()
	{
		int delay = 0;
		while (delay > 0)
		{
			delay--;
			yield return null;
		}
		GameRuleBook = UnityEngine.Object.Instantiate(GameRuleBookPrefab);
		undergroundComputer = GameRuleBook.SecondScreenPage.GetComponent<UndergroundComputer>();
		undergroundComputer.slotPortals = snapshotPortals;
		GameRuleBook.UiCamera = UICamera;
		GameRuleBook.transform.SetParent(UICamera.transform, worldPositionStays: false);
		GameRuleProp.GameRuleBook = GameRuleBook;
		UICamera.gameObject.AddPrefabAsChild<ModsDisplayController>(modsDisplayControllerPrefab);
		undergroundComputer.Initialize();
		yield return null;
		undergroundComputer.OnSelectLevelCodesTab();
		yield return null;
		treehouseRenderControl.EnableOverlay();
		yield return null;
		GameRuleBookInitialized = true;
		if (onRuleBookInitialized != null)
		{
			onRuleBookInitialized();
		}
	}

	private void Start()
	{
		ChangeListener(adding: true);
		LobbyManager.instance.CurrentLevelSelectController = this;
		if (Matchmaker.CurrentMatchmakingLobby.IsValid())
		{
			LobbyManager.instance.AllLocal = false;
		}
		else
		{
			LobbyManager.instance.AllLocal = true;
		}
		GameState.GetInstance().PreservePlayers = false;
		GameState.GetInstance().Paused = false;
		LobbyManager.instance.reloadingScene = false;
		portals = UnityEngine.Object.FindObjectsOfType<LevelPortal>();
		LevelPortal[] array = portals;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].levelSelectController = this;
		}
		AkSoundEngine.PostEvent("SFX_Lobby_Challenge_Muffle ", base.gameObject);
		StartCoroutine(WaitForInitialization());
	}

	private void SetupLobbyAfterWait()
	{
		GameState instance = GameState.GetInstance();
		StatTracker.Instance.SaveGameForAllUsers();
		if (base.hasAuthority)
		{
			CallCmdSetTreehouseGrowState(StatTracker.Instance.TreehouseLevel);
		}
		else
		{
			TreehouseGrower.SetNewState(treehouseState);
		}
		RestoreCustomPortalInfo();
		MainCamera.AddTarget(StartViewBounds);
		bool flag = false;
		bool flag2 = false;
		Character.Animals[] associatedCharacters;
		foreach (Controller controller in instance.Controllers)
		{
			associatedCharacters = controller.GetAssociatedCharacters();
			flag2 = false;
			for (int num = 3; num >= 0; num--)
			{
				if (associatedCharacters[num] != Character.Animals.NONE)
				{
					flag = true;
					flag2 = true;
				}
			}
			if (!flag2 && !flag && controller.IsAssumingUser())
			{
				flag = true;
			}
			controller.AssumeUser(assume: false);
		}
		associatedCharacters = instance.Keyboard.GetAssociatedCharacters();
		flag2 = false;
		for (int num2 = 3; num2 >= 0; num2--)
		{
			if (associatedCharacters[num2] != Character.Animals.NONE)
			{
				flag = true;
				flag2 = true;
			}
		}
		if (!flag2 && !flag && instance.Keyboard.IsAssumingUser())
		{
			flag = true;
		}
		instance.Keyboard.AssumeUser(assume: false);
		int num3 = 0;
		MainCamera.ForceShowAllPlayer(showAll: true);
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
			if (componentInChildren.Picked && !componentInChildren.Sitting)
			{
				MainCamera.AddTarget(componentInChildren);
				num3++;
				componentInChildren.SetLobbyCollider(enable: true);
			}
		}
		MainCamera.ForceShowAllPlayer(showAll: false);
		MainCamera.AddTarget(StartViewBounds);
		if (num3 > 0)
		{
			MainCamera.SetFrameSizes(CameraHeight);
		}
		if (base.hasAuthority)
		{
			Matchmaker.Instance.CurrentLobby.SetLobbyVisible(visible: true);
			Matchmaker.Instance.CurrentLobby.SetMatchProgress(0);
			if (GameSettings.GetInstance().WasUsingCustomRules)
			{
				GameSettings.GetInstance().SetAllDefaults();
			}
			GameSettings.GetInstance().ApplySaveFileOverrides();
		}
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null && lobbyPlayer.IsLocalPlayer)
			{
				if (lobbyPlayer.EmoteSystem != null)
				{
					lobbyPlayer.EmoteSystem.SceneCamera = MainCamera.GetComponent<Camera>();
					lobbyPlayer.EmoteSystem.UICamera = UICamera;
					lobbyPlayer.EmoteSystem.SetEmoteContext(EmoteContext.CONTEXT_LOBBY);
					continue;
				}
				Debug.LogError("Lobby Player " + lobbyPlayer.networkNumber + " (" + lobbyPlayer.playerName + ") has no EmoteSystem");
			}
		}
		outfitManager.RebuildDatabase();
		for (int j = 0; j != LobbyManager.instance.lobbySlots.Length; j++)
		{
			LobbyPlayer lobbyPlayer2 = (LobbyPlayer)LobbyManager.instance.lobbySlots[j];
			if (lobbyPlayer2 != null)
			{
				JoinedPlayers[j] = lobbyPlayer2;
			}
		}
		try
		{
			checkForAvailableUnlocks();
		}
		catch (Exception ex)
		{
			Debug.LogError("Error while checking available unlocks: " + ex.Message + "\n" + ex.StackTrace);
		}
		if (Modifiers.GetInstance().IsNonDefault || GameSettings.GetInstance().HaveNonDefaultRules)
		{
			Modifiers.GetInstance().OnModifiersDynamicChange();
			GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
		}
	}

	private void GetFreeFormCamEnabled()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-freeCam")
			{
				freeFormCamEnabled = true;
			}
		}
	}

	private void Update()
	{
		TimeInTreehouse += Time.unscaledDeltaTime;
		if (!HostIsLoaded)
		{
			return;
		}
		if (requestTransitionToMainMenu)
		{
			TransitionToMainMenu();
		}
		if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
		{
			SaveSystemProtector.Protect();
		}
		if (freeFormCamEnabled && !GameState.DebugMode && Input.GetKeyDown(KeyCode.P))
		{
			UICamera.enabled = !UICamera.enabled;
		}
		if (GameState.DebugMode)
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				UICamera.enabled = !UICamera.enabled;
			}
			if (Input.GetKeyUp(KeyCode.Home))
			{
				SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
				StatCount stat = saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed");
				StatCount stat2 = saveFileDataForMainUser.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked");
				StatCount stat3 = saveFileDataForMainUser.GetStat<StatCount>("GamesSinceLastLevelUnlocked");
				if (stat.count < 3)
				{
					stat.Set(3);
				}
				if (stat2.count < 5)
				{
					stat2.Set(5);
				}
				if (stat3.count < 5)
				{
					stat3.Set(5);
				}
				checkForAvailableUnlocks();
			}
		}
		if (levelChosen)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (!(lobbyPlayer == null))
			{
				num2++;
				if (lobbyPlayer.PlayerStatus != LobbyPlayer.Status.CURSOR && lobbyPlayer.PlayerStatus != LobbyPlayer.Status.INACTIVE)
				{
					num++;
				}
			}
		}
		if (MainCamera.HasTarget(StartViewBounds) && num == num2)
		{
			MainCamera.RemoveTarget(StartViewBounds);
		}
		else if (!MainCamera.HasTarget(StartViewBounds) && num < num2)
		{
			MainCamera.AddTarget(StartViewBounds);
		}
		if (!base.hasAuthority)
		{
			return;
		}
		if (Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.CurrentMatchmakingLobby is GamesparksMatchmakingLobby gamesparksMatchmakingLobby)
		{
			bool flag = LobbyPlayer.LocalMachinePlatform == LobbyPlayer.SocialPlatform.PSN;
			if (!flag)
			{
				foreach (LobbyPlayer lobbyPlayer4 in LobbyManager.instance.GetLobbyPlayers())
				{
					if (lobbyPlayer4.platform == LobbyPlayer.SocialPlatform.PSN)
					{
						flag = true;
						break;
					}
				}
			}
			gamesparksMatchmakingLobby.PSNTainted = flag;
			bool pSNHidden = false;
			foreach (LobbyPlayer lobbyPlayer5 in LobbyManager.instance.GetLobbyPlayers())
			{
				if (lobbyPlayer5.platform != LobbyPlayer.SocialPlatform.PSN && lobbyPlayer5.platform != LobbyPlayer.SocialPlatform.Steam && lobbyPlayer5.platform != LobbyPlayer.SocialPlatform.Origin)
				{
					pSNHidden = true;
				}
				if (lobbyPlayer5.IsLocalPlayer)
				{
					float num3 = GameSettings.GetInstance().AFKLobbyFilterTime;
					if ((lobbyPlayer5.CharacterInstance != null && lobbyPlayer5.CharacterInstance.Enabled && lobbyPlayer5.CharacterInstance.TimeSpentAFK >= num3) || (lobbyPlayer5.CursorInstance != null && lobbyPlayer5.CursorInstance.Enabled && lobbyPlayer5.CursorInstance.TimeSpentAFK >= num3))
					{
						gamesparksMatchmakingLobby.SetHostIsAFK(isAFK: true);
					}
					else
					{
						gamesparksMatchmakingLobby.SetHostIsAFK(isAFK: false);
					}
				}
			}
			gamesparksMatchmakingLobby.PSNHidden = pSNHidden;
			gamesparksMatchmakingLobby.DisallowCrossplay = !GameSettings.GetInstance().CrossPlatformToggle;
			bool isNonDefault = Modifiers.GetInstance().IsNonDefault;
			if (isNonDefault != gamesparksMatchmakingLobby.usingMods)
			{
				gamesparksMatchmakingLobby.SetLobbyUsingMods(isNonDefault);
			}
		}
		int num4 = 0;
		for (int j = 0; j != JoinedPlayers.Length; j++)
		{
			if (JoinedPlayers[j] != null && JoinedPlayers[j].PlayerStatus >= LobbyPlayer.Status.CHARACTER && JoinedPlayers[j].CharacterInstance != null && !JoinedPlayers[j].CharacterInstance.InMenu)
			{
				num4++;
			}
		}
		int num5 = 0;
		lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			if ((LobbyPlayer)lobbySlots[i] != null)
			{
				num5++;
			}
		}
		int seatsTaken = HotSeatCouch.GetSeatsTaken();
		int num6 = num5 - seatsTaken;
		bool flag2 = num4 == num6;
		if (!readyStarted)
		{
			if (base.hasAuthority)
			{
				forcingStart = 0;
				lobbySlots = LobbyManager.instance.lobbySlots;
				for (int i = 0; i < lobbySlots.Length; i++)
				{
					LobbyPlayer lobbyPlayer2 = (LobbyPlayer)lobbySlots[i];
					if (!(lobbyPlayer2 == null) && lobbyPlayer2.Initialized && lobbyPlayer2.IsLocalPlayer && lobbyPlayer2.PlayerStatus == LobbyPlayer.Status.READY)
					{
						forcingStart++;
					}
				}
				if (forcingStart == 0)
				{
					forceStartTimer = 0f;
				}
			}
			if (LobbyManager.instance.IsInOnlineGame && forcingStart > 0 && num4 >= MinPlayers && flag2)
			{
				forceStartTimer += Time.deltaTime;
				if (forceStartTimer >= ForceStartWarnTime && !CountDownStart.Visible)
				{
					int countFrom = (int)ForceStartTime - Mathf.FloorToInt(forceStartTimer) + 3;
					CallRpcStartCountDown(countFrom, countDownStart.TimerMessage.HOSTFORCE);
				}
			}
			else
			{
				forceStartTimer = 0f;
				if (CountDownStart.Visible)
				{
					CallRpcCountDownHide();
					MsgLobbyVoting msgLobbyVoting = new MsgLobbyVoting();
					msgLobbyVoting.VoteStarted = false;
					NetworkServer.SendToAll(NetMsgTypes.LobbyVoting, msgLobbyVoting);
				}
			}
		}
		if (countDownCoolDownTimer > 0f)
		{
			countDownCoolDownTimer -= Time.deltaTime;
		}
		else
		{
			countDownCoolDownTimer = 0f;
		}
		if ((countDownCoolDownTimer <= 0f && num5 >= MinPlayers && ((readyCountList.Count == num6 && num4 >= MinPlayers) || (forceStartTimer >= ForceStartTime && num4 >= MinPlayers && flag2))) || castingVotes)
		{
			if (GameSettings.GetInstance().AvailableBlocks == 0)
			{
				return;
			}
			if (!readyStarted)
			{
				readyStarted = true;
				LevelPortal[] array = portals;
				foreach (LevelPortal levelPortal in array)
				{
					foreach (VoteArrow value in levelPortal.Votes.Values)
					{
						if (!ArrowsInContention.ContainsKey(value) && value.ChrPresent)
						{
							ArrowsInContention.Add(value, levelPortal);
						}
						if (value.ChrPresent)
						{
							levelPortal.StartCountDown();
						}
						else
						{
							levelPortal.LowlightPortal();
						}
						value.lightState = VoteArrow.LightState.FLASHING;
					}
				}
				LevelPortal levelPortal2 = null;
				bool flag3 = false;
				foreach (VoteArrow key in ArrowsInContention.Keys)
				{
					if (levelPortal2 == null)
					{
						levelPortal2 = ArrowsInContention[key];
					}
					else if (levelPortal2 != ArrowsInContention[key])
					{
						flag3 = true;
					}
				}
				Matchmaker.Instance.CurrentLobby.SetMatchProgress(100);
				CallRpcStartCountDown(3, flag3 ? countDownStart.TimerMessage.VOTING : countDownStart.TimerMessage.STARTING);
				MsgLobbyVoting msgLobbyVoting2 = new MsgLobbyVoting();
				msgLobbyVoting2.VoteStarted = true;
				NetworkServer.SendToAll(NetMsgTypes.LobbyVoting, msgLobbyVoting2);
				Debug.Log("Buttons all Pressed");
			}
			readyTimer += Time.deltaTime;
			if (readyTimer > readyTime && !castingVotes)
			{
				Debug.Log("Starting Vote");
				castingVotes = true;
				CallRpcPlaySound("UI_Lobby_Level_StartingVote");
				Matchmaker.Instance.CurrentLobby.SetLobbyVisible(visible: false);
				Matchmaker.Instance.CurrentLobby.SetMatchProgress(100);
				lobbySlots = LobbyManager.instance.lobbySlots;
				for (int i = 0; i < lobbySlots.Length; i++)
				{
					LobbyPlayer lobbyPlayer3 = (LobbyPlayer)lobbySlots[i];
					if (!(lobbyPlayer3 == null))
					{
						if (lobbyPlayer3.CharacterInstance != null)
						{
							lobbyPlayer3.CharacterInstance.CallRpcSetReady(ready: true);
						}
						else
						{
							lobbyPlayer3.RemovePlayer();
						}
					}
				}
				MainCamera.ClearTransformTargets();
				CallRpcClearCameraTransformTargets();
				LevelPortal levelPortal3 = null;
				bool flag4 = false;
				foreach (VoteArrow key2 in ArrowsInContention.Keys)
				{
					key2.VoteLocked = true;
					MainCamera.AddTarget(key2.levelPortal.GetComponent<Collider2D>());
					CallRpcAddCameraTargetLevelPortal(key2.levelPortal.PortalID);
					if (levelPortal3 == null)
					{
						levelPortal3 = ArrowsInContention[key2];
					}
					else if (levelPortal3 != ArrowsInContention[key2])
					{
						flag4 = true;
					}
				}
				CallRpcLockVotes();
				if (flag4)
				{
					StartCoroutine(FlashLights());
					StartCoroutine(RemoveArrows());
				}
				else
				{
					votingDone = true;
				}
			}
			if (!votingDone)
			{
				return;
			}
			LevelPortal lp = null;
			using (Dictionary<VoteArrow, LevelPortal>.Enumerator enumerator4 = ArrowsInContention.GetEnumerator())
			{
				if (enumerator4.MoveNext())
				{
					lp = enumerator4.Current.Value;
				}
			}
			LaunchLevel(lp);
		}
		else
		{
			CancelCountdownProcess();
			readyTimer = 0f;
		}
	}

	public void CancelCountdownProcess()
	{
		if (!readyStarted)
		{
			return;
		}
		countDownCoolDownTimer = countDownCoolDown;
		readyStarted = false;
		castingVotes = false;
		votingDone = false;
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null)
			{
				lobbyPlayer.NetworkLockedForLoad = false;
			}
		}
		LevelPortal[] array = portals;
		foreach (LevelPortal levelPortal in array)
		{
			foreach (VoteArrow value in levelPortal.Votes.Values)
			{
				value.TempDisabled = false;
				value.lightState = VoteArrow.LightState.OFF;
				value.VoteLocked = false;
			}
			levelPortal.ExitCountDown();
		}
		for (int j = 0; j != JoinedPlayers.Length; j++)
		{
			if (JoinedPlayers[j] == null)
			{
				continue;
			}
			LobbyPlayer.Status playerStatus = JoinedPlayers[j].PlayerStatus;
			if (playerStatus != LobbyPlayer.Status.INACTIVE && playerStatus != LobbyPlayer.Status.COUCH && playerStatus != LobbyPlayer.Status.CURSOR)
			{
				Player player = PlayerManager.GetInstance().GetPlayer(j + 1);
				if (player != null && player.PlayerCharacter != null)
				{
					player.PlayerCharacter.Ready = false;
				}
			}
		}
		ArrowsInContention.Clear();
		Matchmaker.Instance.CurrentLobby.SetMatchProgress(0);
		CallRpcCountDownHide();
		MsgLobbyVoting msgLobbyVoting = new MsgLobbyVoting();
		msgLobbyVoting.VoteStarted = false;
		NetworkServer.SendToAll(NetMsgTypes.LobbyVoting, msgLobbyVoting);
	}

	public void LaunchLevel(LevelPortal lp)
	{
		GameState instance = GameState.GetInstance();
		int num = 0;
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			if (!((LobbyPlayer)lobbySlots[i] == null))
			{
				num++;
			}
		}
		int seatsTaken = HotSeatCouch.GetSeatsTaken();
		AkSoundEngine.PostEvent("UI_Lobby_Level_Selected", base.gameObject);
		AkSoundEngine.PostEvent("SFX_Lobby_Stop", base.gameObject);
		List<LobbyPlayer> list = new List<LobbyPlayer>();
		lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (!(lobbyPlayer == null))
			{
				if (lobbyPlayer.CharacterInstance != null)
				{
					lobbyPlayer.CharacterInstance.Disable(moveAway: false);
				}
				if (lobbyPlayer.CursorInstance != null)
				{
					lobbyPlayer.CursorInstance.Disable(sound: false);
				}
				if (!(lobbyPlayer == null) && base.hasAuthority && lobbyPlayer.PickedAnimal == Character.Animals.NONE)
				{
					list.Add(lobbyPlayer);
				}
			}
		}
		if (base.hasAuthority)
		{
			foreach (LobbyPlayer item in list)
			{
				LobbyManager.instance.RemoveLobbyPlayer(item);
				LobbyManagerManager.Instance.Update();
			}
		}
		string text = null;
		CustomLevelPortal customLevelPortal = lp as CustomLevelPortal;
		Guid guid = Guid.NewGuid();
		if (base.hasAuthority)
		{
			Guid guid2 = guid;
			Debug.Log("Generating new MatchGUID: " + guid2.ToString());
			if (Matchmaker.CurrentMatchmakingLobby != null)
			{
				Matchmaker.CurrentMatchmakingLobby.SetMatchGuid(guid);
			}
		}
		if (AnalyticsWrapper.EnabledOnPlatform && base.hasAuthority)
		{
			if (base.hasAuthority && customLevelPortal != null && customLevelPortal.snapshotInfo != null)
			{
				text = customLevelPortal.snapshotInfo.code;
			}
			LobbyTags lobbyTag = LobbyTags.Fun;
			if (Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.CurrentMatchmakingLobby.IsValid())
			{
				lobbyTag = Matchmaker.CurrentMatchmakingLobby.GetLobbyTag();
			}
			int num2 = 0;
			AnalyticEvent.MatchStartHostEvent(numPlayers: (!Matchmaker.CurrentMatchmakingLobby.IsValid()) ? PlayerManager.GetInstance().NumPlayers : Matchmaker.CurrentMatchmakingLobby.GetPlayerCount(), matchGuid: guid, online: Matchmaker.CurrentMatchmakingLobby.IsValid(), level: lp.TargetLevel, gameMode: GameSettings.GetInstance().GameMode, levelCode: text, lobbyTag: lobbyTag, twitchIntegration: GameSettings.GetInstance().enableTwitchVoting);
			GameRulePreset gameRulePreset = GameSettings.GetInstance().GetCurrentRuleset();
			bool flag = false;
			if (gameRulePreset == null)
			{
				gameRulePreset = GameRulePreset.GetRulesetFromCurrentRules();
				flag = gameRulePreset != null;
			}
			AnalyticEvent.MatchRulesEvent(guid, gameRulePreset);
			AnalyticEvent.MatchPointsEvent(guid, gameRulePreset);
			AnalyticEvent.MatchBlocksEvent(guid, gameRulePreset);
			AnalyticEvent.MatchModifiersEvent(guid, Modifiers.GetInstance());
			if (flag)
			{
				UnityEngine.Object.Destroy(gameRulePreset);
				gameRulePreset = null;
			}
		}
		if (!text.NullOrEmpty())
		{
			GameSparksManager.Instance.CreateQuery().NotifySnapshotPlayed(text);
			Debug.Log("Playing snapshot code " + GameSparksQuery.GetFormattedSnapshotCode(text));
		}
		if (!lp.snapshotXml.NullOrEmpty())
		{
			QuickSaver.levelPortalXml = lp.snapshotXml;
		}
		else
		{
			QuickSaver.levelPortalXml = null;
		}
		if (base.hasAuthority)
		{
			for (int j = 0; j < snapshotPortals.Length; j++)
			{
				snapshotPortalInfo[j] = snapshotPortals[j].snapshotInfo;
			}
			StatTracker.Instance.GetSaveFileDataForMainUser().SetPortalInfo(snapshotPortalInfo);
			MemorizeLastLobbyPreset();
		}
		foreach (Controller controller in instance.Controllers)
		{
			controller.RemoveReceiver(this);
		}
		instance.Keyboard.RemoveReceiver(this);
		CallRpcSetGameMode(GameSettings.GetInstance().GameMode);
		PlayedSnapshotInfo nextLevelInfo = new PlayedSnapshotInfo
		{
			snapshotType = FeaturedQuickFilter.LevelTypes.Any,
			nextLevel = lp.TargetLevel
		};
		if (customLevelPortal != null && customLevelPortal.snapshotInfo != null)
		{
			nextLevelInfo.snapshotName = customLevelPortal.snapshotInfo.snapshotName;
			nextLevelInfo.snapshotCode = customLevelPortal.snapshotInfo.code;
			nextLevelInfo.snapshotType = customLevelPortal.snapshotInfo.levelType;
			if (customLevelPortal.snapshotInfo.authorInfo != null)
			{
				CustomLevelPortal.AuthorInfo authorInfo = customLevelPortal.snapshotInfo.authorInfo;
				nextLevelInfo.authorID = authorInfo.GSID;
				nextLevelInfo.authorDisplayName = authorInfo.displayName;
				nextLevelInfo.authorPlatform = authorInfo.platform;
				nextLevelInfo.authorPlatformID = authorInfo.platformID;
			}
		}
		Placeable.SetInitialSequenceID(0);
		Placeable.AllPlaceables.Clear();
		Teleporter.AllTeleporters.Clear();
		AnimalCannon.AllAnimalCannon.Clear();
		GameState.LevelName levelName = lp.TargetLevel;
		string text2 = GameState.GetLevelSceneName(lp.TargetLevel);
		if (lp.TargetLevel == GameState.LevelName.RANDOM)
		{
			text2 = "";
			SaveFileData mainUserSaveFileData = StatTracker.Instance.mainUserSaveFileData;
			if (mainUserSaveFileData != null)
			{
				StatBoolArray stat = mainUserSaveFileData.GetStat<StatBoolArray>("LevelsUnlocked");
				GameState.LevelName[] array = (GameState.LevelName[])Enum.GetValues(typeof(GameState.LevelName));
				int num3 = 0;
				int num4 = 0;
				int num5 = 50;
				while (text2.NullOrEmpty() && num4 < num5)
				{
					num3 = UnityEngine.Random.Range(0, array.Length);
					levelName = array[num3];
					if (levelName != GameState.LevelName.BLANKLEVEL && levelName < GameState.LevelName.RANDOM && stat.values[(int)levelName])
					{
						text2 = GameState.GetLevelSceneName(levelName);
					}
					num4++;
				}
				if (text2.NullOrEmpty())
				{
					Debug.LogWarning("Random level selection failed or timed out. Falling back to default level.");
					levelName = GameState.LevelName.FARM;
					text2 = GameState.GetLevelSceneName(levelName);
				}
			}
			else
			{
				levelName = ((UnityEngine.Random.Range(0, 2) != 0) ? GameState.LevelName.ROOFTOPS : GameState.LevelName.FARM);
				text2 = GameState.GetLevelSceneName(levelName);
			}
		}
		nextLevelInfo.nextLevel = levelName;
		CallRpcSetNextLevel(nextLevelInfo);
		CallRpcPlayMusic(GameState.GetLevelMusString(levelName));
		CallRpcPlayMusic(GameState.GetLevelAmbienceString(levelName));
		StartCoroutine(FadeToLevel(text2, local: false));
		levelChosen = true;
	}

	private IEnumerator FlashLights()
	{
		LevelPortal[] array = portals;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].LowlightPortal();
		}
		int tick = 0;
		while (castingVotes)
		{
			foreach (VoteArrow key in ArrowsInContention.Keys)
			{
				key.levelPortal.neutralLightPortal();
			}
			int num = 0;
			foreach (VoteArrow key2 in ArrowsInContention.Keys)
			{
				if (tick % ArrowsInContention.Count == num)
				{
					key2.lightState = VoteArrow.LightState.SOLID;
					key2.levelPortal.highLightPortal();
				}
				else
				{
					key2.lightState = VoteArrow.LightState.OFF;
				}
				num++;
			}
			tick++;
			if (ArrowsInContention.Count > 1)
			{
				CallRpcPlaySound("UI_Lobby_Level_LightsFlash");
			}
			yield return new WaitForSeconds(flashingTime);
			if (!castingVotes)
			{
				break;
			}
		}
	}

	private IEnumerator RemoveArrows()
	{
		do
		{
			yield return new WaitForSeconds(arrowRemovalTime);
			if (!castingVotes)
			{
				yield break;
			}
			VoteArrow maybeRemove = null;
			int num = UnityEngine.Random.Range(0, ArrowsInContention.Count);
			int num2 = 0;
			foreach (VoteArrow key in ArrowsInContention.Keys)
			{
				if (num2 == num)
				{
					maybeRemove = key;
				}
				num2++;
			}
			CallRpcPlaySound("UI_Lobby_Level_RemoveVoteArrow");
			ArrowsInContention.Remove(maybeRemove);
			bool flag = true;
			foreach (LevelPortal value in ArrowsInContention.Values)
			{
				if (value == maybeRemove.levelPortal)
				{
					flag = false;
				}
			}
			maybeRemove.TempDisabled = true;
			maybeRemove.VoteLocked = false;
			maybeRemove.lightState = VoteArrow.LightState.OFF;
			CallRpcTurnOffArrow(maybeRemove.levelPortal.PortalID);
			if (flag)
			{
				maybeRemove.levelPortal.LowlightPortal();
				yield return new WaitForSeconds(1f);
				MainCamera.RemoveTarget(maybeRemove.levelPortal.GetComponent<Collider2D>());
				CallRpcRemoveCameraTargetLevelPortal(maybeRemove.levelPortal.PortalID);
			}
		}
		while (ArrowsInContention.Count > 1);
		Debug.Log("arrows all remove one remaining");
		CallRpcPlaySound("UI_Lobby_Level_VotingDone");
		yield return new WaitForSeconds(arrowRemovalTime);
		if (castingVotes)
		{
			votingDone = true;
		}
	}

	[ClientRpc]
	private void RpcLockVotes()
	{
		if (!base.hasAuthority)
		{
			LevelPortal[] array = portals;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].LockVotes();
			}
		}
	}

	[ClientRpc]
	private void RpcTurnOffArrow(GameState.PortalID targetPortalID)
	{
		if (base.hasAuthority)
		{
			return;
		}
		LevelPortal[] array = portals;
		foreach (LevelPortal levelPortal in array)
		{
			if (levelPortal.PortalID == targetPortalID)
			{
				levelPortal.turnOffAnArrow();
				break;
			}
		}
	}

	[ClientRpc]
	private void RpcRemoveCameraTargetLevelPortal(GameState.PortalID targetPortalID)
	{
		if (base.hasAuthority)
		{
			return;
		}
		LevelPortal[] array = portals;
		foreach (LevelPortal levelPortal in array)
		{
			if (levelPortal.PortalID == targetPortalID)
			{
				MainCamera.RemoveTarget(levelPortal.GetComponent<Collider2D>());
				break;
			}
		}
	}

	[ClientRpc]
	private void RpcAddCameraTargetLevelPortal(GameState.PortalID targetPortalID)
	{
		if (base.hasAuthority)
		{
			return;
		}
		LevelPortal[] array = portals;
		foreach (LevelPortal levelPortal in array)
		{
			if (levelPortal.PortalID == targetPortalID)
			{
				MainCamera.AddTarget(levelPortal.GetComponent<Collider2D>());
				break;
			}
		}
	}

	[ClientRpc]
	private void RpcClearCameraTransformTargets()
	{
		if (!base.hasAuthority)
		{
			MainCamera.ClearTransformTargets();
		}
	}

	[ClientRpc]
	private void RpcMagicSmokePoof(Vector3 position, Color color, int layer)
	{
		SpriteRenderer componentInChildren = UnityEngine.Object.Instantiate(magicSmoke, position, Quaternion.identity).GetComponentInChildren<SpriteRenderer>();
		componentInChildren.color = color;
		componentInChildren.gameObject.layer = layer;
		AkSoundEngine.PostEvent("UI_Lobby_Cursor_Disappear_Poof", base.gameObject);
	}

	private void setClientCharacterUnlocks()
	{
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
			if ((unlockedCharacters & (1 << (int)startingPoint.AssociatedCharacter)) > 0)
			{
				if (componentInChildren != null)
				{
					componentInChildren.Enable();
				}
			}
			else if (componentInChildren != null)
			{
				componentInChildren.Disable(moveAway: false);
			}
		}
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (levelChosen || castingVotes)
		{
			return;
		}
		if (Controller.FullScreenComputerIsActive)
		{
			if ((e.Key != InputEvent.InputKey.Start || !e.Valueb || !e.Changed) && (e.Key != InputEvent.InputKey.Accept || !e.Valueb || !e.Changed))
			{
				return;
			}
			Character.Animals[] associatedCharacters = e.Sender.GetAssociatedCharacters();
			foreach (Character.Animals animals in associatedCharacters)
			{
				if (animals == Character.Animals.NONE)
				{
					continue;
				}
				NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
				foreach (NetworkLobbyPlayer networkLobbyPlayer in lobbySlots)
				{
					if (!(networkLobbyPlayer == null))
					{
						LobbyPlayer lobbyPlayer = networkLobbyPlayer as LobbyPlayer;
						if (lobbyPlayer.IsLocalPlayer && lobbyPlayer.CharacterInstance != null && !lobbyPlayer.CharacterInstance.InMenu && (lobbyPlayer.PlayerStatus == LobbyPlayer.Status.CHARACTER || lobbyPlayer.PlayerStatus == LobbyPlayer.Status.READY) && animals == lobbyPlayer.CharacterInstance.CharacterSprite)
						{
							GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: true, lobbyPlayer.networkNumber));
							return;
						}
					}
				}
			}
			return;
		}
		int controlMask = e.Sender.GetControlMask();
		if ((e.Sender.IsKeyboard && Controller.InputFieldWasActiveRecently) || (e.Key != InputEvent.InputKey.Start && e.Key != InputEvent.InputKey.Accept && e.Key != InputEvent.InputKey.Esc && e.Key != InputEvent.InputKey.Scoreboard) || !e.Valueb || !e.Changed)
		{
			return;
		}
		for (int k = 0; k != JoinedPlayers.Length; k++)
		{
			LobbyPlayer lobbyPlayer2 = JoinedPlayers[k];
			if ((lobbyPlayer2 == null || lobbyPlayer2.PlayerStatus == LobbyPlayer.Status.INACTIVE) && controlMask == 0 && e.Key != InputEvent.InputKey.Scoreboard && canAddPlayerForController(e.Sender))
			{
				TryAddLocalPlayer(e.Sender);
				break;
			}
			if (controlMask <= 0 || !(lobbyPlayer2 != null) || !lobbyPlayer2.IsLocalPlayer || !e.Sender.ControlsPlayer(lobbyPlayer2.localNumber))
			{
				continue;
			}
			Character playerCharacter = PlayerManager.GetInstance().GetPlayer(lobbyPlayer2.localNumber).PlayerCharacter;
			if (e.Key == InputEvent.InputKey.Accept && lobbyPlayer2.PlayerStatus == LobbyPlayer.Status.CHARACTER && HotSeatCouch.CharacterAtCouch(playerCharacter) && !playerCharacter.InMenu)
			{
				if (!HotSeatCouch.IsSeatAvailable() || playersLeft <= 0 || waitingForCouchPlayerCreation)
				{
					continue;
				}
				waitingForCouchPlayerCreation = true;
				Player player = PlayerManager.GetInstance().GetPlayer(k + 1);
				HotSeatCouch.SitPlayer(player);
				GameSettings.GetInstance().GameMode = GameState.PreviousMode(GameState.GameMode.CREATIVE);
				PartyModeButton.SimulatePress();
				Debug.Log("Using Shared controller Couch locked Party mode Button");
				PartyModeButton.Lock();
				GameState.GetInstance().UsingHotSeat = true;
				PlayerJoinIndicators[k].ReadyEnabled();
				lobbyPlayer2.PlayerStatus = LobbyPlayer.Status.COUCH;
				for (int l = 0; l != JoinedPlayers.Length; l++)
				{
					if (JoinedPlayers[l] == null || JoinedPlayers[l].PlayerStatus == LobbyPlayer.Status.INACTIVE)
					{
						PlayerManager.GetInstance().AddPlayer(e.Sender).HotseatPlayer = true;
						e.Sender.AddPlayer(l + 1);
						break;
					}
				}
			}
			else
			{
				if (!GameRuleBookInitialized || (e.Key != InputEvent.InputKey.Start && e.Key != InputEvent.InputKey.Esc && e.Key != InputEvent.InputKey.Scoreboard) || GameRuleBook.FrozenOnPage || HotSeatCouch.PlayerSitting(lobbyPlayer2.LocalPlayer))
				{
					continue;
				}
				PickCursor cursor = GameRuleBook.GetCursor(lobbyPlayer2.networkNumber);
				if (cursor != null)
				{
					GameRuleBook.RemovePlayer(lobbyPlayer2.networkNumber, e.Sender);
					cursor.Freeze();
					cursor.Disable();
					GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: false, lobbyPlayer2.networkNumber, BookSoundEffect: false));
					break;
				}
				if (LobbyManager.instance != null && LobbyManager.instance.HasPlayersLockedForLoad)
				{
					Debug.Log("No rule book for you -- we are locked for load!!");
					break;
				}
				bool anyPlayerInMenuNotProp = AnyPlayerInMenuNotProp;
				if (lobbyPlayer2.CharacterInstance == null || !lobbyPlayer2.CharacterInstance.InMenu)
				{
					GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: true, lobbyPlayer2.networkNumber, BookSoundEffect: false));
				}
				if (!Controller.InputFieldIsActive && !anyPlayerInMenuNotProp && GameRuleBook.ActiveCursors == 1)
				{
					GameRuleBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.TabletInterface);
					_ = e.Key;
					_ = 20;
				}
				if (lobbyPlayer2.CharacterInstance != null && lobbyPlayer2.CharacterInstance.InMenu)
				{
					UsableProp useableProp = lobbyPlayer2.CharacterInstance.GetUseableProp();
					if (useableProp != null)
					{
						useableProp.Release();
					}
				}
				break;
			}
		}
	}

	public void OpenLegacyRulebook(bool gotoRules)
	{
		if (gotoRules)
		{
			GameRuleBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.RulePage);
		}
		else
		{
			GameRuleBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.TableOfContents);
		}
	}

	private bool canAddPlayerForController(Controller controller)
	{
		return true;
	}

	private void TryAddLocalPlayer(Controller sender)
	{
		if (controllersRequestingJoinIn.Contains(sender))
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < LobbyManager.instance.lobbySlots.Length; i++)
		{
			if (LobbyManager.instance.lobbySlots[i] == null)
			{
				PlayerJoinIndicators[i].localJoining();
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Debug.Log("Cannot add local player: Lobby is full.");
			return;
		}
		controllersRequestingJoinIn.Add(sender);
		AddLocalPlayer(sender);
	}

	private void AddCouchPlayer2MPSDCustomFields(LobbyPlayer player)
	{
		_ = (UnityMatchmaker)Matchmaker.Instance;
		StringBuilder stringBuilder = new StringBuilder(player.GSID);
		if (!player.MainUser)
		{
			stringBuilder.Append("-");
			stringBuilder.Append(player.networkNumber);
		}
		Debug.Log("Adding Player Couch Mode Final");
	}

	private void revertJoinSlot(int slotNumber)
	{
		if (slotNumber != -1 && PlayerJoinIndicators[slotNumber] != null)
		{
			PlayerJoinIndicators[slotNumber].PressEnabled();
		}
	}

	private void AddLocalPlayer(Controller sender)
	{
		Player player = PlayerManager.GetInstance().AddPlayer(sender);
		sender.AddPlayer(player.Number);
		StartCoroutine(GetAssosiatedLobbyPlayers(player));
		controllersRequestingJoinIn.Remove(sender);
	}

	private IEnumerator GetAssosiatedLobbyPlayers(Player p)
	{
		yield return new WaitForSeconds(1f);
		if (p.AssociatedLobbyPlayer != null)
		{
			Debug.Log("Adding Player Couch Mode\t" + p.AssociatedLobbyPlayer.playerName + "GSID\t" + p.AssociatedLobbyPlayer.GSID + "STEAMID\t" + p.AssociatedLobbyPlayer.SteamID);
			AddCouchPlayer2MPSDCustomFields(p.AssociatedLobbyPlayer);
		}
		else
		{
			Debug.Log("p.AssociatedLobbyPlayer is NULL");
		}
	}

	private void setupLobby()
	{
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer == null)
			{
				continue;
			}
			if (lobbyPlayer.IsLocalPlayer)
			{
				if (lobbyPlayer.LocalPlayer.LoggedOut)
				{
					lobbyPlayer.RemovePlayer();
					continue;
				}
				setupController(lobbyPlayer);
			}
			else
			{
				lobbyPlayer.FindLobbyObjects();
				UpdatePlayerIndicatorToPlayerState(lobbyPlayer);
			}
			if (!GameState.GetInstance().currentSnapshotInfo.snapshotName.NullOrEmpty() || GameState.GetInstance().lastLevelPlayed == GameState.GetLevelSceneName(GameState.LevelName.BLANKLEVEL))
			{
				foreach (LobbyStartPoint startingPoint in StartingPoints)
				{
					if (startingPoint.AssociatedCharacter == lobbyPlayer.PickedAnimal)
					{
						Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
						if (componentInChildren != null && lobbyPlayer.PlayerStatus != LobbyPlayer.Status.COUCH)
						{
							componentInChildren.PositionCharacter(UndergroundCharacterPosition[lobbyPlayer.networkNumber - 1].position, groundScaleOffset: true);
						}
					}
				}
			}
			if (base.hasAuthority)
			{
				LobbyManager.instance.PlayerTracker.RemoveGamePlayer(lobbyPlayer.networkNumber);
			}
		}
		if (base.hasAuthority)
		{
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			foreach (LobbyStartPoint startingPoint2 in StartingPoints)
			{
				if (!saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked").values[(int)startingPoint2.AssociatedCharacter])
				{
					Character componentInChildren2 = startingPoint2.GetComponentInChildren<Character>();
					if (componentInChildren2 != null && !componentInChildren2.Picked)
					{
						componentInChildren2.Disable(moveAway: false);
					}
				}
				else
				{
					NetworkunlockedCharacters = unlockedCharacters | (1 << (int)startingPoint2.AssociatedCharacter);
				}
			}
		}
		else
		{
			setClientCharacterUnlocks();
		}
		Controller.AddGlobalReceiver(this);
		MainCamera.SetBounds(CameraBounds.bounds);
		if (base.hasAuthority)
		{
			MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
			msgSwitchToMode.toMode = GameSettings.GetInstance().GameMode;
			NetworkServer.SendToAll(NetMsgTypes.SwitchToMode, msgSwitchToMode);
			if (GameSettings.GetInstance().LockPartyButton)
			{
				PartyModeButton.Lock();
				Debug.Log("Setup Lobby Locked the party button.");
			}
			else
			{
				PartyModeButton.Unlock();
			}
			if (lastLobbyRulesetIdx != -1)
			{
				GameSettings.GetInstance().ToPreset(lastLobbyRulesetIdx);
				lastLobbyRulesetIdx = -1;
			}
			else if (lastLobbyRulesetCopy != null)
			{
				GameSettings.GetInstance().ApplyTemporaryRuleset(lastLobbyRulesetCopy, loadRules: true, loadPoints: true, loadBlocks: true, loadModifiers: true);
				ClearLastLobbyRulesetCopy();
			}
		}
		GameEventManager.SendEvent(new GameModeSetEvent(GameSettings.GetInstance().GameMode));
		levelUnlockCounter.CountLevels();
	}

	private void RestoreCustomPortalInfo()
	{
		if (!base.hasAuthority)
		{
			return;
		}
		List<SaveFileData.PortalSnapshotEntry> portalSnapshotEntries = StatTracker.Instance.GetSaveFileDataForMainUser().portalSnapshotEntries;
		for (int i = 0; i < snapshotPortals.Length; i++)
		{
			CustomLevelPortal.SnapshotInfo snapshotInfo = snapshotPortalInfo[i];
			SaveFileData.PortalSnapshotEntry savedSnapshotInfo = ((i < portalSnapshotEntries.Count) ? portalSnapshotEntries[i] : null);
			if (savedSnapshotInfo == null || savedSnapshotInfo.name.NullOrEmpty())
			{
				continue;
			}
			bool flag = true;
			if (snapshotInfo == null)
			{
				flag = false;
			}
			else
			{
				bool flag2 = false;
				if (savedSnapshotInfo.code.NullOrEmpty() != snapshotInfo.code.NullOrEmpty())
				{
					flag = false;
				}
				else if (savedSnapshotInfo.code.NullOrEmpty())
				{
					flag2 = true;
				}
				if (!flag2 && savedSnapshotInfo.code != snapshotInfo.code)
				{
					flag = false;
				}
				if (flag && savedSnapshotInfo.name != snapshotInfo.snapshotName)
				{
					flag = false;
				}
			}
			if (flag)
			{
				ApplyCustomPortalSnapshotInfo(i, snapshotInfo);
			}
			else if (savedSnapshotInfo.code.NullOrEmpty())
			{
				int portalIdx = i;
				QuickSaver.FindLocalSaveFilenameWithoutExt(savedSnapshotInfo.name, delegate(string localSaveFilenameWithoutExt)
				{
					snapshotPortals[portalIdx].NetworkisLoading = true;
					Action<XmlDocument> OnDocumentLoaded = delegate(XmlDocument xmlDocument)
					{
						if (xmlDocument != null)
						{
							GameState.LevelName levelNameEnumFromSceneName = GetLevelNameEnumFromSceneName(QuickSaver.ParseAttrStr(xmlDocument.DocumentElement, "levelSceneName"));
							CustomLevelPortal.SnapshotInfo snapshotInfo2 = new CustomLevelPortal.SnapshotInfo
							{
								snapshotName = savedSnapshotInfo.name,
								targetLevel = levelNameEnumFromSceneName,
								xml = xmlDocument.OuterXml,
								levelType = QuickSaver.InferLevelTypeFromFilename(localSaveFilenameWithoutExt)
							};
							ApplyCustomPortalSnapshotInfo(portalIdx, snapshotInfo2);
						}
						snapshotPortals[portalIdx].NetworkisLoading = false;
					};
					string fullpath = QuickSaver.LocalSavesFolder + "/" + localSaveFilenameWithoutExt + ".snapshot";
					if (RamFS.PlatformUsesRamFS)
					{
						RamFS.AddReadFileOperation(fullpath, delegate(RamFS.FSOperationReturnCode returnCode, byte[] fileContents)
						{
							if (returnCode == RamFS.FSOperationReturnCode.OK)
							{
								XmlDocument doc2 = null;
								WorkerThreadManager.Instance.AddFileOpJob(delegate
								{
									doc2 = QuickSaver.GetXmlDocFromBytes(fileContents);
								}, delegate
								{
									OnDocumentLoaded(doc2);
								});
							}
							else
							{
								OnDocumentLoaded(null);
							}
						});
					}
					else
					{
						XmlDocument doc = null;
						WorkerThreadManager.Instance.AddFileOpJob(delegate
						{
							doc = QuickSaver.TryLoadSnapshotXMLFromPath(fullpath);
						}, delegate
						{
							OnDocumentLoaded(doc);
						});
					}
				});
			}
			else
			{
				snapshotPortals[i].NetworkisLoading = true;
				GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
				gameSparksQuery.GetXmlStringFromSnapshotCode(savedSnapshotInfo.code, incrementGetCount: false);
				int portalIdx2 = i;
				gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery response)
				{
					if (!response.HasError)
					{
						if (response.ResultData.ContainsKey("archived") && (bool)response.ResultData["archived"])
						{
							Debug.LogError("Ignored archived code while restoring custom portals");
							snapshotPortals[portalIdx2].NetworkisLoading = false;
						}
						else
						{
							string name = response.ResultData["name"] as string;
							byte[] xmlBytes = response.ResultData["bytes"] as byte[];
							FeaturedQuickFilter.LevelTypes levelType = (FeaturedQuickFilter.LevelTypes)response.ResultData["levelType"];
							GSData authorInfo = response.ResultData["authorInfo"] as GSData;
							XmlDocument doc = null;
							WorkerThreadManager.Instance.AddFileOpJob(delegate
							{
								doc = QuickSaver.GetXmlDocFromBytes(xmlBytes);
							}, delegate
							{
								if (doc != null)
								{
									GameState.LevelName levelNameEnumFromSceneName = GetLevelNameEnumFromSceneName(QuickSaver.ParseAttrStr(doc.DocumentElement, "levelSceneName"));
									CustomLevelPortal.SnapshotInfo snapshotInfo2 = new CustomLevelPortal.SnapshotInfo
									{
										snapshotName = name,
										targetLevel = levelNameEnumFromSceneName,
										xml = doc.OuterXml,
										code = savedSnapshotInfo.code,
										levelType = levelType
									};
									if (authorInfo != null)
									{
										string authorID = authorInfo.GetString("playerID");
										string authorDisplayName = authorInfo.GetString("authorDisplayName");
										GSData gSData = authorInfo.GetGSData("authorPlatformIds");
										snapshotInfo2.authorInfo = new CustomLevelPortal.AuthorInfo(authorID, authorDisplayName, gSData);
									}
									ApplyCustomPortalSnapshotInfo(portalIdx2, snapshotInfo2);
								}
								snapshotPortals[portalIdx2].NetworkisLoading = false;
							});
						}
					}
					else
					{
						snapshotPortals[portalIdx2].NetworkisLoading = false;
						Debug.LogError(response.Error);
					}
				});
			}
			snapshotPortalInfo[i] = null;
		}
	}

	private void setupController(LobbyPlayer lobbyPl)
	{
		GameState instance = GameState.GetInstance();
		Player localPlayer = lobbyPl.LocalPlayer;
		Character.Animals[] associatedCharacters = localPlayer.UseController.GetAssociatedCharacters();
		if (!localPlayer.UseController.ControlsPlayer(localPlayer.Number))
		{
			localPlayer.UseController.AddPlayer(localPlayer.Number);
		}
		bool flag = lobbyPl.PlayerStatus == LobbyPlayer.Status.COUCH;
		for (int num = associatedCharacters.Length - 1; num >= 0; num--)
		{
			if (associatedCharacters[num] != Character.Animals.NONE && lobbyPl.PickedAnimal == associatedCharacters[num])
			{
				Debug.Log("Setting up " + associatedCharacters[num]);
				MainCamera.SetFrameSizes(CameraHeight);
				lobbyPl.PlayerStatus = (flag ? LobbyPlayer.Status.COUCH : LobbyPlayer.Status.CHARACTER);
				bool flag2 = false;
				foreach (LobbyStartPoint startingPoint in StartingPoints)
				{
					if (startingPoint.AssociatedCharacter == associatedCharacters[num])
					{
						Character character = (localPlayer.PlayerCharacter = startingPoint.GetComponentInChildren<Character>());
						LobbyCursor lobbyCursor = (LobbyCursor)localPlayer.AssociatedLobbyPlayer.CursorInstance;
						if (lobbyCursor != null)
						{
							lobbyCursor.Picked = character;
							lobbyCursor.UseCamera = MainCamera.GetComponent<Camera>();
							localPlayer.UseController.AddReceiver(lobbyCursor);
						}
						localPlayer.AssociatedLobbyPlayer.CallCmdAssignCharacter(character.netId.Value, lobbyPl.networkNumber, lobbyPl.localNumber, restoreAssignment: true);
						localPlayer.AssociatedLobbyPlayer.DoCharacterPickedEvent(character.CharacterSprite, lobbyPl.CursorInstance.netId, clearOutfit: false);
						character.SetOutfitsFromArray(localPlayer.AssociatedLobbyPlayer.characterOutfitsList);
						flag2 = character.Enabled;
					}
				}
				if (flag2 || flag)
				{
					localPlayer.PlayerCharacter.Enable();
					localPlayer.PlayerCharacter.SetPickedImmediate(picked: true);
					localPlayer.PlayerCharacter.PlayerColor = lobbyPl.PlayerColor;
					PlayerJoinIndicators[lobbyPl.networkNumber - 1].setAnimalName(localPlayer.PlayerCharacter.CharacterSprite, lobbyPl.IsWearingSkin);
					PlayerJoinIndicators[lobbyPl.networkNumber - 1].PickLevelEnabled();
				}
				else
				{
					PlayerJoinIndicators[lobbyPl.networkNumber - 1].ChooseCharacterEnabled();
					lobbyPl.UnpickCharacter();
				}
				PlayerJoinIndicators[lobbyPl.networkNumber - 1].setTintColor(lobbyPl.PlayerColor);
				if (flag)
				{
					HotSeatCouch.SitPlayer(localPlayer);
					instance.UsingHotSeat = true;
					if (!PartyModeButton.Locked)
					{
						PartyModeButton.Lock();
						Debug.Log("Setting up the Contoller Locked the Party Button");
					}
					lobbyPl.PlayerStatus = LobbyPlayer.Status.COUCH;
					PlayerJoinIndicators[lobbyPl.networkNumber - 1].ReadyEnabled();
				}
				else
				{
					flag = true;
				}
			}
		}
	}

	public void TransitionToMainMenu(string abortReason = null)
	{
		requestTransitionToMainMenu = false;
		if (!transitioningToMainMenu && base.gameObject.activeInHierarchy)
		{
			LobbyManagerManager.Instance.SetAbortReason(abortReason);
			StartCoroutine(BackToMainMenuInASecond(abortReason));
		}
		else
		{
			Debug.LogWarning("TransitionToMainMenu: Already transitioning...");
		}
	}

	private IEnumerator BackToMainMenuInASecond(string abortReason = null)
	{
		if (!transitioningToMainMenu)
		{
			Debug.LogWarning("BackToMainMenuInASecond");
			transitioningToMainMenu = true;
			yield return new WaitForSeconds(1f);
			FadeOut.FadeIn();
			while (FadeOut.State != UISplashScreen.STATE.SHOW)
			{
				yield return null;
			}
			GameEventManager.SendEvent(new ClearChatEvent());
			LobbyManagerManager.Instance.AbortGameInProgress(abortReason);
		}
		else
		{
			Debug.LogWarning("Already transitioning to main menu...");
		}
	}

	public void BackToMainMenu(string abortReason = null)
	{
		if (!transitioningToMainMenu)
		{
			transitioningToMainMenu = true;
			LobbyManagerManager.Instance.SetAbortReason(abortReason);
			Debug.LogWarning("BackToMainMenu");
			Controller.RemoveGlobalReceiver(this);
			LoadingInterstitialSplash.Instance.FadeIn();
			if (GetComponent<NetworkIdentity>().isServer)
			{
				StartCoroutine(FadeToLevel("MainMenu", local: true, abortReason));
			}
			else
			{
				StartCoroutine(FadeToLevel("MainMenu", local: false, abortReason));
			}
		}
		else
		{
			Debug.LogWarning("Already transitioning to main menu...");
		}
	}

	protected void CleanupForLoad()
	{
		for (int i = 0; i < TreehouseGeneralArt.Length; i++)
		{
			if (TreehouseGeneralArt[i] != null)
			{
				UnityEngine.Object.Destroy(TreehouseGeneralArt[i].gameObject);
			}
		}
		if (GameRuleBook != null)
		{
			UnityEngine.Object.Destroy(GameRuleBook.gameObject);
		}
	}

	private IEnumerator FadeToLevel(string level, bool local = true, string abortReason = null)
	{
		while (FadeOut.State != UISplashScreen.STATE.SHOW)
		{
			yield return null;
		}
		Debug.Log("Loading next level: " + level);
		CleanupForLoad();
		yield return new WaitForSeconds(0.2f);
		yield return Resources.UnloadUnusedAssets();
		if (!local)
		{
			LobbyManager.instance.playScene = level;
		}
		if (level.Equals("MainMenu"))
		{
			GameState.GetInstance().lastLevelPlayed = "";
			LobbyManagerManager.Instance.AbortGameInProgress(abortReason);
			yield break;
		}
		GameState.GetInstance().lastLevelPlayed = level;
		NetworkLobbyPlayer[] lobbySlots;
		if (local)
		{
			SceneManagerWrapper.LoadScene(level);
			lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
				if (lobbyPlayer != null)
				{
					lobbyPlayer.PlayerStatus = LobbyPlayer.Status.INACTIVE;
				}
			}
			yield break;
		}
		NetworkIdentity component = GetComponent<NetworkIdentity>();
		if (!component || !component.isServer)
		{
			yield break;
		}
		Debug.Log("Server switching scenes");
		lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer2 = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer2 == null)
			{
				continue;
			}
			lobbyPlayer2.CharacterInstance = null;
			if (lobbyPlayer2.CursorInstance != null)
			{
				LobbyCursor component2 = lobbyPlayer2.CursorInstance.GetComponent<LobbyCursor>();
				if (component2 != null)
				{
					component2.InGame = true;
				}
			}
		}
		LobbyManager.instance.ServerChangeScene(level);
	}

	private void checkForAvailableUnlocks()
	{
		_ = StatTracker.Instance;
		GameState.GetInstance().nextUnlocks.Clear();
		GameState.LevelName[] array = (GameState.LevelName[])Enum.GetValues(typeof(GameState.LevelName));
		LobbyPlayer firstLocalLobbyPlayer = GetFirstLocalLobbyPlayer();
		if (firstLocalLobbyPlayer == null)
		{
			Debug.LogError("Could not find a local lobby player!");
			return;
		}
		Dictionary<LobbyPlayer, SaveFileData> dictionary = new Dictionary<LobbyPlayer, SaveFileData>();
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && item.AssociatedLobbyPlayer != null && item.AssociatedLobbyPlayer.IsLocalPlayer)
			{
				SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(item.Number);
				if (saveFileDataForLocalPlayer != null)
				{
					dictionary.Add(item.AssociatedLobbyPlayer, saveFileDataForLocalPlayer);
				}
			}
		}
		foreach (KeyValuePair<LobbyPlayer, SaveFileData> item2 in dictionary)
		{
			LobbyPlayer key = item2.Key;
			SaveFileData value = item2.Value;
			StatCountArray stat = value.GetStat<StatCountArray>("LevelsPlayed");
			StatBoolArray stat2 = value.GetStat<StatBoolArray>("LevelsUnlocked");
			StatBoolArray stat3 = value.GetStat<StatBoolArray>("CharactersUnlocked");
			StatCountArray stat4 = value.GetStat<StatCountArray>("OutfitsUnlocked");
			StatCount stat5 = value.GetStat<StatCount>("GamesSinceLastLevelUnlocked");
			StatCount stat6 = value.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked");
			StatCount stat7 = value.GetStat<StatCount>("GamesPlayed");
			GameState instance = GameState.GetInstance();
			if (stat.values[0] > 0 && stat.values[1] > 0 && !stat2.values[2])
			{
				Debug.Log("Old Mansion ready to unlock");
				instance.nextUnlocks[key] = LevelUnlocks[2];
				LevelPortal levelPortal = portals.FirstOrDefault((LevelPortal p) => !(p is CustomLevelPortal) && p.TargetLevel == GameState.LevelName.ROOFTOPS);
				if (levelPortal != null)
				{
					SendUnlockMessageFromClient(firstLocalLobbyPlayer, levelPortal.TargetLevel);
					break;
				}
			}
			if (stat5.count > 2)
			{
				int i = 1;
				while (i != array.Length - 1)
				{
					if (i != 10)
					{
						if (array[i + 1] >= GameState.LevelName.RANDOM)
						{
							break;
						}
						int num = i + 1;
						if (i == 9)
						{
							num++;
						}
						if (stat.values[i] > 0 && !stat2.values[num] && stat2.values[i])
						{
							LevelPortal levelPortal2 = portals.FirstOrDefault((LevelPortal p) => !(p is CustomLevelPortal) && p.TargetLevel == (GameState.LevelName)i);
							if (levelPortal2 != null)
							{
								GameState.LevelName levelName = (GameState.LevelName)num;
								Debug.Log(levelName.ToString() + " ready to unlock");
								instance.nextUnlocks[key] = LevelUnlocks[num];
								SendUnlockMessageFromClient(firstLocalLobbyPlayer, levelPortal2.TargetLevel);
							}
							return;
						}
					}
					int num2 = i + 1;
					i = num2;
				}
			}
			if (stat6.count >= 3)
			{
				int[][] array2 = new int[11][]
				{
					new int[3] { 2, 6, 0 },
					new int[3] { 4, 5, 1 },
					new int[3] { 6, 7, 2 },
					new int[3] { 7, 8, 3 },
					new int[3] { 11, 9, 4 },
					new int[3] { 13, 10, 5 },
					new int[3] { 14, 11, 6 },
					new int[3] { 19, 12, 7 },
					new int[3] { 21, 13, 8 },
					new int[3] { 22, 14, 9 },
					new int[3] { 23, 15, 10 }
				};
				foreach (int[] array3 in array2)
				{
					if (CheckCharacterUnlockInLevel((GameState.LevelName)array3[0], (Character.Animals)array3[1]))
					{
						instance.nextUnlocks[key] = CharacterUnlocks[array3[2]];
						SendUnlockMessageFromClient(firstLocalLobbyPlayer, (GameState.LevelName)array3[0]);
						return;
					}
				}
			}
			if (stat7.count < 3 || (!(UnityEngine.Random.value < 0.333f) && (!instance.guaranteedUnlocks.ContainsKey(key) || !instance.guaranteedUnlocks[key])))
			{
				continue;
			}
			List<LevelPortal> list = portals.Where((LevelPortal p) => !(p is CustomLevelPortal) && p.gameObject.activeInHierarchy && p.TargetLevel < GameState.LevelName.RANDOM && p.TargetLevel != GameState.LevelName.BLANKLEVEL).ToList();
			int index = UnityEngine.Random.Range(0, list.Count);
			LevelPortal levelPortal3 = list[index];
			object[] outfitUnlocks = OutfitUnlocks;
			Array.Sort(outfitUnlocks, (object a, object b) => (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
			for (int num3 = 0; num3 != OutfitUnlocks.Length; num3++)
			{
				UnLockInfo unLockInfo = OutfitUnlocks[num3];
				if (stat3.values[(int)unLockInfo.AssociatedCharacter] && (stat4.values[(int)unLockInfo.AssociatedCharacter] & unLockInfo.OutfitMaskNumber) == 0)
				{
					Debug.Log("Outfit ready to unlock in " + levelPortal3.TargetLevel);
					instance.nextUnlocks[key] = unLockInfo;
					instance.guaranteedUnlocks[key] = true;
					SendUnlockMessageFromClient(firstLocalLobbyPlayer, levelPortal3.TargetLevel);
					return;
				}
			}
			Debug.Log("All outfits already unlocked!");
		}
	}

	public void GotoCreditCameraMode()
	{
		MainCamera.SetBounds(CameraboundsWithCredits.bounds);
	}

	public void GotoRegularCameraBounds()
	{
		MainCamera.SetBounds(CameraBounds.bounds);
	}

	[Command]
	private void CmdCreateCursorForPlayer(GameObject lobbyPlayerObj, bool showCursor)
	{
		StartCoroutine(createCursorForPlayer(lobbyPlayerObj, showCursor));
	}

	private IEnumerator createCursorForPlayer(GameObject lobbyPlayerObj, bool showCursor)
	{
		LobbyPlayer lobbyPlayer = null;
		if (lobbyPlayerObj == null)
		{
			Debug.LogWarning("Lobby player with netid " + lobbyPlayerObj?.ToString() + " hasn't spawned yet.");
		}
		else
		{
			while (lobbyPlayer == null || lobbyPlayer.localNumber == 0)
			{
				if (lobbyPlayerObj == null)
				{
					yield break;
				}
				lobbyPlayer = lobbyPlayerObj.GetComponent<LobbyPlayer>();
				yield return null;
			}
		}
		Cursor cursor = UnityEngine.Object.Instantiate(CursorPrefab, CursorSpawnPoint[lobbyPlayer.networkNumber - 1].position, Quaternion.identity);
		cursor.NetworknetworkNumber = lobbyPlayer.networkNumber;
		cursor.NetworklocalNumber = lobbyPlayer.localNumber;
		cursor.CursorColor = lobbyPlayer.PlayerColor;
		cursor.SetBounds(CursorBounds);
		cursor.UseCamera = MainCamera.GetComponent<Camera>();
		if (!showCursor)
		{
			cursor.Disable(sound: false);
		}
		else
		{
			lobbyPlayer.PlayerStatus = LobbyPlayer.Status.CURSOR;
		}
		bool spawnSuccess = NetworkServer.SpawnWithClientAuthority(cursor.gameObject, lobbyPlayer.gameObject);
		AkSoundEngine.PostEvent("UI_Lobby_Cursor_Creation_Poof", base.gameObject);
		while (cursor.netId.IsEmpty())
		{
			yield return null;
		}
		lobbyPlayer.CallCmdAssignCursor(cursor.gameObject, lobbyPlayer.networkNumber, lobbyPlayer.localNumber);
		if (spawnSuccess)
		{
			Debug.Log("Spawning lobby cursor");
		}
		else
		{
			Debug.LogError("Lobby Cursor not spawned!");
		}
		if (lobbyPlayer.IsLocalPlayer && lobbyPlayer.LocalPlayer != null && lobbyPlayer.LocalPlayer.UseController != null)
		{
			cursor.SetLocalController(lobbyPlayer.LocalPlayer.UseController);
		}
	}

	private IEnumerator setupLobbyCursor(GameObject lobbyCursorObj)
	{
		LobbyCursor lobbyCursor = null;
		do
		{
			lobbyCursor = lobbyCursorObj.GetComponent<LobbyCursor>();
			yield return null;
		}
		while (lobbyCursor == null || lobbyCursor.AssociatedLobbyPlayer == null);
		lobbyCursor.SetBounds(CursorBounds);
		SpriteRenderer componentInChildren = UnityEngine.Object.Instantiate(magicSmoke, CursorSpawnPoint[lobbyCursor.AssociatedLobbyPlayer.networkNumber - 1].position, Quaternion.identity).GetComponentInChildren<SpriteRenderer>();
		componentInChildren.color = lobbyCursor.AssociatedLobbyPlayer.PlayerColor;
		componentInChildren.gameObject.layer = LayerMask.NameToLayer("LobbyCursors");
		PlayerJoinIndicators[lobbyCursor.networkNumber - 1].setTintColor(lobbyCursor.AssociatedLobbyPlayer.PlayerColor);
		LobbyPlayer.Status playerStatus = lobbyCursor.AssociatedLobbyPlayer.PlayerStatus;
		if (playerStatus == LobbyPlayer.Status.CURSOR || playerStatus == LobbyPlayer.Status.INACTIVE)
		{
			PlayerJoinIndicators[lobbyCursor.networkNumber - 1].ChooseCharacterEnabled();
		}
		lobbyCursor.AssociatedLobbyPlayer.RunAfterInitialized(delegate
		{
			if (lobbyCursor.AssociatedLobbyPlayer.IsLocalPlayer)
			{
				Player player = PlayerManager.GetInstance().GetPlayer(lobbyCursor.localNumber);
				lobbyCursor.LocalPlayer = player;
				lobbyCursor.UseCamera = MainCamera.GetComponent<Camera>();
				player.PlayerCursor = lobbyCursor;
			}
		});
		MainCamera.AddTarget(lobbyCursor);
	}

	public void ResetCharacter(GameObject characterGameObject, GameObject lobbyPlayerGameObject)
	{
		if (!readyStarted && !castingVotes)
		{
			CallRpcResetCharacter(characterGameObject, lobbyPlayerGameObject);
		}
	}

	[ClientRpc]
	private void RpcResetCharacter(GameObject characterObj, GameObject lobbyPlObj)
	{
		Character component = characterObj.GetComponent<Character>();
		StatBoolArray stat = StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatBoolArray>("CharactersUnlocked");
		LobbyStartPoint lobbyStartPoint = null;
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			if (stat.values[(int)startingPoint.AssociatedCharacter] && startingPoint.AssociatedCharacter == component.CharacterSprite)
			{
				lobbyStartPoint = startingPoint;
				break;
			}
		}
		if (lobbyStartPoint != null)
		{
			component.Active = false;
			component.Enable(playSound: false);
			component.PositionCharacter(lobbyStartPoint.transform.position, groundScaleOffset: true);
			component.transform.parent = lobbyStartPoint.transform;
			component.SetPickedImmediate(picked: false);
			component.NetworknetworkNumber = 0;
			component.NetworklocalNumber = 0;
			AkSoundEngine.PostEvent("UI_Lobby_Cursor_Creation_Poof", base.gameObject);
			AkSoundEngine.SetSwitch("Character", component.CharacterSprite.ToString(), base.gameObject);
			AkSoundEngine.PostEvent("UI_Lobby_Character_UnSelected", base.gameObject);
			GameObject obj = UnityEngine.Object.Instantiate(magicSmoke, component.transform.position, Quaternion.identity);
			obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			obj.gameObject.layer = LayerMask.NameToLayer("LobbyCursors");
		}
		else
		{
			component.Disable(moveAway: false);
		}
		AkSoundEngine.PostEvent("UI_Lobby_Character_UnSelected", base.gameObject);
		LobbyPlayer lobbyPlayer = null;
		if (lobbyPlObj != null)
		{
			lobbyPlayer = lobbyPlObj.GetComponent<LobbyPlayer>();
		}
		if (!(lobbyPlayer != null))
		{
			return;
		}
		GameEventManager.SendEvent(new CharacterVoteEvent(isVoting: false, lobbyPlayer.netId, component.netId));
		PlayerJoinIndicators[lobbyPlayer.networkNumber - 1].ChooseCharacterEnabled();
		lobbyPlayer.PlayerStatus = LobbyPlayer.Status.CURSOR;
		if (lobbyPlayer.IsLocalPlayer)
		{
			lobbyPlayer.LocalPlayer.UseController.AssociateCharacter(Character.Animals.NONE, lobbyPlayer.localNumber);
			lobbyPlayer.LocalPlayer.PlayerCharacter = null;
		}
		LobbyCursor lobbyCursor = lobbyPlayer.CursorInstance as LobbyCursor;
		if (lobbyCursor != null)
		{
			lobbyCursor.InGame = false;
			lobbyCursor.Enable();
		}
		if (!lobbyPlayer.IsLocalPlayer)
		{
			if (lobbyPlayer.hasAuthority)
			{
				lobbyPlayer.CallCmdRemoveCharacter();
			}
			if (lobbyCursor != null)
			{
				lobbyCursor.MakeMagicSmoke(lobbyPlayer.CursorInstance.transform, 1f, useCursorColor: true);
			}
		}
		lobbyPlayer.OnCharUnpickedConfirmed();
	}

	private void OnApplicationQuit()
	{
		shuttingDown = true;
	}

	private void OnDestroy()
	{
		lastInstance = null;
		if (!shuttingDown)
		{
			ChangeListener(adding: false);
			Controller.RemoveGlobalReceiver(this);
			if (!GameState.wasDestroyed)
			{
				GameState.GetInstance().Keyboard.RemoveReceiver(this);
				foreach (Controller controller in GameState.GetInstance().Controllers)
				{
					controller.RemoveReceiver(this);
				}
			}
		}
		if (NetworkManager.activeTransport is UnetRelayTransport unetRelayTransport)
		{
			unetRelayTransport.OnRemoteEndpointReported -= OnServerAddressReport;
		}
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<LobbyPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<LocalPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<CharacterPickedEvent>(this, adding);
		GameEventManager.ChangeListener<CharacterVoteEvent>(this, adding);
		GameEventManager.ChangeListener<LobbyPlayerCreatedEvent>(this, adding);
		GameEventManager.ChangeListener<LobbyCursorCreatedEvent>(this, adding);
		GameEventManager.ChangeListener<CheatUnlockEvent>(this, adding);
		GameEventManager.ChangeListener<CheatUnlockHalfEvent>(this, adding);
		GameEventManager.ChangeListener<OneUnlockMaker>(this, adding);
		GameEventManager.ChangeListener<NetworkClientDisconnectEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<GameModeSetEvent>(this, adding);
		GameEventManager.ChangeListener<ResetDataEvent>(this, adding);
		GameEventManager.ChangeListener<DrivingPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<PlatformPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<CheatKonamiEvent>(this, adding);
	}

	public void OnLobbyPlayerObjectDestroyed(LobbyPlayer lobbyPl)
	{
		OnCharacterVote(lobbyPl, voting: false);
		if (!base.hasAuthority)
		{
			return;
		}
		if (lobbyPl.IsLocalPlayer && lobbyPl.LocalPlayer != null && ControllerMonitor.Instance.IsMainControllerSet)
		{
			Controller useController = lobbyPl.LocalPlayer.UseController;
			if (useController != null && ControllerMonitor.Instance.mainController.controller == useController && HotSeatCouch.PlayersWithController(useController) == 0)
			{
				requestTransitionToMainMenu = true;
			}
		}
		if (lobbyPl.CharacterInstance != null)
		{
			NetworkIdentity component = lobbyPl.CharacterInstance.GetComponent<NetworkIdentity>();
			if (component.clientAuthorityOwner != null)
			{
				component.RemoveClientAuthority(lobbyPl.connectionToClient);
			}
			if (base.isServer)
			{
				CallRpcResetCharacter(lobbyPl.CharacterInstance.gameObject, lobbyPl.gameObject);
			}
		}
		if (lobbyPl.CursorInstance != null && base.isServer)
		{
			CallRpcMagicSmokePoof(lobbyPl.CursorInstance.transform.position, lobbyPl.PlayerColor, lobbyPl.CursorInstance.gameObject.layer);
		}
		if (lobbyPl.LocalPlayer == null)
		{
			return;
		}
		Player localPlayer = lobbyPl.LocalPlayer;
		if (localPlayer == null || localPlayer.UseController == null)
		{
			return;
		}
		int lastPlayerNumberAfter = localPlayer.UseController.GetLastPlayerNumberAfter(localPlayer.Number);
		if (lastPlayerNumberAfter <= 0 || lastPlayerNumberAfter == localPlayer.Number)
		{
			return;
		}
		Player player = PlayerManager.GetInstance().GetPlayer(lastPlayerNumberAfter);
		if (player.AssociatedLobbyPlayer.PlayerStatus == LobbyPlayer.Status.COUCH)
		{
			HotSeatCouch.UnsitPlayer(player);
			PlayerJoinIndicators[lastPlayerNumberAfter - 1].PickLevelEnabled();
			player.AssociatedLobbyPlayer.PlayerStatus = LobbyPlayer.Status.CHARACTER;
			if (HotSeatCouch.GetSeatsTaken() == 0)
			{
				PartyModeButton.Unlock();
				GameState.GetInstance().UsingHotSeat = false;
			}
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(GameModeSetEvent))
		{
			GameModeSetEvent gameModeSetEvent = e as GameModeSetEvent;
			GameState.GameMode mode = gameModeSetEvent.Mode;
			if (mode == GameState.GameMode.FREEPLAY || mode == GameState.GameMode.CHALLENGE)
			{
				MinPlayers = 1;
			}
			else
			{
				MinPlayers = 2;
			}
			RichPresenceManager.Instance.SetLobbyPresenceString(gameModeSetEvent.Mode, LobbyManager.instance.IsInOnlineGame);
		}
		if (type == typeof(ResetDataEvent) && base.hasAuthority)
		{
			StatBoolArray stat = StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatBoolArray>("CharactersUnlocked");
			foreach (LobbyStartPoint startingPoint in StartingPoints)
			{
				if (startingPoint != null && !stat.values[(int)startingPoint.AssociatedCharacter])
				{
					Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
					if (componentInChildren != null && !componentInChildren.Picked)
					{
						componentInChildren.Disable(moveAway: false);
					}
				}
			}
			CallCmdSetTreehouseGrowState(1);
		}
		if (type == typeof(CheatUnlockEvent))
		{
			Debug.Log("Unlocking all characters");
			foreach (LobbyStartPoint startingPoint2 in StartingPoints)
			{
				Character componentInChildren2 = startingPoint2.GetComponentInChildren<Character>();
				if (componentInChildren2 != null && !componentInChildren2.Sitting && !componentInChildren2.Picked)
				{
					componentInChildren2.Enable(playSound: false);
					componentInChildren2.PositionCharacter(startingPoint2.transform.position, groundScaleOffset: true);
				}
			}
			float totalPlaytime = 0f;
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			if (saveFileDataForMainUser != null)
			{
				totalPlaytime = saveFileDataForMainUser.GetStat<StatFloat>("TotalMatchTime").value;
			}
			AnalyticEvent.CheatCodeUsedEvent(TreehouseGrower.TreeState, totalPlaytime);
		}
		if (type == typeof(CheatUnlockHalfEvent))
		{
			Debug.Log("Unlocking half of characters");
			SaveFileData saveFileDataForMainUser2 = StatTracker.Instance.GetSaveFileDataForMainUser();
			foreach (LobbyStartPoint startingPoint3 in StartingPoints)
			{
				Character componentInChildren3 = startingPoint3.GetComponentInChildren<Character>();
				if (componentInChildren3 != null && !componentInChildren3.Sitting && !componentInChildren3.Picked && saveFileDataForMainUser2.GetStat<StatBoolArray>("CharactersUnlocked").values[(int)startingPoint3.AssociatedCharacter])
				{
					componentInChildren3.Enable(playSound: false);
					componentInChildren3.PositionCharacter(startingPoint3.transform.position, groundScaleOffset: true);
				}
			}
			_ = saveFileDataForMainUser2?.GetStat<StatFloat>("TotalMatchTime").value;
		}
		if (type == typeof(OneUnlockMaker))
		{
			Debug.Log("Makes One Unlock Happen");
			LobbyPlayer firstLocalLobbyPlayer = GetFirstLocalLobbyPlayer();
			SaveFileData saveFileDataForMainUser3 = StatTracker.Instance.GetSaveFileDataForMainUser();
			StatBoolArray stat2 = saveFileDataForMainUser3.GetStat<StatBoolArray>("CharactersUnlocked");
			StatCountArray stat3 = saveFileDataForMainUser3.GetStat<StatCountArray>("OutfitsUnlocked");
			LevelPortal[] array = portals;
			foreach (LevelPortal levelPortal in array)
			{
				if (levelPortal.TargetLevel != GameState.LevelName.FARM || levelPortal is CustomLevelPortal)
				{
					continue;
				}
				levelPortal.NetworklevelHasUnlock = true;
				AkSoundEngine.PostEvent("UI_Lobby_Level_UnlockIsAvailable", base.gameObject);
				UnlockInLevel = levelPortal.TargetLevel;
				_ = StatTracker.Instance;
				object[] outfitUnlocks = OutfitUnlocks;
				Array.Sort(outfitUnlocks, (object a, object b) => (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
				for (int num = 0; num != OutfitUnlocks.Length; num++)
				{
					UnLockInfo unLockInfo = OutfitUnlocks[num];
					if (stat2.values[(int)unLockInfo.AssociatedCharacter] && (stat3.values[(int)unLockInfo.AssociatedCharacter] & unLockInfo.OutfitMaskNumber) == 0)
					{
						GameState.GetInstance().nextUnlocks[firstLocalLobbyPlayer] = unLockInfo;
						GameState.GetInstance().guaranteedUnlocks[firstLocalLobbyPlayer] = true;
						levelPortal.NetworklevelHasUnlock = true;
						AkSoundEngine.PostEvent("UI_Lobby_Level_UnlockIsAvailable", base.gameObject);
						UnlockInLevel = levelPortal.TargetLevel;
						return;
					}
				}
			}
		}
		if (type == typeof(CharacterVoteEvent))
		{
			CharacterVoteEvent characterVoteEvent = e as CharacterVoteEvent;
			GameObject gameObject = ClientScene.FindLocalObject(characterVoteEvent.PlayerObjectId);
			if (gameObject != null)
			{
				LobbyPlayer component = gameObject.GetComponent<LobbyPlayer>();
				if (component != null)
				{
					PlayerJoinIndicators[component.networkNumber - 1].setTintColor(component.PlayerColor);
					if (characterVoteEvent.IsVoting)
					{
						PlayerJoinIndicators[component.networkNumber - 1].ReadyEnabled();
						component.PlayerStatus = LobbyPlayer.Status.READY;
					}
					else
					{
						PlayerJoinIndicators[component.networkNumber - 1].PickLevelEnabled();
						component.PlayerStatus = LobbyPlayer.Status.CHARACTER;
					}
					OnCharacterVote(component, characterVoteEvent.IsVoting);
				}
				else
				{
					Debug.LogError("No LobbyPlayer component found");
				}
			}
			else
			{
				Debug.LogError("Could not find lobby player obj");
			}
		}
		if (type == typeof(LobbyPlayerCreatedEvent))
		{
			LobbyPlayerCreatedEvent lobbyPlayerCreatedEvent = e as LobbyPlayerCreatedEvent;
			CancelCountdownProcess();
			LobbyPlayer component2 = lobbyPlayerCreatedEvent.LobbyPlayerObj.GetComponent<LobbyPlayer>();
			if (base.hasAuthority)
			{
				CallCmdCreateCursorForPlayer(lobbyPlayerCreatedEvent.LobbyPlayerObj, showCursor: true);
				if (!component2.IsLocalPlayer)
				{
					LobbyManager.instance.AllLocal = false;
				}
			}
			StartCoroutine(GrabGSIDForPlayer(component2));
			for (int num2 = 0; num2 != JoinedPlayers.Length; num2++)
			{
				if (JoinedPlayers[num2] == null)
				{
					JoinedPlayers[num2] = lobbyPlayerCreatedEvent.LobbyPlayerObj.GetComponent<LobbyPlayer>();
					break;
				}
			}
			UpdatePlayerIndicatorToPlayerState(component2);
			waitingForCouchPlayerCreation = false;
		}
		if (type == typeof(LobbyPlayerRemovedEvent))
		{
			LobbyPlayerRemovedEvent lobbyPlayerRemovedEvent = e as LobbyPlayerRemovedEvent;
			if (base.hasAuthority && base.isServer)
			{
				RemoveUnlocksWithNoPlayer();
				CallRpcRemovePlayer(lobbyPlayerRemovedEvent.PlayerNumber - 1);
				Character characterFromNetworkNumber = GetCharacterFromNetworkNumber(lobbyPlayerRemovedEvent.PlayerNumber);
				if (characterFromNetworkNumber != null)
				{
					CallRpcResetCharacter(characterFromNetworkNumber.gameObject, null);
				}
			}
		}
		if (type == typeof(NetworkClientDisconnectEvent))
		{
			NetworkClientDisconnectEvent networkClientDisconnectEvent = e as NetworkClientDisconnectEvent;
			Debug.Log("Client removed from lobby");
			if (networkClientDisconnectEvent.ConnectionToClient.clientOwnedObjects != null)
			{
				NetworkInstanceId[] array2 = new NetworkInstanceId[networkClientDisconnectEvent.ConnectionToClient.clientOwnedObjects.Count];
				networkClientDisconnectEvent.ConnectionToClient.clientOwnedObjects.CopyTo(array2);
				NetworkInstanceId[] array3 = array2;
				for (int i = 0; i < array3.Length; i++)
				{
					GameObject gameObject2 = ClientScene.FindLocalObject(array3[i]);
					Debug.Log("Resetting authority for object: " + gameObject2);
					Character component3 = gameObject2.GetComponent<Character>();
					Cursor cursor = null;
					if (component3 == null)
					{
						cursor = gameObject2.GetComponent<Cursor>();
					}
					if (component3 != null || cursor != null)
					{
						NetworkIdentity component4 = gameObject2.GetComponent<NetworkIdentity>();
						if (component4.clientAuthorityOwner != null)
						{
							component4.RemoveClientAuthority(networkClientDisconnectEvent.ConnectionToClient);
						}
					}
					if (component3 != null)
					{
						ResetCharacter(gameObject2, null);
					}
				}
			}
			if (networkClientDisconnectEvent.ConnectionToClient.playerControllers != null)
			{
				foreach (PlayerController playerController in networkClientDisconnectEvent.ConnectionToClient.playerControllers)
				{
					if (!playerController.IsValid)
					{
						continue;
					}
					LobbyPlayer component5 = playerController.gameObject.GetComponent<LobbyPlayer>();
					PlayerJoinIndicators[component5.networkNumber - 1].PressEnabled();
					PlayerJoinIndicators[component5.networkNumber - 1].setTintColor(Color.white);
					for (int num3 = 0; num3 != JoinedPlayers.Length; num3++)
					{
						if (JoinedPlayers[num3] == component5)
						{
							JoinedPlayers[num3] = null;
						}
					}
					component5.RemovePlayer();
				}
			}
			GameEventManager.SendEvent(new NetworkClientCleanedUpEvent(networkClientDisconnectEvent.ConnectionToClient));
		}
		if (type == typeof(LobbyCursorCreatedEvent))
		{
			LobbyCursorCreatedEvent lobbyCursorCreatedEvent = e as LobbyCursorCreatedEvent;
			StartCoroutine(setupLobbyCursor(lobbyCursorCreatedEvent.LobbyCursorObj));
			if (forceStartTimer > 0f)
			{
				forceStartTimer = 0f;
				if (base.hasAuthority)
				{
					int countFrom = (int)ForceStartTime - Mathf.FloorToInt(forceStartTimer) + 2;
					CallRpcStartCountDown(countFrom, countDownStart.TimerMessage.HOSTFORCE);
				}
			}
		}
		if (type == typeof(LocalPlayerRemovedEvent))
		{
			bool flag = false;
			foreach (Player item in PlayerManager.GetInstance())
			{
				if (item != null)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (LobbyManager.instance.IsHost)
				{
					NetworkServer.SendToAll(NetMsgTypes.HostEndedGame, new MsgHostEndedGame());
				}
				TransitionToMainMenu();
			}
		}
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PortalHasUnlock && base.hasAuthority)
			{
				MsgPortalHasUnlock msg = (MsgPortalHasUnlock)networkMessageReceivedEvent.ReadMessage;
				LevelPortal levelPortal2 = portals.FirstOrDefault((LevelPortal p) => !(p is CustomLevelPortal) && p.TargetLevel == msg.LevelWithUnlock);
				if (levelPortal2 != null)
				{
					LobbyPlayer lobbyPlayer = FindLobbyPlayer(msg.PlayerNetworkNumber);
					if (lobbyPlayer != null)
					{
						Debug.Log("There is an unlock at " + levelPortal2.TargetLevel.ToString() + " for player " + lobbyPlayer.playerName);
						SetUnlockForPlayer(lobbyPlayer, msg.LevelWithUnlock);
					}
					else
					{
						Debug.LogError("Could not find Lobby Player number " + msg.PlayerNetworkNumber + " - unlock question mark not added.");
					}
				}
				else
				{
					Debug.LogError("Could not find portal for level " + msg.LevelWithUnlock);
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ClientLoadedTreehouse)
			{
				MsgClientLoadedTreehouse msgClientLoadedTreehouse = (MsgClientLoadedTreehouse)networkMessageReceivedEvent.ReadMessage;
				LobbyPlayer lobbyPlayer2 = FindLobbyPlayer(msgClientLoadedTreehouse.NetworkPlayerNumber);
				if (lobbyPlayer2 == null)
				{
					Debug.LogError("LevelSelectController / MsgClientLoadedTreehouse: Lobby player not found");
				}
				if (base.hasAuthority)
				{
					MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
					msgSwitchToMode.toMode = GameSettings.GetInstance().GameMode;
					NetworkServer.SendToClientOfPlayer(lobbyPlayer2.gameObject, NetMsgTypes.SwitchToMode, msgSwitchToMode);
					MsgSetGameModeLock msgSetGameModeLock = new MsgSetGameModeLock();
					msgSetGameModeLock.Locked = PartyModeButton.Locked;
					NetworkServer.SendToClientOfPlayer(lobbyPlayer2.gameObject, NetMsgTypes.SetGameModeLock, msgSetGameModeLock);
					SendAllRules(lobbyPlayer2);
					MsgGameRuleSet msg2 = new MsgGameRuleSet
					{
						NewRule = TabletRule.OnlineSettingsAFKKickTime,
						Value = GameSettings.GetInstance().AFKAutoKickTime
					};
					NetworkServer.SendToClientOfPlayer(lobbyPlayer2.gameObject, NetMsgTypes.GameRuleSet, msg2);
					foreach (Character allCharacter in Character.AllCharacters)
					{
						if (allCharacter != null)
						{
							int[] outfitsAsArray = allCharacter.GetOutfitsAsArray();
							if (outfitsAsArray[0] != -1 || outfitsAsArray[1] != -1 || outfitsAsArray[2] != -1 || outfitsAsArray[3] != -1)
							{
								MsgCommunicateCharacterOutfits msgCommunicateCharacterOutfits = new MsgCommunicateCharacterOutfits();
								msgCommunicateCharacterOutfits.Animal = allCharacter.CharacterSprite;
								msgCommunicateCharacterOutfits.OutfitArray = outfitsAsArray;
								NetworkServer.SendToClientOfPlayer(lobbyPlayer2.gameObject, NetMsgTypes.CommunicateCharacterOutfits, msgCommunicateCharacterOutfits);
							}
						}
					}
					SendCustomPortalStatus(lobbyPlayer2);
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.CommunicateCharacterOutfits)
			{
				MsgCommunicateCharacterOutfits msgCommunicateCharacterOutfits2 = (MsgCommunicateCharacterOutfits)networkMessageReceivedEvent.ReadMessage;
				Character characterFromAnimal = GetCharacterFromAnimal(msgCommunicateCharacterOutfits2.Animal);
				if (characterFromAnimal != null)
				{
					characterFromAnimal.SetOutfitsFromArray(msgCommunicateCharacterOutfits2.OutfitArray);
				}
				else
				{
					Debug.Log("Could not find Character object for " + msgCommunicateCharacterOutfits2.Animal);
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetCustomPortalInfo)
			{
				MsgSetCustomPortalInfo msgSetCustomPortalInfo = (MsgSetCustomPortalInfo)networkMessageReceivedEvent.ReadMessage;
				CustomLevelPortal[] array4 = snapshotPortals;
				foreach (CustomLevelPortal customLevelPortal in array4)
				{
					if (customLevelPortal.PortalID == msgSetCustomPortalInfo.PortalID)
					{
						customLevelPortal.SetAppearanceForClient(msgSetCustomPortalInfo.targetLevel, msgSetCustomPortalInfo.snapshotName, msgSetCustomPortalInfo.code, msgSetCustomPortalInfo.AuthorInfo);
						break;
					}
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.AFKTimerChanged && !base.hasAuthority)
			{
				MsgAFKTimerChanged msgAFKTimerChanged = (MsgAFKTimerChanged)networkMessageReceivedEvent.ReadMessage;
				GameSettings.GetInstance().CurrentLobbyAFKAutoKickTime = msgAFKTimerChanged.Time;
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.HostEndedGame && !base.hasAuthority)
			{
				TransitionToMainMenu(ScriptLocalization.Network.Host_ended_game);
			}
		}
		if (type == typeof(DrivingPlayerRemovedEvent))
		{
			DrivingPlayerRemovedEvent drivingPlayerRemovedEvent = e as DrivingPlayerRemovedEvent;
			Debug.LogError("LevelSelectController: Responding to DrivingPlayerRemovedEvent (" + base.gameObject.activeInHierarchy + ")");
			TransitionToMainMenu(drivingPlayerRemovedEvent.abortReason);
		}
		if (type == typeof(PlatformPlayerRemovedEvent))
		{
			PlatformPlayerRemovedEvent platformPlayerRemovedEvent = e as PlatformPlayerRemovedEvent;
			if (platformPlayerRemovedEvent.RemovedPlayer != null && platformPlayerRemovedEvent.RemovedPlayer.AssociatedLobbyPlayer != null)
			{
				Debug.Log("Removing logged out player from the lobby: " + platformPlayerRemovedEvent.RemovedPlayer);
				platformPlayerRemovedEvent.RemovedPlayer.AssociatedLobbyPlayer.RemovePlayer();
			}
		}
		if (type == typeof(CheatKonamiEvent))
		{
			GameSettings instance = GameSettings.GetInstance();
			if (instance.respawnMode == RespawnMode.Off)
			{
				instance.respawnMode = RespawnMode.RespawnsPerMatch;
			}
			instance.numRespawns = 30;
			GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.RespawnMode));
		}
	}

	private IEnumerator GrabGSIDForPlayer(LobbyPlayer lobbyPlayer)
	{
		if (!lobbyPlayer.IsLocalPlayer)
		{
			yield break;
		}
		if (GameSparksManager.Instance.MainUserGSID.NullOrEmpty())
		{
			Debug.LogWarning("No Main User GSID found... Trying to refresh...");
			int maxRetries = 3;
			int i = 0;
			while (i < maxRetries)
			{
				float timeout = 10f;
				while (!GameSparksManager.Instance.Available)
				{
					timeout -= Time.unscaledDeltaTime;
					if (timeout < 0f)
					{
						break;
					}
					yield return null;
				}
				if (timeout < 0f)
				{
					Debug.LogError("GS not available after 10 seconds...");
					break;
				}
				if (GameSparksManager.Instance.MainUserGSID.NullOrEmpty())
				{
					bool waitingForResponse = true;
					bool gotID = false;
					GameSparksManager.Instance.RetryReadingMainUserGSID(delegate(bool success)
					{
						waitingForResponse = false;
						gotID = success;
					});
					while (waitingForResponse)
					{
						yield return null;
					}
					if (gotID)
					{
						Debug.Log("Got GSID after " + (i + 1) + " attempt(s)");
						break;
					}
				}
				int num = i + 1;
				i = num;
			}
			if (GameSparksManager.Instance.MainUserGSID.NullOrEmpty())
			{
				Debug.LogError("Could not find main user GSID after nudging backend 3 times...");
			}
		}
		GameSparksManager.Instance.FindAndSetPlayerGSID(lobbyPlayer, delegate(bool result)
		{
			if (result)
			{
				Debug.Log("Player GSID was successfully found.");
			}
			else
			{
				Debug.LogError("Player GSID was not found!");
			}
		});
	}

	private void UpdatePlayerIndicatorToPlayerState(LobbyPlayer lobbyPlayer)
	{
		if (lobbyPlayer.networkNumber > 0 && lobbyPlayer.networkNumber <= PlayerJoinIndicators.Length)
		{
			playerJoinIndicator playerJoinIndicator2 = PlayerJoinIndicators[lobbyPlayer.networkNumber - 1];
			if (lobbyPlayer.PlayerStatus == LobbyPlayer.Status.READY || lobbyPlayer.PlayerStatus == LobbyPlayer.Status.COUCH)
			{
				playerJoinIndicator2.ReadyEnabled();
			}
			else if (lobbyPlayer.PlayerStatus == LobbyPlayer.Status.CHARACTER)
			{
				playerJoinIndicator2.PickLevelEnabled();
			}
			else if (lobbyPlayer.PlayerStatus == LobbyPlayer.Status.CURSOR)
			{
				playerJoinIndicator2.ChooseCharacterEnabled();
			}
			playerJoinIndicator2.setTintColor(lobbyPlayer.PlayerColor);
			if (lobbyPlayer.PickedAnimal != Character.Animals.NONE)
			{
				playerJoinIndicator2.setAnimalName(lobbyPlayer.PickedAnimal, lobbyPlayer.IsWearingSkin);
			}
		}
	}

	private void SendAllRules(LobbyPlayer lobbyPl)
	{
		Debug.Log("Sending all rules to player " + lobbyPl.networkNumber);
		GameSettings instance = GameSettings.GetInstance();
		GameRulePreset gameRulePreset = null;
		bool flag = false;
		if (instance.HasDirtyRuleset)
		{
			gameRulePreset = ScriptableObject.CreateInstance<GameRulePreset>();
			gameRulePreset.Name = null;
			gameRulePreset.Description = null;
			gameRulePreset.LoadRulesFromSettings();
			flag = true;
		}
		else
		{
			gameRulePreset = instance.GetCurrentRuleset();
		}
		MsgApplyRuleset msgApplyRuleset = gameRulePreset.GenerateApplyRulesetMessage(loadRules: true, loadPoints: true, loadBlocks: true, loadMods: true);
		msgApplyRuleset.temporary = true;
		NetworkServer.SendToClientOfPlayer(lobbyPl.gameObject, NetMsgTypes.ApplyRuleset, msgApplyRuleset);
		if (flag)
		{
			UnityEngine.Object.Destroy(gameRulePreset);
		}
	}

	[ClientRpc]
	public void RpcPlayerPickedCharacter(int playerNumber, Character.Animals animal, Color color, bool hotseat)
	{
		LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(playerNumber + 1);
		PlayerJoinIndicators[playerNumber].PickLevelEnabled();
		PlayerJoinIndicators[playerNumber].setAnimalName(animal, lobbyPlayer.IsWearingSkin);
		PlayerJoinIndicators[playerNumber].setTintColor(color);
	}

	[ClientRpc]
	public void RpcStartCountDown(int countFrom, countDownStart.TimerMessage message)
	{
		CountDownStart.StartCountDown(countFrom, message);
	}

	[ClientRpc]
	public void RpcCountDownHide()
	{
		CountDownStart.Hide();
	}

	[ClientRpc]
	public void RpcRemoveStartView()
	{
		MainCamera.RemoveTarget(StartViewBounds);
	}

	[ClientRpc]
	public void RpcAddStartView()
	{
		MainCamera.AddTarget(StartViewBounds);
	}

	[ClientRpc]
	private void RpcRemovePlayer(int index)
	{
		PlayerJoinIndicators[index].PressEnabled();
		PlayerJoinIndicators[index].setTintColor(Color.white);
	}

	[Command]
	public void CmdSetTreehouseGrowState(int newtreehouseState)
	{
		TreehouseGrower.SetNewState(newtreehouseState);
		NetworktreehouseState = newtreehouseState;
		CallRpcSetTreeHouseGrowState(newtreehouseState);
	}

	[ClientRpc]
	public void RpcSetTreeHouseGrowState(int newtreehouseState)
	{
		TreehouseGrower.SetNewState(newtreehouseState);
	}

	[ClientRpc]
	public void RpcPlayMusic(string music)
	{
		AkSoundEngine.PostEvent(music, base.gameObject);
	}

	[ClientRpc]
	public void RpcPlaySound(string sound)
	{
		AkSoundEngine.PostEvent(sound, base.gameObject);
	}

	[ClientRpc]
	public void RpcSetGameMode(GameState.GameMode gameMode)
	{
		Debug.Log("Receiving signal to set game mode to " + gameMode);
		GameSettings.GetInstance().GameMode = gameMode;
	}

	[ClientRpc]
	public void RpcSetNextLevel(PlayedSnapshotInfo nextLevelInfo)
	{
		GameState instance = GameState.GetInstance();
		if (UnlockInLevel != nextLevelInfo.nextLevel)
		{
			instance.nextUnlocks.Clear();
		}
		instance.SelectedLevel = nextLevelInfo.nextLevel;
		instance.currentSnapshotInfo = nextLevelInfo;
		FadeOut.showLevelInfoNextLoad = true;
		FadeOut.FadeIn();
	}

	public void ShowHotseatMessageForPlayer(LobbyPlayer lobbyPl)
	{
		HotseatPlayerMessage.ShowMessage(lobbyPl.LocalPlayer, HotSeatCouch.GetAllPlayersWithController(lobbyPl.LocalPlayer.UseController), HotSeatMessageTime, playersLeft > 0);
	}

	public void HideCursor(Cursor cursor, bool sound)
	{
		MainCamera.RemoveTarget(cursor);
		MainCamera.unitBuffer = true;
		MainCamera.SetFrameSizes(CameraHeight);
		cursor.Disable(sound);
	}

	public bool IsCharacterTaken(Character.Animals animal)
	{
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
			if (componentInChildren.CharacterSprite == animal && componentInChildren.Picked)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetLocalizedLevelName(GameState.LevelName level)
	{
		return level switch
		{
			GameState.LevelName.FARM => ScriptLocalization.LevelNames.The_Farm, 
			GameState.LevelName.ROOFTOPS => ScriptLocalization.LevelNames.Rooftops, 
			GameState.LevelName.OLDMANSION => ScriptLocalization.LevelNames.Old_House, 
			GameState.LevelName.WATERFALL => ScriptLocalization.LevelNames.Waterfall, 
			GameState.LevelName.PYRAMID => ScriptLocalization.LevelNames.Desert, 
			GameState.LevelName.WINDMILL => ScriptLocalization.LevelNames.Windmill, 
			GameState.LevelName.METALPLANT => ScriptLocalization.LevelNames.Metal_Plant, 
			GameState.LevelName.ICEBERG => ScriptLocalization.LevelNames.Iceberg, 
			GameState.LevelName.DANCEPARTY => ScriptLocalization.LevelNames.Dance_Party, 
			GameState.LevelName.PIER => ScriptLocalization.LevelNames.The_Pier, 
			GameState.LevelName.BLANKLEVEL => ScriptLocalization.LevelNames.Blank_Level, 
			GameState.LevelName.JUNGLETEMPLE => ScriptLocalization.LevelNames.Jungle, 
			GameState.LevelName.VOLCANO => LocalizationManager.GetTranslation("LevelNames/Volcano"), 
			GameState.LevelName.CRUMBLINGBRIDGE => LocalizationManager.GetTranslation("LevelNames/CrumblingBridge"), 
			GameState.LevelName.TRONLEVEL => LocalizationManager.GetTranslation("LevelNames/TronLevel"), 
			GameState.LevelName.NUCLEARPLANT => LocalizationManager.GetTranslation("LevelNames/NuclearPlant"), 
			GameState.LevelName.SPACELEVEL => ScriptLocalization.LevelNames.SpaceLevel, 
			GameState.LevelName.BALLROOM => ScriptLocalization.LevelNames.TheBallroom, 
			GameState.LevelName.ROLLERCOASTER => ScriptLocalization.LevelNames.Rollercoaster, 
			GameState.LevelName.METRO => ScriptLocalization.LevelNames.Metro, 
			GameState.LevelName.WATERTOWER => ScriptLocalization.LevelNames.WaterTower, 
			GameState.LevelName.RAFT => ScriptLocalization.LevelNames.Raft, 
			GameState.LevelName.SPACESTATION => ScriptLocalization.LevelNames.SpaceStation, 
			GameState.LevelName.PICTUREFRAME => ScriptLocalization.LevelNames.PictureFrame, 
			_ => null, 
		};
	}

	public static GameState.LevelName GetLevelNameEnumFromSceneName(string sceneName)
	{
		return sceneName switch
		{
			"DanceParty" => GameState.LevelName.DANCEPARTY, 
			"Farm" => GameState.LevelName.FARM, 
			"Iceberg" => GameState.LevelName.ICEBERG, 
			"MetalPlant" => GameState.LevelName.METALPLANT, 
			"Pier" => GameState.LevelName.PIER, 
			"Pyramid" => GameState.LevelName.PYRAMID, 
			"RicketyHouse" => GameState.LevelName.OLDMANSION, 
			"Rooftops" => GameState.LevelName.ROOFTOPS, 
			"Waterfall" => GameState.LevelName.WATERFALL, 
			"WindMill" => GameState.LevelName.WINDMILL, 
			"BlankLevel" => GameState.LevelName.BLANKLEVEL, 
			"JungleTemple" => GameState.LevelName.JUNGLETEMPLE, 
			"Volcano" => GameState.LevelName.VOLCANO, 
			"CrumblingBridge" => GameState.LevelName.CRUMBLINGBRIDGE, 
			"TronLevel" => GameState.LevelName.TRONLEVEL, 
			"NuclearPlant" => GameState.LevelName.NUCLEARPLANT, 
			"SpaceLevel" => GameState.LevelName.SPACELEVEL, 
			"Ballroom" => GameState.LevelName.BALLROOM, 
			"RollerCoaster" => GameState.LevelName.ROLLERCOASTER, 
			"Metro" => GameState.LevelName.METRO, 
			"WaterTower" => GameState.LevelName.WATERTOWER, 
			"Raft" => GameState.LevelName.RAFT, 
			"SpaceStation" => GameState.LevelName.SPACESTATION, 
			"PictureFrame" => GameState.LevelName.PICTUREFRAME, 
			"Prototype1" => GameState.LevelName.PROTOTYPE1, 
			"Prototype2" => GameState.LevelName.PROTOTYPE2, 
			"Prototype3" => GameState.LevelName.PROTOTYPE3, 
			"Prototype4" => GameState.LevelName.PROTOTYPE4, 
			"Prototype5" => GameState.LevelName.PROTOTYPE5, 
			"Prototype6" => GameState.LevelName.PROTOTYPE6, 
			_ => GameState.LevelName.FARM, 
		};
	}

	private void SendCustomPortalStatus(LobbyPlayer lobbyPlayer)
	{
		if (base.hasAuthority)
		{
			CustomLevelPortal[] array = snapshotPortals;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateAppearanceForClient(lobbyPlayer);
			}
		}
	}

	private void ApplyCustomPortalSnapshotInfo(int portalIdx, CustomLevelPortal.SnapshotInfo snapshotInfo)
	{
		Debug.Log("Applying info for custom portal #" + portalIdx);
		ExecuteOnRuleBookInitialized(delegate
		{
			snapshotPortals[portalIdx].snapshotInfo = snapshotInfo;
			Sprite spriteForLevel = undergroundComputer.GetSpriteForLevel(snapshotInfo.targetLevel);
			snapshotPortals[portalIdx].SetContents(snapshotInfo.targetLevel, snapshotInfo.snapshotName, snapshotInfo.code, snapshotInfo.xml, spriteForLevel, snapshotInfo.authorInfo);
			undergroundComputer.ComputerSlots[portalIdx].SetComputerSlotAppearance(snapshotInfo.snapshotName, spriteForLevel);
		});
	}

	public IEnumerable<Character> EnumerateCharacters()
	{
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
			if (componentInChildren != null)
			{
				yield return componentInChildren;
			}
		}
	}

	public Character GetCharacterFromAnimal(Character.Animals animal)
	{
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
			if (componentInChildren != null && componentInChildren.CharacterSprite == animal)
			{
				return componentInChildren;
			}
		}
		return null;
	}

	private Character GetCharacterFromNetworkNumber(int playerNetworkNumber)
	{
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			if (!(startingPoint == null))
			{
				Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
				if (componentInChildren != null && componentInChildren.networkNumber == playerNetworkNumber)
				{
					return componentInChildren;
				}
			}
		}
		return null;
	}

	public static LobbyPlayer GetFirstLocalLobbyPlayer()
	{
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (!(lobbyPlayer == null) && lobbyPlayer.Initialized && lobbyPlayer.IsLocalPlayer)
			{
				return lobbyPlayer;
			}
		}
		return null;
	}

	private void SendUnlockMessageFromClient(LobbyPlayer lobbyPl, GameState.LevelName targetLevel)
	{
		MsgPortalHasUnlock msgPortalHasUnlock = new MsgPortalHasUnlock();
		msgPortalHasUnlock.LevelWithUnlock = targetLevel;
		msgPortalHasUnlock.PlayerNetworkNumber = lobbyPl.networkNumber;
		LobbyManager.instance.client.Send(NetMsgTypes.PortalHasUnlock, msgPortalHasUnlock);
		AkSoundEngine.PostEvent("UI_Lobby_Level_UnlockIsAvailable", base.gameObject);
		UnlockInLevel = targetLevel;
	}

	private void SetUnlockForPlayer(LobbyPlayer lobbyPl, GameState.LevelName targetLevel)
	{
		if (base.hasAuthority)
		{
			RemoveUnlocksWithNoPlayer();
			if (!unlockQuestionMarks.ContainsKey(lobbyPl.playerNodeID))
			{
				unlockQuestionMarks.Add(lobbyPl.playerNodeID, targetLevel);
			}
			LevelPortal levelPortal = portals.FirstOrDefault((LevelPortal p) => !(p is CustomLevelPortal) && p.TargetLevel == targetLevel);
			if (levelPortal != null)
			{
				levelPortal.NetworklevelHasUnlock = true;
			}
		}
	}

	private void RemoveUnlocksWithNoPlayer()
	{
		if (!base.hasAuthority)
		{
			return;
		}
		List<uint> list = new List<uint>();
		foreach (KeyValuePair<uint, GameState.LevelName> unlockQuestionMark in unlockQuestionMarks)
		{
			bool flag = false;
			NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
				if (lobbyPlayer != null && lobbyPlayer.playerNodeID == unlockQuestionMark.Key)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(unlockQuestionMark.Key);
			}
		}
		foreach (uint item in list)
		{
			GameState.LevelName levelName = unlockQuestionMarks[item];
			unlockQuestionMarks.Remove(item);
			foreach (LevelPortal item2 in portals.Where((LevelPortal p) => !(p is CustomLevelPortal)))
			{
				if (item2.TargetLevel != levelName)
				{
					continue;
				}
				foreach (KeyValuePair<uint, GameState.LevelName> unlockQuestionMark2 in unlockQuestionMarks)
				{
					if (unlockQuestionMark2.Value == levelName)
					{
						return;
					}
				}
				item2.NetworklevelHasUnlock = false;
				break;
			}
		}
	}

	private bool CheckCharacterUnlockInLevel(GameState.LevelName targetLevel, Character.Animals animal)
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		StatBoolArray stat = saveFileDataForMainUser.GetStat<StatBoolArray>("LevelsUnlocked");
		StatBoolArray stat2 = saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked");
		if (stat.values[(int)targetLevel] && !stat2.values[(int)animal] && portals.FirstOrDefault((LevelPortal p) => !(p is CustomLevelPortal) && p.TargetLevel == targetLevel) != null)
		{
			Debug.Log(animal.ToString() + " ready to unlock");
			return true;
		}
		return false;
	}

	private LobbyPlayer FindLobbyPlayer(int networkNumber)
	{
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null && lobbyPlayer.networkNumber == networkNumber)
			{
				return lobbyPlayer;
			}
		}
		return null;
	}

	public void ExecuteOnRuleBookInitialized(Action action)
	{
		if (GameRuleBookInitialized)
		{
			action();
		}
		else
		{
			onRuleBookInitialized = (Action)Delegate.Combine(onRuleBookInitialized, action);
		}
	}

	private void OnCharacterVote(LobbyPlayer lobbyPl, bool voting)
	{
		if (voting)
		{
			if (!readyCountList.Contains(lobbyPl))
			{
				readyCountList.Add(lobbyPl);
			}
			if (base.hasAuthority && lobbyPl.IsLocalPlayer)
			{
				forcingStart++;
			}
			return;
		}
		if (readyCountList.Contains(lobbyPl))
		{
			readyCountList.Remove(lobbyPl);
		}
		if (base.hasAuthority && lobbyPl.IsLocalPlayer)
		{
			forcingStart--;
			if (forcingStart <= 0)
			{
				forcingStart = 0;
				forceStartTimer = 0f;
			}
		}
	}

	public static void ClearLastLobbyRulesetCopy()
	{
		if (lastLobbyRulesetCopy != null)
		{
			UnityEngine.Object.Destroy(lastLobbyRulesetCopy);
			lastLobbyRulesetCopy = null;
		}
	}

	public static void MemorizeLastLobbyPreset()
	{
		ClearLastLobbyRulesetCopy();
		GameSettings instance = GameSettings.GetInstance();
		GameRulePreset currentRuleset = instance.GetCurrentRuleset();
		if (currentRuleset != null)
		{
			lastLobbyRulesetIdx = instance.GetRulesetIndex(currentRuleset);
		}
		if (lastLobbyRulesetIdx == -1)
		{
			lastLobbyRulesetCopy = ScriptableObject.CreateInstance<GameRulePreset>();
			lastLobbyRulesetCopy.LoadRulesFromSettings();
		}
	}

	public void RefreshCharacterPosition()
	{
		foreach (LobbyStartPoint startingPoint in StartingPoints)
		{
			Character componentInChildren = startingPoint.GetComponentInChildren<Character>();
			if (componentInChildren != null && componentInChildren.AssociatedLobbyPlayer == null)
			{
				componentInChildren.PositionCharacter(startingPoint.transform.position, groundScaleOffset: true);
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdCreateCursorForPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCreateCursorForPlayer called on client.");
		}
		else
		{
			((LevelSelectController)obj).CmdCreateCursorForPlayer(reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetTreehouseGrowState(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTreehouseGrowState called on client.");
		}
		else
		{
			((LevelSelectController)obj).CmdSetTreehouseGrowState((int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdCreateCursorForPlayer(GameObject lobbyPlayerObj, bool showCursor)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCreateCursorForPlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCreateCursorForPlayer(lobbyPlayerObj, showCursor);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCreateCursorForPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(lobbyPlayerObj);
		networkWriter.Write(showCursor);
		SendCommandInternal(networkWriter, 0, "CmdCreateCursorForPlayer");
	}

	public void CallCmdSetTreehouseGrowState(int newtreehouseState)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTreehouseGrowState called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTreehouseGrowState(newtreehouseState);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTreehouseGrowState);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)newtreehouseState);
		SendCommandInternal(networkWriter, 0, "CmdSetTreehouseGrowState");
	}

	protected static void InvokeRpcRpcLockVotes(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLockVotes called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcLockVotes();
		}
	}

	protected static void InvokeRpcRpcTurnOffArrow(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTurnOffArrow called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcTurnOffArrow((GameState.PortalID)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcRemoveCameraTargetLevelPortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveCameraTargetLevelPortal called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcRemoveCameraTargetLevelPortal((GameState.PortalID)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcAddCameraTargetLevelPortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddCameraTargetLevelPortal called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcAddCameraTargetLevelPortal((GameState.PortalID)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcClearCameraTransformTargets(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearCameraTransformTargets called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcClearCameraTransformTargets();
		}
	}

	protected static void InvokeRpcRpcMagicSmokePoof(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMagicSmokePoof called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcMagicSmokePoof(reader.ReadVector3(), reader.ReadColor(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcResetCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetCharacter called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcResetCharacter(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeRpcRpcPlayerPickedCharacter(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayerPickedCharacter called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcPlayerPickedCharacter((int)reader.ReadPackedUInt32(), (Character.Animals)reader.ReadInt32(), reader.ReadColor(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcStartCountDown(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartCountDown called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcStartCountDown((int)reader.ReadPackedUInt32(), (countDownStart.TimerMessage)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcCountDownHide(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCountDownHide called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcCountDownHide();
		}
	}

	protected static void InvokeRpcRpcRemoveStartView(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveStartView called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcRemoveStartView();
		}
	}

	protected static void InvokeRpcRpcAddStartView(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddStartView called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcAddStartView();
		}
	}

	protected static void InvokeRpcRpcRemovePlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemovePlayer called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcRemovePlayer((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcSetTreeHouseGrowState(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetTreeHouseGrowState called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcSetTreeHouseGrowState((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcPlayMusic(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayMusic called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcPlayMusic(reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcPlaySound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySound called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcPlaySound(reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcSetGameMode(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetGameMode called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcSetGameMode((GameState.GameMode)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcSetNextLevel(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetNextLevel called on server.");
		}
		else
		{
			((LevelSelectController)obj).RpcSetNextLevel(GeneratedNetworkCode._ReadPlayedSnapshotInfo_LevelSelectController(reader));
		}
	}

	public void CallRpcLockVotes()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcLockVotes called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcLockVotes);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcLockVotes");
	}

	public void CallRpcTurnOffArrow(GameState.PortalID targetPortalID)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcTurnOffArrow called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcTurnOffArrow);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)targetPortalID);
		SendRPCInternal(networkWriter, 0, "RpcTurnOffArrow");
	}

	public void CallRpcRemoveCameraTargetLevelPortal(GameState.PortalID targetPortalID)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveCameraTargetLevelPortal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveCameraTargetLevelPortal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)targetPortalID);
		SendRPCInternal(networkWriter, 0, "RpcRemoveCameraTargetLevelPortal");
	}

	public void CallRpcAddCameraTargetLevelPortal(GameState.PortalID targetPortalID)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAddCameraTargetLevelPortal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAddCameraTargetLevelPortal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)targetPortalID);
		SendRPCInternal(networkWriter, 0, "RpcAddCameraTargetLevelPortal");
	}

	public void CallRpcClearCameraTransformTargets()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcClearCameraTransformTargets called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcClearCameraTransformTargets);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcClearCameraTransformTargets");
	}

	public void CallRpcMagicSmokePoof(Vector3 position, Color color, int layer)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcMagicSmokePoof called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcMagicSmokePoof);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		networkWriter.Write(color);
		networkWriter.WritePackedUInt32((uint)layer);
		SendRPCInternal(networkWriter, 0, "RpcMagicSmokePoof");
	}

	public void CallRpcResetCharacter(GameObject characterObj, GameObject lobbyPlObj)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcResetCharacter called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcResetCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(characterObj);
		networkWriter.Write(lobbyPlObj);
		SendRPCInternal(networkWriter, 0, "RpcResetCharacter");
	}

	public void CallRpcPlayerPickedCharacter(int playerNumber, Character.Animals animal, Color color, bool hotseat)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayerPickedCharacter called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayerPickedCharacter);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)playerNumber);
		networkWriter.Write((int)animal);
		networkWriter.Write(color);
		networkWriter.Write(hotseat);
		SendRPCInternal(networkWriter, 0, "RpcPlayerPickedCharacter");
	}

	public void CallRpcStartCountDown(int countFrom, countDownStart.TimerMessage message)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcStartCountDown called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcStartCountDown);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)countFrom);
		networkWriter.Write((int)message);
		SendRPCInternal(networkWriter, 0, "RpcStartCountDown");
	}

	public void CallRpcCountDownHide()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcCountDownHide called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcCountDownHide);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcCountDownHide");
	}

	public void CallRpcRemoveStartView()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveStartView called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveStartView);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcRemoveStartView");
	}

	public void CallRpcAddStartView()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAddStartView called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAddStartView);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcAddStartView");
	}

	public void CallRpcRemovePlayer(int index)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemovePlayer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemovePlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)index);
		SendRPCInternal(networkWriter, 0, "RpcRemovePlayer");
	}

	public void CallRpcSetTreeHouseGrowState(int newtreehouseState)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetTreeHouseGrowState called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetTreeHouseGrowState);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)newtreehouseState);
		SendRPCInternal(networkWriter, 0, "RpcSetTreeHouseGrowState");
	}

	public void CallRpcPlayMusic(string music)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayMusic called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayMusic);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(music);
		SendRPCInternal(networkWriter, 0, "RpcPlayMusic");
	}

	public void CallRpcPlaySound(string sound)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlaySound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlaySound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(sound);
		SendRPCInternal(networkWriter, 0, "RpcPlaySound");
	}

	public void CallRpcSetGameMode(GameState.GameMode gameMode)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetGameMode called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetGameMode);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)gameMode);
		SendRPCInternal(networkWriter, 0, "RpcSetGameMode");
	}

	public void CallRpcSetNextLevel(PlayedSnapshotInfo nextLevelInfo)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetNextLevel called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetNextLevel);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WritePlayedSnapshotInfo_LevelSelectController(networkWriter, nextLevelInfo);
		SendRPCInternal(networkWriter, 0, "RpcSetNextLevel");
	}

	static LevelSelectController()
	{
		kCmdCmdCreateCursorForPlayer = 1135798428;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LevelSelectController), kCmdCmdCreateCursorForPlayer, InvokeCmdCmdCreateCursorForPlayer);
		kCmdCmdSetTreehouseGrowState = -817412350;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LevelSelectController), kCmdCmdSetTreehouseGrowState, InvokeCmdCmdSetTreehouseGrowState);
		kRpcRpcLockVotes = -1170850260;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcLockVotes, InvokeRpcRpcLockVotes);
		kRpcRpcTurnOffArrow = -1456268567;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcTurnOffArrow, InvokeRpcRpcTurnOffArrow);
		kRpcRpcRemoveCameraTargetLevelPortal = 1214523332;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcRemoveCameraTargetLevelPortal, InvokeRpcRpcRemoveCameraTargetLevelPortal);
		kRpcRpcAddCameraTargetLevelPortal = -435579861;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcAddCameraTargetLevelPortal, InvokeRpcRpcAddCameraTargetLevelPortal);
		kRpcRpcClearCameraTransformTargets = -666017226;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcClearCameraTransformTargets, InvokeRpcRpcClearCameraTransformTargets);
		kRpcRpcMagicSmokePoof = 2095684074;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcMagicSmokePoof, InvokeRpcRpcMagicSmokePoof);
		kRpcRpcResetCharacter = -1117105780;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcResetCharacter, InvokeRpcRpcResetCharacter);
		kRpcRpcPlayerPickedCharacter = 90202678;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcPlayerPickedCharacter, InvokeRpcRpcPlayerPickedCharacter);
		kRpcRpcStartCountDown = -1855717631;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcStartCountDown, InvokeRpcRpcStartCountDown);
		kRpcRpcCountDownHide = 284121601;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcCountDownHide, InvokeRpcRpcCountDownHide);
		kRpcRpcRemoveStartView = 1744585041;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcRemoveStartView, InvokeRpcRpcRemoveStartView);
		kRpcRpcAddStartView = 1189109240;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcAddStartView, InvokeRpcRpcAddStartView);
		kRpcRpcRemovePlayer = -1785910281;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcRemovePlayer, InvokeRpcRpcRemovePlayer);
		kRpcRpcSetTreeHouseGrowState = 1980616972;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcSetTreeHouseGrowState, InvokeRpcRpcSetTreeHouseGrowState);
		kRpcRpcPlayMusic = -2126530273;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcPlayMusic, InvokeRpcRpcPlayMusic);
		kRpcRpcPlaySound = -2121165815;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcPlaySound, InvokeRpcRpcPlaySound);
		kRpcRpcSetGameMode = 192903557;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcSetGameMode, InvokeRpcRpcSetGameMode);
		kRpcRpcSetNextLevel = 1899909505;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LevelSelectController), kRpcRpcSetNextLevel, InvokeRpcRpcSetNextLevel);
		NetworkCRC.RegisterBehaviour("LevelSelectController", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)treehouseState);
			writer.WritePackedUInt32((uint)unlockedCharacters);
			writer.Write(HostIsLoaded);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)treehouseState);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)unlockedCharacters);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(HostIsLoaded);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			treehouseState = (int)reader.ReadPackedUInt32();
			unlockedCharacters = (int)reader.ReadPackedUInt32();
			HostIsLoaded = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			treehouseState = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			unlockedCharacters = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			HostIsLoaded = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
