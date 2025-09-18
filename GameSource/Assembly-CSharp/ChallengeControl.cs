using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ChallengeControl : GameControl
{
	public float CameraResetSpeed;

	public float CameraResetZoomSpeed;

	public int OnlineCountdownTime;

	public float HostResyncInterval;

	public float singlePlayerDelayTime = 0.2f;

	public DigitalClock DigitalClockPrefab;

	public ReadyMessage ReadyMessagePrefab;

	public ChallengeScoreboard ScoreboardPrefab;

	public countDownStart CountDownStartPrefab;

	public PlayerStatusDisplay statusDisplayPrefab;

	public CoinStatusDisplay coinDisplayPrefab;

	public SuicideNote RetryMessagePrefab;

	protected DigitalClock digitalClockInstance;

	protected ReadyMessage readyMessageInstance;

	protected ChallengeScoreboard scoreboardInstance;

	protected countDownStart countDownStartInstance;

	protected PlayerStatusDisplay statusDisplayInstance;

	protected CoinStatusDisplay coinDisplayInstance;

	protected SuicideNote retryMessageInstance;

	private bool waitingForCharacters;

	private float runTime;

	private float cumulativeTime;

	private bool runStarted;

	private bool countingDown;

	private bool runFailed;

	private float lastResync;

	public Color FailColor = new Color(0.8667f, 0.549f, 0.549f, 1f);

	private List<GamePlayer> DyingCharacters = new List<GamePlayer>();

	protected bool AttachmentsCheckedOnce;

	protected float PlaceModeDelay;

	private bool everSucceeded;

	protected Dictionary<GamePlayer, float> playerEndTimes = new Dictionary<GamePlayer, float>();

	protected Dictionary<GamePlayer, float> playerDieTimes = new Dictionary<GamePlayer, float>();

	private static int kRpcRpcShowScoreboard;

	private static int kRpcRpcHideScoreboard;

	private static int kRpcRpcTriggerRetry;

	private static int kRpcRpcPassLeaderboardData;

	private static int kRpcRpcTriggerStartCountdown;

	private static int kRpcRpcSetSlotStatus;

	private static int kRpcRpcResyncRunTime;

	public bool IsBookShown
	{
		get
		{
			if (invBookInstance != null)
			{
				return invBookInstance.Visible;
			}
			return false;
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<GamePlayerRemovedEvent>(this, adding);
	}

	protected override void SetupStart(GameState.GameMode mode)
	{
		base.SetupStart(mode);
		Debug.Log("Setting up Challenge Mode");
		List<Canvas> list = new List<Canvas>();
		invBookInstance = UnityEngine.Object.Instantiate(InventoryBookPrefab);
		invBookInstance.transform.parent = UICamera.transform;
		invBookInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
		invBookInstance.UiCamera = UICamera;
		invBookInstance.Hide();
		countDownStartInstance = UnityEngine.Object.Instantiate(CountDownStartPrefab);
		countDownStartInstance.transform.SetParent(UICamera.transform);
		countDownStartInstance.transform.localPosition = Vector3.zero;
		list.AddRange(countDownStartInstance.GetComponentsInChildren<Canvas>());
		scoreboardInstance = UnityEngine.Object.Instantiate(ScoreboardPrefab);
		scoreboardInstance.transform.SetParent(UICamera.transform);
		scoreboardInstance.transform.localPosition = new Vector3(0f, -0.5f, 0f);
		scoreboardInstance.SetBestTimes(0f, 0f);
		scoreboardInstance.TotalCoins = UnityEngine.Object.FindObjectsOfType<Coin>().Length;
		scoreboardInstance.UICamera = UICamera;
		scoreboardInstance.SetShowingOnHost(base.hasAuthority);
		scoreboardInstance.SetChallengeController(this);
		list.AddRange(scoreboardInstance.GetComponentsInChildren<Canvas>());
		statusDisplayInstance = UnityEngine.Object.Instantiate(statusDisplayPrefab, UICamera.transform.position, Quaternion.identity);
		statusDisplayInstance.transform.SetParent(UICamera.transform);
		statusDisplayInstance.transform.Translate(0f, 0f, 1f);
		statusDisplayInstance.IconCanvas.worldCamera = UICamera;
		statusDisplayInstance.HideAllSlots();
		statusDisplayInstance.SetSlotCount(PlayerQueue.Count);
		statusDisplayInstance.SetAlpha(0f);
		list.AddRange(statusDisplayInstance.GetComponentsInChildren<Canvas>());
		coinDisplayInstance = UnityEngine.Object.Instantiate(coinDisplayPrefab, UICamera.transform.position, Quaternion.identity);
		coinDisplayInstance.transform.SetParent(UICamera.transform);
		coinDisplayInstance.transform.Translate(0f, 0f, 1f);
		coinDisplayInstance.IconCanvas.worldCamera = UICamera;
		coinDisplayInstance.Reset();
		coinDisplayInstance.SetAlpha(0f);
		list.AddRange(coinDisplayInstance.GetComponentsInChildren<Canvas>());
		retryMessageInstance = UnityEngine.Object.Instantiate(RetryMessagePrefab, UICamera.transform.position, Quaternion.identity);
		retryMessageInstance.transform.Translate(0f, 0f, 1f);
		retryMessageInstance.transform.SetParent(UICamera.transform);
		retryMessageInstance.GetComponentInChildren<Canvas>().worldCamera = UICamera;
		retryMessageInstance.Hide();
		list.AddRange(retryMessageInstance.GetComponentsInChildren<Canvas>());
		if (base.hasAuthority)
		{
			foreach (GamePlayer item in PlayerQueue)
			{
				LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(item.networkNumber);
				if (lobbyPlayer != null)
				{
					scoreboardInstance.SetPlayerGSID(lobbyPlayer.networkNumber, lobbyPlayer.GSID);
				}
			}
		}
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		LevelSelectController.PlayedSnapshotInfo snapshotInfo = GameState.GetInstance().currentSnapshotInfo;
		bool isFavourite = saveFileDataForMainUser.IsFavorite(snapshotInfo.snapshotName, snapshotInfo.snapshotCode);
		int levelRating = 0;
		int userRating = 0;
		bool isFlagged = false;
		UnityAction RatingAndReportCameIn = delegate
		{
			scoreboardInstance.SetLevelInfo(snapshotInfo);
			scoreboardInstance.SetLevelData(!snapshotInfo.snapshotCode.NullOrEmpty(), levelRating, userRating, isFavourite, isFlagged);
		};
		bool gotRating = false;
		bool gotReport = false;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetMyLevelRating(snapshotInfo.snapshotCode);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			gotRating = true;
			if (!query.HasError)
			{
				userRating = (int)query.ResultData["myVote"];
				levelRating = (int)query.ResultData["levelRating"];
			}
			if (gotReport)
			{
				RatingAndReportCameIn();
			}
		});
		GameSparksQuery query2 = GameSparksManager.Instance.CreateQuery();
		query2.GetMyLevelReport(snapshotInfo.snapshotCode);
		GameSparksQuery gameSparksQuery2 = query2;
		gameSparksQuery2.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery2.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			gotReport = true;
			if (!query2.HasError)
			{
				isFlagged = query2.ResultData != null;
			}
			if (gotRating)
			{
				RatingAndReportCameIn();
			}
		});
		int num = 0;
		foreach (GamePlayer item2 in PlayerQueue)
		{
			scoreboardInstance.SetPlayerCharacter(item2.networkNumber, item2.CharacterInstance.CharacterSprite);
			scoreboardInstance.AddCursorForPlayer(item2);
			statusDisplayInstance.SetupSlot(item2.networkNumber, scoreboardInstance.GetPlayer(item2.networkNumber).LiveSprite, scoreboardInstance.GetPlayer(item2.networkNumber).DeadSprite);
			item2.TurnOrder = num;
			num++;
			if (item2.IsLocalPlayer)
			{
				if (item2 == null)
				{
					Debug.LogError("Player queue contains null player");
				}
				else
				{
					invBookInstance.AddPlayer(item2.localNumber, item2.networkNumber, item2.LocalPlayer.UseController, item2.CharacterInstance.CharacterSprite);
				}
			}
		}
		readyMessageInstance = UnityEngine.Object.Instantiate(ReadyMessagePrefab, UICamera.transform.position, Quaternion.identity);
		readyMessageInstance.canvas.worldCamera = UICamera;
		readyMessageInstance.transform.Translate(0f, 0f, 1f);
		readyMessageInstance.transform.parent = UICamera.transform;
		readyMessageInstance.WaitForPlayer = true;
		readyMessageInstance.SetupForChallengeMode();
		readyMessageInstance.Hide();
		list.AddRange(readyMessageInstance.GetComponentsInChildren<Canvas>());
		digitalClockInstance = UnityEngine.Object.Instantiate(DigitalClockPrefab, UICamera.transform.position, Quaternion.identity);
		digitalClockInstance.TimeCanvas.worldCamera = UICamera;
		digitalClockInstance.transform.Translate(0f, 0f, 1f);
		digitalClockInstance.transform.parent = UICamera.transform;
		digitalClockInstance.Reset();
		digitalClockInstance.Hide();
		graphPaper.gameObject.SetActive(value: false);
		list.AddRange(digitalClockInstance.GetComponentsInChildren<Canvas>());
		foreach (Canvas item3 in list)
		{
			item3.sortingLayerName = "Haze";
			item3.planeDistance = 50f;
		}
		NotifySetupStartDone();
	}

	protected override void ToPlaceMode()
	{
		base.ToPlaceMode();
		statusDisplayInstance.HideAllSlots();
		coinDisplayInstance.Reset();
		foreach (GamePlayer item in PlayerQueue)
		{
			if (item.IsLocalPlayer)
			{
				item.CharacterInstance.Disable();
			}
			MainCamera.RemoveTarget(item.CharacterInstance);
			if (base.hasAuthority)
			{
				CallRpcSetSlotStatus(item.networkNumber, StatusSlot.SlotState.ALIVE);
			}
		}
		roundNumber++;
		DyingCharacters.Clear();
		if (base.hasAuthority)
		{
			int matchProgress = Mathf.Clamp(50, 1, 100);
			Matchmaker.Instance.CurrentLobby.SetMatchProgress(matchProgress);
		}
		PlaceModeDelay = 0.3f;
		MainCamera.unitBuffer = true;
		MainCamera.UseDeadZone = false;
	}

	protected override void AfterAFixedUpdate()
	{
		if (!AttachmentsCheckedOnce)
		{
			base.AfterAFixedUpdate();
			AttachmentsCheckedOnce = true;
		}
	}

	protected override void ToPlayMode()
	{
		if (base.Phase != GamePhase.PLAY)
		{
			AkSoundEngine.PostEvent("Plateform_Phase", base.gameObject);
		}
		base.ToPlayMode();
		MainCamera.ForceShowAllPlayer(showAll: true);
		foreach (GamePlayer item in PlayerQueue)
		{
			item.CharacterInstance.Enable();
			item.CharacterInstance.Waiting = true;
			if (Modifiers.GetInstance().PlayerPlayerCollisions)
			{
				LevelLayout.SpawnCharacter(item.CharacterInstance, (float)item.TurnOrder / (float)PlayerQueue.Count);
			}
			else
			{
				LevelLayout.SpawnCharacter(item.CharacterInstance, 0f);
			}
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, item.CharacterInstance.transform.position, 0.5f);
			MainCamera.AddTarget(item.CharacterInstance);
			if (base.hasAuthority)
			{
				CallRpcSetSlotStatus(item.networkNumber, StatusSlot.SlotState.ALIVE);
			}
		}
		MainCamera.ForceShowAllPlayer(showAll: false);
		LevelLayout.RemoveStartAndGoalsFromCameraTargets(MainCamera);
		AkSoundEngine.PostEvent("UI_InGame_Ready", base.gameObject);
		waitingForCharacters = true;
		runStarted = false;
		DyingCharacters.Clear();
		StartCoroutine(waitForCharacters());
		if (Modifiers.GetInstance().AppliedAndNonDefault)
		{
			GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
		}
	}

	protected override void DoStart()
	{
		base.DoStart();
		startDelayTimer += Time.unscaledDeltaTime;
		if (startDelayTimer >= StartDelay)
		{
			AkSoundEngine.PostEvent("UI_InGame_Level_Start_ZoomIn", base.gameObject);
			GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.START));
			if (base.hasAuthority)
			{
				nextPhase = StartPhase;
			}
			else
			{
				nextPhase = GamePhase.WAIT;
			}
			GameControl.LogCurrentModAndRuleInfo();
		}
	}

	protected override void DoPlaceMode()
	{
		base.DoPlaceMode();
		if (base.hasAuthority)
		{
			if (PlaceModeDelay > 0f)
			{
				PlaceModeDelay -= Time.unscaledDeltaTime;
			}
			else
			{
				nextPhase = GamePhase.PLAY;
			}
		}
	}

	protected override void DoPlayMode()
	{
		base.DoPlayMode();
		if (postSetupStart && !runStarted)
		{
			if (!acceptDown)
			{
				if (!countingDown && !readyMessageInstance.Visible)
				{
					readyMessageInstance.Show(GameState.GetInstance().currentSnapshotInfo.snapshotCode.NullOrEmpty());
				}
				foreach (GamePlayer item in PlayerQueue)
				{
					if (!(item.CharacterInstance == null))
					{
						item.CharacterInstance.Waiting = true;
						scoreboardInstance.SetPlayerEndTime(item.networkNumber, 0f);
						LevelLayout.SpawnCharacter(item.CharacterInstance, (float)item.TurnOrder / (float)PlayerQueue.Count);
					}
				}
				retryMessageInstance.Pause();
				runTime = 0f;
				playerEndTimes.Clear();
				playerDieTimes.Clear();
				digitalClockInstance.Reset();
				digitalClockInstance.SetColor(Color.white);
			}
			else if (!paused && !IsBookShown && !waitingForCharacters && !countingDown)
			{
				if (PlayerQueue.Count == 1)
				{
					TriggerSinglePlayerStart();
				}
				else
				{
					LobbyManager.instance.client.Send(NetMsgTypes.PlayerReadyToStart, new MsgPlayerReadyToStart());
				}
			}
		}
		digitalClockInstance.ShowSecondsAsTime(runTime);
		if (!waitingForCharacters)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			foreach (GamePlayer item2 in PlayerQueue)
			{
				if (item2.CharacterInstance == null)
				{
					Debug.LogWarning("Null character found. Make sure DoPlayMode doesn't run after player objects have been destroyed");
					continue;
				}
				if (item2.CharacterInstance.Success)
				{
					num2++;
					if (MainCamera.HasTarget(item2.CharacterInstance))
					{
						MainCamera.RemoveTarget(item2.CharacterInstance);
						if (!MainCamera.AnyPlayersTracked())
						{
							MainCamera.ForceShowAllPlayer(showAll: true);
						}
						scoreboardInstance.SetPlayerEndTime(item2.networkNumber, runTime);
						scoreboardInstance.SetPlayerAlive(item2.networkNumber, alive: true);
						scoreboardInstance.SetPlayerCoins(item2.networkNumber, item2.CharacterInstance.CoinsCollected);
						if (base.hasAuthority)
						{
							CallRpcSetSlotStatus(item2.networkNumber, StatusSlot.SlotState.SUCCESS);
						}
						if (!playerEndTimes.ContainsKey(item2))
						{
							playerEndTimes.Add(item2, runTime);
						}
					}
				}
				else if (item2.CharacterInstance.Dead)
				{
					num++;
					bool flag = !item2.CharacterInstance.isGhost;
					if (Modifiers.GetInstance().PostDeathBehavior == Modifiers.PostDeathBehaviors.Agony)
					{
						if (!item2.CharacterInstance.IsDeadAndSettled && !item2.CharacterInstance.IsDeadAndDiedInPit)
						{
							flag = false;
						}
						if (item2.CharacterInstance.agonyTimer > 0f)
						{
							num--;
						}
					}
					else if (item2.CharacterInstance.isGhost && !item2.CharacterInstance.WantsToRetry)
					{
						num--;
					}
					if (flag)
					{
						if (MainCamera.HasTarget(item2.CharacterInstance) && item2.CharacterInstance.agonyTimer <= 0f)
						{
							MainCamera.RemoveTarget(item2.CharacterInstance);
							if (!MainCamera.AnyPlayersTracked())
							{
								MainCamera.ForceShowAllPlayer(showAll: true);
							}
							scoreboardInstance.SetPlayerEndTime(item2.networkNumber, runTime);
							scoreboardInstance.SetPlayerAlive(item2.networkNumber, alive: false);
							if (!playerDieTimes.ContainsKey(item2))
							{
								playerDieTimes.Add(item2, runTime);
							}
						}
						if (!runFailed)
						{
							digitalClockInstance.FadeToColor(FailColor, 1f);
							runFailed = true;
						}
					}
				}
				else if (item2.CharacterInstance.Dying)
				{
					num3++;
					if (!runFailed)
					{
						digitalClockInstance.FadeToColor(FailColor, 1f);
						runFailed = true;
					}
				}
				if (item2.CharacterInstance.Dying && !DyingCharacters.Contains(item2))
				{
					if (!item2.CharacterInstance.WantsToRetry && base.hasAuthority)
					{
						CallRpcSetSlotStatus(item2.networkNumber, StatusSlot.SlotState.DEAD);
					}
					DyingCharacters.Add(item2);
				}
				if (item2.CharacterInstance.WantsToRetry)
				{
					num4++;
				}
			}
			if (num4 < num && PlayerQueue.Count > 1)
			{
				retryMessageInstance.Show();
			}
			if (runFailed && num == 0 && num3 == 0)
			{
				digitalClockInstance.FadeToColor(Color.white, 0.5f);
				runFailed = false;
			}
			if (runStarted)
			{
				if (num + num2 == PlayerQueue.Count)
				{
					if (danceTimer == 0f)
					{
						if (num2 > 0)
						{
							if (num == 0)
							{
								everSucceeded = true;
							}
							danceTimer = DanceTime;
							{
								foreach (GamePlayer item3 in PlayerQueue)
								{
									if (item3.CharacterInstance != null && item3.CharacterInstance.Success)
									{
										GoalBlock goalBlockByID = LevelLayout.GetGoalBlockByID(item3.CharacterInstance.LastFlagID);
										if (goalBlockByID != null)
										{
											LevelLayout.AddZoomedOutCameraTarget(MainCamera, goalBlockByID.transform);
										}
										else if (LevelLayout.Goal != null)
										{
											LevelLayout.AddZoomedOutCameraTarget(MainCamera, LevelLayout.Goal);
										}
									}
								}
								return;
							}
						}
						danceTimer = 0.1f;
						LevelLayout.AddZoomedOutCameraTarget(MainCamera, LevelLayout.StartPoint);
						return;
					}
					danceTimer -= Time.unscaledDeltaTime;
					if (!(danceTimer < 0f))
					{
						return;
					}
					danceTimer = 0f;
					if (base.hasAuthority)
					{
						foreach (GamePlayer item4 in PlayerQueue)
						{
							if (item4.CharacterInstance != null && item4.CharacterInstance.WantsToRetry)
							{
								nextPhase = GamePhase.PLACE;
								CallRpcTriggerRetry();
								return;
							}
						}
					}
					if (num2 == 0)
					{
						MainCamera.AddTarget(LevelLayout.StartPoint);
					}
					if (base.hasAuthority)
					{
						cumulativeTime = 0f;
						foreach (GamePlayer item5 in PlayerQueue)
						{
							if (playerEndTimes.ContainsKey(item5))
							{
								cumulativeTime += playerEndTimes[item5];
							}
							else if (playerDieTimes.ContainsKey(item5))
							{
								cumulativeTime += playerDieTimes[item5];
							}
						}
						CallRpcShowScoreboard(runTime, cumulativeTime, num == 0);
					}
					GameEventManager.SendEvent(new RoundCompleteEvent(pointsAwarded: true));
				}
				else
				{
					runTime += Time.unscaledDeltaTime;
					if (base.hasAuthority)
					{
						lastResync += Time.unscaledDeltaTime;
						if (lastResync >= HostResyncInterval)
						{
							lastResync = 0f;
							CallRpcResyncRunTime(runTime);
						}
					}
					danceTimer -= Time.unscaledDeltaTime;
					if (danceTimer < 0f)
					{
						danceTimer = 0f;
					}
				}
			}
		}
		if (!base.hasAuthority)
		{
			return;
		}
		for (int i = 0; i != LobbyManager.instance.lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)LobbyManager.instance.lobbySlots[i];
			if (!(lobbyPlayer != null))
			{
				continue;
			}
			if (lobbyPlayer.LocalPlayer != null)
			{
				Character playerCharacter = lobbyPlayer.LocalPlayer.PlayerCharacter;
				if (playerCharacter != null && !playerCharacter.Dying && !playerCharacter.Dead && playerCharacter.HasExceededAFKLimit)
				{
					playerCharacter.KillCharacter("AFK Auto-Kill", deathFreezeOn: false, 0);
				}
			}
			else
			{
				GamePlayer gamePlayer = LobbyManager.instance.PlayerTracker.GetGamePlayer(lobbyPlayer.networkNumber);
				if (gamePlayer != null && !gamePlayer.WasKicked && gamePlayer.CharacterInstance != null && gamePlayer.CharacterInstance.HasExceededAFKLimit)
				{
					LobbyManager.instance.IssueKickMessage(lobbyPlayer.networkNumber, LobbyManager.KickReasons.AFK);
				}
			}
		}
	}

	public override void AfterScoreBoard()
	{
		base.AfterScoreBoard();
		scoreboardInstance.Hide();
	}

	protected override void sendEndAnalytics()
	{
		base.sendEndAnalytics();
		if (base.hasAuthority)
		{
			AnalyticEvent.MatchEndHostEvent(base.MatchGuid, 0, kicks, quits - kicks, Time.timeSinceLevelLoad, roundNumber, everSucceeded);
		}
		AnalyticEvent.MatchEndClientEvent(base.MatchGuid, ZoomCamera.GlobalCameraTime, ZoomCamera.LocalCameraTime);
	}

	private IEnumerator waitForCharacters()
	{
		bool allReady = false;
		while (!allReady)
		{
			allReady = true;
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
				else if (item.CharacterInstance.Success || (item.CharacterInstance.Dead && base.Phase != GamePhase.SUDDENDEATH))
				{
					allReady = false;
					break;
				}
			}
			yield return null;
		}
		scoreboardInstance.ResetRound();
		Debug.Log("Done waiting for characters");
		waitingForCharacters = false;
	}

	public override void ReceiveEvent(InputEvent e)
	{
		base.ReceiveEvent(e);
		if (base.Phase == GamePhase.PLAY && !runStarted && e.PlayerBitMask > 0 && (e.Key == InputEvent.InputKey.Zoom || e.Key == InputEvent.InputKey.LeftTrigger) && e.Changed)
		{
			if (e.Valueb)
			{
				zoomOut();
			}
			else
			{
				zoomIn();
			}
		}
	}

	private void zoomIn()
	{
		LevelLayout.RemoveStartAndGoalsFromCameraTargets(MainCamera);
	}

	private void zoomOut()
	{
		LevelLayout.AddStartAndGoalsToCameraTargets(MainCamera);
	}

	[ClientRpc]
	private void RpcShowScoreboard(float roundTime, float sumOfTimes, bool success)
	{
		scoreboardInstance.ShowNewResult(roundTime, sumOfTimes);
		scoreboardInstance.Show();
		scoreboard = true;
		digitalClockInstance.Hide();
		statusDisplayInstance.FadeToAlpha(0f);
		coinDisplayInstance.FadeToAlpha(0f);
		GameEventManager.SendEvent(new PlayersDoneRunning());
		GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.PLAY));
		if (base.hasAuthority)
		{
			nextPhase = GamePhase.PLACE;
			NotifyChallengeAttempt(success, Mathf.CeilToInt(roundTime));
		}
	}

	[ClientRpc]
	public void RpcHideScoreboard()
	{
		if (!base.hasAuthority)
		{
			nextPhase = GamePhase.PLACE;
			scoreboardInstance.Hide();
			scoreboard = false;
		}
	}

	[ClientRpc]
	private void RpcTriggerRetry()
	{
		nextPhase = GamePhase.PLACE;
		if (!base.hasAuthority)
		{
			GameEventManager.SendEvent(new RoundCompleteEvent(pointsAwarded: true));
		}
		GameEventManager.SendEvent(new PlayersDoneRunning());
		GameEventManager.SendEvent(new ScoreboardEvent(show: false, afterTally: true));
		GameEventManager.SendEvent(new EndPhaseEvent(GamePhase.PLAY));
		if (base.hasAuthority)
		{
			nextPhase = GamePhase.PLACE;
			NotifyChallengeAttempt(success: false, Mathf.CeilToInt(runTime));
		}
	}

	[ClientRpc]
	public void RpcPassLeaderboardData(string noCoinsListJson, string allCoinsListJson, string metadataJson)
	{
		if (!base.hasAuthority)
		{
			scoreboardInstance.OnReceiveLeaderboardDataFromHost(noCoinsListJson, allCoinsListJson, metadataJson);
		}
	}

	[ClientRpc]
	public void RpcTriggerStartCountdown(int countdownTime)
	{
		countingDown = true;
		readyMessageInstance.Hide();
		float num = 0.5f;
		if (!base.hasAuthority)
		{
			num -= LobbyManager.instance.GetAveragePingToServer();
		}
		StartCoroutine(waitForCountdown(countdownTime, num));
	}

	private void TriggerSinglePlayerStart()
	{
		countingDown = true;
		readyMessageInstance.canvasFadeTime = singlePlayerDelayTime;
		readyMessageInstance.canvasGroup.alpha = 1f;
		readyMessageInstance.Hide();
		StartCoroutine(waitForSinglePlayerStart(singlePlayerDelayTime));
	}

	private IEnumerator waitForSinglePlayerStart(float delay)
	{
		zoomIn();
		MainCamera.ZipToTarget(CameraResetSpeed, CameraResetZoomSpeed);
		LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayers().FirstOrDefault();
		if (lobbyPlayer != null)
		{
			Player localPlayer = lobbyPlayer.LocalPlayer;
			if (localPlayer != null)
			{
				if (localPlayer.PlayerCharacter != null)
				{
					float timer = 0f;
					bool jumpReleased = false;
					while (timer < delay)
					{
						if (jumpReleased)
						{
							if (localPlayer.PlayerCharacter.jumpDown)
							{
								break;
							}
						}
						else if (!localPlayer.PlayerCharacter.jump)
						{
							jumpReleased = true;
						}
						timer += Time.unscaledDeltaTime;
						yield return null;
					}
				}
				else
				{
					Debug.LogError("Local player has no character");
				}
			}
			else
			{
				Debug.LogError("Could not find LocalPlayer");
			}
		}
		else
		{
			Debug.LogError("Could not find first player");
		}
		countingDown = false;
		startRun(singlePlayer: true);
	}

	private IEnumerator waitForCountdown(int countdownTime, float buffer)
	{
		float bufferTimer = 0f;
		while (bufferTimer < buffer)
		{
			bufferTimer += Time.unscaledDeltaTime;
			yield return null;
		}
		float lengthOfASecond = 0.5f;
		countDownStartInstance.StartCountDown(countdownTime, countDownStart.TimerMessage.STARTING, lengthOfASecond);
		countDownStartInstance.Show();
		float waitTimer = 0f;
		while (waitTimer < (float)countdownTime * lengthOfASecond)
		{
			waitTimer += Time.unscaledDeltaTime;
			yield return null;
		}
		countDownStartInstance.Hide();
		countingDown = false;
		startRun(singlePlayer: false);
	}

	[ClientRpc]
	public void RpcSetSlotStatus(int playerNetworkNumber, StatusSlot.SlotState newState)
	{
		if (statusDisplayInstance != null)
		{
			statusDisplayInstance.SetSlot(playerNetworkNumber, newState);
		}
	}

	[ClientRpc]
	private void RpcResyncRunTime(float hostRunTime)
	{
		if (!base.hasAuthority)
		{
			float averagePingToServer = LobbyManager.instance.GetAveragePingToServer();
			runTime = hostRunTime + averagePingToServer;
		}
	}

	private void startRun(bool singlePlayer)
	{
		digitalClockInstance.Show();
		statusDisplayInstance.FadeToAlpha(1f);
		readyMessageInstance.Hide();
		foreach (GamePlayer item in PlayerQueue)
		{
			MainCamera.AddTarget(item.CharacterInstance);
			if (item.IsLocalPlayer)
			{
				item.CharacterInstance.Waiting = false;
				item.CharacterInstance.Enable();
				item.CharacterInstance.OnGround = true;
				if (item.CharacterInstance.jump)
				{
					item.CharacterInstance.ForceJump();
				}
			}
		}
		if (!singlePlayer)
		{
			zoomIn();
			MainCamera.ZipToTarget(CameraResetSpeed, CameraResetZoomSpeed);
		}
		GameEventManager.SendEvent(new LevelResetEvent());
		Debug.Log("Resetting Everything");
		foreach (ActiveBlock activeBlock in activeBlocks)
		{
			if (!(activeBlock == null) && !activeBlock.Active)
			{
				activeBlock.Active = true;
			}
		}
		AkSoundEngine.PostEvent("UI_InGame_Go", base.gameObject);
		runStarted = true;
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
				GamePlayer gamePlayer = PlayerQueue.Dequeue();
				if (gamePlayer.networkNumber == gamePlayerRemovedEvent.PlayerNetworkNumber)
				{
					scoreboardInstance.SetPlayerDisconnected(gamePlayer.networkNumber, disconnected: true);
					statusDisplayInstance.SetSlot(gamePlayer.networkNumber, StatusSlot.SlotState.DEAD);
					if (gamePlayer.CharacterInstance != null)
					{
						if (gamePlayer.CharacterInstance.Enabled)
						{
							SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, gamePlayer.CharacterInstance.transform.position);
						}
						MainCamera.RemoveTarget(gamePlayer.CharacterInstance);
						Coin[] componentsInChildren = GetComponentsInChildren<Coin>();
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							componentsInChildren[j].transform.parent = null;
						}
						UnityEngine.Object.Destroy(gamePlayer.CharacterInstance.gameObject);
					}
				}
				else
				{
					PlayerQueue.Enqueue(gamePlayer);
				}
			}
			resetTurnOrders();
		}
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (base.hasAuthority)
			{
				if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PlayerWantsToRetry)
				{
					MsgPlayerWantsToRetry msgPlayerWantsToRetry = networkMessageReceivedEvent.ReadMessage as MsgPlayerWantsToRetry;
					GamePlayer gamePlayer2 = LobbyManager.instance.PlayerTracker.GetGamePlayer(msgPlayerWantsToRetry.networkNumber);
					if (gamePlayer2 != null)
					{
						gamePlayer2.CharacterInstance.CallCmdSetWantsToRetry(value: true);
						CallRpcSetSlotStatus(gamePlayer2.networkNumber, StatusSlot.SlotState.RETRY);
					}
				}
				if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PlayerReadyToStart && !countingDown)
				{
					CallRpcTriggerStartCountdown(OnlineCountdownTime);
				}
			}
		}
		if (type == typeof(PauseEvent) && runStarted && (e as PauseEvent).Paused)
		{
			runTime += UnityEngine.Random.Range(0.02f, 0.05f);
		}
	}

	private void NotifyChallengeAttempt(bool success, int secondsInLevel)
	{
		string snapshotCode = GameState.GetInstance().currentSnapshotInfo.snapshotCode;
		if (!snapshotCode.NullOrEmpty())
		{
			GameSparksManager.Instance.CreateQuery().AddChallengeAttempt(snapshotCode, scoreboardInstance.CollectPlayerIds(), success, secondsInLevel);
		}
	}

	protected override void CleanUpSceneForLoad()
	{
		base.CleanUpSceneForLoad();
		CheckNullAndDestroy(scoreboardInstance);
		CheckNullAndDestroy(readyMessageInstance);
		CheckNullAndDestroy(countDownStartInstance);
		CheckNullAndDestroy(statusDisplayInstance);
		CheckNullAndDestroy(coinDisplayInstance);
		CheckNullAndDestroy(digitalClockInstance);
		CheckNullAndDestroy(retryMessageInstance);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcShowScoreboard(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowScoreboard called on server.");
		}
		else
		{
			((ChallengeControl)obj).RpcShowScoreboard(reader.ReadSingle(), reader.ReadSingle(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcHideScoreboard(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHideScoreboard called on server.");
		}
		else
		{
			((ChallengeControl)obj).RpcHideScoreboard();
		}
	}

	protected static void InvokeRpcRpcTriggerRetry(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTriggerRetry called on server.");
		}
		else
		{
			((ChallengeControl)obj).RpcTriggerRetry();
		}
	}

	protected static void InvokeRpcRpcPassLeaderboardData(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPassLeaderboardData called on server.");
		}
		else
		{
			((ChallengeControl)obj).RpcPassLeaderboardData(reader.ReadString(), reader.ReadString(), reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcTriggerStartCountdown(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTriggerStartCountdown called on server.");
		}
		else
		{
			((ChallengeControl)obj).RpcTriggerStartCountdown((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcSetSlotStatus(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSlotStatus called on server.");
		}
		else
		{
			((ChallengeControl)obj).RpcSetSlotStatus((int)reader.ReadPackedUInt32(), (StatusSlot.SlotState)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcResyncRunTime(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResyncRunTime called on server.");
		}
		else
		{
			((ChallengeControl)obj).RpcResyncRunTime(reader.ReadSingle());
		}
	}

	public void CallRpcShowScoreboard(float roundTime, float sumOfTimes, bool success)
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
		networkWriter.Write(roundTime);
		networkWriter.Write(sumOfTimes);
		networkWriter.Write(success);
		SendRPCInternal(networkWriter, 0, "RpcShowScoreboard");
	}

	public void CallRpcHideScoreboard()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcHideScoreboard called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcHideScoreboard);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcHideScoreboard");
	}

	public void CallRpcTriggerRetry()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcTriggerRetry called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcTriggerRetry);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcTriggerRetry");
	}

	public void CallRpcPassLeaderboardData(string noCoinsListJson, string allCoinsListJson, string metadataJson)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPassLeaderboardData called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPassLeaderboardData);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(noCoinsListJson);
		networkWriter.Write(allCoinsListJson);
		networkWriter.Write(metadataJson);
		SendRPCInternal(networkWriter, 0, "RpcPassLeaderboardData");
	}

	public void CallRpcTriggerStartCountdown(int countdownTime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcTriggerStartCountdown called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcTriggerStartCountdown);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)countdownTime);
		SendRPCInternal(networkWriter, 0, "RpcTriggerStartCountdown");
	}

	public void CallRpcSetSlotStatus(int playerNetworkNumber, StatusSlot.SlotState newState)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetSlotStatus called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetSlotStatus);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)playerNetworkNumber);
		networkWriter.Write((int)newState);
		SendRPCInternal(networkWriter, 0, "RpcSetSlotStatus");
	}

	public void CallRpcResyncRunTime(float hostRunTime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcResyncRunTime called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcResyncRunTime);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(hostRunTime);
		SendRPCInternal(networkWriter, 0, "RpcResyncRunTime");
	}

	static ChallengeControl()
	{
		kRpcRpcShowScoreboard = 1529362753;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ChallengeControl), kRpcRpcShowScoreboard, InvokeRpcRpcShowScoreboard);
		kRpcRpcHideScoreboard = -932386490;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ChallengeControl), kRpcRpcHideScoreboard, InvokeRpcRpcHideScoreboard);
		kRpcRpcTriggerRetry = 728138400;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ChallengeControl), kRpcRpcTriggerRetry, InvokeRpcRpcTriggerRetry);
		kRpcRpcPassLeaderboardData = -505038938;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ChallengeControl), kRpcRpcPassLeaderboardData, InvokeRpcRpcPassLeaderboardData);
		kRpcRpcTriggerStartCountdown = -1934977289;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ChallengeControl), kRpcRpcTriggerStartCountdown, InvokeRpcRpcTriggerStartCountdown);
		kRpcRpcSetSlotStatus = -1620689726;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ChallengeControl), kRpcRpcSetSlotStatus, InvokeRpcRpcSetSlotStatus);
		kRpcRpcResyncRunTime = -604612614;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ChallengeControl), kRpcRpcResyncRunTime, InvokeRpcRpcResyncRunTime);
		NetworkCRC.RegisterBehaviour("ChallengeControl", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
