using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class VersusControl : GameControl
{
	public GraphScoreBoard GraphScoreBoardPrefab;

	public ReadyMessage ReadyMessagePrefab;

	public SuddenDeathMessage SuddenDeathMessagePrefab;

	public PlacementEndingMessage PlacementEndingMessagePrefab;

	public WinMessage WinMessagePrefab;

	public PartyBox PartyBoxPrefab;

	public DigitalClock DigitalClockPrefab;

	public LastTurnMessage LastTurnPrefab;

	public ScorecardPopupMessage ScorecardMessagePrefab;

	public SuicideNote SuicideNotePrefab;

	public float ShowScoreTime;

	public float WaitTime;

	public float MaxRunTime;

	public float MaxLevelTime;

	public float turnMessageTime;

	public float blockDifficultyWeight = 0.5f;

	public float deathDifficultyWeight = 0.5f;

	public float BlockDifficulyMultiplier;

	public float DeathDifficultyMultiplier;

	public int MaxLoseStreak = 3;

	public BlockGroup forceStartHazard;

	public float startHazardProp;

	public BlockGroup forceTeleporter;

	public float forceTeleporterProp;

	public BlockGroup forceHoney;

	public float forceHoneyProp;

	protected float scoreTimer;

	protected float readyTimer;

	protected float levelTimer;

	protected float countdownTimer;

	protected float turnMessageTimer;

	protected float placementTimer;

	public bool DisplayScoreboard;

	private float scoreboardCooldown;

	private float scoreboardHeld;

	private bool showingPlacementWarning;

	private bool piecesRemoved;

	protected int nextToBuild;

	protected int lastToBuild;

	protected bool awardPoints;

	protected bool lastRoundsMode;

	protected int lastRoundsToGo = 3;

	protected bool lastRoundsTimer;

	protected float lastRoundsTimeLimit;

	protected float lastRoundsPlaceTime = 30f;

	protected float minLastRoundsRunTime = 60f;

	protected bool IsSecondBox;

	private float spectatorVerticalOffset = 0.3f;

	protected GraphScoreBoard graphScoreBoardInstance;

	protected ReadyMessage readyMessageInstance;

	protected SuddenDeathMessage suddenDeathMessageInstance;

	protected PlacementEndingMessage placementEndingMessageInstance;

	protected WinMessage winMessageInstance;

	protected PartyBox partyBoxInstance;

	protected DigitalClock digitalClockInstance;

	protected LastTurnMessage lastTurnInstance;

	protected ScorecardPopupMessage scorecardMessageInstance;

	protected SuicideNote suicideNoteInstance;

	private ScoreKeeper scorekeeperInstance;

	public RunTimer runTimer;

	protected int startingBlocks;

	protected int maxBlockAtStart;

	protected float runTime;

	protected float fastestTime;

	private GamePlayer[] winOrder = new GamePlayer[4];

	private Queue<GamePlayer> WinnerQueue;

	private int lastWinner = -1;

	private bool runStarted;

	private bool placeStarted;

	protected int[] RemainingPlacements = new int[4];

	protected int remainingPartyBoxes = 1;

	protected List<Placeable> PlacedThisRound = new List<Placeable>();

	[HideInInspector]
	public int objectsHoldingUpPlacePhase;

	[SyncVar]
	private bool waitingForCharacters;

	[SyncVar]
	private bool skipTurnMessage;

	[SyncVar]
	protected string RandomStartPositionString = "1234";

	protected float GameIntensityLevel;

	protected float CurrenTargetDifficulty;

	protected bool ChangingToPlayPhase;

	public float forcePlacePhaseContinueTime = 10f;

	protected float forcePlacePhaseContinueTimer;

	private HashSet<int> forcedPieceSpawns = new HashSet<int>();

	public bool DebugPartyBox;

	public List<string> partyboxDebugInfo = new List<string>();

	protected bool wasShowingScore;

	private Coroutine delayRpcSelectRandom;

	private float AgonyTimeLimitTimer;

	private bool AgonoyTimerLimitTriggered;

	private bool AgonyFinalCountDownStarted;

	private static int kRpcRpcSetupPlacementCursors;

	private static int kRpcRpcShowScorecardMessage;

	private static int kRpcRpcIncrementCharacterSuccess;

	private static int kRpcRpcIncrementCharacterWins;

	private static int kRpcRpcBackToBasicAchievement;

	private static int kRpcRpcShowLastTurnMessage;

	private static int kRpcRpcHideLastTurnMessage;

	private static int kRpcRpcShowScoreboard;

	private static int kRpcRpcShowPlacementWarning;

	private static int kRpcRpcSendPlacementTimerDone;

	private static int kRpcRpcRemoveUnplacedObjects;

	private static int kRpcRpcSetCountdown;

	private static int kRpcRpcSetWinner;

	private static int kRpcRpcShowPartyBox;

	private static int kRpcRpcForceSelectRandomBlocks;

	private static int kRpcRpcForceSpawnPiece;

	private static int kRpcRpcForceSpawnVariantPiece;

	private static int kRpcRpcRunTimerHit;

	private static int kRpcRpcTriggerAgonyRunTimer;

	protected bool playersLeftToPlace
	{
		get
		{
			for (int i = 0; i != 4; i++)
			{
				if (RemainingPlacements[i] > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	protected int maxScore
	{
		get
		{
			int num = GameSettings.GetInstance().MaxScore;
			int num2 = num;
			if (lastRoundsMode && lastRoundsToGo <= 1)
			{
				num2 = 0;
				foreach (GamePlayer item in PlayerQueue)
				{
					int playerTotal = scorekeeperInstance.GetPlayerTotal(item);
					if (playerTotal > num2)
					{
						num2 = playerTotal;
					}
				}
			}
			return Mathf.Min(num2, num);
		}
	}

	protected int currentWinningScore
	{
		get
		{
			int num = 0;
			foreach (GamePlayer item in PlayerQueue)
			{
				int playerTotal = scorekeeperInstance.GetPlayerTotal(item);
				if (playerTotal > num)
				{
					num = playerTotal;
				}
			}
			return num;
		}
	}

	public int[] Scores
	{
		get
		{
			int[] array = new int[GameSettings.GetInstance().MaxPlayers];
			foreach (GamePlayer item in PlayerQueue)
			{
				array[item.networkNumber - 1] = scorekeeperInstance.GetPlayerTotal(item);
			}
			return array;
		}
	}

	public PartyBox PartyBox => partyBoxInstance;

	public bool PartyBoxStillActive
	{
		get
		{
			if (partyBoxInstance != null)
			{
				return partyBoxInstance.IsStillActive;
			}
			return false;
		}
	}

	public bool NetworkwaitingForCharacters
	{
		get
		{
			return waitingForCharacters;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref waitingForCharacters, 8u);
		}
	}

	public bool NetworkskipTurnMessage
	{
		get
		{
			return skipTurnMessage;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref skipTurnMessage, 16u);
		}
	}

	public string NetworkRandomStartPositionString
	{
		get
		{
			return RandomStartPositionString;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref RandomStartPositionString, 32u);
		}
	}

	public int GetRank(int networkNumber)
	{
		int num = 1;
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item.networkNumber != networkNumber)
			{
				continue;
			}
			int playerTotal = scorekeeperInstance.GetPlayerTotal(item);
			int[] scores = Scores;
			for (int i = 0; i < scores.Length; i++)
			{
				if (scores[i] > playerTotal)
				{
					num++;
				}
			}
		}
		return num;
	}

	protected override void Start()
	{
		float num = blockDifficultyWeight + deathDifficultyWeight;
		blockDifficultyWeight /= num;
		deathDifficultyWeight /= num;
		levelTimer = 0f;
		forceStartHazard.filterPool();
		forceTeleporter.filterPool();
		forceHoney.filterPool();
		base.Start();
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.PARTY)
		{
			partyBoxInstance = UnityEngine.Object.Instantiate(PartyBoxPrefab, UICamera.transform.position, Quaternion.identity);
			partyBoxInstance.transform.Translate(0f, 0f, 1f);
			partyBoxInstance.transform.parent = UICamera.transform;
			partyBoxInstance.UICamera = UICamera.GetComponent<Camera>();
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<PartyBoxEvent>(this, adding);
		GameEventManager.ChangeListener<DestroyPieceEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkClientDisconnectEvent>(this, adding);
		GameEventManager.ChangeListener<GamePlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<PartyCursorSpawnedEvent>(this, adding);
		GameEventManager.ChangeListener<PlacementSkippedEvent>(this, adding);
	}

	protected override void Update()
	{
		if (deadSession || CleanUpStarted)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < showScoreButtons.Length; i++)
		{
			if (showScoreButtons[i])
			{
				flag = true;
				break;
			}
		}
		if (scoreboardCooldown > 0f)
		{
			scoreboardCooldown -= Time.unscaledDeltaTime;
			scoreboardHeld = 0f;
		}
		else
		{
			if (flag)
			{
				scoreboardHeld += Time.unscaledDeltaTime;
			}
			if (scoreboardHeld > 0.1f && scoreTimer <= 0f && !graphScoreBoardInstance.DrawingScore && !graphScoreBoardInstance.GameShowingScore)
			{
				graphScoreBoardInstance.Show(0f, GameShowing: false);
				int num = 0;
				foreach (GamePlayer item in PlayerQueue)
				{
					if (item != null && item.IsLocalPlayer)
					{
						num++;
					}
				}
				if (num > 1)
				{
					scoreboardCooldown += 1f;
				}
			}
		}
		if (graphScoreBoardInstance != null)
		{
			bool num2 = wasShowingScore && !flag;
			if (!graphScoreBoardInstance.GameShowingScore && !graphScoreBoardInstance.DrawingScore)
			{
				wasShowingScore = flag;
			}
			if (num2 && scoreTimer <= 0f && !graphScoreBoardInstance.DrawingScore && !graphScoreBoardInstance.GameShowingScore)
			{
				graphScoreBoardInstance.Hide(afterTally: false, !LobbyManager.instance.IsInOnlineGame);
				scoreboardHeld = 0f;
			}
		}
		if (base.Phase == GamePhase.PLACE)
		{
			holdingForNextPhase = objectsHoldingUpPlacePhase > 0;
		}
		else
		{
			holdingForNextPhase = false;
		}
		base.Update();
		if (PlayerQueue.Count == 0)
		{
			return;
		}
		if (!paused)
		{
			levelTimer += Time.unscaledDeltaTime;
		}
		if (runStarted)
		{
			if (paused && !runTimer.Paused)
			{
				runTimer.PauseRun();
			}
			if (!paused && runTimer.Paused)
			{
				runTimer.UnpauseRun();
			}
		}
		AkSoundEngine.SetRTPCValue("game_intensity_level", Mathf.Clamp(GameIntensityLevel * 100f, 0f, 100f));
	}

	protected override void SetupStart(GameState.GameMode mode)
	{
		base.SetupStart(mode);
		Debug.Log("Setting up VS Mode");
		mode = GameSettings.GetInstance().GameMode;
		invBookInstance = UnityEngine.Object.Instantiate(InventoryBookPrefab);
		invBookInstance.transform.parent = UICamera.transform;
		invBookInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
		invBookInstance.UiCamera = UICamera;
		invBookInstance.Hide();
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item.IsLocalPlayer)
			{
				if (item == null)
				{
					Debug.LogError("Player queue contains null player");
					continue;
				}
				((PiecePlacementCursor)item.CursorInstance).InventoryBookMenu = invBookInstance;
				invBookInstance.AddPlayer(item.localNumber, item.networkNumber, item.LocalPlayer.UseController, item.CharacterInstance.CharacterSprite).Disable();
			}
		}
		Vector2 vector = default(Vector2);
		vector.y = UICamera.orthographicSize;
		vector.x = vector.y * UICamera.aspect;
		graphScoreBoardInstance = UnityEngine.Object.Instantiate(GraphScoreBoardPrefab);
		graphScoreBoardInstance.transform.SetParent(UICamera.transform);
		graphScoreBoardInstance.transform.localPosition = new Vector3(0f, -0.5f, 0f);
		scorekeeperInstance = ScoreKeeper.Instance;
		scorekeeperInstance.IsOnServer = base.hasAuthority;
		scorekeeperInstance.Setup();
		List<Canvas> list = new List<Canvas>();
		readyMessageInstance = UnityEngine.Object.Instantiate(ReadyMessagePrefab, UICamera.transform.position, Quaternion.identity);
		readyMessageInstance.canvas.worldCamera = UICamera;
		readyMessageInstance.transform.Translate(0f, 0f, 1f);
		readyMessageInstance.transform.parent = UICamera.transform;
		if (GameState.GetInstance().UsingHotSeat)
		{
			readyMessageInstance.WaitForPlayer = true;
		}
		readyMessageInstance.SetupForVersusMode();
		readyMessageInstance.Hide();
		list.AddRange(readyMessageInstance.GetComponentsInChildren<Canvas>());
		suddenDeathMessageInstance = UnityEngine.Object.Instantiate(SuddenDeathMessagePrefab, UICamera.transform.position, Quaternion.identity);
		suddenDeathMessageInstance.TitleMessage.worldCamera = UICamera;
		suddenDeathMessageInstance.InstructionMessage.worldCamera = UICamera;
		suddenDeathMessageInstance.PressACanvas.worldCamera = UICamera;
		suddenDeathMessageInstance.transform.Translate(0f, 0f, 1f);
		suddenDeathMessageInstance.transform.parent = UICamera.transform;
		if (GameState.GetInstance().UsingHotSeat)
		{
			suddenDeathMessageInstance.WaitForPlayer = true;
		}
		suddenDeathMessageInstance.Hide(forceQuickHide: true);
		list.AddRange(suddenDeathMessageInstance.GetComponentsInChildren<Canvas>());
		placementEndingMessageInstance = UnityEngine.Object.Instantiate(PlacementEndingMessagePrefab, UICamera.transform.position, Quaternion.identity);
		placementEndingMessageInstance.MessageCanvas.worldCamera = UICamera;
		placementEndingMessageInstance.transform.Translate(0f, 0f, 1f);
		placementEndingMessageInstance.transform.parent = UICamera.transform;
		placementEndingMessageInstance.Hide(forceQuickHide: true);
		list.Add(placementEndingMessageInstance.MessageCanvas);
		digitalClockInstance = UnityEngine.Object.Instantiate(DigitalClockPrefab, UICamera.transform.position, Quaternion.identity);
		digitalClockInstance.TimeCanvas.worldCamera = UICamera;
		digitalClockInstance.transform.Translate(0f, 0f, 1f);
		digitalClockInstance.transform.parent = UICamera.transform;
		digitalClockInstance.Reset();
		digitalClockInstance.Hide(forceQuickHide: true);
		list.AddRange(digitalClockInstance.GetComponentsInChildren<Canvas>());
		winMessageInstance = UnityEngine.Object.Instantiate(WinMessagePrefab, UICamera.transform.position, Quaternion.identity);
		winMessageInstance.transform.Translate(0f, 0f, 1f);
		winMessageInstance.transform.parent = UICamera.transform;
		winMessageInstance.Hide(forceQuickHide: true);
		list.AddRange(winMessageInstance.GetComponentsInChildren<Canvas>());
		lastTurnInstance = UnityEngine.Object.Instantiate(LastTurnPrefab, UICamera.transform.position, Quaternion.identity);
		lastTurnInstance.transform.Translate(0f, 0f, 1f);
		lastTurnInstance.transform.SetParent(UICamera.transform);
		lastTurnInstance.GetComponentInChildren<Canvas>().worldCamera = UICamera;
		lastTurnInstance.Hide(forceQuickHide: true);
		suicideNoteInstance = UnityEngine.Object.Instantiate(SuicideNotePrefab, UICamera.transform.position, Quaternion.identity);
		suicideNoteInstance.transform.Translate(0f, 0f, 1f);
		suicideNoteInstance.transform.SetParent(UICamera.transform);
		suicideNoteInstance.GetComponentInChildren<Canvas>().worldCamera = UICamera;
		suicideNoteInstance.Hide(forceQuickHide: true);
		list.AddRange(lastTurnInstance.GetComponentsInChildren<Canvas>());
		foreach (Canvas item2 in list)
		{
			item2.sortingLayerName = "Haze";
			item2.planeDistance = 50f;
		}
		scorecardMessageInstance = UnityEngine.Object.Instantiate(ScorecardMessagePrefab, UICamera.transform.position, Quaternion.identity);
		scorecardMessageInstance.transform.Translate(0f, 0f, -1f);
		scorecardMessageInstance.transform.parent = graphScoreBoardInstance.transform.parent;
		scorecardMessageInstance.Message.worldCamera = UICamera;
		scorecardMessageInstance.MatchHasWinnerPoints = GameSettings.GetInstance().AnyWinnerPointsEnabled();
		if (mode == GameState.GameMode.PARTY)
		{
			partyBoxInstance.SetPlayerCount(PlayerQueue.Count);
			partyBoxInstance.HasAuthority = base.hasAuthority;
		}
		for (int i = 0; i != PlayerQueue.Count; i++)
		{
			GamePlayer gamePlayer = PlayerQueue.Dequeue();
			gamePlayer.TurnOrder = i;
			gamePlayer.CursorInstance.UseCamera = MainCamera.GetComponent<Camera>();
			PlayerQueue.Enqueue(gamePlayer);
		}
		graphScoreBoardInstance.SetPlayerCount(PlayerQueue.Count);
		for (int j = 0; j != PlayerQueue.Count; j++)
		{
			GamePlayer gamePlayer2 = PlayerQueue.Dequeue();
			LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(gamePlayer2.networkNumber);
			if (lobbyPlayer != null)
			{
				graphScoreBoardInstance.SetPlayerCharacter(j, gamePlayer2.CharacterInstance.CharacterSprite, gamePlayer2.IsWearingSkin, lobbyPlayer, gamePlayer2.Handicap);
				PlayerQueue.Enqueue(gamePlayer2);
				if (base.hasAuthority && mode == GameState.GameMode.PARTY)
				{
					PartyPickCursor partyPickCursor = partyBoxInstance.AddPlayer(gamePlayer2.networkNumber, gamePlayer2.PickedAnimal);
					GameObject gameObject = ClientScene.FindLocalObject(gamePlayer2.netId);
					if (!(gameObject == null))
					{
						GamePlayer component = gameObject.GetComponent<GamePlayer>();
						component.CallCmdAssignCursor(partyPickCursor.gameObject, component.networkNumber, component.localNumber);
					}
				}
			}
			else
			{
				Debug.LogError("Could not find lobbyPlayer with network number " + gamePlayer2.networkNumber);
			}
		}
		LevelSelectController.PlayedSnapshotInfo currentSnapshotInfo = GameState.GetInstance().currentSnapshotInfo;
		if (!currentSnapshotInfo.snapshotName.NullOrEmpty())
		{
			string text = currentSnapshotInfo.snapshotName;
			if (!currentSnapshotInfo.snapshotCode.NullOrEmpty())
			{
				text = text + " - " + GameSparksQuery.GetFormattedSnapshotCode(currentSnapshotInfo.snapshotCode);
			}
			graphScoreBoardInstance.ShowCustomLevelText(text);
		}
		else
		{
			graphScoreBoardInstance.HideCustomLevelText();
		}
		if (GameSettings.GetInstance().respawnMode != RespawnMode.Off && livesDisplayController != null)
		{
			livesDisplayController.OnStartNewMatch();
		}
		if (base.hasAuthority)
		{
			StartCoroutine(WaitForPreGameEvents(OnFinishSetupStart));
		}
		else
		{
			OnFinishSetupStart();
		}
	}

	private void OnFinishSetupStart()
	{
		NotifySetupStartDone();
		if (base.hasAuthority)
		{
			StartCoroutine(WaitForSetupStart(delegate
			{
				nextPhase = GamePhase.PLACE;
			}));
		}
	}

	protected override void ToPlaceMode()
	{
		if (base.Phase != GamePhase.PLACE)
		{
			AkSoundEngine.PostEvent("Construction_Phase", base.gameObject);
		}
		base.ToPlaceMode();
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item.IsLocalPlayer)
			{
				item.CharacterInstance.Disable();
			}
			MainCamera.RemoveTarget(item.CharacterInstance);
		}
		GameSettings instance = GameSettings.GetInstance();
		if (instance.respawnMode != RespawnMode.Off && livesDisplayController != null)
		{
			livesDisplayController.OnStartNewRound();
		}
		int num = 1;
		if (instance.GameMode == GameState.GameMode.CREATIVE)
		{
			num = instance.CreativePiecesPerRound;
		}
		else if (instance.GameMode == GameState.GameMode.PARTY)
		{
			switch (instance.partyBoxMode)
			{
			case PartyBoxMode.Standard:
				if (instance.DoublePartyBox == DoublePartyBox.Off || (PlayerQueue.Count > 2 && instance.DoublePartyBox == DoublePartyBox.TwoPlayers))
				{
					remainingPartyBoxes = instance.PartyBoxesPerRound;
				}
				else
				{
					remainingPartyBoxes = 2 * instance.PartyBoxesPerRound;
				}
				IsSecondBox = false;
				break;
			case PartyBoxMode.Disabled:
				num = 0;
				remainingPartyBoxes = 0;
				break;
			case PartyBoxMode.AutoRandom:
				num = 1;
				if (instance.DoublePartyBox == DoublePartyBox.Off || (PlayerQueue.Count > 2 && instance.DoublePartyBox == DoublePartyBox.TwoPlayers))
				{
					remainingPartyBoxes = instance.PartyBoxesPerRound;
				}
				else
				{
					remainingPartyBoxes = 2 * instance.PartyBoxesPerRound;
				}
				break;
			}
		}
		foreach (GamePlayer item2 in PlayerQueue)
		{
			if (base.hasAuthority && instance.GameMode == GameState.GameMode.CREATIVE && instance.CreativePiecesPerRound > 1)
			{
				((PiecePlacementCursor)item2.CursorInstance).CallRpcSetPlacementsLeftText(num);
			}
			PlacedThisRound.Clear();
		}
		MainCamera.ClearTargets();
		roundNumber++;
		placeStarted = false;
		piecesRemoved = false;
		forcePlacePhaseContinueTimer = 0f;
		if (base.hasAuthority)
		{
			ShuffleStartPosition();
		}
		fastestTime = float.PositiveInfinity;
		placementTimer = instance.PlaceTime;
		if (instance.GameLimitType == GameLimitType.TIME && lastRoundsMode && lastRoundsPlaceTime < placementTimer)
		{
			placementTimer = lastRoundsPlaceTime;
		}
		showingPlacementWarning = false;
		if (instance.GameMode == GameState.GameMode.CREATIVE)
		{
			foreach (GamePlayer item3 in PlayerQueue)
			{
				RemainingPlacements[item3.networkNumber - 1] = num;
			}
			placeStarted = true;
			if (!GameState.GetInstance().UsingHotSeat)
			{
				float num2 = (float)(360 / PlayerQueue.Count) * (MathF.PI / 180f);
				foreach (GamePlayer item4 in PlayerQueue)
				{
					item4.CursorInstance.transform.position = new Vector3(Mathf.Cos(num2 * (float)item4.TurnOrder) * LevelLayout.CursorSpawnRadius, Mathf.Sin(num2 * (float)item4.TurnOrder) * LevelLayout.CursorSpawnRadius, 0f) + LevelLayout.CursorSpawnPoint.position;
					item4.CursorInstance.Enable();
					MainCamera.AddTarget(item4.CursorInstance);
				}
			}
			else
			{
				GamePlayer gamePlayer = PlayerQueue.Dequeue();
				gamePlayer.CursorInstance.Enable();
				gamePlayer.CursorInstance.transform.position = LevelLayout.CursorSpawnPoint.position;
				MainCamera.AddTarget(gamePlayer.CursorInstance);
				PlayerQueue.Enqueue(gamePlayer);
				nextToBuild = PlayerQueue.Peek().TurnOrder;
				lastToBuild = gamePlayer.TurnOrder;
			}
		}
		else
		{
			switch (instance.partyBoxMode)
			{
			case PartyBoxMode.Standard:
				showPartyBox();
				break;
			case PartyBoxMode.AutoRandom:
				ForceSelectRandomBlocks();
				break;
			}
			awardPoints = false;
		}
		if ((float)currentWinningScore > (float)instance.MaxScore * 0.8f)
		{
			GameIntensityLevel = (float)currentWinningScore / (float)instance.MaxScore;
		}
		else
		{
			switch (instance.GameLimitType)
			{
			case GameLimitType.NONE:
				GameIntensityLevel = (float)currentWinningScore / (float)instance.MaxScore;
				break;
			case GameLimitType.TIME:
				GameIntensityLevel = levelTimer / (float)instance.MaxTime;
				break;
			case GameLimitType.ROUNDS:
				GameIntensityLevel = (float)roundNumber / (float)instance.MaxRounds;
				break;
			}
		}
		if (base.hasAuthority)
		{
			int matchProgress = Mathf.Clamp(100 - (int)(GameIntensityLevel * 100f), 1, 100);
			Matchmaker.Instance.CurrentLobby.SetMatchProgress(matchProgress);
		}
		if (instance.GameMode == GameState.GameMode.PARTY)
		{
			GameIntensityLevel += (1f - CurrenTargetDifficulty) * 0.2f;
		}
		if (roundNumber == 1 && (Modifiers.GetInstance().AppliedAndNonDefault || GameSettings.GetInstance().HaveNonDefaultRules))
		{
			GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
		}
	}

	private void ForceSelectRandomBlocks()
	{
		SetupPartyBoxForRound(doChoosePieces: false);
		if (!base.hasAuthority)
		{
			return;
		}
		partyBoxInstance.ComputeEffectiveBlockWeights();
		forcedPieceSpawns.Clear();
		PlaceableMetadataList metaList = LobbyManager.instance.CurrentGameController.MetaList;
		List<PickableBlock> list = null;
		int num = 0;
		if (partyBoxInstance.twitchSelectedItems.Count > 0)
		{
			list = partyBoxInstance.twitchSelectedItems;
			partyBoxInstance.twitchSelectedItems = new List<PickableBlock>();
		}
		foreach (GamePlayer item in PlayerQueue)
		{
			PickableBlock pickableBlock = null;
			bool isTwitchItem = false;
			if (list != null && num < list.Count)
			{
				pickableBlock = list[num];
				num++;
				isTwitchItem = true;
			}
			else
			{
				pickableBlock = partyBoxInstance.SelectRandomPiece();
			}
			if (pickableBlock.placeablePrefab.FilterOverride.Length != 0)
			{
				Placeable placeable = pickableBlock.placeablePrefab.FilterOverride[0];
				int indexForPlaceable = metaList.GetIndexForPlaceable(placeable.Name);
				int variantIndex = metaList.FindVariantIndex(pickableBlock.placeablePrefab);
				CallRpcForceSpawnVariantPiece(item.networkNumber, indexForPlaceable, variantIndex, isTwitchItem);
				Debug.LogWarning("DEBUG: Party Box Auto-Pick> Spawning a " + placeable.name + " (variant: " + pickableBlock.placeablePrefab.Name + ") for player " + item.networkNumber);
			}
			else
			{
				int indexForPlaceable2 = metaList.GetIndexForPlaceable(pickableBlock.placeablePrefab.Name);
				if (indexForPlaceable2 != -1)
				{
					CallRpcForceSpawnPiece(item.networkNumber, indexForPlaceable2, isTwitchItem);
					Debug.LogWarning(" DEBUG: Party Box Auto-Pick> Spawning a " + pickableBlock.placeablePrefab.Name + " for player " + item.networkNumber);
				}
				else
				{
					Debug.LogError("Error finding forced piece to spawn");
				}
			}
		}
		StartCoroutine(HostWaitToSetupPlacementCursors());
	}

	private void OnForcedPieceSpawned(int networkNumber)
	{
		forcedPieceSpawns.Add(networkNumber);
	}

	private IEnumerator HostWaitToSetupPlacementCursors()
	{
		bool stillWaiting = true;
		while (stillWaiting)
		{
			stillWaiting = false;
			foreach (GamePlayer item in PlayerQueue)
			{
				if (!forcedPieceSpawns.Contains(item.networkNumber))
				{
					stillWaiting = true;
					break;
				}
			}
			if (stillWaiting)
			{
				yield return null;
			}
		}
		CallRpcSetupPlacementCursors(waitForForcedPieces: true);
	}

	[ClientRpc]
	private void RpcSetupPlacementCursors(bool waitForForcedPieces)
	{
		if (waitForForcedPieces)
		{
			StartCoroutine(WaitForForcedPiecesAndSetupCursors());
		}
		else
		{
			SetupPlacementCursors();
		}
	}

	private IEnumerator WaitForForcedPiecesAndSetupCursors()
	{
		bool waitingForPieces = true;
		while (waitingForPieces)
		{
			waitingForPieces = false;
			foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
			{
				if (allGameNetID == 0)
				{
					continue;
				}
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
				if (gameObject == null)
				{
					continue;
				}
				GamePlayer component = gameObject.GetComponent<GamePlayer>();
				if (!(component == null) && !(component.CursorInstance == null))
				{
					PiecePlacementCursor component2 = component.CursorInstance.GetComponent<PiecePlacementCursor>();
					if (!(component2 != null) || !(component2.Piece != null) || component2.Piece.Placed)
					{
						waitingForPieces = true;
						break;
					}
				}
			}
			if (waitingForPieces)
			{
				yield return null;
			}
		}
		SetupPlacementCursors();
	}

	private void showPartyBox()
	{
		SetupPartyBoxForRound(doChoosePieces: true);
		partyBoxInstance.ShowBox(IsSecondBox);
	}

	private void SetupPartyBoxForRound(bool doChoosePieces)
	{
		MainCamera.ForceShowAllPlayer(showAll: false);
		showingPlacementWarning = false;
		if (placementEndingMessageInstance != null)
		{
			placementEndingMessageInstance.Hide();
		}
		placementTimer = GameSettings.GetInstance().PlaceTime + 3f;
		if (GameSettings.GetInstance().GameLimitType == GameLimitType.TIME && lastRoundsMode && lastRoundsPlaceTime < placementTimer)
		{
			placementTimer = lastRoundsPlaceTime;
		}
		placeStarted = false;
		foreach (GamePlayer item in PlayerQueue)
		{
			RemainingPlacements[item.networkNumber - 1] = 1;
		}
		if (!base.hasAuthority)
		{
			return;
		}
		if (DebugPartyBox)
		{
			partyboxDebugInfo.Clear();
		}
		winners = 0;
		worldDeaths = 0;
		blockDeaths = 0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		using (Queue<GamePlayer>.Enumerator enumerator = PlayerQueue.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.CharacterInstance.LastDeath)
				{
				case "Won":
					winners++;
					break;
				case "Falling":
				case "World":
				case "Drowning":
					worldDeaths++;
					break;
				default:
					blockDeaths++;
					break;
				}
			}
		}
		if (GameSettings.GetInstance().competitiveRandomizer)
		{
			num += UnityEngine.Random.Range(-2, 3);
			num = Mathf.Max(0, num);
			partyBoxInstance.blockSelectionMode = PartyBox.BlockSelectionMode.TrueRandom;
			partyBoxInstance.ChoosePieces(PlayerQueue.Count + 1 + num, null);
			return;
		}
		if (!IsSecondBox)
		{
			if (DebugPartyBox)
			{
				partyboxDebugInfo.Add("Partybox Debug Info: Round =" + roundNumber + " Box = 1");
			}
			if (winners == 0 && roundNumber > 1)
			{
				loseStreak++;
			}
			else
			{
				loseStreak = 0;
			}
		}
		else if (DebugPartyBox)
		{
			partyboxDebugInfo.Add("Partybox Debug Info: Round =" + roundNumber + " Box = 2");
		}
		if (roundNumber == 1 && !IsSecondBox)
		{
			partyBoxInstance.blockSelectionMode = PartyBox.BlockSelectionMode.ForceStartMostlyPlatforms;
			if (DebugPartyBox)
			{
				partyboxDebugInfo.Add("First Round box!  Should be lots of platforms and random other things.");
			}
		}
		else
		{
			if (loseStreak >= MaxLoseStreak && !IsSecondBox)
			{
				partyBoxInstance.blockSelectionMode = PartyBox.BlockSelectionMode.ForcePlatforms;
				if (blockDeaths == PlayerQueue.Count)
				{
					if (DebugPartyBox)
					{
						partyboxDebugInfo.Add("Blocks are killing everbody, you will get some bombs to destory stuff with. ");
					}
					num2 += UnityEngine.Random.Range(2, PlayerQueue.Count);
					num++;
				}
				else if (worldDeaths == PlayerQueue.Count)
				{
					if (DebugPartyBox)
					{
						partyboxDebugInfo.Add("Probably not enough blocks in the world to get through. ");
					}
					num2++;
				}
				else
				{
					if (DebugPartyBox)
					{
						partyboxDebugInfo.Add("Lose streak, but mixed reasons. Chance of bombs. Give players more choice.");
					}
					num2 += UnityEngine.Random.Range(0, PlayerQueue.Count);
					num += 2;
				}
				loseStreak = MaxLoseStreak - 2;
			}
			else
			{
				if (loseStreak > 0 && UnityEngine.Random.value > Mathf.Pow(2f, -loseStreak) && !IsSecondBox)
				{
					num2++;
					if (DebugPartyBox)
					{
						partyboxDebugInfo.Add("Extra Bombs cause lose streak is starting.");
					}
				}
				if (levelDensity <= LevelLayout.MinDensity && !IsSecondBox)
				{
					partyBoxInstance.blockSelectionMode = PartyBox.BlockSelectionMode.ForcePlatforms;
					num2 = 0;
					if (DebugPartyBox)
					{
						partyboxDebugInfo.Add("Level is too empty.  Add some platforms. Remove extra bombs");
					}
				}
				else
				{
					float num4 = blockDifficultyWeight * nonzeroAverageChallenge;
					float num5 = deathDifficultyWeight * (float)(worldDeaths + blockDeaths - winners) / (float)PlayerQueue.Count;
					float num6 = deathDifficultyWeight * (float)loseStreak / (float)MaxLoseStreak;
					float value = num4 + num5 + num6;
					value = (CurrenTargetDifficulty = 1f - Mathf.Clamp01(value));
					float density = 1f - Mathf.Clamp01((levelDensity - LevelLayout.MinDensity) / LevelLayout.MaxDensity);
					partyBoxInstance.blockSelectionMode = PartyBox.BlockSelectionMode.Random;
					partyBoxInstance.Density = density;
					partyBoxInstance.Difficulty = value;
					if (DebugPartyBox)
					{
						partyboxDebugInfo.Add("Block + Death + LoseStreak = Inv Total Diff ");
						partyboxDebugInfo.Add(num4.ToString("F2") + "+" + num5.ToString("F2") + "+" + num6.ToString("F2") + "=" + value.ToString("F2"));
						partyboxDebugInfo.Add("Target Density: " + density.ToString("F2"));
					}
				}
			}
			num += UnityEngine.Random.Range(-2, 3);
			num = Mathf.Max(0, num);
		}
		if (PlayerQueue.Count == 2)
		{
			if (UnityEngine.Random.value < GameSettings.GetInstance().twoPlayerCoinProbability)
			{
				num3++;
				if (DebugPartyBox)
				{
					partyboxDebugInfo.Add("Adding random coin for 2 players");
				}
			}
			num++;
			if (DebugPartyBox)
			{
				partyboxDebugInfo.Add("Adding extra block for 2 players");
			}
		}
		if (Application.isEditor)
		{
			int num7 = PlayerQueue.Count + 1;
			if (DebugPartyBox)
			{
				Debug.Log("Choosing party box pieces: " + num7 + " blocks + " + num + " extra blocks. " + num2 + " forced bombs, " + num3 + " forced coins.");
				partyboxDebugInfo.Add(num7 + " blocks, " + num + " extra blocks, " + num2 + " forced bombs, " + num3 + " forced coins");
			}
		}
		int num8 = num2 + num3;
		List<Placeable> list = partyBoxInstance.GenerateAdditionalPieces(num2, num3);
		int count = list.Count;
		if (count < num8)
		{
			num += num8 - count;
			if (DebugPartyBox)
			{
				partyboxDebugInfo.Add("Filter required extra blocks be generated instead of bombs or coins ");
			}
		}
		if (DebugPartyBox)
		{
			partyboxDebugInfo.Add("PartyBox Mode: " + partyBoxInstance.blockSelectionMode);
		}
		if (doChoosePieces)
		{
			partyBoxInstance.ChoosePieces(PlayerQueue.Count + 1 + num, list);
		}
	}

	public void ShuffleStartPosition()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < PlayerQueue.Count; i++)
		{
			list.Add(i + 1);
		}
		int num = 0;
		for (int j = 0; j < PlayerQueue.Count; j++)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			int num2 = list[index];
			num += num2 * (int)Mathf.Pow(10f, j);
			list.RemoveAt(index);
		}
		NetworkRandomStartPositionString = num.ToString();
	}

	protected override void ToPlayMode()
	{
		if (base.Phase != GamePhase.PLAY)
		{
			AkSoundEngine.PostEvent("Plateform_Phase", base.gameObject);
		}
		base.ToPlayMode();
		MainCamera.ForceShowAllPlayer(showAll: true);
		if (!GameState.GetInstance().UsingHotSeat)
		{
			Dictionary<GamePlayer, Vector3> dictionary = new Dictionary<GamePlayer, Vector3>();
			Modifiers instance = Modifiers.GetInstance();
			foreach (GamePlayer item in PlayerQueue)
			{
				int num = int.Parse(RandomStartPositionString[item.TurnOrder].ToString()) - 1;
				if (instance.PlayerPlayerCollisions && instance.CharacterSizeMode >= 3)
				{
					dictionary[item] = LevelLayout.GetLargeCharacterSpawnPosition(num, PlayerQueue.Count);
				}
				else
				{
					dictionary[item] = LevelLayout.GetSpawnPosition((float)num / Mathf.Max((float)PlayerQueue.Count - 1f, 1f));
				}
			}
			foreach (GamePlayer item2 in PlayerQueue)
			{
				item2.CharacterInstance.Enable();
				item2.CharacterInstance.Waiting = true;
				LevelLayout.SpawnCharacter(item2.CharacterInstance, dictionary[item2]);
				if (instance.PlayerPlayerCollisions && instance.CharacterSizeMode >= 3)
				{
					item2.CharacterInstance.forceCrouchCommand();
				}
				SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, item2.CharacterInstance.transform.position, 0.5f);
				MainCamera.AddTarget(item2.CharacterInstance);
			}
		}
		else
		{
			GamePlayer gamePlayer = PlayerQueue.Peek();
			gamePlayer.CharacterInstance.Enable();
			gamePlayer.CharacterInstance.Waiting = true;
			gamePlayer.CharacterInstance.PositionCharacter(LevelLayout.GetSpawnPosition(0f));
			foreach (GamePlayer item3 in PlayerQueue)
			{
				if (gamePlayer != item3)
				{
					item3.CharacterInstance.Disable();
				}
				Spectator spectatorImage = item3.CharacterInstance.SpectatorImage;
				if (item3 == gamePlayer || spectatorImage.GetState() == Spectator.SpectatorState.DEAD || spectatorImage.GetState() == Spectator.SpectatorState.DYING)
				{
					spectatorImage.Hide();
					if (item3.CharacterInstance.OutfitArt != null)
					{
						item3.CharacterInstance.OutfitArt.SwitchToCharacter();
					}
					continue;
				}
				spectatorImage.Show();
				if (item3.CharacterInstance.OutfitArt != null)
				{
					item3.CharacterInstance.OutfitArt.SwitchToSpectator();
				}
				if (spectatorImage.GetState() == Spectator.SpectatorState.IDLE)
				{
					spectatorImage.transform.position = LevelLayout.SpectatorStart[item3.TurnOrder % LevelLayout.SpectatorStart.Length].position - new Vector3(0f, spectatorVerticalOffset, 0f);
					if (LevelLayout.SpectatorStartParent != null)
					{
						spectatorImage.transform.parent = LevelLayout.SpectatorStartParent;
					}
					else
					{
						spectatorImage.transform.parent = LevelLayout.StartPoint;
					}
				}
				else
				{
					if (spectatorImage.GetState() != Spectator.SpectatorState.VICTORY)
					{
						continue;
					}
					Transform[] array = null;
					GoalBlock goalBlockByID = LevelLayout.GetGoalBlockByID(item3.CharacterInstance.LastFlagID);
					array = ((!(goalBlockByID != null)) ? LevelLayout.SpectatorGoal : goalBlockByID.SpectatorPositions);
					if (array != null)
					{
						spectatorImage.transform.position = array[item3.TurnOrder % array.Length].position - new Vector3(0f, spectatorVerticalOffset, 0f);
						if (LevelLayout.SpectatorGoalParent != null)
						{
							spectatorImage.transform.parent = LevelLayout.SpectatorGoalParent;
						}
						else
						{
							spectatorImage.transform.parent = LevelLayout.Goal;
						}
					}
				}
			}
			MainCamera.AddTarget(gamePlayer.CharacterInstance);
		}
		MainCamera.ForceShowAllPlayer(showAll: false);
		for (int i = 0; i != winOrder.Length; i++)
		{
			winOrder[i] = null;
		}
		runStarted = false;
		AkSoundEngine.PostEvent("UI_InGame_Ready", base.gameObject);
		readyTimer = 0f;
		NetworkwaitingForCharacters = true;
		AgonoyTimerLimitTriggered = false;
		AgonyFinalCountDownStarted = false;
		if (!GameState.GetInstance().UsingHotSeat)
		{
			StartCoroutine(waitForCharacters());
		}
		else
		{
			NetworkwaitingForCharacters = false;
		}
	}

	protected override void ToSuddenDeath()
	{
		base.ToSuddenDeath();
		if (GameState.GetInstance().UsingHotSeat)
		{
			ToTimedSuddenDeath();
		}
		else
		{
			if (countdownTimer > 0f)
			{
				digitalClockInstance.Show();
				digitalClockInstance.ShowSecondsAsTime(countdownTimer);
			}
			MainCamera.ForceShowAllPlayer(showAll: false);
			MainCamera.ClearTargets();
			foreach (GamePlayer item in PlayerQueue)
			{
				if (scorekeeperInstance.GetPlayerTotal(item) >= maxScore)
				{
					if (item.IsLocalPlayer)
					{
						resetPlayerCharacter(item.CharacterInstance);
					}
					item.CharacterInstance.Waiting = true;
					MainCamera.AddTarget(item.CharacterInstance);
				}
				else if (item.IsLocalPlayer)
				{
					item.CharacterInstance.RemoveSuccess();
					item.CharacterInstance.Disable();
				}
			}
			runStarted = false;
			AkSoundEngine.PostEvent("UI_InGame_SuddenDeathAnnounced", base.gameObject);
			readyTimer = 0f;
		}
		NetworkwaitingForCharacters = true;
		if (!GameState.GetInstance().UsingHotSeat)
		{
			StartCoroutine(waitForCharacters());
		}
		else
		{
			NetworkwaitingForCharacters = false;
		}
	}

	protected void ToTimedSuddenDeath()
	{
		Debug.Log("To timed sudden death!");
		GamePlayer gamePlayer = WinnerQueue.Peek();
		gamePlayer.CharacterInstance.Disable();
		gamePlayer.CharacterInstance.Enable();
		gamePlayer.CharacterInstance.Waiting = true;
		gamePlayer.CharacterInstance.PositionCharacter(LevelLayout.GetSpawnPosition(0f));
		MainCamera.ForceShowAllPlayer(showAll: false);
		foreach (GamePlayer item in PlayerQueue)
		{
			Spectator spectatorImage = item.CharacterInstance.SpectatorImage;
			if (item == gamePlayer || spectatorImage.GetState() == Spectator.SpectatorState.DEAD || spectatorImage.GetState() == Spectator.SpectatorState.DYING)
			{
				spectatorImage.Hide();
				if (item.CharacterInstance.OutfitArt != null)
				{
					item.CharacterInstance.OutfitArt.SwitchToCharacter();
				}
				continue;
			}
			spectatorImage.Show();
			if (item.CharacterInstance.OutfitArt != null)
			{
				item.CharacterInstance.OutfitArt?.SwitchToSpectator();
			}
			if (spectatorImage.GetState() == Spectator.SpectatorState.IDLE)
			{
				spectatorImage.transform.position = LevelLayout.SpectatorStart[item.TurnOrder % LevelLayout.SpectatorStart.Length].position - new Vector3(0f, spectatorVerticalOffset, 0f);
			}
			else if (spectatorImage.GetState() == Spectator.SpectatorState.VICTORY)
			{
				Transform[] array = null;
				GoalBlock goalBlockByID = LevelLayout.GetGoalBlockByID(item.CharacterInstance.LastFlagID);
				array = ((!(goalBlockByID != null)) ? LevelLayout.SpectatorGoal : goalBlockByID.SpectatorPositions);
				if (array != null)
				{
					spectatorImage.transform.position = array[item.TurnOrder % array.Length].position - new Vector3(0f, spectatorVerticalOffset, 0f);
				}
			}
		}
		MainCamera.AddTarget(gamePlayer.CharacterInstance);
		runStarted = false;
		AkSoundEngine.PostEvent("UI_InGame_SuddenDeathAnnounced", base.gameObject);
		readyTimer = 0f;
	}

	protected override void DoStart()
	{
		base.DoStart();
		startDelayTimer += Time.unscaledDeltaTime;
		if (!(startDelayTimer >= StartDelay) || LoadingInterstitialSplash.Instance.State != UISplashScreen.STATE.HIDE)
		{
			return;
		}
		AkSoundEngine.PostEvent("UI_InGame_Level_Start_ZoomIn", base.gameObject);
		GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.START));
		if (base.hasAuthority)
		{
			nextToBuild = PlayerQueue.Peek().networkNumber;
			if (GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY)
			{
				nextPhase = StartPhase;
			}
		}
		else
		{
			nextPhase = GamePhase.WAIT;
		}
		GameControl.LogCurrentModAndRuleInfo();
	}

	private IEnumerator WaitForPreGameEvents(UnityAction onFinish)
	{
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.PARTY)
		{
			bool allIn = false;
			while (!allIn)
			{
				allIn = true;
				PartyPickCursor[] array = UnityEngine.Object.FindObjectsOfType<PartyPickCursor>();
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].SpawnedOnClient)
					{
						allIn = false;
						break;
					}
				}
				yield return null;
			}
		}
		while (base.PlayersStillLoadingSnapshot)
		{
			yield return null;
		}
		onFinish();
	}

	private bool AreAllPiecesReady()
	{
		bool result = true;
		foreach (Placeable item in PlacedThisRound)
		{
			if (item != null && !item.MarkedForDestruction && !item.Placed)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	protected override void DoPlaceMode()
	{
		base.DoPlaceMode();
		if (!base.hasAuthority)
		{
			return;
		}
		readyMessageInstance.Hide();
		if (!(scoreTimer <= 0f))
		{
			return;
		}
		if (!playersLeftToPlace)
		{
			GameSettings instance = GameSettings.GetInstance();
			if (instance.GameMode == GameState.GameMode.PARTY && remainingPartyBoxes > 0)
			{
				switch (instance.partyBoxMode)
				{
				case PartyBoxMode.Standard:
					if (AreAllPiecesReady())
					{
						CallRpcShowPartyBox();
					}
					break;
				case PartyBoxMode.AutoRandom:
					if (AreAllPiecesReady())
					{
						SetupPartyBoxForRound(doChoosePieces: false);
						StartCoroutine(DelayRpcForceSelectRandomBlocks());
					}
					break;
				}
				return;
			}
			forcePlacePhaseContinueTimer += Time.unscaledDeltaTime;
			if ((!AreAllPiecesReady() && !(forcePlacePhaseContinueTimer > forcePlacePhaseContinueTime)) || objectsHoldingUpPlacePhase > 0 || ChangingToPlayPhase)
			{
				return;
			}
			ChangingToPlayPhase = true;
			if (forcePlacePhaseContinueTimer > forcePlacePhaseContinueTime)
			{
				Debug.Log("Game Forced Place Phase to continue with timer.  Clearing all unplaced blocks.  This should not happen.   If it does happen, requires investigation.");
				foreach (Placeable allPlaceable in Placeable.AllPlaceables)
				{
					if (allPlaceable != null && !allPlaceable.Placed)
					{
						Debug.Log("Block:" + allPlaceable.Name + " was unplaced.  Deleteting");
						allPlaceable.DestroySelf(destroyChildren: true, useSmoke: false);
					}
				}
			}
			StartCoroutine(ChangeToPlayPhaseDelayed());
			return;
		}
		GameSettings instance2 = GameSettings.GetInstance();
		if (!instance2.UsePlaceTimer)
		{
			return;
		}
		if (!paused)
		{
			placementTimer -= Time.unscaledDeltaTime;
		}
		if (placementTimer > 0f && placementTimer <= instance2.PlacementWarnTime + 1f && !showingPlacementWarning)
		{
			Debug.Log("Sending RPC to show placement warning");
			CallRpcShowPlacementWarning(instance2.PlacementWarnTime, placeStarted);
			showingPlacementWarning = true;
		}
		if (!(placementTimer <= 0f))
		{
			return;
		}
		if (placeStarted)
		{
			if (!piecesRemoved)
			{
				Debug.Log("Sending RPC to remove unplaced objects");
				CallRpcRemoveUnplacedObjects();
			}
		}
		else if (showingPlacementWarning)
		{
			Debug.Log("Sending RPC to close party box");
			CallRpcSendPlacementTimerDone();
			showingPlacementWarning = false;
		}
	}

	private IEnumerator ChangeToPlayPhaseDelayed()
	{
		yield return new WaitForSeconds(0.1f);
		GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.PLACE));
		nextPhase = GamePhase.PLAY;
		ChangingToPlayPhase = false;
	}

	protected override void DoPlayMode()
	{
		base.DoPlayMode();
		runTime += Time.unscaledDeltaTime;
		GamePlayer gamePlayer = PlayerQueue.Peek();
		Character characterInstance = gamePlayer.CharacterInstance;
		if ((!GameState.GetInstance().UsingHotSeat && readyTimer <= WaitTime) || (GameState.GetInstance().UsingHotSeat && !runStarted && !acceptDown))
		{
			readyTimer += Time.unscaledDeltaTime;
			readyMessageInstance.Show();
			foreach (GamePlayer item in PlayerQueue)
			{
				item.CharacterInstance.Waiting = true;
			}
			if (GameState.GetInstance().UsingHotSeat)
			{
				suicideNoteInstance.Pause();
			}
			runTime = 0f;
			digitalClockInstance.Reset();
		}
		else if (!runStarted)
		{
			readyMessageInstance.Hide();
			lastTurnInstance.Hide();
			if (GameState.GetInstance().UsingHotSeat)
			{
				characterInstance.Waiting = false;
				suicideNoteInstance.Unpause();
				suicideNoteInstance.Reset();
				GameEventManager.SendEvent(new LevelResetEvent());
			}
			else
			{
				foreach (GamePlayer item2 in PlayerQueue)
				{
					if (item2 == null || item2.CharacterInstance == null)
					{
						Debug.LogWarning("Null character found - make sure DoPlayMode doesn't run after player objects have been destroyed.");
						continue;
					}
					MainCamera.AddTarget(item2.CharacterInstance);
					if (item2.IsLocalPlayer)
					{
						item2.CharacterInstance.Waiting = false;
						item2.CharacterInstance.Enable();
						if (Modifiers.GetInstance().PlayerPlayerCollisions || Modifiers.GetInstance().CharacterSizeMode >= 3)
						{
							item2.CharacterInstance.StartInvincibleTimer(Modifiers.GetInstance().PlayerCollisionsStartInvincibilityTime);
						}
						if (item2.CharacterInstance.hasAuthority && (paused || softPaused))
						{
							item2.CharacterInstance.Pause(softPaused);
						}
					}
				}
			}
			foreach (ActiveBlock activeBlock in activeBlocks)
			{
				if (!(activeBlock == null) && !activeBlock.Active)
				{
					activeBlock.Active = true;
				}
			}
			AkSoundEngine.PostEvent("UI_InGame_Go", base.gameObject);
			runStarted = true;
			GameSettings instance = GameSettings.GetInstance();
			float time = instance.RunTimerLimit;
			if (lastRoundsMode && lastRoundsTimer && instance.RunTimerLimit == 0)
			{
				time = lastRoundsTimeLimit;
			}
			runTimer.OnStartRun(time, alwaysShowClock: false);
		}
		if (base.hasAuthority && Modifiers.GetInstance().PostDeathBehavior == Modifiers.PostDeathBehaviors.Agony)
		{
			CheckAgonyTimeLimit();
		}
		if (digitalClockInstance.Visible)
		{
			digitalClockInstance.ShowSecondsAsTime(runTime);
		}
		if (!waitingForCharacters)
		{
			if (GameState.GetInstance().UsingHotSeat && (characterInstance.Success || characterInstance.Dead))
			{
				runTimer.OnEndRun();
				if (characterInstance.Dead)
				{
					characterInstance.SpectatorImage.SetState(Spectator.SpectatorState.DEAD);
				}
				if (characterInstance.Success)
				{
					if (danceTimer == 0f)
					{
						if (runTime < fastestTime)
						{
							fastestTime = runTime;
						}
						characterInstance.SpectatorImage.SetState(Spectator.SpectatorState.VICTORY);
						danceTimer = DanceTime;
						return;
					}
					danceTimer -= Time.unscaledDeltaTime;
					if (!(danceTimer < 0f))
					{
						return;
					}
					danceTimer = 0f;
				}
				PlayerQueue.Enqueue(PlayerQueue.Dequeue());
				if (gamePlayer.TurnOrder == lastToBuild)
				{
					int num = 0;
					int num2 = 0;
					foreach (GamePlayer item3 in PlayerQueue)
					{
						if (item3.CharacterInstance.Success)
						{
							num++;
						}
					}
					if (num > 0 && num < PlayerQueue.Count)
					{
						awardPoints = true;
					}
					else
					{
						awardPoints = false;
					}
					num2 = PlayerQueue.Count - num;
					bool flag = false;
					foreach (GamePlayer item4 in PlayerQueue)
					{
						if (item4.CharacterInstance.Success)
						{
							if (item4.CharacterInstance.Dying || item4.CharacterInstance.Dead || item4.CharacterInstance.isZombie)
							{
								flag = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.winDead, item4.networkNumber), addImmediate: true) || flag;
							}
							else
							{
								flag = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.win, item4.networkNumber), addImmediate: true) || flag;
								CallRpcIncrementCharacterSuccess((int)item4.CharacterInstance.CharacterSprite);
							}
							if (num == 1 && num2 > 1)
							{
								flag = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.soloWin, item4.networkNumber), addImmediate: true) || flag;
							}
							if (scorekeeperInstance.IsPlayerInLoseStreak(item4.networkNumber))
							{
								flag = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.comeback, item4.networkNumber), addImmediate: true) || flag;
							}
						}
					}
					if (!awardPoints)
					{
						scorekeeperInstance.ClearNewPointBlocks();
					}
					scorecardMessageInstance.AllWin = num == PlayerQueue.Count;
					scorecardMessageInstance.NoWin = num == 0;
					scorecardMessageInstance.PointsAwarded = flag || scorekeeperInstance.AreThereNonCoinPoints();
					scorecardMessageInstance.coinPoints = scorekeeperInstance.AreThereCoinPoints();
					scorecardMessageInstance.racePoints = false;
					scorecardMessageInstance.Show();
					foreach (GamePlayer item5 in PlayerQueue)
					{
						item5.CharacterInstance.SpectatorImage.Hide();
						if (item5.CharacterInstance.OutfitArt != null)
						{
							item5.CharacterInstance.OutfitArt.SwitchToCharacter();
						}
						item5.CharacterInstance.SpectatorImage.SetState(Spectator.SpectatorState.IDLE);
					}
					graphScoreBoardInstance.Show(ShowScoreTime);
					if (num == 0)
					{
						AkSoundEngine.PostEvent("UI_InGame_NoPointAwarded", base.gameObject);
					}
					else if (num == PlayerQueue.Count)
					{
						AkSoundEngine.PostEvent("UI_InGame_TooEasy", base.gameObject);
					}
					scorekeeperInstance.RemoveSpecialPoints();
					CallRpcShowScoreboard(ShowScoreTime, num2 > 0 && num > 0, !awardPoints);
					GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.PLAY));
					ChoosePostPlayPhase();
				}
				else
				{
					characterInstance.Disable();
					MainCamera.RemoveTarget(characterInstance);
					ToPlayMode();
				}
			}
			else
			{
				int num3 = 0;
				int num4 = 0;
				foreach (GamePlayer item6 in PlayerQueue)
				{
					if (item6.CharacterInstance == null)
					{
						Debug.LogWarning("Null character found - make sure DoPlayMode doesn't run after player objects have been destroyed.");
					}
					else if (item6.CharacterInstance.Success)
					{
						num4++;
						for (int i = 0; i != winOrder.Length; i++)
						{
							if (winOrder[i] == null || winOrder[i] == item6)
							{
								winOrder[i] = item6;
								break;
							}
						}
						if (!MainCamera.HasTarget(item6.CharacterInstance))
						{
							continue;
						}
						danceTimer = DanceTime;
						MainCamera.RemoveTarget(item6.CharacterInstance);
						if (!MainCamera.AnyPlayersTracked())
						{
							MainCamera.ForceShowAllPlayer(showAll: true);
						}
						if (!ZoomCamera.LocalOnly || item6.CharacterInstance.hasAuthority)
						{
							GoalBlock goalBlockByID = LevelLayout.GetGoalBlockByID(item6.CharacterInstance.LastFlagID);
							if (goalBlockByID != null)
							{
								MainCamera.AddTarget(goalBlockByID.transform);
							}
							else if (LevelLayout.Goal != null)
							{
								MainCamera.AddTarget(LevelLayout.Goal);
							}
						}
					}
					else
					{
						if (!item6.CharacterInstance.Dead)
						{
							continue;
						}
						num3++;
						bool flag2 = !item6.CharacterInstance.isGhost;
						if (Modifiers.GetInstance().PostDeathBehavior == Modifiers.PostDeathBehaviors.Agony)
						{
							if (!item6.CharacterInstance.IsDeadAndSettled && !item6.CharacterInstance.IsDeadAndDiedInPit)
							{
								flag2 = false;
							}
							if (item6.CharacterInstance.agonyTimer > 0f)
							{
								num3--;
							}
						}
						if (flag2)
						{
							MainCamera.RemoveTarget(item6.CharacterInstance);
							if (!MainCamera.AnyPlayersTracked())
							{
								MainCamera.ForceShowAllPlayer(showAll: true);
							}
						}
					}
				}
				if (PlayerQueue.Count == num3 + num4)
				{
					if (danceTimer == 0f)
					{
						danceTimer = DanceTime;
						if (num4 <= 0)
						{
							return;
						}
						{
							foreach (GamePlayer item7 in PlayerQueue)
							{
								if (item7.CharacterInstance != null && item7.CharacterInstance.Success)
								{
									GoalBlock goalBlockByID2 = LevelLayout.GetGoalBlockByID(item7.CharacterInstance.LastFlagID);
									if (goalBlockByID2 != null)
									{
										MainCamera.AddTarget(goalBlockByID2.transform);
									}
									else if (LevelLayout.Goal != null)
									{
										MainCamera.AddTarget(LevelLayout.Goal);
									}
								}
							}
							return;
						}
					}
					danceTimer -= Time.unscaledDeltaTime;
					if (!(danceTimer < 0f))
					{
						return;
					}
					danceTimer = 0f;
					if (base.hasAuthority)
					{
						bool flag3 = true;
						if (PlayerQueue.Count == 1)
						{
							foreach (GamePlayer item8 in PlayerQueue)
							{
								if (item8.CharacterInstance.Success)
								{
									if (item8.CharacterInstance.Dying || item8.CharacterInstance.Dead || item8.CharacterInstance.isZombie)
									{
										scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.winDead, item8.networkNumber), addImmediate: true);
										continue;
									}
									scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.win, item8.networkNumber), addImmediate: true);
									CallRpcIncrementCharacterSuccess((int)item8.CharacterInstance.CharacterSprite);
								}
							}
						}
						else
						{
							bool flag4 = false;
							flag3 = num3 > 0 && num4 > 0;
							GameSettings instance2 = GameSettings.GetInstance();
							foreach (GamePlayer item9 in PlayerQueue)
							{
								if (!item9.CharacterInstance.Success)
								{
									continue;
								}
								if (item9.CharacterInstance.Dying || item9.CharacterInstance.Dead || item9.CharacterInstance.isZombie)
								{
									if (flag3 || instance2.AlwaysAwardPointType(PointBlock.pointBlockType.winDead))
									{
										flag4 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.winDead, item9.networkNumber)) || flag4;
									}
								}
								else if (flag3 || instance2.AlwaysAwardPointType(PointBlock.pointBlockType.win))
								{
									flag4 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.win, item9.networkNumber)) || flag4;
									CallRpcIncrementCharacterSuccess((int)item9.CharacterInstance.CharacterSprite);
								}
								if (num4 == 1 && num3 > 1 && (flag3 || instance2.AlwaysAwardPointType(PointBlock.pointBlockType.soloWin)))
								{
									flag4 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.soloWin, item9.networkNumber)) || flag4;
								}
								Debug.Log(item9.networkNumber + " lose streak " + scorekeeperInstance.GetPlayerLoseStreak(item9) + " / " + GameSettings.GetInstance().ComebackStreak);
								if (scorekeeperInstance.IsPlayerInLoseStreak(item9.networkNumber) && (flag3 || instance2.AlwaysAwardPointType(PointBlock.pointBlockType.comeback)))
								{
									flag4 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.comeback, item9.networkNumber)) || flag4;
								}
							}
							bool flag5 = PlayerQueue.Count > 2 && num4 > 1;
							bool flag6 = false;
							if (winOrder[0] != null)
							{
								if (flag5 || GameSettings.GetInstance().AlwaysAwardPointType(PointBlock.pointBlockType.first))
								{
									flag6 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.first, winOrder[0].networkNumber)) || flag4;
								}
								if (winOrder[1] != null)
								{
									if (flag5 || GameSettings.GetInstance().AlwaysAwardPointType(PointBlock.pointBlockType.second))
									{
										flag6 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.second, winOrder[1].networkNumber)) || flag4;
									}
									if (winOrder[2] != null)
									{
										if (flag5 || GameSettings.GetInstance().AlwaysAwardPointType(PointBlock.pointBlockType.third))
										{
											flag6 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.third, winOrder[2].networkNumber)) || flag4;
										}
										if (winOrder[3] != null && (flag5 || GameSettings.GetInstance().AlwaysAwardPointType(PointBlock.pointBlockType.fourth)))
										{
											flag6 = scorekeeperInstance.AwardPoint(new PointBlock(PointBlock.pointBlockType.fourth, winOrder[3].networkNumber)) || flag4;
										}
									}
								}
							}
							if (flag6)
							{
								flag4 = true;
							}
							if (!flag3)
							{
								scorekeeperInstance.ClearNewPointBlocks();
							}
							bool allWin = num3 == 0;
							bool noWin = num4 == 0;
							bool flag7 = flag4 || scorekeeperInstance.AreThereNonCoinPoints();
							bool coinPoints = scorekeeperInstance.AreThereCoinPoints();
							scorecardMessageInstance.AllWin = allWin;
							scorecardMessageInstance.NoWin = noWin;
							scorecardMessageInstance.PointsAwarded = flag7;
							scorecardMessageInstance.coinPoints = coinPoints;
							scorecardMessageInstance.racePoints = flag6;
							if (num3 == PlayerQueue.Count)
							{
								AkSoundEngine.PostEvent("UI_InGame_NoPointAwarded", base.gameObject);
							}
							if (num3 == 0)
							{
								AkSoundEngine.PostEvent("UI_InGame_TooEasy", base.gameObject);
							}
							scorecardMessageInstance.Show();
							CallRpcShowScorecardMessage(allWin, noWin, flag7, coinPoints, flag6);
						}
						scorekeeperInstance.RemoveSpecialPoints();
						CallRpcShowScoreboard(ShowScoreTime, num3 > 0 && num4 > 0, !flag3);
					}
					awardPoints = num3 > 0 && num4 > 0;
					GameEventManager.SendEvent(new RoundCompleteEvent(awardPoints || scorekeeperInstance.AreThereCoinPoints()));
				}
				else
				{
					danceTimer -= Time.unscaledDeltaTime;
					if (danceTimer < 0f)
					{
						danceTimer = 0f;
						if (LevelLayout.Goal != null)
						{
							MainCamera.RemoveTarget(LevelLayout.Goal);
						}
						foreach (GoalBlock goalBlock in LevelLayout.goalBlocks)
						{
							MainCamera.RemoveTarget(goalBlock.transform);
						}
					}
				}
			}
		}
		if (!LobbyManager.instance.IsInOnlineGame)
		{
			return;
		}
		if (base.hasAuthority)
		{
			for (int j = 0; j != LobbyManager.instance.lobbySlots.Length; j++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)LobbyManager.instance.lobbySlots[j];
				if (lobbyPlayer != null && lobbyPlayer.LocalPlayer != null)
				{
					Character playerCharacter = lobbyPlayer.LocalPlayer.PlayerCharacter;
					if (playerCharacter != null && !playerCharacter.Dying && !playerCharacter.Dead && playerCharacter.HasExceededAFKLimit)
					{
						playerCharacter.KillCharacter("AFK Auto-Kill", deathFreezeOn: false, 0);
					}
				}
			}
			return;
		}
		for (int k = 0; k != LobbyManager.instance.lobbySlots.Length; k++)
		{
			LobbyPlayer lobbyPlayer2 = (LobbyPlayer)LobbyManager.instance.lobbySlots[k];
			if (lobbyPlayer2 != null && lobbyPlayer2.IsLocalPlayer && lobbyPlayer2.LocalPlayer != null)
			{
				GamePlayer gamePlayer2 = LobbyManager.instance.PlayerTracker.GetGamePlayer(lobbyPlayer2.networkNumber);
				if (gamePlayer2 != null && !gamePlayer2.WasKicked && gamePlayer2.CharacterInstance != null && gamePlayer2.CharacterInstance.HasExceededAFKLimit)
				{
					gamePlayer2.WasKicked = true;
					gamePlayer2.CharacterInstance.CallCmdIShouldBeKicked(LobbyManager.KickReasons.AFK);
				}
			}
		}
	}

	protected override void DoSuddenDeath()
	{
		base.DoSuddenDeath();
		GameIntensityLevel = 100f;
		if (scoreTimer > 0f || turnMessageTimer > 0f)
		{
			return;
		}
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item.InPhase != GamePhase.SUDDENDEATH)
			{
				Debug.Log("Waiting for " + item.PickedAnimal.ToString() + " to enter sudden death");
				return;
			}
		}
		if (GameState.GetInstance().UsingHotSeat)
		{
			DoTimedSuddenDeath();
		}
		else
		{
			if (waitingForCharacters)
			{
				return;
			}
			if (readyTimer <= WaitTime)
			{
				readyTimer += Time.unscaledDeltaTime;
				suddenDeathMessageInstance.Show();
				foreach (GamePlayer item2 in PlayerQueue)
				{
					if (item2.IsLocalPlayer)
					{
						item2.CharacterInstance.Waiting = true;
					}
				}
			}
			else if (!runStarted)
			{
				suddenDeathMessageInstance.Hide();
				lastTurnInstance.Hide();
				foreach (GamePlayer item3 in PlayerQueue)
				{
					item3.CharacterInstance.Waiting = false;
					if (Modifiers.GetInstance().PlayerPlayerCollisions || Modifiers.GetInstance().CharacterSizeMode >= 3)
					{
						item3.CharacterInstance.StartInvincibleTimer(Modifiers.GetInstance().PlayerCollisionsStartInvincibilityTime);
					}
				}
				foreach (ActiveBlock activeBlock in activeBlocks)
				{
					if (!(activeBlock == null))
					{
						activeBlock.Active = true;
					}
				}
				AkSoundEngine.PostEvent("UI_InGame_Go", base.gameObject);
				runStarted = true;
			}
			if (countdownTimer > 0f)
			{
				countdownTimer -= Time.unscaledDeltaTime;
				digitalClockInstance.Show();
				if (countdownTimer <= 0f)
				{
					countdownTimer = 0f;
					if (base.hasAuthority)
					{
						CallRpcSetCountdown(0f);
						digitalClockInstance.ShowSecondsAsTime(0f);
						nextPhase = GamePhase.END;
						return;
					}
				}
				digitalClockInstance.ShowSecondsAsTime(countdownTimer);
			}
			bool flag = false;
			int num = 0;
			int num2 = 0;
			foreach (GamePlayer item4 in PlayerQueue)
			{
				if (scorekeeperInstance.GetPlayerTotal(item4) < maxScore)
				{
					continue;
				}
				num2++;
				if (!(item4.CharacterInstance != null) || !item4.CharacterInstance.Enabled)
				{
					continue;
				}
				if (flag)
				{
					item4.CharacterInstance.Waiting = true;
				}
				else if (item4.CharacterInstance.Success)
				{
					Debug.Log("there's a winner: " + item4.PickedAnimal);
					winner = item4;
					flag = true;
					scorekeeperInstance.AddPointsDirectly(item4, 1);
					if (base.hasAuthority)
					{
						nextPhase = GamePhase.END;
					}
				}
				else if (item4.CharacterInstance.Dead)
				{
					num++;
					if (item4.IsLocalPlayer && AllowRespawn && item4.CharacterInstance.LocallyDead)
					{
						resetPlayerCharacter(item4.CharacterInstance);
					}
				}
			}
			if (num2 == 0)
			{
				fadeToLobby();
			}
			if (num2 == 1)
			{
				foreach (GamePlayer item5 in PlayerQueue)
				{
					if (scorekeeperInstance.GetPlayerTotal(item5) >= maxScore)
					{
						Debug.Log("there's a winner: " + item5.PickedAnimal);
						winner = item5;
						flag = true;
						if (base.hasAuthority)
						{
							nextPhase = GamePhase.END;
						}
					}
				}
			}
			if (base.hasAuthority && num2 == num && runStarted)
			{
				ToSuddenDeath();
			}
		}
	}

	private void resetPlayerCharacter(Character c)
	{
		Vector3 vector = LevelLayout.GetSpawnPosition(0f);
		Modifiers instance = Modifiers.GetInstance();
		if (instance.PlayerPlayerCollisions)
		{
			List<Character> list = new List<Character>();
			foreach (GamePlayer item in PlayerQueue)
			{
				if (item != null && item.CharacterInstance != null && item.CharacterInstance != c)
				{
					Character characterInstance = item.CharacterInstance;
					if ((vector - characterInstance.transform.position).magnitude < 10f)
					{
						list.Add(characterInstance);
					}
				}
			}
			if (list.Count > 0)
			{
				float num = 0.05f;
				float num2 = 0.33f * instance.CharacterScale * 2f;
				float y = LevelLayout.GetSpawnPosition(0f).y;
				bool flag = true;
				for (int i = 0; i < 3 && flag; i++)
				{
					flag = false;
					foreach (Character item2 in list)
					{
						Vector3 vector2 = item2.transform.position - vector;
						float magnitude = vector2.magnitude;
						if (magnitude < num)
						{
							Vector3 normalized = new Vector3(UnityEngine.Random.Range(-1f, 1f), 1f, 0f).normalized;
							vector += normalized * (num2 + num);
							flag = true;
						}
						else if (magnitude < num2)
						{
							vector = item2.transform.position + vector2 / magnitude * (num2 + num);
							flag = true;
						}
					}
					if (vector.y < y)
					{
						vector.y = y;
						flag = true;
					}
				}
			}
		}
		c.Disable(moveAway: false);
		c.PositionCharacter(vector);
		c.Enable();
		c.StartInvincibleTimer(1f);
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, c.transform.position, 0.5f);
	}

	protected void DoTimedSuddenDeath()
	{
		GamePlayer gamePlayer = WinnerQueue.Peek();
		Character characterInstance = gamePlayer.CharacterInstance;
		runTime += Time.unscaledDeltaTime;
		GameIntensityLevel = 100f;
		if ((!GameState.GetInstance().UsingHotSeat && readyTimer <= WaitTime) || (GameState.GetInstance().UsingHotSeat && !runStarted && !acceptDown))
		{
			readyTimer += Time.unscaledDeltaTime;
			suddenDeathMessageInstance.IsTimer = true;
			suddenDeathMessageInstance.Show();
			characterInstance.Waiting = true;
			runTime = 0f;
			digitalClockInstance.Reset();
		}
		else if (!runStarted)
		{
			digitalClockInstance.Show();
			suddenDeathMessageInstance.Hide();
			lastTurnInstance.Hide();
			characterInstance.Waiting = false;
			foreach (ActiveBlock activeBlock in activeBlocks)
			{
				if (!(activeBlock == null))
				{
					activeBlock.Active = true;
				}
			}
			AkSoundEngine.PostEvent("UI_InGame_Go", base.gameObject);
			runStarted = true;
		}
		digitalClockInstance.ShowSecondsAsTime(runTime);
		if (!characterInstance.Success && !characterInstance.Dead)
		{
			return;
		}
		if (characterInstance.Dead)
		{
			awardPoints = true;
			characterInstance.SpectatorImage.SetState(Spectator.SpectatorState.DEAD);
		}
		if (characterInstance.Success)
		{
			if (!GameState.GetInstance().UsingHotSeat)
			{
				characterInstance.SpectatorImage.SetState(Spectator.SpectatorState.VICTORY);
			}
			Debug.Log(runTime + " vs. " + fastestTime + ((winOrder[0] != null) ? (" (" + winOrder[0].CharacterInstance.CharacterSprite.ToString() + ")") : ""));
			if (runTime < fastestTime)
			{
				fastestTime = runTime;
				winOrder[0] = gamePlayer;
				suddenDeathMessageInstance.TimeToBeat = fastestTime;
			}
		}
		do
		{
			PlayerQueue.Enqueue(PlayerQueue.Dequeue());
		}
		while (scorekeeperInstance.GetPlayerTotal(PlayerQueue.Peek()) < maxScore);
		WinnerQueue.Enqueue(WinnerQueue.Dequeue());
		if (gamePlayer.TurnOrder == lastWinner)
		{
			Debug.Log("Sudden death over");
			if (winOrder[0] != null)
			{
				winner = winOrder[0];
				Debug.Log("Winner: " + winner.networkNumber);
				nextPhase = GamePhase.END;
			}
			else
			{
				characterInstance.Disable();
				MainCamera.RemoveTarget(characterInstance);
				ToSuddenDeath();
			}
		}
		else
		{
			characterInstance.Disable();
			MainCamera.RemoveTarget(characterInstance);
			ToSuddenDeath();
		}
	}

	protected override void sendEndAnalytics()
	{
		base.sendEndAnalytics();
		int num = maxScore;
		Dictionary<GamePlayer, int> dictionary = new Dictionary<GamePlayer, int>();
		foreach (GamePlayer item in PlayerQueue)
		{
			int playerTotal = scorekeeperInstance.GetPlayerTotal(item);
			dictionary.Add(item, playerTotal);
			if (playerTotal < num)
			{
				num = playerTotal;
			}
		}
		List<int> list = dictionary.Values.ToList();
		list.Sort();
		foreach (Player item2 in PlayerManager.GetInstance())
		{
			if (item2 != null)
			{
				int num2 = dictionary[item2.AssociatedGamePlayer];
				int num3 = 0;
				for (num3 = 0; num3 < list.Count && num2 != list[num3]; num3++)
				{
				}
				AnalyticEvent.PlayerRankingEvent(base.MatchGuid, num3, num2, item2.AssociatedLobbyPlayer.SkillMean, item2.AssociatedLobbyPlayer.SkillStdDev);
			}
		}
		if (base.hasAuthority)
		{
			AnalyticEvent.MatchEndHostEvent(base.MatchGuid, maxScore - num, kicks, quits - kicks, Time.timeSinceLevelLoad, roundNumber, winner != null);
		}
		AnalyticEvent.MatchEndClientEvent(base.MatchGuid, ZoomCamera.GlobalCameraTime, ZoomCamera.LocalCameraTime);
	}

	protected override void SetupEnd()
	{
		base.SetupEnd();
		MainCamera.ClearTargets();
		Dictionary<GamePlayer, int> dictionary = new Dictionary<GamePlayer, int>();
		foreach (GamePlayer item in PlayerQueue)
		{
			dictionary.Add(item, scorekeeperInstance.GetPlayerTotal(item));
			if (winner == null && item.CharacterInstance != null && item.CharacterInstance.Success)
			{
				if (!(winner == null))
				{
					winner = null;
					break;
				}
				winner = item;
			}
		}
		GameEventManager.SendEvent(new GameResultsEvent(dictionary));
		if (base.hasAuthority)
		{
			if (winner != null)
			{
				CallRpcSetWinner(winner.gameObject);
			}
			else
			{
				CallRpcSetWinner(null);
			}
		}
		StartCoroutine(waitForWinnerSet());
	}

	protected override void DoEnd()
	{
		base.DoEnd();
		if (winnerSet && winner != null && !graphScoreBoardInstance.DrawingScore)
		{
			winMessageInstance.Show();
		}
		winTimer += Time.unscaledDeltaTime;
		if (winTimer > WinTime)
		{
			PrepareToLeave();
			fadeToLobby();
			if (base.hasAuthority)
			{
				nextPhase = GamePhase.WAIT;
			}
		}
	}

	private IEnumerator waitForWinnerSet()
	{
		while (!winnerSet)
		{
			yield return null;
		}
		if (winner != null && winner.CharacterInstance != null)
		{
			Character characterInstance = winner.CharacterInstance;
			Debug.Log(characterInstance.CharacterSprite.ToString() + "  WINS");
			GoalBlock goalBlockByID = LevelLayout.GetGoalBlockByID(characterInstance.LastFlagID);
			Transform[] array = ((!(goalBlockByID != null)) ? LevelLayout.SpectatorGoal : goalBlockByID.SpectatorPositions);
			if (array != null)
			{
				characterInstance.transform.position = array[0].position;
			}
			characterInstance.Enable();
			if (base.hasAuthority)
			{
				CallRpcIncrementCharacterWins((int)characterInstance.CharacterSprite);
			}
			if (!scorekeeperInstance.SpecialPointCheck(winner) && base.hasAuthority)
			{
				CallRpcBackToBasicAchievement(winner.networkNumber);
			}
			MainCamera.AddTarget(array[0]);
			MainCamera.AddTarget(characterInstance.transform);
			characterInstance.Ready = true;
			winMessageInstance.SetWinnerNameSprite(characterInstance.LocalizedNameWinMessage, characterInstance.CharacterSprite, characterInstance.AssociatedGamePlayer.IsWearingSkin);
			lastRoundsMode = false;
		}
		else
		{
			Debug.Log("Tie game!");
			if (LevelLayout.Goal != null)
			{
				MainCamera.AddTarget(LevelLayout.Goal);
			}
			if (base.hasAuthority)
			{
				WinTime = 4f;
				CallRpcShowLastTurnMessage(-1);
			}
		}
		AkSoundEngine.PostEvent("UI_InGame_VictoryScreen_ZoomIn", base.gameObject);
		AkSoundEngine.PostEvent("MUS_Victory", base.gameObject);
	}

	protected void ChoosePostPlayPhase()
	{
		WinnerQueue = new Queue<GamePlayer>();
		int num = maxScore;
		foreach (GamePlayer item in PlayerQueue)
		{
			if (scorekeeperInstance.GetPlayerTotal(item) >= num)
			{
				lastWinner = item.TurnOrder;
				WinnerQueue.Enqueue(item);
			}
			Character characterInstance = item.CharacterInstance;
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, characterInstance.transform.position, 0.5f);
			characterInstance.Disable(moveAway: false);
			MainCamera.RemoveTarget(characterInstance);
		}
		if (WinnerQueue.Count > 0)
		{
			Debug.Log("There's a winner");
			if (WinnerQueue.Count > 1)
			{
				Debug.Log("There's " + WinnerQueue.Count + " winners");
				if (base.hasAuthority)
				{
					countdownTimer = 60f;
					CallRpcSetCountdown(60f);
				}
				nextPhase = GamePhase.SUDDENDEATH;
				if (GameState.GetInstance().UsingHotSeat)
				{
					fastestTime = float.PositiveInfinity;
					for (int i = 0; i != winOrder.Length; i++)
					{
						winOrder[i] = null;
					}
				}
				if (base.hasAuthority)
				{
					NetworkskipTurnMessage = true;
				}
				return;
			}
			foreach (GamePlayer item2 in PlayerQueue)
			{
				if (scorekeeperInstance.GetPlayerTotal(item2) >= maxScore)
				{
					winner = item2;
					break;
				}
			}
			Debug.Log("Winner: " + winner.networkNumber);
			nextPhase = GamePhase.END;
		}
		else
		{
			nextPhase = GamePhase.PLACE;
		}
	}

	public override void ShowScoreboard()
	{
		base.ShowScoreboard();
		if (!paused)
		{
			MainCamera.AllowFollow(follow: false);
		}
	}

	public override void AfterScoreBoard()
	{
		base.AfterScoreBoard();
		StartCoroutine(AfterScoreBoardCoroutine());
	}

	public IEnumerator AfterScoreBoardCoroutine()
	{
		float num = ((roundNumber <= 0) ? 0f : (levelTimer / (float)roundNumber));
		GameSettings instance = GameSettings.GetInstance();
		if (instance.GameLimitType == GameLimitType.TIME)
		{
			float num2 = (float)instance.MaxTime - levelTimer;
			float placeTime = lastRoundsPlaceTime;
			if (instance.UsePlaceTimer)
			{
				placeTime = instance.PlaceTime;
			}
			float num3 = minLastRoundsRunTime;
			if (instance.RunTimerLimit > 0)
			{
				num3 = instance.RunTimerLimit;
			}
			float num4 = placeTime + num3;
			if (num2 < (float)(instance.numLastRounds + 1) * Mathf.Min(num4, num))
			{
				if (instance.GameMode == GameState.GameMode.PARTY)
				{
					num4 += lastRoundsPlaceTime;
				}
				num4 = Mathf.Min(num, num4);
				if (num2 < num4)
				{
					num2 = num4;
				}
				int num5 = Mathf.Min(Mathf.FloorToInt(num2 / num4), instance.numLastRounds);
				if (!lastRoundsMode)
				{
					lastRoundsToGo = num5;
					lastRoundsTimer = true;
					lastRoundsTimeLimit = num2 / (float)num5;
					lastRoundsMode = true;
				}
				else
				{
					lastRoundsToGo--;
				}
			}
		}
		else if (instance.GameLimitType == GameLimitType.ROUNDS && roundNumber >= instance.MaxRounds - instance.numLastRounds)
		{
			if (!lastRoundsMode)
			{
				lastRoundsMode = true;
				if (instance.MaxRounds <= instance.numLastRounds)
				{
					lastRoundsToGo = instance.MaxRounds - roundNumber;
					if (lastRoundsToGo < 0)
					{
						lastRoundsToGo = 0;
					}
				}
				else
				{
					lastRoundsToGo = instance.numLastRounds;
				}
			}
			else
			{
				lastRoundsToGo--;
			}
		}
		MainCamera.AllowFollow(follow: true);
		scoreTimer = 0f;
		graphScoreBoardInstance.Hide();
		scorecardMessageInstance.Hide();
		if (!skipTurnMessage && lastRoundsMode && nextPhase == GamePhase.PLACE && winner == null && base.hasAuthority)
		{
			CallRpcShowLastTurnMessage(lastRoundsToGo);
			yield return new WaitForSeconds(turnMessageTime);
			CallRpcHideLastTurnMessage();
		}
		scoreboard = false;
	}

	private IEnumerator waitForCharacters()
	{
		bool allReady = false;
		float forceWaitOver = 10f;
		while (!allReady && forceWaitOver > 0f)
		{
			allReady = true;
			forceWaitOver -= Time.unscaledDeltaTime;
			foreach (GamePlayer item in PlayerQueue)
			{
				if (item == null)
				{
					Debug.LogWarning("waitForCharacters: Null player found in queue");
				}
				else if (item.CharacterInstance == null)
				{
					Debug.LogWarning("waitForCharacters: Null CharacterInstance for player " + item.networkNumber);
				}
				else if ((item.CharacterInstance.Success && base.Phase != GamePhase.SUDDENDEATH) || (item.CharacterInstance.Dead && base.Phase != GamePhase.SUDDENDEATH))
				{
					allReady = false;
					break;
				}
			}
			yield return null;
		}
		if (forceWaitOver < 0f)
		{
			Debug.Log("Force wait over was required. Something weird happened.");
		}
		Debug.Log("Done waiting for characters");
		NetworkwaitingForCharacters = false;
	}

	protected override void DrawDebug()
	{
		base.DrawDebug();
		float num = 0f;
		if (GameSettings.GetInstance().GameLimitType == GameLimitType.TIME)
		{
			num = (float)GameSettings.GetInstance().MaxTime - levelTimer;
		}
		if (num < 0f)
		{
			num = 0f;
		}
		int num2 = (int)num / 60;
		float num3 = num - (float)(60 * num2);
		int num4 = (int)levelTimer / 60;
		float num5 = levelTimer - (float)(num4 * 60);
		DrawDebugText("Time: " + num4 + ":" + num5.ToString("0#.00") + " (" + num2 + ":" + num3.ToString("0#.00") + " left)");
		if (!DebugPartyBox)
		{
			return;
		}
		foreach (string item in partyboxDebugInfo)
		{
			DrawDebugText(item);
		}
	}

	protected override void resetTurnOrders()
	{
		base.resetTurnOrders();
		if (lastToBuild == PlayerQueue.Count)
		{
			lastToBuild = PlayerQueue.Count - 1;
		}
		if (nextToBuild == PlayerQueue.Count)
		{
			nextToBuild = 0;
		}
	}

	public override void ReceiveEvent(InputEvent e)
	{
		if (GameState.GetInstance().UsingHotSeat)
		{
			if (e.PlayerBitMask == 0 || ((base.Phase == GamePhase.PLAY || base.Phase == GamePhase.SUDDENDEATH) && (e.PlayerBitMask & (1 << PlayerQueue.Peek().localNumber - 1)) == 0))
			{
				return;
			}
			if (base.Phase == GamePhase.PLACE)
			{
				bool flag = false;
				foreach (GamePlayer item in PlayerQueue)
				{
					if ((e.PlayerBitMask & (1 << item.localNumber - 1)) != 0 && item.TurnOrder == lastToBuild)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
		}
		base.ReceiveEvent(e);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		Type type = e.GetType();
		if (type == typeof(GamePlayerRemovedEvent))
		{
			GamePlayerRemovedEvent gamePlayerRemovedEvent = e as GamePlayerRemovedEvent;
			Debug.Log("Player removed from game");
			for (int i = 0; i < PlayerQueue.Count; i++)
			{
				int num = maxScore;
				GamePlayer gamePlayer = PlayerQueue.Dequeue();
				if (gamePlayer.networkNumber == gamePlayerRemovedEvent.PlayerNetworkNumber)
				{
					graphScoreBoardInstance.MarkPlayerDisconnected(gamePlayer.PickedAnimal);
					scorekeeperInstance.MarkPlayerDisconnected(gamePlayer);
					if (gamePlayer.CharacterInstance != null)
					{
						if (gamePlayer.CharacterInstance.Enabled)
						{
							SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, gamePlayer.CharacterInstance.transform.position);
						}
						MainCamera.RemoveTarget(gamePlayer.CharacterInstance);
						UnityEngine.Object.Destroy(gamePlayer.CharacterInstance.gameObject);
					}
					if (gamePlayer.CursorInstance != null)
					{
						if (gamePlayer.CursorInstance.Enabled)
						{
							SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, gamePlayer.CursorInstance.transform.position);
						}
						if (gamePlayer.CursorInstance is PiecePlacementCursor)
						{
							PiecePlacementCursor piecePlacementCursor = (PiecePlacementCursor)gamePlayer.CursorInstance;
							RemainingPlacements[gamePlayer.networkNumber - 1] = 0;
							if (piecePlacementCursor.Piece != null)
							{
								UnityEngine.Object.Destroy(piecePlacementCursor.Piece.gameObject);
							}
						}
						MainCamera.RemoveTarget(gamePlayer.CursorInstance);
						UnityEngine.Object.Destroy(gamePlayer.CursorInstance.gameObject);
					}
					if (base.Phase != GamePhase.SUDDENDEATH)
					{
						continue;
					}
					int num2 = 0;
					foreach (GamePlayer item in PlayerQueue)
					{
						if (scorekeeperInstance.GetPlayerTotal(item) >= num)
						{
							num2++;
						}
					}
					if (num2 == 0)
					{
						fadeToLobby();
					}
				}
				else
				{
					PlayerQueue.Enqueue(gamePlayer);
				}
			}
			resetTurnOrders();
		}
		if (type == typeof(NetworkClientDisconnectEvent))
		{
			Debug.Log("Client removed from game");
			NetworkClientDisconnectEvent obj = e as NetworkClientDisconnectEvent;
			fadeToLobby();
			GameEventManager.SendEvent(new NetworkClientCleanedUpEvent(obj.ConnectionToClient));
		}
		if (type == typeof(DestroyPieceEvent))
		{
			DestroyPieceEvent destroyPieceEvent = e as DestroyPieceEvent;
			if (base.Phase == GamePhase.PLACE && GameSettings.GetInstance().GameMode == GameState.GameMode.CREATIVE && base.hasAuthority && destroyPieceEvent.Piece.PickedUp)
			{
				RemainingPlacements[destroyPieceEvent.PlayerNetworkNumber - 1]--;
				OnRemainingPlacementsChanged(destroyPieceEvent.PlayerNetworkNumber);
			}
		}
		if (type == typeof(PiecePlacedEvent))
		{
			PiecePlacedEvent piecePlacedEvent = e as PiecePlacedEvent;
			if (piecePlacedEvent.PlayerNumber != 0)
			{
				Debug.Log(piecePlacedEvent.PlayerNumber + " placed " + piecePlacedEvent.PlacedBlock?.ToString() + "(" + RemainingPlacements[piecePlacedEvent.PlayerNumber - 1] + " left)");
			}
		}
		if (type == typeof(PlacementSkippedEvent))
		{
			PlacementSkippedEvent placementSkippedEvent = e as PlacementSkippedEvent;
			RemainingPlacements[placementSkippedEvent.PlayerNumber - 1] = 0;
			Debug.Log("Placement skipped");
			foreach (GamePlayer item2 in PlayerQueue)
			{
				if (placementSkippedEvent.PlayerNumber == item2.networkNumber)
				{
					MainCamera.RemoveTarget(item2.CursorInstance);
					if (!MainCamera.AnyPlayersTracked())
					{
						MainCamera.ForceShowAllPlayer(showAll: true);
					}
					item2.CursorInstance.Hide();
				}
			}
			if (playersLeftToPlace && GameState.GetInstance().UsingHotSeat)
			{
				GamePlayer gamePlayer2 = PlayerQueue.Dequeue();
				Debug.Log("Switching hotseat to " + gamePlayer2.CharacterInstance.CharacterSprite);
				gamePlayer2.CursorInstance.Enable();
				gamePlayer2.CursorInstance.transform.position = LevelLayout.CursorSpawnPoint.position;
				MainCamera.AddTarget(gamePlayer2.CursorInstance);
				PlayerQueue.Enqueue(gamePlayer2);
				nextToBuild = PlayerQueue.Peek().TurnOrder;
				lastToBuild = gamePlayer2.TurnOrder;
			}
		}
		if (type == typeof(PartyCursorSpawnedEvent))
		{
			PartyCursorSpawnedEvent partyCursorSpawnedEvent = e as PartyCursorSpawnedEvent;
			if (partyBoxInstance != null)
			{
				partyCursorSpawnedEvent.SpawnedCursor.transform.parent = partyBoxInstance.transform;
				if (partyCursorSpawnedEvent.SpawnedCursor.hasAuthority)
				{
					partyCursorSpawnedEvent.SpawnedCursor.transform.localPosition = Vector3.forward;
					partyCursorSpawnedEvent.SpawnedCursor.SetBounds(new Bounds(partyCursorSpawnedEvent.SpawnedCursor.transform.localPosition, partyBoxInstance.BoundingBox.size));
					partyCursorSpawnedEvent.SpawnedCursor.UseCamera = partyBoxInstance.UICamera;
				}
			}
			else
			{
				Debug.LogError("ERROR: Party box not spawned.");
			}
		}
		if (type == typeof(StartPhaseEvent) && (e as StartPhaseEvent).Phase == GamePhase.PLAY)
		{
			placementEndingMessageInstance.Hide();
		}
		if (type == typeof(EndPhaseEvent) && (e as EndPhaseEvent).Phase == GamePhase.PLAY && GameState.GetInstance().UsingHotSeat)
		{
			PlayerQueue.Enqueue(PlayerQueue.Dequeue());
		}
		if (type == typeof(PartyBoxEvent) && !(e as PartyBoxEvent).Opened)
		{
			SetupPlacementCursors();
		}
		if (!(type == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PiecePlaced)
		{
			MsgPiecePlaced msgPiecePlaced = (MsgPiecePlaced)networkMessageReceivedEvent.ReadMessage;
			if (msgPiecePlaced.PieceID != 0)
			{
				if (!msgPiecePlaced.ResetPosition)
				{
					RemainingPlacements[msgPiecePlaced.PlayerNumber - 1]--;
				}
				bool flag = false;
				foreach (Placeable allPlaceable in Placeable.AllPlaceables)
				{
					if (allPlaceable != null && allPlaceable.ID == msgPiecePlaced.PieceID)
					{
						PlacedThisRound.Add(allPlaceable);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Debug.LogError("Piece not added to PlacedThisRound: ID " + msgPiecePlaced.PieceID + " not found.");
				}
				OnRemainingPlacementsChanged(msgPiecePlaced.PlayerNumber);
			}
		}
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ForcedPieceSpawned && base.hasAuthority)
		{
			MsgForcedPieceSpawned msgForcedPieceSpawned = (MsgForcedPieceSpawned)networkMessageReceivedEvent.ReadMessage;
			OnForcedPieceSpawned(msgForcedPieceSpawned.playerNumber);
		}
	}

	private void SetupPlacementCursors()
	{
		if (base.hasAuthority)
		{
			remainingPartyBoxes--;
			IsSecondBox = true;
			foreach (GamePlayer item in PlayerQueue)
			{
				RemainingPlacements[item.networkNumber - 1] = 1;
			}
		}
		float num = (float)(360 / LobbyManager.instance.lobbySlots.Length) * (MathF.PI / 180f);
		foreach (uint allGameNetID in LobbyManager.instance.PlayerTracker.GetAllGameNetIDs())
		{
			if (allGameNetID == 0)
			{
				continue;
			}
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
			if (gameObject == null)
			{
				continue;
			}
			GamePlayer component = gameObject.GetComponent<GamePlayer>();
			if (component == null || component.CursorInstance == null)
			{
				continue;
			}
			PiecePlacementCursor component2 = component.CursorInstance.GetComponent<PiecePlacementCursor>();
			if (component2 != null && component2.Piece != null)
			{
				MainCamera.AddTarget(component.CursorInstance);
				if (component.IsLocalPlayer)
				{
					component.CursorInstance.Enable();
					component.CursorInstance.transform.position = new Vector3(Mathf.Cos(num * (float)component.networkNumber) * LevelLayout.CursorSpawnRadius, Mathf.Sin(num * (float)component.networkNumber) * LevelLayout.CursorSpawnRadius, 0f) + LevelLayout.CursorSpawnPoint.position;
				}
			}
			else if (component.IsLocalPlayer)
			{
				Debug.LogWarning("Placement Cursor has no piece; skipping");
				MsgPiecePlaced msgPiecePlaced = new MsgPiecePlaced();
				msgPiecePlaced.PlayerNumber = component.CursorInstance.networkNumber;
				msgPiecePlaced.PiecePosition = Vector3.zero;
				msgPiecePlaced.PieceScale = Vector3.zero;
				msgPiecePlaced.PieceRotation = Quaternion.identity;
				msgPiecePlaced.PieceID = 0;
				NetworkManager.singleton.client.Send(NetMsgTypes.PiecePlaced, msgPiecePlaced);
			}
		}
		showingPlacementWarning = false;
		if (placementEndingMessageInstance != null)
		{
			placementEndingMessageInstance.Hide();
		}
		placementTimer = GameSettings.GetInstance().PlaceTime;
		placeStarted = true;
		piecesRemoved = false;
	}

	private void OnRemainingPlacementsChanged(int playerNumber)
	{
		if (base.hasAuthority && GameSettings.GetInstance().GameMode == GameState.GameMode.CREATIVE)
		{
			foreach (GamePlayer item in PlayerQueue)
			{
				if (playerNumber == item.networkNumber)
				{
					(item.CursorInstance as PiecePlacementCursor).CallRpcSetPlacementsLeftText(RemainingPlacements[playerNumber - 1]);
					break;
				}
			}
		}
		if (RemainingPlacements[playerNumber - 1] > 0)
		{
			return;
		}
		Debug.Log("Player " + playerNumber + " done placing");
		foreach (GamePlayer item2 in PlayerQueue)
		{
			if (playerNumber == item2.networkNumber)
			{
				item2.CursorInstance.Hide();
				MainCamera.RemoveTarget(item2.CursorInstance);
				if (!MainCamera.AnyPlayersTracked())
				{
					MainCamera.ForceShowAllPlayer(showAll: true);
				}
			}
		}
		if (playersLeftToPlace && GameState.GetInstance().UsingHotSeat)
		{
			GamePlayer gamePlayer = PlayerQueue.Dequeue();
			Debug.Log("Switching hotseat to " + gamePlayer.CharacterInstance.CharacterSprite);
			gamePlayer.CursorInstance.Enable();
			gamePlayer.CursorInstance.transform.position = LevelLayout.CursorSpawnPoint.position;
			MainCamera.AddTarget(gamePlayer.CursorInstance);
			PlayerQueue.Enqueue(gamePlayer);
			nextToBuild = PlayerQueue.Peek().TurnOrder;
			lastToBuild = gamePlayer.TurnOrder;
			placementTimer = GameSettings.GetInstance().PlaceTime;
			showingPlacementWarning = false;
			placementEndingMessageInstance.Hide();
			piecesRemoved = false;
		}
	}

	[ClientRpc]
	private void RpcShowScorecardMessage(bool allWin, bool noWin, bool anyPoints, bool coinPoints, bool racePoints)
	{
		if (!base.hasAuthority)
		{
			scorecardMessageInstance.AllWin = allWin;
			scorecardMessageInstance.NoWin = noWin;
			scorecardMessageInstance.PointsAwarded = anyPoints;
			scorecardMessageInstance.coinPoints = coinPoints;
			scorecardMessageInstance.racePoints = racePoints;
			scorecardMessageInstance.Show();
		}
	}

	[ClientRpc]
	private void RpcIncrementCharacterSuccess(int characterSpriteNumber)
	{
		StatTracker.Instance.GetSaveFileDataForAnimal((Character.Animals)characterSpriteNumber, fallback: true)?.IncrementStat("CharacterSuccess", characterSpriteNumber);
	}

	[ClientRpc]
	private void RpcIncrementCharacterWins(int characterSpriteNumber)
	{
		StatTracker.Instance.GetSaveFileDataForAnimal((Character.Animals)characterSpriteNumber, fallback: true)?.IncrementStat("CharacterWins", characterSpriteNumber);
	}

	[ClientRpc]
	private void RpcBackToBasicAchievement(int networkNumber)
	{
		SaveFileData saveFileDataFromNetworkNumber = StatTracker.Instance.GetSaveFileDataFromNetworkNumber(networkNumber, fallback: true);
		if (saveFileDataFromNetworkNumber != null)
		{
			AchievementChecker.Instance.Back_to_the_Basics_AchievementUnlock(saveFileDataFromNetworkNumber);
		}
	}

	[ClientRpc]
	private void RpcShowLastTurnMessage(int numberRemaining)
	{
		lastTurnInstance.ShowTurns(numberRemaining);
	}

	[ClientRpc]
	private void RpcHideLastTurnMessage()
	{
		lastTurnInstance.Hide();
	}

	[ClientRpc]
	private void RpcShowScoreboard(float ShowScoreTime, bool checkStreaks, bool clearPoints)
	{
		if (clearPoints)
		{
			scorekeeperInstance.ClearNewPointBlocks();
		}
		graphScoreBoardInstance.displayNewScore(scorekeeperInstance.newPointBlocks);
		scorekeeperInstance.TallyPointBlockAllPlayers(checkStreaks);
		graphScoreBoardInstance.Show(ShowScoreTime);
		scoreboard = true;
		runTimer.OnEndRun();
		GameEventManager.SendEvent(new PlayersDoneRunning());
		if (base.hasAuthority)
		{
			GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.PLAY));
			ChoosePostPlayPhase();
			return;
		}
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item != null && item.CharacterInstance != null)
			{
				item.CharacterInstance.Disable(moveAway: false);
			}
		}
	}

	[ClientRpc]
	private void RpcShowPlacementWarning(float time, bool showText)
	{
		placementEndingMessageInstance.Show(time, showText);
	}

	[ClientRpc]
	private void RpcSendPlacementTimerDone()
	{
		GameEventManager.SendEvent(new PlacementTimerDoneEvent());
	}

	[ClientRpc]
	private void RpcRemoveUnplacedObjects()
	{
		if (piecesRemoved)
		{
			return;
		}
		GameEventManager.SendEvent(new PlacementTimerDoneEvent());
		foreach (GamePlayer item in PlayerQueue)
		{
			if (!(item.CursorInstance != null) || (GameState.GetInstance().UsingHotSeat && PlayerQueue.LastOrDefault() != item))
			{
				continue;
			}
			PiecePlacementCursor piecePlacementCursor = (PiecePlacementCursor)item.CursorInstance;
			if (piecePlacementCursor.Piece != null)
			{
				UnityEngine.Object.Destroy(piecePlacementCursor.Piece.gameObject);
			}
			RemainingPlacements[item.networkNumber - 1] = 0;
			if (item.CursorInstance.Enabled && item.IsLocalPlayer)
			{
				piecePlacementCursor.SetPiece(null);
				if (base.hasAuthority)
				{
					piecePlacementCursor.CallRpcSetPlacementsLeftText(0);
				}
				MsgPiecePlaced msgPiecePlaced = new MsgPiecePlaced();
				msgPiecePlaced.PlayerNumber = piecePlacementCursor.networkNumber;
				msgPiecePlaced.PiecePosition = Vector3.zero;
				msgPiecePlaced.PieceScale = Vector3.zero;
				msgPiecePlaced.PieceRotation = Quaternion.identity;
				msgPiecePlaced.PieceID = 0;
				Debug.Log(piecePlacementCursor.name + " Done placement timer");
				NetworkManager.singleton.client.Send(NetMsgTypes.PiecePlaced, msgPiecePlaced);
			}
		}
		if (GameState.GetInstance().UsingHotSeat)
		{
			placementTimer = GameSettings.GetInstance().PlaceTime;
			showingPlacementWarning = false;
			placementEndingMessageInstance.Hide();
			piecesRemoved = false;
		}
		else
		{
			piecesRemoved = true;
		}
	}

	[ClientRpc]
	private void RpcSetCountdown(float time)
	{
		countdownTimer = time;
	}

	[ClientRpc]
	private void RpcSetWinner(GameObject winner)
	{
		if (winner != null)
		{
			base.winner = winner.GetComponent<GamePlayer>();
		}
		else
		{
			base.winner = null;
		}
		winnerSet = true;
	}

	[ClientRpc]
	private void RpcShowPartyBox()
	{
		showPartyBox();
	}

	private IEnumerator DelayRpcForceSelectRandomBlocks()
	{
		yield return new WaitForSeconds(1f);
		CallRpcForceSelectRandomBlocks();
	}

	[ClientRpc]
	private void RpcForceSelectRandomBlocks()
	{
		ForceSelectRandomBlocks();
	}

	private void SignalForcedPieceSpawned(int networkNumber)
	{
		foreach (GamePlayer item in PlayerQueue)
		{
			if (networkNumber == item.networkNumber && item.IsLocalPlayer)
			{
				MsgForcedPieceSpawned msgForcedPieceSpawned = new MsgForcedPieceSpawned();
				msgForcedPieceSpawned.playerNumber = networkNumber;
				NetworkManager.singleton.client.Send(NetMsgTypes.ForcedPieceSpawned, msgForcedPieceSpawned);
				break;
			}
		}
	}

	[ClientRpc]
	private void RpcForceSpawnPiece(int networkNumber, int pickableIndex, bool isTwitchItem)
	{
		PickableBlock pickableByIndex = LobbyManager.instance.CurrentGameController.MetaList.GetPickableByIndex(pickableIndex);
		GameEventManager.SendEvent(new PickBlockEvent(networkNumber, pickableByIndex));
		if (isTwitchItem)
		{
			SyncListInt playersWithTwitchItem = TwitchChatController.instance.twitchChatClientState.playersWithTwitchItem;
			if (!playersWithTwitchItem.Contains(networkNumber))
			{
				playersWithTwitchItem.Add(networkNumber);
			}
		}
		SignalForcedPieceSpawned(networkNumber);
	}

	[ClientRpc]
	private void RpcForceSpawnVariantPiece(int networkNumber, int basePickableIndex, int variantIndex, bool isTwitchItem)
	{
		PlaceableMetadataList metaList = LobbyManager.instance.CurrentGameController.MetaList;
		PickableBlock pickableByIndex = metaList.GetPickableByIndex(basePickableIndex);
		if (pickableByIndex != null)
		{
			PickableBlock pickableForVariantIndex = metaList.GetPickableForVariantIndex(pickableByIndex, variantIndex);
			GameEventManager.SendEvent(new PickBlockEvent(networkNumber, pickableForVariantIndex));
			if (isTwitchItem)
			{
				SyncListInt playersWithTwitchItem = TwitchChatController.instance.twitchChatClientState.playersWithTwitchItem;
				if (!playersWithTwitchItem.Contains(networkNumber))
				{
					playersWithTwitchItem.Add(networkNumber);
				}
			}
		}
		else
		{
			Debug.LogError("Error force-spawning variant piece for player " + networkNumber);
		}
		SignalForcedPieceSpawned(networkNumber);
	}

	public void OnRunTimerLimitReached()
	{
		if (base.hasAuthority)
		{
			CallRpcRunTimerHit();
		}
	}

	[ClientRpc]
	public void RpcRunTimerHit()
	{
		GamePlayer[] array = UnityEngine.Object.FindObjectsOfType<GamePlayer>();
		foreach (GamePlayer gamePlayer in array)
		{
			if (gamePlayer != null && gamePlayer.CharacterInstance != null)
			{
				gamePlayer.CharacterInstance.OnRunTimerHit();
			}
		}
	}

	protected override void CleanUpSceneForLoad()
	{
		base.CleanUpSceneForLoad();
		CheckNullAndDestroy(graphScoreBoardInstance);
		CheckNullAndDestroy(readyMessageInstance);
		CheckNullAndDestroy(suddenDeathMessageInstance);
		CheckNullAndDestroy(placementEndingMessageInstance);
		CheckNullAndDestroy(winMessageInstance);
		CheckNullAndDestroy(digitalClockInstance);
		CheckNullAndDestroy(lastTurnInstance);
		CheckNullAndDestroy(scorecardMessageInstance);
		CheckNullAndDestroy(suicideNoteInstance);
		CheckNullAndDestroy(partyBoxInstance);
	}

	public void CheckAgonyTimeLimit()
	{
		Modifiers instance = Modifiers.GetInstance();
		GameSettings instance2 = GameSettings.GetInstance();
		bool flag = runStarted && ((instance2.RunTimerLimit == 0 && runTimer.TimeLeft == 0f) || runTimer.TimeLeft > (float)instance.agonyTimeLimit);
		if (base.hasAuthority && flag && instance.agonyTimeLimit > 0)
		{
			int num = 0;
			int num2 = 0;
			foreach (GamePlayer item in PlayerQueue)
			{
				if (item != null && item.CharacterInstance != null)
				{
					if (item.CharacterInstance.Success)
					{
						num++;
					}
					else if ((!item.CharacterInstance.Dead && !item.CharacterInstance.Dying) || item.lives > 0)
					{
						num2++;
					}
					else if ((item.CharacterInstance.IsDeadAndSettled && item.CharacterInstance.agonyTimer == 0f) || item.CharacterInstance.IsDeadAndDiedInPit)
					{
						num++;
					}
				}
			}
			if (num2 == 0 && num != 0 && num != PlayerQueue.Count && !AgonoyTimerLimitTriggered)
			{
				AgonyTimeLimitTimer = instance.agonyTimeLimitInvisible;
				AgonoyTimerLimitTriggered = true;
			}
		}
		if (AgonoyTimerLimitTriggered && !AgonyFinalCountDownStarted)
		{
			AgonyTimeLimitTimer -= Time.deltaTime;
			if (AgonyTimeLimitTimer < 0f)
			{
				AgonyFinalCountDownStarted = true;
				Debug.LogWarning("Starting Dead Hop Run Limit");
				runTimer.OnStartRun(instance.agonyTimeLimit, alwaysShowClock: true);
				CallRpcTriggerAgonyRunTimer(instance.agonyTimeLimit);
			}
		}
	}

	[ClientRpc]
	private void RpcTriggerAgonyRunTimer(float time)
	{
		if (!base.hasAuthority)
		{
			runTimer.OnStartRun(time, alwaysShowClock: true);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcSetupPlacementCursors(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetupPlacementCursors called on server.");
		}
		else
		{
			((VersusControl)obj).RpcSetupPlacementCursors(reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcShowScorecardMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowScorecardMessage called on server.");
		}
		else
		{
			((VersusControl)obj).RpcShowScorecardMessage(reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcIncrementCharacterSuccess(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcIncrementCharacterSuccess called on server.");
		}
		else
		{
			((VersusControl)obj).RpcIncrementCharacterSuccess((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcIncrementCharacterWins(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcIncrementCharacterWins called on server.");
		}
		else
		{
			((VersusControl)obj).RpcIncrementCharacterWins((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcBackToBasicAchievement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBackToBasicAchievement called on server.");
		}
		else
		{
			((VersusControl)obj).RpcBackToBasicAchievement((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcShowLastTurnMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowLastTurnMessage called on server.");
		}
		else
		{
			((VersusControl)obj).RpcShowLastTurnMessage((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcHideLastTurnMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHideLastTurnMessage called on server.");
		}
		else
		{
			((VersusControl)obj).RpcHideLastTurnMessage();
		}
	}

	protected static void InvokeRpcRpcShowScoreboard(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowScoreboard called on server.");
		}
		else
		{
			((VersusControl)obj).RpcShowScoreboard(reader.ReadSingle(), reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcShowPlacementWarning(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowPlacementWarning called on server.");
		}
		else
		{
			((VersusControl)obj).RpcShowPlacementWarning(reader.ReadSingle(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSendPlacementTimerDone(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSendPlacementTimerDone called on server.");
		}
		else
		{
			((VersusControl)obj).RpcSendPlacementTimerDone();
		}
	}

	protected static void InvokeRpcRpcRemoveUnplacedObjects(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveUnplacedObjects called on server.");
		}
		else
		{
			((VersusControl)obj).RpcRemoveUnplacedObjects();
		}
	}

	protected static void InvokeRpcRpcSetCountdown(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCountdown called on server.");
		}
		else
		{
			((VersusControl)obj).RpcSetCountdown(reader.ReadSingle());
		}
	}

	protected static void InvokeRpcRpcSetWinner(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetWinner called on server.");
		}
		else
		{
			((VersusControl)obj).RpcSetWinner(reader.ReadGameObject());
		}
	}

	protected static void InvokeRpcRpcShowPartyBox(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowPartyBox called on server.");
		}
		else
		{
			((VersusControl)obj).RpcShowPartyBox();
		}
	}

	protected static void InvokeRpcRpcForceSelectRandomBlocks(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcForceSelectRandomBlocks called on server.");
		}
		else
		{
			((VersusControl)obj).RpcForceSelectRandomBlocks();
		}
	}

	protected static void InvokeRpcRpcForceSpawnPiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcForceSpawnPiece called on server.");
		}
		else
		{
			((VersusControl)obj).RpcForceSpawnPiece((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcForceSpawnVariantPiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcForceSpawnVariantPiece called on server.");
		}
		else
		{
			((VersusControl)obj).RpcForceSpawnVariantPiece((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcRunTimerHit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRunTimerHit called on server.");
		}
		else
		{
			((VersusControl)obj).RpcRunTimerHit();
		}
	}

	protected static void InvokeRpcRpcTriggerAgonyRunTimer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTriggerAgonyRunTimer called on server.");
		}
		else
		{
			((VersusControl)obj).RpcTriggerAgonyRunTimer(reader.ReadSingle());
		}
	}

	public void CallRpcSetupPlacementCursors(bool waitForForcedPieces)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetupPlacementCursors called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetupPlacementCursors);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(waitForForcedPieces);
		SendRPCInternal(networkWriter, 0, "RpcSetupPlacementCursors");
	}

	public void CallRpcShowScorecardMessage(bool allWin, bool noWin, bool anyPoints, bool coinPoints, bool racePoints)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowScorecardMessage called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowScorecardMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(allWin);
		networkWriter.Write(noWin);
		networkWriter.Write(anyPoints);
		networkWriter.Write(coinPoints);
		networkWriter.Write(racePoints);
		SendRPCInternal(networkWriter, 0, "RpcShowScorecardMessage");
	}

	public void CallRpcIncrementCharacterSuccess(int characterSpriteNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcIncrementCharacterSuccess called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcIncrementCharacterSuccess);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)characterSpriteNumber);
		SendRPCInternal(networkWriter, 0, "RpcIncrementCharacterSuccess");
	}

	public void CallRpcIncrementCharacterWins(int characterSpriteNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcIncrementCharacterWins called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcIncrementCharacterWins);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)characterSpriteNumber);
		SendRPCInternal(networkWriter, 0, "RpcIncrementCharacterWins");
	}

	public void CallRpcBackToBasicAchievement(int networkNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcBackToBasicAchievement called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcBackToBasicAchievement);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		SendRPCInternal(networkWriter, 0, "RpcBackToBasicAchievement");
	}

	public void CallRpcShowLastTurnMessage(int numberRemaining)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowLastTurnMessage called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowLastTurnMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)numberRemaining);
		SendRPCInternal(networkWriter, 0, "RpcShowLastTurnMessage");
	}

	public void CallRpcHideLastTurnMessage()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcHideLastTurnMessage called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcHideLastTurnMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcHideLastTurnMessage");
	}

	public void CallRpcShowScoreboard(float ShowScoreTime, bool checkStreaks, bool clearPoints)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowScoreboard called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowScoreboard);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(ShowScoreTime);
		networkWriter.Write(checkStreaks);
		networkWriter.Write(clearPoints);
		SendRPCInternal(networkWriter, 0, "RpcShowScoreboard");
	}

	public void CallRpcShowPlacementWarning(float time, bool showText)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowPlacementWarning called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowPlacementWarning);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(time);
		networkWriter.Write(showText);
		SendRPCInternal(networkWriter, 0, "RpcShowPlacementWarning");
	}

	public void CallRpcSendPlacementTimerDone()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSendPlacementTimerDone called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSendPlacementTimerDone);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcSendPlacementTimerDone");
	}

	public void CallRpcRemoveUnplacedObjects()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveUnplacedObjects called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveUnplacedObjects);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcRemoveUnplacedObjects");
	}

	public void CallRpcSetCountdown(float time)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetCountdown called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetCountdown);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(time);
		SendRPCInternal(networkWriter, 0, "RpcSetCountdown");
	}

	public void CallRpcSetWinner(GameObject winner)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetWinner called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetWinner);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(winner);
		SendRPCInternal(networkWriter, 0, "RpcSetWinner");
	}

	public void CallRpcShowPartyBox()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowPartyBox called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowPartyBox);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcShowPartyBox");
	}

	public void CallRpcForceSelectRandomBlocks()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcForceSelectRandomBlocks called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcForceSelectRandomBlocks);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcForceSelectRandomBlocks");
	}

	public void CallRpcForceSpawnPiece(int networkNumber, int pickableIndex, bool isTwitchItem)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcForceSpawnPiece called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcForceSpawnPiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)pickableIndex);
		networkWriter.Write(isTwitchItem);
		SendRPCInternal(networkWriter, 0, "RpcForceSpawnPiece");
	}

	public void CallRpcForceSpawnVariantPiece(int networkNumber, int basePickableIndex, int variantIndex, bool isTwitchItem)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcForceSpawnVariantPiece called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcForceSpawnVariantPiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)networkNumber);
		networkWriter.WritePackedUInt32((uint)basePickableIndex);
		networkWriter.WritePackedUInt32((uint)variantIndex);
		networkWriter.Write(isTwitchItem);
		SendRPCInternal(networkWriter, 0, "RpcForceSpawnVariantPiece");
	}

	public void CallRpcRunTimerHit()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRunTimerHit called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRunTimerHit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcRunTimerHit");
	}

	public void CallRpcTriggerAgonyRunTimer(float time)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcTriggerAgonyRunTimer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcTriggerAgonyRunTimer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(time);
		SendRPCInternal(networkWriter, 0, "RpcTriggerAgonyRunTimer");
	}

	static VersusControl()
	{
		kRpcRpcSetupPlacementCursors = 1518060336;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcSetupPlacementCursors, InvokeRpcRpcSetupPlacementCursors);
		kRpcRpcShowScorecardMessage = -2096648857;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcShowScorecardMessage, InvokeRpcRpcShowScorecardMessage);
		kRpcRpcIncrementCharacterSuccess = -594807292;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcIncrementCharacterSuccess, InvokeRpcRpcIncrementCharacterSuccess);
		kRpcRpcIncrementCharacterWins = -908615242;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcIncrementCharacterWins, InvokeRpcRpcIncrementCharacterWins);
		kRpcRpcBackToBasicAchievement = -142140920;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcBackToBasicAchievement, InvokeRpcRpcBackToBasicAchievement);
		kRpcRpcShowLastTurnMessage = 1332500402;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcShowLastTurnMessage, InvokeRpcRpcShowLastTurnMessage);
		kRpcRpcHideLastTurnMessage = -367140147;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcHideLastTurnMessage, InvokeRpcRpcHideLastTurnMessage);
		kRpcRpcShowScoreboard = -1829094026;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcShowScoreboard, InvokeRpcRpcShowScoreboard);
		kRpcRpcShowPlacementWarning = -1939150503;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcShowPlacementWarning, InvokeRpcRpcShowPlacementWarning);
		kRpcRpcSendPlacementTimerDone = 923876015;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcSendPlacementTimerDone, InvokeRpcRpcSendPlacementTimerDone);
		kRpcRpcRemoveUnplacedObjects = -235783083;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcRemoveUnplacedObjects, InvokeRpcRpcRemoveUnplacedObjects);
		kRpcRpcSetCountdown = 1536682036;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcSetCountdown, InvokeRpcRpcSetCountdown);
		kRpcRpcSetWinner = -1912117988;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcSetWinner, InvokeRpcRpcSetWinner);
		kRpcRpcShowPartyBox = -1017607321;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcShowPartyBox, InvokeRpcRpcShowPartyBox);
		kRpcRpcForceSelectRandomBlocks = -1542822869;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcForceSelectRandomBlocks, InvokeRpcRpcForceSelectRandomBlocks);
		kRpcRpcForceSpawnPiece = 1202231065;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcForceSpawnPiece, InvokeRpcRpcForceSpawnPiece);
		kRpcRpcForceSpawnVariantPiece = -1877339426;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcForceSpawnVariantPiece, InvokeRpcRpcForceSpawnVariantPiece);
		kRpcRpcRunTimerHit = 638089428;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcRunTimerHit, InvokeRpcRpcRunTimerHit);
		kRpcRpcTriggerAgonyRunTimer = -1570413413;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VersusControl), kRpcRpcTriggerAgonyRunTimer, InvokeRpcRpcTriggerAgonyRunTimer);
		NetworkCRC.RegisterBehaviour("VersusControl", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(waitingForCharacters);
			writer.Write(skipTurnMessage);
			writer.Write(RandomStartPositionString);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(waitingForCharacters);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(skipTurnMessage);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(RandomStartPositionString);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			waitingForCharacters = reader.ReadBoolean();
			skipTurnMessage = reader.ReadBoolean();
			RandomStartPositionString = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 8) != 0)
		{
			waitingForCharacters = reader.ReadBoolean();
		}
		if ((num & 0x10) != 0)
		{
			skipTurnMessage = reader.ReadBoolean();
		}
		if ((num & 0x20) != 0)
		{
			RandomStartPositionString = reader.ReadString();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
