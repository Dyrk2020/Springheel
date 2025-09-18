using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using GameEvent;
using I2.Loc;
using SevenZip.Compression.LZMA;
using Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameControl : NetworkBehaviour, IGameEventListener, InputReceiver
{
	public enum GamePhase
	{
		NONE,
		START,
		PLAY,
		PLACE,
		SUDDENDEATH,
		END,
		WAIT
	}

	public Level LevelLayout;

	public Graphpaper graphPaper;

	public ZoomCamera MainCamera;

	public Camera UICamera;

	private LoadingInterstitialSplash fadeOut;

	public InventoryBook InventoryBookPrefab;

	public ModsDisplayController modsDisplayControllerPrefab;

	public PauseFade pauseFade;

	public Character CharacterPrefab;

	public Cursor CursorPrefab;

	public Character DebugCharacter;

	public PiecePlacementCursor DebugCursor;

	public GameState.GameMode DefaultMode;

	public NetworkSurrogate NetSurrogatePrefab;

	public UnLockBox UnLockBoxPrefab;

	[SyncVar]
	public string AssociatedScene;

	public GamePhase StartPhase = GamePhase.PLACE;

	public float StartDelay;

	public float WinTime;

	public float DanceTime;

	public PlaceableMetadataList MetaList;

	protected InventoryBook invBookInstance;

	protected Queue<GamePlayer> PlayerQueue = new Queue<GamePlayer>();

	protected List<Placeable> placedBlocks = new List<Placeable>();

	protected List<Placeable> destroyedBlocks = new List<Placeable>();

	protected List<ActiveBlock> activeBlocks = new List<ActiveBlock>();

	protected List<Placeable> attachments = new List<Placeable>();

	protected Dictionary<int, int> msgToDestroyBlocks = new Dictionary<int, int>();

	protected GamePlayer winner;

	protected bool winnerSet;

	protected float startDelayTimer;

	protected float winTimer;

	protected float danceTimer;

	protected bool firstPlayFrame;

	protected bool AfterOneFixedUpdate;

	protected bool nextFixedUpdate;

	protected float blocksArea;

	protected float levelDensity;

	protected float blocksChallenge;

	protected float averageBlockChallenge;

	protected float nonzeroAverageChallenge;

	protected float totalChallenge;

	protected int challengeBlocks;

	protected int blockDeaths;

	protected int suicideDeaths;

	protected int worldDeaths;

	protected int winners;

	protected int loseStreak;

	protected bool paused;

	protected bool softPaused;

	protected bool scoreboard;

	protected bool holdingForNextPhase;

	protected GamePhase nextPhase;

	protected bool moveUp;

	protected bool moveDown;

	protected int roundNumber;

	protected bool AllowRespawn = true;

	private int unlockNumber;

	[SyncVar]
	private int unlockOffset;

	protected bool[] showScoreButtons = new bool[PlayerManager.maxPlayers];

	protected bool accept;

	protected bool back;

	protected bool inventory;

	protected bool pause;

	protected bool acceptUp;

	protected bool backUp;

	protected bool inventoryUp;

	protected bool pauseUp;

	protected bool acceptDown;

	protected bool backDown;

	protected bool inventoryDown;

	protected bool pauseDown;

	protected int inputPlayerNumber;

	protected int pausedDownPlayer;

	private int debugTextRow;

	[SyncVar]
	private bool waitingForPlayers;

	private int snapshotReceiveBufferIdx;

	private byte[] snapshotReceiveBuffer;

	private List<int> playerNumbersStillLoadingSnapshot;

	private List<MsgNetworkSurrogateSpawned> deferredNetworkSurrogateMessages = new List<MsgNetworkSurrogateSpawned>();

	private Dictionary<int, NetworkSurrogate> orphanedNetSurrogates = new Dictionary<int, NetworkSurrogate>();

	private List<int> netSurrogatesToRemoveCache = new List<int>(128);

	private bool sceneInitNotificationSent;

	private bool hostBeaconReceived;

	private bool firstUpdateDone;

	protected bool deadSession;

	protected bool CleanUpStarted;

	protected bool localSetupStartDone;

	protected bool postSetupStart;

	public LivesDisplayController livesDisplayController;

	private GUIStyle debugStyle = new GUIStyle();

	protected int kicks;

	protected int quits;

	private bool GoingBackToMainMenu;

	private float pauseLimiter;

	private float pauseLimitTimer = 1.5f;

	private static int kRpcRpcStartPhase;

	private static int kRpcRpcPlayMusic;

	private static int kRpcRpcStartFadingOut;

	private static int kRpcRpcPropagateBlockIDs;

	private static int kRpcRpcSetUpCompressedSnapshotTransfer;

	private static int kRpcRpcSendCompressedSnapshotChunk;

	private static int kRpcRpcLoadCompressedSnapshot;

	private static int kRpcRpcHostReadyBeacon;

	private static int kRpcRpcDoInitialPlacement;

	public GamePhase Phase { get; protected set; }

	public GamePhase NextPhase => nextPhase;

	public InventoryBook InventoryBook => invBookInstance;

	public Queue<GamePlayer> CurrentPlayerQueue => PlayerQueue;

	public int RoundNumber => roundNumber;

	public Guid MatchGuid { get; protected set; }

	protected bool PlayersStillLoadingSnapshot
	{
		get
		{
			if (playerNumbersStillLoadingSnapshot == null || playerNumbersStillLoadingSnapshot.Count == 0)
			{
				return false;
			}
			HashSet<int> hashSet = new HashSet<int>();
			foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
			{
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
				if (!(gameObject == null))
				{
					GamePlayer component = gameObject.GetComponent<GamePlayer>();
					hashSet.Add(component.networkNumber);
				}
			}
			HashSet<int> hashSet2 = new HashSet<int>();
			for (int i = 0; i < playerNumbersStillLoadingSnapshot.Count; i++)
			{
				if (!hashSet.Contains(playerNumbersStillLoadingSnapshot[i]))
				{
					hashSet2.Add(playerNumbersStillLoadingSnapshot[i]);
				}
			}
			foreach (int item in hashSet2)
			{
				playerNumbersStillLoadingSnapshot.Remove(item);
			}
			return playerNumbersStillLoadingSnapshot.Count > 0;
		}
	}

	public string NetworkAssociatedScene
	{
		get
		{
			return AssociatedScene;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref AssociatedScene, 1u);
		}
	}

	public int NetworkunlockOffset
	{
		get
		{
			return unlockOffset;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref unlockOffset, 2u);
		}
	}

	public bool NetworkwaitingForPlayers
	{
		get
		{
			return waitingForPlayers;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref waitingForPlayers, 4u);
		}
	}

	private void Awake()
	{
		LobbyManager.instance.CurrentGameController = this;
		ChangeListener(adding: true);
		debugStyle.fontSize = 30;
	}

	protected virtual void Start()
	{
		findLevelObjects();
		fadeOut = LoadingInterstitialSplash.Instance;
		pauseFade.gameObject.SetActive(value: true);
		UICamera.transform.position = new Vector3(1000f, 1000f, 0f);
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
					continue;
				}
				Debug.LogError("Lobby Player " + lobbyPlayer.networkNumber + " (" + lobbyPlayer.playerName + ") has no EmoteSystem");
			}
		}
		GameState instance = GameState.GetInstance();
		instance.ResetPieceCount();
		instance.Paused = false;
		instance.TimeStarted = Time.time;
		instance.TimeElapsed = 0f;
		msgToDestroyBlocks.Clear();
		GameEventManager.SendEvent(new NewMatchEvent(GameSettings.GetInstance().GameMode, GameState.GetInstance().SelectedLevel, GameState.GetInstance().currentSnapshotInfo.snapshotCode));
		foreach (Placeable allPlaceable in Placeable.AllPlaceables)
		{
			if (allPlaceable != null)
			{
				if (allPlaceable.ParentPiece == null && allPlaceable.transform.parent != null && allPlaceable.GetComponent<PlaceableMetadata>() != null)
				{
					allPlaceable.transform.parent = null;
				}
				if (allPlaceable is ActiveBlock)
				{
					activeBlocks.Add(allPlaceable as ActiveBlock);
					(allPlaceable as ActiveBlock).Active = false;
				}
			}
		}
		if (livesDisplayController != null)
		{
			livesDisplayController.Initialize();
		}
		if (base.hasAuthority)
		{
			NetworkwaitingForPlayers = true;
			StartCoroutine(waitForNetworkPlayers());
		}
		else if (Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.CurrentMatchmakingLobby is GamesparksMatchmakingLobby)
		{
			StartCoroutine(queryGamesparksLobby());
		}
		MainCamera.smoothFollowCamOn = false;
		MainCamera.SetUnitBuffer(LevelLayout);
		string formattedSnapshotCode = GameSparksQuery.GetFormattedSnapshotCode(instance.currentSnapshotInfo.snapshotCode);
		RichPresenceManager.Instance.SetGamePresenceString(instance.SelectedLevel, formattedSnapshotCode, GameSettings.GetInstance().GameMode, LobbyManager.instance.IsInOnlineGame);
		UICamera.gameObject.AddPrefabAsChild<ModsDisplayController>(modsDisplayControllerPrefab);
	}

	private IEnumerator initialPlacement()
	{
		yield return new WaitForFixedUpdate();
		Placeable[] array = Placeable.AllPlaceables.ToArray();
		Array.Sort(array, (Placeable a, Placeable b) => GetHierarchyDepth(b.transform).CompareTo(GetHierarchyDepth(a.transform)));
		for (int num = 0; num != array.Length; num++)
		{
			Placeable placeable = array[num];
			if (!placeable.Placed)
			{
				placeable.Place(0);
				if (base.hasAuthority && placeable.IsNetworked && placeable.NetSurrogate == null)
				{
					SpawnNetSurrogate(placeable.ID);
				}
				placedBlocks.Add(placeable);
				if (placeable.HasReverseAttachments)
				{
					attachments.Add(placeable);
				}
			}
			if (StartPhase == GamePhase.PLACE)
			{
				placeable.EnablePlaced();
			}
		}
	}

	private int GetHierarchyDepth(Transform t)
	{
		int num = 0;
		while (t != null)
		{
			t = t.parent;
			num++;
		}
		return num;
	}

	protected void findLevelObjects()
	{
		if (LevelLayout == null)
		{
			Level[] array = Resources.FindObjectsOfTypeAll<Level>();
			for (int i = 0; i != array.Length; i++)
			{
				if (array[i].gameObject.scene.name == AssociatedScene)
				{
					LevelLayout = array[i];
				}
			}
		}
		if (graphPaper == null)
		{
			Graphpaper[] array2 = Resources.FindObjectsOfTypeAll<Graphpaper>();
			for (int j = 0; j != array2.Length; j++)
			{
				if (array2[j].gameObject.scene.name == AssociatedScene)
				{
					graphPaper = array2[j];
				}
			}
		}
		if (LevelLayout == null && SceneManager.GetActiveScene().name != "TreeHouseLobby")
		{
			Debug.LogError("No level layout!");
		}
		else if (LevelLayout != null)
		{
			MainCamera.SetBounds(LevelLayout.GetCameraBounds());
			if (base.hasAuthority)
			{
				NetworkunlockOffset = UnityEngine.Random.Range(0, LevelLayout.UnlockSpawnLocations.Length);
			}
		}
		if (graphPaper == null && SceneManager.GetActiveScene().name != "TreeHouseLobby")
		{
			Debug.LogError("No Graph Paper!");
		}
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
		Time.timeScale = Modifiers.GetInstance().GameSpeed;
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<GameStartEvent>(this, adding);
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, adding);
		GameEventManager.ChangeListener<DestroyPieceEvent>(this, adding);
		GameEventManager.ChangeListener<PlayerSucceedEvent>(this, adding);
		GameEventManager.ChangeListener<SoftPauseEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkCursorSpawnedEvent>(this, adding);
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<HoldRespawnEvent>(this, adding);
		GameEventManager.ChangeListener<DrivingPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<PlatformPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<QuicksaverLevelFinishedLoading>(this, adding);
		GameEventManager.ChangeListener<ControllerConnectionEvent>(this, adding);
	}

	protected virtual void FixedUpdate()
	{
		if (nextFixedUpdate)
		{
			AfterAFixedUpdate();
			nextFixedUpdate = false;
		}
		if (AfterOneFixedUpdate)
		{
			AfterOneFixedUpdate = false;
			nextFixedUpdate = true;
		}
	}

	private void TryFlushOrphanedNetSurrogates()
	{
		netSurrogatesToRemoveCache.Clear();
		List<int> list = netSurrogatesToRemoveCache;
		foreach (KeyValuePair<int, NetworkSurrogate> orphanedNetSurrogate in orphanedNetSurrogates)
		{
			int key = orphanedNetSurrogate.Key;
			NetworkSurrogate value = orphanedNetSurrogate.Value;
			if (value == null)
			{
				continue;
			}
			value.linkAttempts++;
			bool flag = false;
			foreach (Placeable placedBlock in placedBlocks)
			{
				if (placedBlock != null && placedBlock.ID == orphanedNetSurrogate.Key)
				{
					value.transform.SetParent(placedBlock.transform, worldPositionStays: false);
					value.transform.localPosition = Vector3.zero;
					placedBlock.NetSurrogate = value;
					Debug.Log("Found placeable " + key + " for orphaned net surrogate after " + value.linkAttempts + " attempts.");
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Add(key);
			}
			else if (value.linkAttempts > 3600)
			{
				UnityEngine.Object.Destroy(value.gameObject);
				list.Add(key);
				Debug.LogError("Could not link netsurrogate to intended placeable (" + key + ") after a reasonably long time. Aborting.");
			}
		}
		foreach (int item in list)
		{
			orphanedNetSurrogates.Remove(item);
		}
	}

	private void sendClientAnalytics()
	{
		if (!AnalyticsWrapper.EnabledOnPlatform || Matchmaker.CurrentMatchmakingLobby == null)
		{
			return;
		}
		MatchGuid = Matchmaker.CurrentMatchmakingLobby.GetMatchGuid();
		int num = 0;
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item == null)
			{
				continue;
			}
			num++;
			LobbyPlayer associatedLobbyPlayer = item.AssociatedLobbyPlayer;
			int num2 = 0;
			foreach (int characterOutfits in associatedLobbyPlayer.characterOutfitsList)
			{
				num2 += characterOutfits;
			}
			AnalyticEvent.CharacterPickedEvent(MatchGuid, associatedLobbyPlayer.PickedAnimal, num2, associatedLobbyPlayer.handicap);
		}
		AnalyticEvent.MatchStartClientEvent(MatchGuid, num, ZoomCamera.GlobalCameraTime, ZoomCamera.LocalCameraTime);
	}

	protected virtual void sendEndAnalytics()
	{
	}

	protected virtual void Update()
	{
		if (deadSession || CleanUpStarted)
		{
			return;
		}
		TryFlushOrphanedNetSurrogates();
		if (pauseLimiter > 0f)
		{
			pauseLimiter -= Time.unscaledDeltaTime;
		}
		if (firstUpdateDone)
		{
			if (!base.hasAuthority && hostBeaconReceived && !sceneInitNotificationSent)
			{
				bool flag = true;
				if (AnalyticsWrapper.EnabledOnPlatform && Matchmaker.CurrentMatchmakingLobby != null)
				{
					Guid guid = default(Guid);
					MatchGuid = Matchmaker.CurrentMatchmakingLobby.GetMatchGuid();
					if (Time.timeSinceLevelLoad < 5f)
					{
						flag = MatchGuid != guid;
					}
					else
					{
						flag = true;
						Debug.LogWarning("MatchGUID timed out. Analytics for this client will not be associated with this match.");
					}
				}
				NetworkPlayerTracker playerTracker = LobbyManager.instance.PlayerTracker;
				if (!playerTracker.WaitingForIDs && !playerTracker.WaitingForGamePlayerInit && flag)
				{
					NotifySceneInitDone();
					sendClientAnalytics();
					sceneInitNotificationSent = true;
				}
			}
		}
		else
		{
			firstUpdateDone = true;
		}
		if (GameState.DebugMode)
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				UICamera.enabled = !UICamera.enabled;
			}
			if (Input.GetKeyDown(KeyCode.G))
			{
				graphPaper.gameObject.SetActive(!graphPaper.gameObject.activeInHierarchy);
			}
			if (Input.GetKeyDown(KeyCode.B))
			{
				nextPhase = GamePhase.PLACE;
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				nextPhase = GamePhase.PLAY;
			}
		}
		if (paused || scoreboard || holdingForNextPhase)
		{
			ResetInput();
		}
		else
		{
			if (base.hasAuthority && (PlayerQueue.Count == 0 || waitingForPlayers))
			{
				return;
			}
			if (nextPhase != Phase)
			{
				switch (nextPhase)
				{
				case GamePhase.START:
					try
					{
						SetupStart(GameSettings.GetInstance().GameMode);
					}
					catch (Exception ex)
					{
						Debug.LogError("Exception in SetupStart!\n" + ex.Message + "\n" + ex.StackTrace);
						LobbyManagerManager.AbortGameInProgressGracefully(LocalizationManager.GetTranslation("Network/GameControlSetupError"));
					}
					Phase = GamePhase.START;
					break;
				case GamePhase.PLACE:
					ToPlaceMode();
					break;
				case GamePhase.PLAY:
					ToPlayMode();
					break;
				case GamePhase.SUDDENDEATH:
					ToSuddenDeath();
					break;
				case GamePhase.END:
					SetupEnd();
					break;
				case GamePhase.WAIT:
					Phase = GamePhase.WAIT;
					break;
				}
			}
			switch (Phase)
			{
			case GamePhase.START:
				DoStart();
				break;
			case GamePhase.PLACE:
				DoPlaceMode();
				CheckCharactersAboveMinimum();
				break;
			case GamePhase.PLAY:
				DoPlayMode();
				CheckCharactersAboveMinimum();
				break;
			case GamePhase.SUDDENDEATH:
				DoSuddenDeath();
				CheckCharactersAboveMinimum();
				break;
			case GamePhase.END:
				DoEnd();
				break;
			}
			ResetInput();
		}
	}

	public void CheckCharactersAboveMinimum()
	{
		foreach (GamePlayer item in PlayerQueue)
		{
			Character characterInstance = item.CharacterInstance;
			if (characterInstance != null && characterInstance.Enabled && characterInstance.transform.position.y < LevelLayout.MinimumCharacterPosition)
			{
				characterInstance.OnHitLevelBottom();
			}
		}
	}

	protected virtual void ResetInput()
	{
		acceptUp = false;
		backUp = false;
		inventoryUp = false;
		pauseUp = false;
		acceptDown = false;
		backDown = false;
		inventoryDown = false;
		pauseDown = false;
		inputPlayerNumber = 0;
		pausedDownPlayer = 0;
		for (int i = 0; i < showScoreButtons.Length; i++)
		{
			showScoreButtons[i] = false;
		}
	}

	protected virtual void SetupStart(GameState.GameMode mode)
	{
		Debug.Log("Setup Start");
		GameSettings.GetInstance().OnGameStart();
		ProcessNextUnlocks();
		GameState.GetInstance().Keyboard.AddReceiver(this);
		LobbyManager.instance.AllLocal = true;
		if (Matchmaker.CurrentMatchmakingLobby != null)
		{
			MatchGuid = Matchmaker.CurrentMatchmakingLobby.GetMatchGuid();
		}
		if (base.hasAuthority)
		{
			sendClientAnalytics();
			NetworkPlayerTracker playerTracker = LobbyManager.instance.PlayerTracker;
			if (playerTracker.WaitingForIDs)
			{
				Debug.LogWarning("Player tracker is still missing NetIDs");
			}
			for (int i = 0; i < playerTracker.NumPlayers; i++)
			{
				NetworkPlayerTracker.NetPlayerInfo playerInfoByIndex = playerTracker.GetPlayerInfoByIndex(i);
				if (playerInfoByIndex.GameNetID == 0)
				{
					Debug.LogWarning("Missing NetID for game player " + playerInfoByIndex.NetworkNumber);
					continue;
				}
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfoByIndex.GameNetID));
				if (gameObject == null)
				{
					continue;
				}
				GamePlayer component = gameObject.GetComponent<GamePlayer>();
				Debug.Log("Setting up player " + component.networkNumber);
				if (component.PickedAnimal == Character.Animals.NONE)
				{
					Debug.Log("Player " + component.networkNumber + " has no character and will be removed");
					GameObject gameObject2 = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfoByIndex.LobbyNetID));
					if (!(gameObject2 != null))
					{
						continue;
					}
					LobbyPlayer component2 = gameObject2.GetComponent<LobbyPlayer>();
					if (!(component2 != null))
					{
						continue;
					}
					component2.PlayerStatus = LobbyPlayer.Status.INACTIVE;
					component2.RemovePlayer();
					for (int j = 0; j != LobbyManager.instance.lobbySlots.Length; j++)
					{
						LobbyPlayer lobbyPlayer = (LobbyPlayer)LobbyManager.instance.lobbySlots[j];
						if (lobbyPlayer != null && j == component2.networkNumber - 1)
						{
							lobbyPlayer.RemovePlayer();
						}
					}
					continue;
				}
				Character character = UnityEngine.Object.Instantiate(CharacterPrefab);
				character.gameObject.name = component.PickedAnimal.ToString();
				character.NetworkCharacterSprite = component.PickedAnimal;
				character.SetOutfitsFromArray(component.characterOutfitsList);
				character.NetworknetworkNumber = component.networkNumber;
				character.NetworklocalNumber = component.localNumber;
				character.Disable();
				character.NetworkFindPlayerOnSpawn = true;
				character.Networkpicked = true;
				NetworkServer.SpawnWithClientAuthority(character.gameObject, component.gameObject);
				Cursor cursor = UnityEngine.Object.Instantiate(CursorPrefab);
				cursor.gameObject.name = component.PickedAnimal.ToString() + " cursor";
				cursor.GetComponent<PiecePlacementCursor>().SetSprites(component.PickedAnimal);
				cursor.NetworknetworkNumber = component.networkNumber;
				cursor.NetworklocalNumber = component.localNumber;
				cursor.SetBounds(LevelLayout.GetCursorBounds());
				cursor.SetCursorColliderBounds(LevelLayout.CursorBounds);
				cursor.Disable(sound: false);
				cursor.NetworkFindPlayerOnSpawn = true;
				NetworkServer.SpawnWithClientAuthority(cursor.gameObject, component.gameObject);
				component.CallCmdAssignCharacter(character.gameObject, component.networkNumber, component.localNumber);
				component.CallCmdAssignCursor(cursor.gameObject, component.networkNumber, component.localNumber);
				PlayerQueue.Enqueue(component);
				if (component.IsLocalPlayer)
				{
					component.Control.AddReceiver(this);
					cursor.SetLocalController(component.Control);
					character.SetLocalController(component.Control);
				}
				else
				{
					LobbyManager.instance.AllLocal = false;
				}
			}
		}
		else
		{
			Debug.Log(PlayerQueue.Count + " players already enqueued");
			NetworkPlayerTracker playerTracker2 = LobbyManager.instance.PlayerTracker;
			if (playerTracker2.WaitingForIDs)
			{
				Debug.LogWarning("Player tracker is still missing NetIDs");
			}
			for (int k = 0; k < playerTracker2.NumPlayers; k++)
			{
				NetworkPlayerTracker.NetPlayerInfo playerInfoByIndex2 = playerTracker2.GetPlayerInfoByIndex(k);
				if (playerInfoByIndex2.GameNetID == 0)
				{
					Debug.LogWarning("Missing NetID for game player " + playerInfoByIndex2.NetworkNumber);
					if (playerInfoByIndex2.LobbyNetID == 0)
					{
						continue;
					}
					GameObject gameObject3 = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfoByIndex2.LobbyNetID));
					if (gameObject3 != null)
					{
						LobbyPlayer component3 = gameObject3.GetComponent<LobbyPlayer>();
						component3.PlayerStatus = LobbyPlayer.Status.INACTIVE;
						Debug.Log("Removing player with playerControllerId " + component3.playerControllerId);
						ClientScene.RemovePlayer(component3.playerControllerId);
						if (component3.CursorInstance != null)
						{
							UnityEngine.Object.Destroy(component3.CursorInstance.gameObject);
						}
					}
					continue;
				}
				GameObject gameObject4 = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfoByIndex2.GameNetID));
				if (gameObject4 == null)
				{
					continue;
				}
				GamePlayer component4 = gameObject4.GetComponent<GamePlayer>();
				if (component4.PickedAnimal <= Character.Animals.NONE)
				{
					continue;
				}
				PlayerQueue.Enqueue(component4);
				if (component4.IsLocalPlayer)
				{
					component4.Control.AddReceiver(this);
					if (component4.CharacterInstance != null)
					{
						component4.CharacterInstance.SetLocalController(component4.Control);
					}
					else
					{
						Debug.LogError("[DEBUG] local GP had no character instance");
					}
					if (component4.CursorInstance != null)
					{
						component4.CursorInstance.SetLocalController(component4.Control);
					}
					else
					{
						Debug.LogError("[DEBUG] local GP had no cursor instance");
					}
				}
				LobbyManager.instance.AllLocal = false;
			}
			Debug.Log(PlayerQueue.Count + " players enqueued");
		}
		bool flag = false;
		int count = PlayerQueue.Count;
		for (int l = 0; l < count; l++)
		{
			GamePlayer gamePlayer = PlayerQueue.Dequeue();
			if (!(gamePlayer != null))
			{
				continue;
			}
			LobbyPlayer lobbyPlayer2 = LobbyManager.instance.GetLobbyPlayer(gamePlayer.networkNumber);
			if (gamePlayer.CharacterInstance != null && lobbyPlayer2 != null)
			{
				PlayerQueue.Enqueue(gamePlayer);
				if (gamePlayer.IsLocalPlayer || lobbyPlayer2.IsLocalPlayer)
				{
					flag = true;
				}
			}
			else
			{
				UnityEngine.Object.Destroy(gamePlayer.gameObject);
				Debug.LogWarning("Player " + gamePlayer.networkNumber + " was removed from queue -- no character instance or lobby player!");
			}
		}
		if ((bool)graphPaper)
		{
			graphPaper.gameObject.SetActive(value: true);
			graphPaper.quickDisableGrid();
		}
		if (base.hasAuthority)
		{
			QuickSaver component5 = GetComponent<QuickSaver>();
			if (component5 != null)
			{
				component5.OnSetupStartLevel(OnReadyToStart);
			}
		}
		MainCamera.smoothFollowCamOn = true;
		if (!flag)
		{
			Debug.LogError("SetupStart had no valid players in the player queue");
			LobbyManagerManager.Instance.AbortGameInProgress(LocalizationManager.GetTranslation("Network/GameControlSetupError"));
		}
		else
		{
			StartCoroutine(WaitForPostSetupStart(delegate
			{
				OutfitManager.ProcessForcedOutfits();
			}));
		}
	}

	private IEnumerator queryGamesparksLobby()
	{
		bool waiting = false;
		bool found = false;
		int retries = 0;
		GamesparksMatchmakingLobby lobby = Matchmaker.CurrentMatchmakingLobby as GamesparksMatchmakingLobby;
		if (lobby == null)
		{
			yield break;
		}
		Guid zero = default(Guid);
		while (!found && retries < 5)
		{
			if (!waiting)
			{
				int num = retries + 1;
				retries = num;
				waiting = true;
				lobby.GetLobbyData(delegate(bool success)
				{
					waiting = false;
					if (success)
					{
						MatchGuid = lobby.GetMatchGuid();
						if (MatchGuid != zero)
						{
							found = true;
						}
					}
				});
				yield return new WaitForSecondsRealtime(0.5f);
			}
			yield return null;
		}
	}

	protected void NotifySceneInitDone()
	{
		Debug.Log("Marking scene init as done for local players...");
		foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
		{
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
			if (gameObject == null)
			{
				continue;
			}
			GamePlayer gp = gameObject.GetComponent<GamePlayer>();
			gp.RunAfterInitialized(delegate
			{
				if (gp.IsLocalPlayer)
				{
					gp.CallCmdSetSceneInitDone(done: true);
				}
			});
		}
	}

	protected void NotifySetupStartDone()
	{
		localSetupStartDone = true;
		Debug.Log("Marking SetupStart as done for local players...");
		foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
		{
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
			if (gameObject == null)
			{
				continue;
			}
			GamePlayer gp = gameObject.GetComponent<GamePlayer>();
			gp.RunAfterInitialized(delegate
			{
				if (gp.IsLocalPlayer)
				{
					gp.CallCmdSetupStartDone();
				}
			});
		}
	}

	private void OnReadyToStart()
	{
		if (base.hasAuthority)
		{
			if (deferredNetworkSurrogateMessages.Count > 0)
			{
				Debug.Log("Sending deferred network surrogate spawned messages");
				foreach (MsgNetworkSurrogateSpawned deferredNetworkSurrogateMessage in deferredNetworkSurrogateMessages)
				{
					NetworkServer.SendToAll(NetMsgTypes.NetworkSurrogateSpawned, deferredNetworkSurrogateMessage);
				}
				deferredNetworkSurrogateMessages.Clear();
			}
			Debug.Log("Sending signal to start phase");
			GameEventManager.SendEvent(new StartPhaseEvent(GamePhase.START));
			CallRpcStartPhase(GamePhase.START);
		}
		Phase = GamePhase.START;
	}

	protected void ProcessNextUnlocks()
	{
		foreach (KeyValuePair<LobbyPlayer, UnLockInfo> nextUnlock in GameState.GetInstance().nextUnlocks)
		{
			LobbyPlayer key = nextUnlock.Key;
			UnLockInfo unLockInfo = nextUnlock.Value;
			if (GameSettings.GetInstance().UseDebugUnlock)
			{
				unLockInfo = GameSettings.GetInstance().DebugUnlock;
			}
			if (unLockInfo != null)
			{
				MsgUnlockAvailable msgUnlockAvailable = new MsgUnlockAvailable();
				msgUnlockAvailable.UnlockType = unLockInfo.unlockType;
				msgUnlockAvailable.AssociatedCharacter = unLockInfo.AssociatedCharacter;
				msgUnlockAvailable.AssociatedLevel = unLockInfo.AssociatedLevel;
				msgUnlockAvailable.connid = LobbyManager.instance.client.connection.connectionId;
				msgUnlockAvailable.DisplayName = key.playerName;
				msgUnlockAvailable.playerLocalNumber = key.localNumber;
				switch (unLockInfo.AssociatedCharacter)
				{
				case Character.Animals.CHICKEN:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.ChickenPart;
					break;
				case Character.Animals.HORSE:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.HorsePart;
					break;
				case Character.Animals.SHEEP:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.SheepPart;
					break;
				case Character.Animals.RACCOON:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.RaccoonPart;
					break;
				case Character.Animals.CHAMELEON:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.ChameleonPart;
					break;
				case Character.Animals.SQUIRREL:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.SquirrelPart;
					break;
				case Character.Animals.ROBOT:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.BunnyPart;
					break;
				case Character.Animals.ELEPHANT:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.ElephantPart;
					break;
				case Character.Animals.MONKEY:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.MonkeyPart;
					break;
				case Character.Animals.SNAKE:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.SnakePart;
					break;
				case Character.Animals.HIPPO:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.HippoPart;
					break;
				case Character.Animals.TURTLE:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.TurtlePart;
					break;
				case Character.Animals.PANDA:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.PandaPart;
					break;
				case Character.Animals.FOX:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.FoxPart;
					break;
				case Character.Animals.PLATYPUS:
					msgUnlockAvailable.OutfitNumber = (int)unLockInfo.PlatypusPart;
					break;
				}
				LobbyManager.instance.client.Send(NetMsgTypes.UnlockAvailable, msgUnlockAvailable);
			}
		}
	}

	protected virtual void ToPlaceMode()
	{
		if (waitingForPlayers)
		{
			return;
		}
		Debug.Log("To Place phase");
		Phase = GamePhase.PLACE;
		LevelLayout.EnablePlacementBounds(enable: true);
		if (base.hasAuthority)
		{
			StartCoroutine(WaitForSetupStart(delegate
			{
				GameEventManager.SendEvent(new StartPhaseEvent(GamePhase.PLACE));
				CallRpcStartPhase(GamePhase.PLACE);
			}));
		}
		else
		{
			StartCoroutine(WaitForPhaseAndFadeOut(GamePhase.PLACE));
		}
		MainCamera.unitBuffer = false;
		MainCamera.UseDeadZone = true;
		MainCamera.ForceShowAllPlayer(showAll: false);
		destroyMarkedPieces();
	}

	protected IEnumerator WaitForPhaseAndFadeOut(GamePhase targetPhase)
	{
		UISplashScreen.STATE state = LoadingInterstitialSplash.Instance.State;
		if (state == UISplashScreen.STATE.FADING_IN || state == UISplashScreen.STATE.SHOW)
		{
			while (Phase != targetPhase)
			{
				yield return null;
			}
			LoadingInterstitialSplash.Instance.FadeOut();
		}
		postSetupStart = true;
	}

	protected IEnumerator WaitForSetupStart(UnityAction runAfter)
	{
		NetworkPlayerTracker playerTracker = LobbyManager.instance.PlayerTracker;
		if (playerTracker.WaitingForSetupStart)
		{
			Debug.Log("Waiting for all players to finish SetupStart");
			yield return null;
			float maxTimeout = 30f;
			while (playerTracker.WaitingForSetupStart && maxTimeout > 0f)
			{
				maxTimeout -= Time.unscaledDeltaTime;
				yield return null;
			}
			if (maxTimeout <= 0f)
			{
				GameObject[] playerObjects = playerTracker.GetPlayerObjects();
				for (int i = 0; i != playerObjects.Length; i++)
				{
					GameObject gameObject = playerObjects[i];
					if (!(gameObject == null))
					{
						GamePlayer component = gameObject.GetComponent<GamePlayer>();
						if (!(component == null) && (!component.SetupStartDone || !component.Initialized))
						{
							LobbyManager.instance.IssueKickMessage(component.networkNumber, LobbyManager.KickReasons.NONE);
						}
					}
				}
			}
		}
		UISplashScreen.STATE state = LoadingInterstitialSplash.Instance.State;
		if (state == UISplashScreen.STATE.FADING_IN || state == UISplashScreen.STATE.SHOW)
		{
			LoadingInterstitialSplash.Instance.FadeOut();
		}
		while (LoadingInterstitialSplash.Instance.State != UISplashScreen.STATE.HIDE)
		{
			yield return null;
		}
		runAfter();
		postSetupStart = true;
	}

	protected IEnumerator WaitForPostSetupStart(UnityAction runAfter)
	{
		while (!postSetupStart)
		{
			yield return null;
		}
		runAfter();
	}

	protected virtual void ToPlayMode()
	{
		Debug.Log("To Play phase");
		Phase = GamePhase.PLAY;
		if (GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
		{
			foreach (GamePlayer item in PlayerQueue)
			{
				item.CursorInstance.Disable(sound: false);
				MainCamera.RemoveTarget(item.CursorInstance);
			}
		}
		AfterOneFixedUpdate = true;
		LevelLayout.EnablePlacementBounds(enable: false);
		destroyMarkedPieces();
		MainCamera.smoothFollowCamOn = true;
		MainCamera.unitBuffer = true;
		MainCamera.UseDeadZone = false;
		MainCamera.ForceShowAllPlayer(showAll: false);
		levelDensity = blocksArea / LevelLayout.ComputedTotalArea;
		averageBlockChallenge = blocksChallenge / (float)placedBlocks.Count;
		nonzeroAverageChallenge = ((challengeBlocks == 0) ? 0f : (blocksChallenge / (float)challengeBlocks));
		if (base.hasAuthority)
		{
			StartCoroutine(WaitForSetupStart(delegate
			{
				GameEventManager.SendEvent(new StartPhaseEvent(GamePhase.PLAY));
				CallRpcStartPhase(GamePhase.PLAY);
			}));
		}
		else
		{
			StartCoroutine(WaitForPhaseAndFadeOut(GamePhase.PLAY));
		}
	}

	public void DestroyMarkedPiecesNow()
	{
		destroyMarkedPieces();
	}

	protected void destroyMarkedPieces()
	{
		foreach (Placeable placedBlock in placedBlocks)
		{
			if (!(placedBlock == null) && placedBlock.MarkedForDestruction)
			{
				destroyedBlocks.Add(placedBlock);
				blocksArea -= placedBlock.Area;
				blocksChallenge -= placedBlock.Challenge;
				if (placedBlock.Challenge > 0f)
				{
					challengeBlocks--;
				}
			}
		}
		foreach (Placeable destroyedBlock in destroyedBlocks)
		{
			if (placedBlocks.Contains(destroyedBlock))
			{
				placedBlocks.Remove(destroyedBlock);
			}
			UnityEngine.Object.Destroy(destroyedBlock.gameObject);
		}
		destroyedBlocks.Clear();
	}

	protected void checkAttachments()
	{
		Placeable[] array = attachments.ToArray();
		foreach (Placeable placeable in array)
		{
			if (placeable == null)
			{
				attachments.Remove(placeable);
				continue;
			}
			Placeable parentPiece = placeable.ParentPiece;
			if (parentPiece == null)
			{
				continue;
			}
			((HoneyPiece)placeable).lastReverseAttachment = null;
			Placeable[] piecesAtAttachPoints = placeable.GetPiecesAtAttachPoints();
			foreach (Placeable placeable2 in piecesAtAttachPoints)
			{
				if (placeable2 == parentPiece)
				{
					continue;
				}
				((HoneyPiece)placeable).lastReverseAttachment = placeable2;
				if (placeable2.Group != parentPiece.Group)
				{
					TryAttachPlaceables(placeable2, parentPiece, placeable);
					if (placeable2.Group == parentPiece.Group)
					{
						parentPiece.AttachedBy.Add(placeable);
						placeable2.AttachedBy.Add(placeable);
					}
				}
			}
			if (parentPiece.Group != null && placeable.Group == null)
			{
				parentPiece.Group.AddLinkNoAttach(parentPiece, placeable);
				parentPiece.AttachedBy.Add(placeable);
			}
		}
		CheckAttachmentRequiredColliders();
		foreach (Placeable placedBlock in placedBlocks)
		{
			if (!(placedBlock == null) && !placedBlock.MarkedForDestruction && !placedBlock.PickedUp)
			{
				placedBlock.UpdateSortOrder();
			}
		}
		foreach (SaveFileData activeUserSaveFileData in StatTracker.Instance.GetActiveUserSaveFileDatas())
		{
			AchievementChecker.Instance.Building_AchievementChecks(activeUserSaveFileData);
		}
	}

	public void CheckAttachmentRequiredColliders()
	{
		Placeable[] array = attachments.ToArray();
		foreach (Placeable placeable in array)
		{
			if (!placeable.PickedUp && !placeable.RequiredColliderConditionsSatisfied())
			{
				Debug.Log("Destroying Glue " + placeable.ID + " (required colliders conditions not satisfied)");
				placeable.DestroySelf();
				attachments.Remove(placeable);
			}
		}
	}

	protected virtual void AfterAFixedUpdate()
	{
		foreach (Placeable placedBlock in placedBlocks)
		{
			if (msgToDestroyBlocks.ContainsKey(placedBlock.ID) && msgToDestroyBlocks[placedBlock.ID] == LobbyManagerManager.Instance.SceneLoadCounter)
			{
				msgToDestroyBlocks.Remove(placedBlock.ID);
				placedBlock.DestroySelf(destroyChildren: false, useSmoke: true, sendNetworkSignal: false);
			}
		}
		checkAttachments();
		firstPlayFrame = false;
		switch (Phase)
		{
		case GamePhase.PLAY:
			foreach (Placeable placedBlock2 in placedBlocks)
			{
				if (!(placedBlock2 == null) && !placedBlock2.MarkedForDestruction && !placedBlock2.PickedUp)
				{
					placedBlock2.SwitchColliderTo(ColliderModeEnum.RunPhase);
				}
			}
			GameEventManager.SendEvent(new TurnOffCheckColliders());
			break;
		case GamePhase.NONE:
		case GamePhase.START:
		case GamePhase.PLACE:
		case GamePhase.SUDDENDEATH:
		case GamePhase.END:
		case GamePhase.WAIT:
			break;
		}
	}

	protected virtual void ToSuddenDeath()
	{
		Debug.Log("To Sudden Death");
		if (Phase != GamePhase.SUDDENDEATH)
		{
			AkSoundEngine.PostEvent("Plateform_Phase", base.gameObject);
		}
		Phase = GamePhase.SUDDENDEATH;
		if (base.hasAuthority)
		{
			CallRpcStartPhase(GamePhase.SUDDENDEATH);
		}
		GameEventManager.SendEvent(new StartPhaseEvent(GamePhase.SUDDENDEATH));
	}

	protected virtual void SetupEnd()
	{
		Debug.Log("Game ending");
		Phase = GamePhase.END;
		if (base.hasAuthority)
		{
			GameEventManager.SendEvent(new StartPhaseEvent(GamePhase.END));
			CallRpcStartPhase(GamePhase.END);
		}
		GameEventManager.SendEvent(new GameEndEvent(GameSettings.GetInstance().GameMode, GameState.GetInstance().SelectedLevel, LobbyManager.instance.IsInOnlineGame, gameCompleted: true, roundNumber));
		if (!LobbyManager.instance.IsInOnlineGame)
		{
			return;
		}
		int num = 0;
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null && lobbyPlayer.LocalPlayer != null)
			{
				num++;
			}
		}
		if (num < 2)
		{
			return;
		}
		foreach (SaveFileData activeUserSaveFileData in StatTracker.Instance.GetActiveUserSaveFileDatas())
		{
			AchievementChecker.Instance.Takin_On_the_World_AchievementUnlock(activeUserSaveFileData);
		}
	}

	protected virtual void DoStart()
	{
	}

	protected virtual void DoPlaceMode()
	{
	}

	protected virtual void DoPlayMode()
	{
	}

	protected virtual void DoSuddenDeath()
	{
	}

	protected virtual void DoEnd()
	{
	}

	public virtual void ShowScoreboard()
	{
	}

	public virtual void AfterScoreBoard()
	{
	}

	public void AddBlock(Placeable p)
	{
		placedBlocks.Add(p);
		ActiveBlock component = p.GetComponent<ActiveBlock>();
		if (component != null)
		{
			activeBlocks.Add(component);
		}
		if (p.HasReverseAttachments)
		{
			attachments.Add(p);
		}
	}

	protected void PrepareToLeave()
	{
		MainCamera.AllowFollow(follow: false);
		foreach (GamePlayer item in PlayerQueue)
		{
			Character characterInstance = item.CharacterInstance;
			Cursor cursorInstance = item.CursorInstance;
			item.CharacterInstance = null;
			item.CursorInstance = null;
			if (characterInstance != null)
			{
				characterInstance.transform.parent = null;
				characterInstance.Disable();
			}
			if (cursorInstance != null)
			{
				cursorInstance.transform.parent = null;
				cursorInstance.Disable(sound: false);
			}
		}
	}

	public virtual void EndGame()
	{
		fadeToLobby();
		PrepareToLeave();
		GameEventManager.SendEvent(new GameEndEvent(GameSettings.GetInstance().GameMode, GameState.GetInstance().SelectedLevel, LobbyManager.instance.IsInOnlineGame, gameCompleted: false, roundNumber));
		if (base.hasAuthority)
		{
			nextPhase = GamePhase.WAIT;
		}
	}

	public void BackToMainMenu(string abortReason = null)
	{
		if (!localSetupStartDone)
		{
			UserMessageManager.Instance.UserMessage(abortReason, 5f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
			SceneManagerWrapper.LoadScene("MainMenu");
		}
		else if (!GoingBackToMainMenu)
		{
			GoingBackToMainMenu = true;
			Controller.RemoveGlobalReceiver(this);
			if (fadeOut != null)
			{
				fadeOut.FadeIn();
			}
			else
			{
				Debug.LogError("fadeOut is null");
			}
			if (GetComponent<NetworkIdentity>().isServer)
			{
				LobbyManagerManager.Instance.StartCoroutine(FadeToLevel("MainMenu", abortReason));
				PrepareToLeave();
			}
			else
			{
				LobbyManagerManager.Instance.StartCoroutine(FadeToLevel("MainMenu", abortReason));
			}
		}
	}

	protected virtual void resetTurnOrders()
	{
		if (PlayerQueue == null || PlayerQueue.Count == 0)
		{
			return;
		}
		bool flag = true;
		int num = 0;
		while (flag)
		{
			GamePlayer gamePlayer = null;
			foreach (GamePlayer item in PlayerQueue)
			{
				if (item.TurnOrder == num)
				{
					num++;
					gamePlayer = null;
					if (num >= PlayerQueue.Count)
					{
						flag = false;
						break;
					}
				}
				else if (gamePlayer == null || (item.TurnOrder > num && item.TurnOrder < gamePlayer.TurnOrder))
				{
					gamePlayer = item;
				}
			}
			if (gamePlayer != null)
			{
				gamePlayer.TurnOrder = num;
			}
		}
	}

	protected IEnumerator FadeOutStay(string levelSceneName)
	{
		if (fadeOut != null)
		{
			fadeOut.FadeIn();
			while (fadeOut.State != UISplashScreen.STATE.SHOW)
			{
				yield return null;
			}
		}
		else
		{
			Debug.LogError("fadeOut is null");
		}
		CleanUpSceneForLoad();
		yield return new WaitForSeconds(0.2f);
		yield return Resources.UnloadUnusedAssets();
		if (!levelSceneName.NullOrEmpty() && base.hasAuthority)
		{
			LobbyManager.instance.ServerChangeScene(levelSceneName);
		}
	}

	protected IEnumerator FadeToLevel(string level, string abortReason = null)
	{
		if (fadeOut != null)
		{
			fadeOut.FadeIn();
			while (fadeOut.State != UISplashScreen.STATE.SHOW)
			{
				yield return null;
			}
		}
		else
		{
			Debug.LogError("fadeOut is null");
		}
		if (base.hasAuthority)
		{
			CallRpcPlayMusic("MUS_Menu_Start");
			switch (GameSettings.GetInstance().GameMode)
			{
			case GameState.GameMode.FREEPLAY:
				CallRpcPlayMusic("Lobby_Freeplay");
				break;
			case GameState.GameMode.CREATIVE:
				CallRpcPlayMusic("Lobby_Normal");
				break;
			case GameState.GameMode.PARTY:
				CallRpcPlayMusic("Lobby_PartyMode");
				break;
			case GameState.GameMode.CHALLENGE:
				CallRpcPlayMusic("Lobby_Challenge");
				break;
			}
		}
		if (level == "MainMenu")
		{
			LobbyManagerManager.Instance.AbortGameInProgress(abortReason);
			yield break;
		}
		IEnumerator gentleLoad = SceneManagerWrapper.DoGentleSceneLoad(level);
		while (gentleLoad.MoveNext())
		{
			yield return null;
		}
	}

	protected void fadeToLobby()
	{
		if (base.hasAuthority)
		{
			CallRpcStartFadingOut(toMainMenu: false, "TreeHouseLobby");
			CallRpcPlayMusic("MUS_Menu_Start");
			switch (GameSettings.GetInstance().GameMode)
			{
			case GameState.GameMode.FREEPLAY:
				CallRpcPlayMusic("Lobby_Freeplay");
				break;
			case GameState.GameMode.CREATIVE:
				CallRpcPlayMusic("Lobby_Normal");
				break;
			case GameState.GameMode.PARTY:
				CallRpcPlayMusic("Lobby_PartyMode");
				break;
			case GameState.GameMode.CHALLENGE:
				CallRpcPlayMusic("Lobby_Challenge");
				break;
			}
		}
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(HoldRespawnEvent))
		{
			HoldRespawnEvent holdRespawnEvent = e as HoldRespawnEvent;
			AllowRespawn = !holdRespawnEvent.Hold;
		}
		if (type == typeof(GameStartEvent))
		{
			GameStartEvent gameStartEvent = e as GameStartEvent;
			if (base.hasAuthority)
			{
				SetupStart(gameStartEvent.GameMode);
			}
		}
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			switch (startPhaseEvent.Phase)
			{
			case GamePhase.PLAY:
			case GamePhase.PLACE:
			case GamePhase.SUDDENDEATH:
			case GamePhase.END:
				nextPhase = startPhaseEvent.Phase;
				Modifiers.GetInstance().OnModifiersDynamicChange();
				break;
			case GamePhase.START:
				LevelLayout.AddStartAndGoalsToCameraTargets(MainCamera);
				break;
			}
			foreach (GamePlayer item in PlayerQueue)
			{
				if (item != null && item.IsLocalPlayer)
				{
					item.SetInPhase(startPhaseEvent.Phase);
				}
			}
		}
		if (type == typeof(EndPhaseEvent) && (e as EndPhaseEvent).Phase == GamePhase.START)
		{
			LevelLayout.RemoveStartAndGoalsFromCameraTargets(MainCamera);
		}
		if (type == typeof(PauseEvent))
		{
			PauseEvent pauseEvent = e as PauseEvent;
			paused = pauseEvent.Paused;
		}
		if (type == typeof(SoftPauseEvent))
		{
			SoftPauseEvent softPauseEvent = e as SoftPauseEvent;
			softPaused = softPauseEvent.SoftPaused;
		}
		if (type == typeof(ScoreboardEvent))
		{
			ScoreboardEvent scoreboardEvent = e as ScoreboardEvent;
			if (scoreboardEvent.Showing)
			{
				if (!scoreboard)
				{
					scoreboard = true;
					ShowScoreboard();
				}
			}
			else if (scoreboardEvent.AfterTally)
			{
				AfterScoreBoard();
			}
			else
			{
				scoreboard = false;
			}
		}
		if (type == typeof(DestroyPieceEvent))
		{
			DestroyPieceEvent destroyPieceEvent = e as DestroyPieceEvent;
			QuickSaver component = GetComponent<QuickSaver>();
			if (!destroyPieceEvent.Piece.IsSaveable || destroyPieceEvent.Piece is GoalBlock)
			{
				component.OnUnsaveablePieceDestroyed(destroyPieceEvent.Piece);
			}
			else
			{
				component.OnPieceDestroyed(destroyPieceEvent.Piece);
			}
		}
		if (type == typeof(PiecePlacedEvent))
		{
			Placeable placedBlock = (e as PiecePlacedEvent).PlacedBlock;
			if (placedBlock != null && !placedBlocks.Contains(placedBlock))
			{
				placedBlocks.Add(placedBlock);
				blocksArea += placedBlock.Area;
				blocksChallenge += placedBlock.Challenge;
				if (placedBlock.Challenge > 0f)
				{
					challengeBlocks++;
				}
				ActiveBlock[] componentsInChildren = placedBlock.GetComponentsInChildren<ActiveBlock>();
				if (componentsInChildren != null)
				{
					ActiveBlock[] array = componentsInChildren;
					foreach (ActiveBlock activeBlock in array)
					{
						if (activeBlock != null)
						{
							activeBlocks.Add(activeBlock);
						}
						if (activeBlock.HasReverseAttachments)
						{
							attachments.Add(activeBlock);
						}
					}
				}
				MultipieceBlock component2 = placedBlock.GetComponent<MultipieceBlock>();
				if (component2 != null)
				{
					MultipiecePart[] parts = component2.Parts;
					foreach (MultipiecePart multipiecePart in parts)
					{
						if (multipiecePart != null)
						{
							placedBlocks.Add(multipiecePart);
							ActiveBlock component3 = multipiecePart.GetComponent<ActiveBlock>();
							if (component3 != null)
							{
								activeBlocks.Add(component3);
							}
							if (multipiecePart.HasReverseAttachments && !attachments.Contains(multipiecePart))
							{
								attachments.Add(multipiecePart);
							}
							blocksArea += multipiecePart.Area;
							blocksChallenge += multipiecePart.Challenge;
							if (multipiecePart.Challenge > 0f)
							{
								challengeBlocks++;
							}
						}
					}
				}
			}
			if (Phase != GamePhase.PLAY)
			{
				AfterOneFixedUpdate = true;
			}
		}
		if (type == typeof(NetworkCursorSpawnedEvent))
		{
			NetworkCursorSpawnedEvent networkCursorSpawnedEvent = e as NetworkCursorSpawnedEvent;
			if (!base.hasAuthority && networkCursorSpawnedEvent.SpawnedCursor is PiecePlacementCursor)
			{
				Debug.Log("Setting bounds for cursor " + networkCursorSpawnedEvent.SpawnedCursor.name + ": " + LevelLayout.CursorBounds.bounds.ToString());
				networkCursorSpawnedEvent.SpawnedCursor.SetBounds(LevelLayout.GetCursorBounds());
				networkCursorSpawnedEvent.SpawnedCursor.SetCursorColliderBounds(LevelLayout.CursorBounds);
			}
		}
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.NetworkSurrogateSpawned)
			{
				MsgNetworkSurrogateSpawned msgNetworkSurrogateSpawned = networkMessageReceivedEvent.ReadMessage as MsgNetworkSurrogateSpawned;
				GameObject gameObject = ClientScene.FindLocalObject(msgNetworkSurrogateSpawned.NetSurrogateID);
				if (gameObject != null)
				{
					NetworkSurrogate component4 = gameObject.GetComponent<NetworkSurrogate>();
					bool flag = false;
					foreach (Placeable placedBlock2 in placedBlocks)
					{
						if (placedBlock2.ID == msgNetworkSurrogateSpawned.SpawnedForPieceID)
						{
							gameObject.transform.SetParent(placedBlock2.transform, worldPositionStays: false);
							gameObject.transform.localPosition = Vector3.zero;
							placedBlock2.NetSurrogate = component4;
							string text = msgNetworkSurrogateSpawned.SpawnedForPieceID.ToString();
							NetworkInstanceId netSurrogateID = msgNetworkSurrogateSpawned.NetSurrogateID;
							Debug.Log("Found placeable " + text + " for netsurrogate with ID " + netSurrogateID.ToString());
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Debug.LogError("Could not attach spawned netsurrogate to object with ID " + msgNetworkSurrogateSpawned.SpawnedForPieceID + ": local block not found.");
						if (!orphanedNetSurrogates.ContainsKey(msgNetworkSurrogateSpawned.SpawnedForPieceID))
						{
							orphanedNetSurrogates.Add(msgNetworkSurrogateSpawned.SpawnedForPieceID, component4);
						}
						else
						{
							Debug.LogError("An orphaned net surrogate has already been registered for placeable " + msgNetworkSurrogateSpawned.SpawnedForPieceID);
						}
					}
				}
				else
				{
					NetworkInstanceId netSurrogateID = msgNetworkSurrogateSpawned.NetSurrogateID;
					Debug.LogError("Could not find local netsurrogate object with ID " + netSurrogateID.ToString());
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PieceDestroyed)
			{
				MsgPieceDestroyed msgPieceDestroyed = networkMessageReceivedEvent.ReadMessage as MsgPieceDestroyed;
				if (!LobbyManager.instance.IsLocalNetworkNumber(msgPieceDestroyed.MachineNetworkNumber))
				{
					bool flag2 = false;
					foreach (Placeable placedBlock3 in placedBlocks)
					{
						if (placedBlock3.ID == msgPieceDestroyed.BlockID && msgPieceDestroyed.SceneLoadNumber == LobbyManagerManager.Instance.SceneLoadCounter)
						{
							if (placedBlock3 != null)
							{
								placedBlock3.DestroySelf(destroyChildren: false, useSmoke: true, sendNetworkSignal: false);
								flag2 = true;
							}
							break;
						}
					}
					if (!flag2)
					{
						try
						{
							msgToDestroyBlocks.Add(msgPieceDestroyed.BlockID, msgPieceDestroyed.SceneLoadNumber);
						}
						catch (Exception)
						{
							Debug.LogWarning("Block already marked for destruction: " + msgPieceDestroyed.BlockID);
						}
					}
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.UnlockAvailable)
			{
				MsgUnlockAvailable msgUnlockAvailable = (MsgUnlockAvailable)networkMessageReceivedEvent.ReadMessage;
				UnLockInfo unLockInfo = null;
				UnLockInfo unLockInfo2 = null;
				switch (msgUnlockAvailable.UnlockType)
				{
				case UnLockInfo.UnlockType.Character:
					if (msgUnlockAvailable.AssociatedCharacter != Character.Animals.NONE)
					{
						unLockInfo2 = UnlockInfoLibrary.Instance.GetCharacterUnlock(msgUnlockAvailable.AssociatedCharacter);
						if (unLockInfo2 != null)
						{
							unLockInfo = UnityEngine.Object.Instantiate(unLockInfo2);
						}
					}
					break;
				case UnLockInfo.UnlockType.Outfit:
					unLockInfo2 = UnlockInfoLibrary.Instance.GetOutfitUnlock(msgUnlockAvailable.AssociatedCharacter, msgUnlockAvailable.OutfitNumber);
					if (unLockInfo2 != null)
					{
						unLockInfo = UnityEngine.Object.Instantiate(unLockInfo2);
					}
					break;
				case UnLockInfo.UnlockType.Level:
					unLockInfo2 = UnlockInfoLibrary.Instance.GetLevelUnlock(msgUnlockAvailable.AssociatedLevel);
					if (unLockInfo2 != null)
					{
						unLockInfo = UnityEngine.Object.Instantiate(unLockInfo2);
					}
					break;
				}
				if (unLockInfo != null)
				{
					unLockInfo.DisplayName = msgUnlockAvailable.DisplayName;
					unLockInfo.unlockType = msgUnlockAvailable.UnlockType;
					unLockInfo.IsLocal = msgUnlockAvailable.connid == LobbyManager.instance.client.connection.connectionId;
					unLockInfo.forPlayerLocalNumber = msgUnlockAvailable.playerLocalNumber;
					CreateUnlockBox(unLockInfo);
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.NetworkClientDisconnected)
			{
				MsgNetworkClientDisconnected msgNetworkClientDisconnected = networkMessageReceivedEvent.ReadMessage as MsgNetworkClientDisconnected;
				quits++;
				if (base.hasAuthority && playerNumbersStillLoadingSnapshot != null)
				{
					playerNumbersStillLoadingSnapshot.Remove(msgNetworkClientDisconnected.PlayerNetworkNumber);
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ClientKicked)
			{
				kicks++;
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SnapshotLoadingDone)
			{
				MsgSnapshotLoadingDone msgSnapshotLoadingDone = networkMessageReceivedEvent.ReadMessage as MsgSnapshotLoadingDone;
				if (base.hasAuthority && playerNumbersStillLoadingSnapshot != null)
				{
					playerNumbersStillLoadingSnapshot.Remove(msgSnapshotLoadingDone.PlayerNetworkNumber);
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SwitchToMode)
			{
				MsgSwitchToMode msgSwitchToMode = networkMessageReceivedEvent.ReadMessage as MsgSwitchToMode;
				GameSettings.GetInstance().GameMode = msgSwitchToMode.toMode;
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PrepareToReloadScene)
			{
				Debug.Log("Received message to reload scene");
				MsgPrepareToReloadScene msgPrepareToReloadScene = networkMessageReceivedEvent.ReadMessage as MsgPrepareToReloadScene;
				GameSettings.GetInstance().GameMode = msgPrepareToReloadScene.reloadToMode;
				GameState.GetInstance().currentSnapshotInfo = msgPrepareToReloadScene.snapshotInfo;
				if (!base.hasAuthority)
				{
					LoadingInterstitialSplash.Instance.showLevelInfoNextLoad = true;
					LoadingInterstitialSplash.Instance.FadeIn();
				}
				foreach (GamePlayer item2 in PlayerQueue)
				{
					if (item2.IsLocalPlayer || base.hasAuthority)
					{
						item2.CallCmdSetSceneInitDone(done: false);
					}
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PunchingBlockTriggered)
			{
				MsgPunchingBlockTriggered msgPunchingBlockTriggered = networkMessageReceivedEvent.ReadMessage as MsgPunchingBlockTriggered;
				if (!LobbyManager.instance.IsLocalNetworkNumber(msgPunchingBlockTriggered.playerNumber))
				{
					foreach (Placeable placedBlock4 in placedBlocks)
					{
						if (placedBlock4.ID == msgPunchingBlockTriggered.blockID)
						{
							PunchingBlock punchingBlock = placedBlock4 as PunchingBlock;
							if (punchingBlock != null)
							{
								punchingBlock.ProcessTriggerMessage(msgPunchingBlockTriggered);
							}
							break;
						}
					}
				}
			}
		}
		if (type == typeof(DrivingPlayerRemovedEvent))
		{
			Debug.Log("Driving player was logged out. Returning to main menu");
			BackToMainMenu();
		}
		if (type == typeof(PlatformPlayerRemovedEvent))
		{
			PlatformPlayerRemovedEvent platformPlayerRemovedEvent = e as PlatformPlayerRemovedEvent;
			if (platformPlayerRemovedEvent.RemovedPlayer != null && platformPlayerRemovedEvent.RemovedPlayer.AssociatedGamePlayer != null)
			{
				platformPlayerRemovedEvent.RemovedPlayer.AssociatedGamePlayer.RemovePlayer();
			}
		}
		if (type == typeof(QuicksaverLevelFinishedLoading))
		{
			MainCamera.SetBounds(LevelLayout.GetCameraBounds());
			PiecePlacementCursor[] array2 = UnityEngine.Object.FindObjectsOfType<PiecePlacementCursor>();
			foreach (PiecePlacementCursor piecePlacementCursor in array2)
			{
				if (piecePlacementCursor != null)
				{
					piecePlacementCursor.SetBounds(LevelLayout.GetCursorBounds());
				}
			}
		}
		if (type == typeof(ControllerConnectionEvent))
		{
			ControllerConnectionEvent controllerConnectionEvent = e as ControllerConnectionEvent;
			if (controllerConnectionEvent.Connected && controllerConnectionEvent.Player != null && controllerConnectionEvent.Player.UseController != null)
			{
				controllerConnectionEvent.Player.UseController.AddReceiver(this);
			}
		}
	}

	public void CreateUnlockBox(UnLockInfo unlockInfo)
	{
		int num = (unlockOffset + unlockNumber) % LevelLayout.UnlockSpawnLocations.Length;
		if (GameSettings.GetInstance().UseDebugUnlockPosition)
		{
			num = GameSettings.GetInstance().DebugUnlockPosition;
		}
		Vector3 position = LevelLayout.UnlockSpawnLocations[num].transform.position;
		UnLockBox unLockBox = UnityEngine.Object.Instantiate(UnLockBoxPrefab, position, Quaternion.identity);
		unLockBox.SetupUnlockTextAndImage(unlockInfo);
		activeBlocks.Add(unLockBox);
		placedBlocks.Add(unLockBox);
		unlockNumber++;
	}

	private IEnumerator waitForNetworkPlayers()
	{
		if (!base.hasAuthority)
		{
			yield break;
		}
		NetworkPlayerTracker playerTracker = LobbyManager.instance.PlayerTracker;
		bool allIn = false;
		while (!allIn)
		{
			allIn = true;
			for (int i = 0; i != playerTracker.NumPlayers; i++)
			{
				NetworkPlayerTracker.NetPlayerInfo playerInfoByIndex = playerTracker.GetPlayerInfoByIndex(i);
				if (playerInfoByIndex.GameNetID != 0)
				{
					continue;
				}
				if (playerInfoByIndex.LobbyNetID == 0)
				{
					break;
				}
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfoByIndex.LobbyNetID));
				if (gameObject != null)
				{
					LobbyPlayer component = gameObject.GetComponent<LobbyPlayer>();
					if (component.PlayerStatus != LobbyPlayer.Status.INACTIVE && component.PlayerStatus != LobbyPlayer.Status.CURSOR)
					{
						allIn = false;
					}
				}
				break;
			}
			yield return null;
		}
		while (playerTracker.WaitingForIDs)
		{
			yield return null;
		}
		while (playerTracker.WaitingForGamePlayerInit)
		{
			yield return null;
		}
		NotifySceneInitDone();
		CallRpcHostReadyBeacon();
		float beaconTimer = 0f;
		float beaconInterval = 0.5f;
		float maxTimeout = 30f;
		Debug.Log("Waiting for scene init");
		while (playerTracker.WaitingForSceneInit && maxTimeout > 0f)
		{
			yield return null;
			maxTimeout -= Time.unscaledDeltaTime;
			beaconTimer += Time.unscaledDeltaTime;
			if (beaconTimer >= beaconInterval)
			{
				CallRpcHostReadyBeacon();
				beaconTimer = 0f;
			}
		}
		if (maxTimeout <= 0f)
		{
			GameObject[] playerObjects = playerTracker.GetPlayerObjects();
			for (int j = 0; j != playerObjects.Length; j++)
			{
				GameObject gameObject2 = playerObjects[j];
				if (!(gameObject2 == null))
				{
					GamePlayer component2 = gameObject2.GetComponent<GamePlayer>();
					if (!(component2 == null) && (!component2.SceneInitDone || !component2.Initialized))
					{
						LobbyManager.instance.IssueKickMessage(component2.networkNumber, LobbyManager.KickReasons.NONE);
						component2.connectionToClient.Disconnect();
					}
				}
			}
		}
		if (LobbyManager.instance.reloadingScene)
		{
			yield return new WaitForSeconds(2f);
			LobbyManager.instance.reloadingScene = false;
		}
		NetworkwaitingForPlayers = false;
		Debug.Log("Done waiting for players");
		CallRpcDoInitialPlacement();
		GameEventManager.SendEvent(new GameStartEvent(GameSettings.GetInstance().GameMode, GameState.GetInstance().SelectedLevel, GameState.GetInstance().currentSnapshotInfo.snapshotCode));
	}

	public virtual void ReceiveEvent(InputEvent e)
	{
		inputPlayerNumber = 0;
		if ((e.PlayerBitMask & 1) == 1)
		{
			inputPlayerNumber = 1;
		}
		else if ((e.PlayerBitMask & 2) == 2)
		{
			inputPlayerNumber = 2;
		}
		else if ((e.PlayerBitMask & 4) == 4)
		{
			inputPlayerNumber = 3;
		}
		else if ((e.PlayerBitMask & 8) == 8)
		{
			inputPlayerNumber = 4;
		}
		switch (e.Key)
		{
		case InputEvent.InputKey.Accept:
			if (inputPlayerNumber <= 0)
			{
				break;
			}
			accept = e.Valueb;
			if (e.Changed)
			{
				if (accept)
				{
					acceptDown = true;
				}
				else
				{
					acceptUp = true;
				}
			}
			break;
		case InputEvent.InputKey.Back:
			back = e.Valueb;
			if (e.Changed)
			{
				if (back)
				{
					backDown = true;
				}
				else
				{
					backUp = true;
				}
			}
			break;
		case InputEvent.InputKey.Inventory:
			inventory = e.Valueb;
			if (e.Changed)
			{
				if (inventory)
				{
					inventoryDown = true;
				}
				else
				{
					inventoryUp = true;
				}
			}
			break;
		case InputEvent.InputKey.Pause:
			pause = e.Valueb;
			if (!e.Changed)
			{
				break;
			}
			if (pause && !GameState.GetInstance().Paused && pauseLimiter <= 0f && Phase != GamePhase.START)
			{
				pauseDown = true;
				GamePlayer gamePlayer = null;
				if (GameState.GetInstance().UsingHotSeat)
				{
					if ((Phase == GamePhase.PLAY || Phase == GamePhase.SUDDENDEATH) && (e.PlayerBitMask & (1 << PlayerQueue.Peek().localNumber - 1)) != 0)
					{
						gamePlayer = PlayerQueue.Peek();
					}
					else if (Phase == GamePhase.PLACE)
					{
						GamePlayer gamePlayer2 = null;
						foreach (GamePlayer item in PlayerQueue)
						{
							if ((e.PlayerBitMask & (1 << item.localNumber - 1)) != 0)
							{
								gamePlayer2 = item;
							}
						}
						gamePlayer = gamePlayer2;
					}
				}
				else
				{
					foreach (GamePlayer item2 in PlayerQueue)
					{
						if (!(item2 == null) && item2.IsLocalPlayer && item2.localNumber == inputPlayerNumber)
						{
							gamePlayer = item2;
						}
					}
				}
				if (gamePlayer != null)
				{
					if (!LobbyManager.instance.IsInOnlineGame)
					{
						pauseLimiter = pauseLimitTimer;
						GameState.GetInstance().Paused = true;
						GameEventManager.SendEvent(new PauseEvent(pause: true, gamePlayer.networkNumber));
					}
					else if (!gamePlayer.CharacterInstance.SoftPaused)
					{
						GameEventManager.SendEvent(new SoftPauseEvent(softpause: true, gamePlayer.networkNumber, GetComponent<NetworkIdentity>().isServer));
					}
					else
					{
						GameEventManager.SendEvent(new SoftPauseEvent(softpause: false, gamePlayer.networkNumber, GetComponent<NetworkIdentity>().isServer));
					}
				}
			}
			else
			{
				pauseUp = true;
			}
			break;
		case InputEvent.InputKey.Scoreboard:
			showScoreButtons[inputPlayerNumber - 1] = e.Valueb;
			if (!PlayerCanShowScoreboard(inputPlayerNumber))
			{
				showScoreButtons[inputPlayerNumber - 1] = false;
			}
			break;
		}
	}

	private bool PlayerCanShowScoreboard(int inputPlayerNumber)
	{
		bool flag = false;
		int num = 0;
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		if ((uint)(gameMode - 1) <= 1u)
		{
			foreach (GamePlayer item in PlayerQueue)
			{
				if (!(item != null) || !item.IsLocalPlayer)
				{
					continue;
				}
				Character characterInstance = item.CharacterInstance;
				if (characterInstance != null && (characterInstance.Dead || characterInstance.Dying) && !characterInstance.isGhost)
				{
					if (item.localNumber == inputPlayerNumber)
					{
						flag = true;
					}
				}
				else
				{
					num++;
				}
			}
		}
		if (flag)
		{
			return num == 0;
		}
		return true;
	}

	protected void DrawDebugText(string text)
	{
		DrawDebugText(text, Color.black, Color.white);
	}

	protected void DrawDebugText(string text, Color color, Color bg)
	{
		int num = 30;
		GUI.color = bg;
		debugStyle.normal.textColor = bg;
		GUI.Label(new Rect(0f, num * debugTextRow, 1024f, num), text, debugStyle);
		GUI.Label(new Rect(2f, num * debugTextRow, 1024f, num), text, debugStyle);
		GUI.Label(new Rect(2f, 2 + num * debugTextRow, 1024f, num), text, debugStyle);
		GUI.Label(new Rect(0f, 2 + num * debugTextRow, 1024f, num), text, debugStyle);
		GUI.color = color;
		debugStyle.normal.textColor = color;
		GUI.Label(new Rect(1f, 1 + num * debugTextRow, 1024f, num), text, debugStyle);
		debugTextRow++;
	}

	protected virtual void DrawDebug()
	{
		if (GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
		{
			DrawDebugText("Frame Time: " + Mathf.Round(Time.unscaledDeltaTime * 1000f) + "ms");
		}
	}

	[ClientRpc]
	private void RpcStartPhase(GamePhase phase)
	{
		if (!base.hasAuthority)
		{
			Debug.Log("Receiving signal to start phase " + phase);
			GameEventManager.SendEvent(new EndPhaseEvent(Phase));
			GameEventManager.SendEvent(new StartPhaseEvent(phase));
			nextPhase = phase;
		}
	}

	[ClientRpc]
	public void RpcPlayMusic(string musicName)
	{
		AkSoundEngine.PostEvent(musicName, base.gameObject);
	}

	[ClientRpc]
	protected void RpcStartFadingOut(bool toMainMenu, string levelSceneName)
	{
		if (toMainMenu)
		{
			AkSoundEngine.PostEvent("MUS_Menu_Start", base.gameObject);
			LobbyManagerManager.Instance.StartCoroutine(FadeToLevel("MainMenu"));
		}
		else
		{
			LobbyManagerManager.Instance.StartCoroutine(FadeOutStay(levelSceneName));
		}
		sendEndAnalytics();
	}

	protected virtual void CleanUpSceneForLoad()
	{
		CleanUpStarted = true;
		for (int i = 0; i < LevelLayout.DeletePreUnload.Length; i++)
		{
			GameObject go = LevelLayout.DeletePreUnload[i];
			CheckNullAndDestroy(go);
		}
		CheckNullAndDestroy(LevelLayout);
		Placeable[] array = new Placeable[Placeable.AllPlaceables.Count];
		Placeable.AllPlaceables.CopyTo(array);
		CheckNullAndDestroy(invBookInstance);
		CheckNullAndDestroy(graphPaper);
		for (int j = 0; j < array.Length; j++)
		{
			CheckNullAndDestroy(array[j]);
		}
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item != null)
			{
				if (item.CharacterInstance != null)
				{
					CheckNullAndDestroy(item.CharacterInstance);
				}
				if (item.CursorInstance != null)
				{
					CheckNullAndDestroy(item.CursorInstance);
				}
			}
		}
	}

	public void SpawnNetSurrogate(int spawnForBlockID)
	{
		Debug.Log("Spawning network surrogate");
		NetworkSurrogate networkSurrogate = UnityEngine.Object.Instantiate(NetSurrogatePrefab);
		NetworkServer.Spawn(networkSurrogate.gameObject);
		MsgNetworkSurrogateSpawned msgNetworkSurrogateSpawned = new MsgNetworkSurrogateSpawned();
		msgNetworkSurrogateSpawned.NetSurrogateID = networkSurrogate.netId;
		msgNetworkSurrogateSpawned.SpawnedForPieceID = spawnForBlockID;
		if (PlayersStillLoadingSnapshot)
		{
			deferredNetworkSurrogateMessages.Add(msgNetworkSurrogateSpawned);
		}
		else
		{
			NetworkServer.SendToAll(NetMsgTypes.NetworkSurrogateSpawned, msgNetworkSurrogateSpawned);
		}
	}

	[ClientRpc]
	public void RpcPropagateBlockIDs(string[] paths, int[] IDs)
	{
		if (base.hasAuthority)
		{
			return;
		}
		Debug.Log("Propagating initial level placeable IDs on client...");
		if (paths.Length == IDs.Length)
		{
			for (int i = 0; i < paths.Length; i++)
			{
				Transform transformFromHierarchyPath = QuickSaver.GetTransformFromHierarchyPath(paths[i]);
				if (transformFromHierarchyPath != null)
				{
					transformFromHierarchyPath.GetComponent<Placeable>().ID = IDs[i];
				}
				else
				{
					Debug.LogError("PropagateBlockIDs: Could not find placeable " + IDs[i] + " at path: " + paths[i]);
				}
			}
			QuickSaver component = GetComponent<QuickSaver>();
			if (component != null)
			{
				component.OnClientBlockIDsPropagated();
			}
			else
			{
				Debug.LogError("QuickSaver component not found on game controller!");
			}
		}
		else
		{
			Debug.LogError("Cannot propagate block IDs: paths and IDs have different lengths");
		}
	}

	public void CompressAndSendSnapshotBytes(byte[] bytes, UnityAction onAllClientsLoadedSnapshot)
	{
		if (!base.hasAuthority)
		{
			return;
		}
		if (bytes[0] == 60)
		{
			int num = bytes.Length;
			bytes = SevenZipHelper.Compress(bytes);
			Debug.Log("Compressed " + num + " byte snapshot to " + bytes.Length + " bytes.");
		}
		else
		{
			Debug.Log("Snapshot was already compressed (Size: " + bytes.Length + " bytes).");
		}
		playerNumbersStillLoadingSnapshot = new List<int>();
		foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
		{
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
			if (!(gameObject == null))
			{
				GamePlayer component = gameObject.GetComponent<GamePlayer>();
				if (!component.IsLocalPlayer)
				{
					playerNumbersStillLoadingSnapshot.Add(component.networkNumber);
				}
			}
		}
		if (bytes.Length < 31743)
		{
			CallRpcLoadCompressedSnapshot(bytes);
		}
		else
		{
			CallRpcSetUpCompressedSnapshotTransfer(bytes.Length);
			byte[] array = new byte[31743];
			for (int i = 0; i < bytes.Length; i += 31743)
			{
				int num2 = Mathf.Min(31743, bytes.Length - i);
				if (num2 < 31743)
				{
					array = new byte[num2];
				}
				Array.Copy(bytes, i, array, 0, num2);
				CallRpcSendCompressedSnapshotChunk(array);
			}
		}
		StartCoroutine(WaitForAllClientsToLoadSnapshot(onAllClientsLoadedSnapshot));
	}

	private IEnumerator WaitForAllClientsToLoadSnapshot(UnityAction onAllClientsLoadedSnapshot)
	{
		while (PlayersStillLoadingSnapshot)
		{
			yield return null;
		}
		Debug.Log("All clients are done loading the snapshot!");
		onAllClientsLoadedSnapshot();
	}

	[ClientRpc]
	public void RpcSetUpCompressedSnapshotTransfer(int bufferLength)
	{
		if (!base.hasAuthority)
		{
			snapshotReceiveBuffer = new byte[bufferLength];
			snapshotReceiveBufferIdx = 0;
		}
	}

	[ClientRpc]
	public void RpcSendCompressedSnapshotChunk(byte[] buffer)
	{
		if (base.hasAuthority)
		{
			return;
		}
		Array.Copy(buffer, 0, snapshotReceiveBuffer, snapshotReceiveBufferIdx, buffer.Length);
		snapshotReceiveBufferIdx += buffer.Length;
		if (snapshotReceiveBufferIdx == snapshotReceiveBuffer.Length)
		{
			QuickSaver component = GetComponent<QuickSaver>();
			if (component != null)
			{
				component.LoadCompressedSnapshotThreaded(snapshotReceiveBuffer, LoadDecompressedSnapshotXmlDocument);
			}
			snapshotReceiveBuffer = null;
			snapshotReceiveBufferIdx = 0;
		}
	}

	[ClientRpc]
	public void RpcLoadCompressedSnapshot(byte[] compressedBytes)
	{
		if (!base.hasAuthority)
		{
			QuickSaver component = GetComponent<QuickSaver>();
			if (component != null)
			{
				component.LoadCompressedSnapshotThreaded(compressedBytes, LoadDecompressedSnapshotXmlDocument);
			}
		}
	}

	public void LoadDecompressedSnapshotXmlDocument(XmlDocument doc)
	{
		if (doc != null)
		{
			QuickSaver component = GetComponent<QuickSaver>();
			if (component != null)
			{
				if (!component.LoadSnapshotFromXmlDocument(doc))
				{
					return;
				}
				QuickSaver.lastLoadedXml = doc.OuterXml;
				Debug.Log("Successfully loaded snapshot from XML Document");
				if (base.hasAuthority)
				{
					return;
				}
				{
					foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
					{
						GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
						if (!(gameObject == null))
						{
							GamePlayer component2 = gameObject.GetComponent<GamePlayer>();
							if (component2.IsLocalPlayer)
							{
								MsgSnapshotLoadingDone msgSnapshotLoadingDone = new MsgSnapshotLoadingDone();
								msgSnapshotLoadingDone.PlayerNetworkNumber = component2.networkNumber;
								LobbyManager.instance.client.Send(NetMsgTypes.SnapshotLoadingDone, msgSnapshotLoadingDone);
							}
						}
					}
					return;
				}
			}
			Debug.LogError("QuickSaver component not found on game controller!");
		}
		else
		{
			Debug.LogError("Loading decompressed snapshot XML failed!");
		}
	}

	[ClientRpc]
	private void RpcHostReadyBeacon()
	{
		hostBeaconReceived = true;
	}

	[ClientRpc]
	private void RpcDoInitialPlacement()
	{
		StartCoroutine(initialPlacement());
	}

	public bool CurrentLevelHasGoal()
	{
		if (LevelLayout.Goal == null && UnityEngine.Object.FindObjectOfType<GoalBlock>() == null)
		{
			return false;
		}
		return true;
	}

	private void TryAttachPlaceables(Placeable pt, Placeable parent, Placeable attach)
	{
		if ((pt.isSetPiece && parent.isSetPiece && !attach.isSetPiece) || !pt.attachableWithGlue)
		{
			return;
		}
		if (pt.Group == null && parent.Group == null)
		{
			bool isMobileBlock = pt.IsMobileBlock;
			bool isMobileBlock2 = parent.IsMobileBlock;
			if ((isMobileBlock && isMobileBlock2) || (isMobileBlock && !pt.isSetPiece && parent.isSetPiece) || (isMobileBlock2 && !parent.isSetPiece && pt.isSetPiece))
			{
				return;
			}
			DoAttach(pt, parent);
		}
		else if (pt.Group != null && parent.Group != null)
		{
			if (pt.Group == parent.Group)
			{
				return;
			}
			bool flag = pt.Group.TopParent != null && pt.Group.TopParent.IsMobileBlock;
			bool flag2 = parent.Group.TopParent != null && parent.Group.TopParent.IsMobileBlock;
			if (flag && flag2)
			{
				return;
			}
			if (flag && pt.Group.TopParent.isSetPiece)
			{
				if (pt.isSetPiece)
				{
					if (!pt.Group.SetPieceConnected(pt))
					{
						return;
					}
				}
				else if (parent.isSetPiece || pt.Group.FindFirstConnectedSetPiece(pt) == null)
				{
					return;
				}
			}
			if (flag2 && parent.Group.TopParent.isSetPiece)
			{
				if (parent.isSetPiece)
				{
					if (!parent.Group.SetPieceConnected(parent))
					{
						return;
					}
				}
				else if (pt.isSetPiece || parent.Group.FindFirstConnectedSetPiece(parent) == null)
				{
					return;
				}
			}
			if (!attach.isSetPiece && ((!flag && pt.Group.ContainsSetPieces) || (!flag2 && parent.Group.ContainsSetPieces)))
			{
				return;
			}
			DoAttach(pt, parent, pt.Group, parent.Group);
		}
		else if (parent.Group != null)
		{
			bool isMobileBlock3 = pt.IsMobileBlock;
			if (parent.Group.TopParent != null && parent.Group.TopParent.IsMobileBlock)
			{
				if (isMobileBlock3 || (pt.isSetPiece && (!parent.isSetPiece || !parent.Group.SetPieceConnected(parent))))
				{
					return;
				}
			}
			else if ((!pt.isSetPiece && !isMobileBlock3) || (!pt.isSetPiece && parent.Group.ContainsSetPieces))
			{
				return;
			}
			DoAttach(pt, parent, parent.Group);
		}
		else if (pt.Group != null)
		{
			bool isMobileBlock4 = parent.IsMobileBlock;
			if (pt.Group.TopParent != null && pt.Group.TopParent.IsMobileBlock)
			{
				if (isMobileBlock4 || (parent.isSetPiece && (!pt.isSetPiece || !pt.Group.SetPieceConnected(pt))))
				{
					return;
				}
			}
			else if ((!pt.isSetPiece && !isMobileBlock4) || (!parent.isSetPiece && pt.Group.ContainsSetPieces))
			{
				return;
			}
			DoAttach(pt, parent, pt.Group);
		}
		if (attach.Group != null)
		{
			attach.Group.ForceAddLink(attach, pt);
		}
	}

	private void DoAttach(Placeable pt, Placeable parent)
	{
		new AttachmentGroup(parent, pt);
		IncrementPieceStats("PiecesGlued", pt, parent);
	}

	private void DoAttach(Placeable pt, Placeable parent, AttachmentGroup group)
	{
		if (group == parent.Group)
		{
			if (!attachments.Contains(pt) && !group.AddLink(parent, pt, newIsTop: true))
			{
				Debug.LogError("DoAttach: Failed to add link.");
			}
		}
		else if (!attachments.Contains(parent) && !group.AddLink(pt, parent))
		{
			Debug.LogError("DoAttach: Failed to add link.");
		}
		if (group.PieceCount > 3)
		{
			IncrementPieceStats("LargeContraptionsMade", pt, parent);
		}
		IncrementPieceStats("PiecesGlued", pt, parent);
	}

	private void DoAttach(Placeable pt, Placeable parent, AttachmentGroup ptGroup, AttachmentGroup parentGroup)
	{
		AttachmentGroup attachmentGroup = AttachmentGroup.MergeGroups(parentGroup, ptGroup);
		if (attachmentGroup != null)
		{
			if (attachmentGroup.PieceCount > 3)
			{
				IncrementPieceStats("LargeContraptionsMade", pt, parent);
			}
		}
		else
		{
			Debug.LogError("Failed to merge groups!");
		}
	}

	private bool RecordSinglePieceStat(string statName, Placeable piece)
	{
		if (piece.placedByPlayerNumber != 0)
		{
			SaveFileData saveFileDataFromNetworkNumber = StatTracker.Instance.GetSaveFileDataFromNetworkNumber(piece.placedByPlayerNumber, fallback: true);
			if (saveFileDataFromNetworkNumber != null)
			{
				saveFileDataFromNetworkNumber.IncrementStat(statName);
				return true;
			}
		}
		return false;
	}

	private void IncrementPieceStats(string statName, Placeable p0, Placeable p1)
	{
		if (!RecordSinglePieceStat(statName, p0) || p0.placedByPlayerNumber != p1.placedByPlayerNumber)
		{
			RecordSinglePieceStat(statName, p1);
		}
	}

	public void DeclareSessionDead()
	{
		deadSession = true;
	}

	public void CheckNullAndDestroy(MonoBehaviour mb)
	{
		if (mb != null && mb.gameObject != null)
		{
			UnityEngine.Object.Destroy(mb.gameObject);
		}
	}

	public void CheckNullAndDestroy(GameObject go)
	{
		if (go != null)
		{
			UnityEngine.Object.Destroy(go);
		}
	}

	public static void LogCurrentModAndRuleInfo()
	{
		string text = Modifiers.GetInstance().GetCurrentModifierListString(forceModsApplied: true);
		if (text == "None")
		{
			text = "No modifiers set";
		}
		string text2 = GameSettings.GetInstance().GetRulesListString(inLobby: false);
		if (text2.NullOrEmpty())
		{
			text2 = "No rules modified";
		}
		Debug.Log("Rules:\n" + text2 + "\n\nMods:\n" + text);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcStartPhase(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartPhase called on server.");
		}
		else
		{
			((GameControl)obj).RpcStartPhase((GamePhase)reader.ReadInt32());
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
			((GameControl)obj).RpcPlayMusic(reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcStartFadingOut(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartFadingOut called on server.");
		}
		else
		{
			((GameControl)obj).RpcStartFadingOut(reader.ReadBoolean(), reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcPropagateBlockIDs(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPropagateBlockIDs called on server.");
		}
		else
		{
			((GameControl)obj).RpcPropagateBlockIDs(GeneratedNetworkCode._ReadArrayString_None(reader), GeneratedNetworkCode._ReadArrayInt32_None(reader));
		}
	}

	protected static void InvokeRpcRpcSetUpCompressedSnapshotTransfer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetUpCompressedSnapshotTransfer called on server.");
		}
		else
		{
			((GameControl)obj).RpcSetUpCompressedSnapshotTransfer((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcSendCompressedSnapshotChunk(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSendCompressedSnapshotChunk called on server.");
		}
		else
		{
			((GameControl)obj).RpcSendCompressedSnapshotChunk(reader.ReadBytesAndSize());
		}
	}

	protected static void InvokeRpcRpcLoadCompressedSnapshot(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLoadCompressedSnapshot called on server.");
		}
		else
		{
			((GameControl)obj).RpcLoadCompressedSnapshot(reader.ReadBytesAndSize());
		}
	}

	protected static void InvokeRpcRpcHostReadyBeacon(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHostReadyBeacon called on server.");
		}
		else
		{
			((GameControl)obj).RpcHostReadyBeacon();
		}
	}

	protected static void InvokeRpcRpcDoInitialPlacement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoInitialPlacement called on server.");
		}
		else
		{
			((GameControl)obj).RpcDoInitialPlacement();
		}
	}

	public void CallRpcStartPhase(GamePhase phase)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcStartPhase called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcStartPhase);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)phase);
		SendRPCInternal(networkWriter, 0, "RpcStartPhase");
	}

	public void CallRpcPlayMusic(string musicName)
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
		networkWriter.Write(musicName);
		SendRPCInternal(networkWriter, 0, "RpcPlayMusic");
	}

	public void CallRpcStartFadingOut(bool toMainMenu, string levelSceneName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcStartFadingOut called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcStartFadingOut);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(toMainMenu);
		networkWriter.Write(levelSceneName);
		SendRPCInternal(networkWriter, 0, "RpcStartFadingOut");
	}

	public void CallRpcPropagateBlockIDs(string[] paths, int[] IDs)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPropagateBlockIDs called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPropagateBlockIDs);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayString_None(networkWriter, paths);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, IDs);
		SendRPCInternal(networkWriter, 0, "RpcPropagateBlockIDs");
	}

	public void CallRpcSetUpCompressedSnapshotTransfer(int bufferLength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetUpCompressedSnapshotTransfer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetUpCompressedSnapshotTransfer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)bufferLength);
		SendRPCInternal(networkWriter, 0, "RpcSetUpCompressedSnapshotTransfer");
	}

	public void CallRpcSendCompressedSnapshotChunk(byte[] buffer)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSendCompressedSnapshotChunk called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSendCompressedSnapshotChunk);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WriteBytesFull(buffer);
		SendRPCInternal(networkWriter, 0, "RpcSendCompressedSnapshotChunk");
	}

	public void CallRpcLoadCompressedSnapshot(byte[] compressedBytes)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcLoadCompressedSnapshot called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcLoadCompressedSnapshot);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WriteBytesFull(compressedBytes);
		SendRPCInternal(networkWriter, 0, "RpcLoadCompressedSnapshot");
	}

	public void CallRpcHostReadyBeacon()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcHostReadyBeacon called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcHostReadyBeacon);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcHostReadyBeacon");
	}

	public void CallRpcDoInitialPlacement()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDoInitialPlacement called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDoInitialPlacement);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcDoInitialPlacement");
	}

	static GameControl()
	{
		kRpcRpcStartPhase = -994477030;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcStartPhase, InvokeRpcRpcStartPhase);
		kRpcRpcPlayMusic = -1459332784;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcPlayMusic, InvokeRpcRpcPlayMusic);
		kRpcRpcStartFadingOut = -2121898124;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcStartFadingOut, InvokeRpcRpcStartFadingOut);
		kRpcRpcPropagateBlockIDs = 167503955;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcPropagateBlockIDs, InvokeRpcRpcPropagateBlockIDs);
		kRpcRpcSetUpCompressedSnapshotTransfer = -1316930388;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcSetUpCompressedSnapshotTransfer, InvokeRpcRpcSetUpCompressedSnapshotTransfer);
		kRpcRpcSendCompressedSnapshotChunk = 1717832863;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcSendCompressedSnapshotChunk, InvokeRpcRpcSendCompressedSnapshotChunk);
		kRpcRpcLoadCompressedSnapshot = -1572334516;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcLoadCompressedSnapshot, InvokeRpcRpcLoadCompressedSnapshot);
		kRpcRpcHostReadyBeacon = 266137534;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcHostReadyBeacon, InvokeRpcRpcHostReadyBeacon);
		kRpcRpcDoInitialPlacement = 509471629;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameControl), kRpcRpcDoInitialPlacement, InvokeRpcRpcDoInitialPlacement);
		NetworkCRC.RegisterBehaviour("GameControl", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(AssociatedScene);
			writer.WritePackedUInt32((uint)unlockOffset);
			writer.Write(waitingForPlayers);
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
			writer.Write(AssociatedScene);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)unlockOffset);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(waitingForPlayers);
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
			AssociatedScene = reader.ReadString();
			unlockOffset = (int)reader.ReadPackedUInt32();
			waitingForPlayers = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			AssociatedScene = reader.ReadString();
		}
		if ((num & 2) != 0)
		{
			unlockOffset = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			waitingForPlayers = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
