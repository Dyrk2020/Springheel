using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChallengeScoreboard : BaseScoreboard, InputReceiver
{
	public struct ChallengePlayer
	{
		public Character.Animals Animal;

		public bool Alive;

		public int Coins;

		public float EndTime;

		public bool Showing;

		public Sprite LiveSprite;

		public Sprite DeadSprite;

		public string GSID;

		public bool Disconnected;
	}

	public class ChallengeTimeData
	{
		public float time;

		public List<string> playerNames;

		public List<string> playerIds;

		public List<GSData> platformIds;

		public static ChallengeTimeData CreateFromGSDataRecord(GSData record)
		{
			return new ChallengeTimeData
			{
				time = (record.GetFloat("time") ?? 0f),
				playerNames = record.GetStringList("playerNames"),
				playerIds = record.GetStringList("playerIds"),
				platformIds = record.GetGSDataList("platformIds")
			};
		}

		public static List<ChallengeTimeData> CreateListFromGSDataRecordList(List<GSData> recordList)
		{
			List<ChallengeTimeData> list = new List<ChallengeTimeData>(recordList.Count);
			foreach (GSData record in recordList)
			{
				ChallengeTimeData item = CreateFromGSDataRecord(record);
				list.Add(item);
			}
			return list;
		}

		private static List<GSData> CreateGSDataListFromJson(string json)
		{
			try
			{
				List<GSData> list = new List<GSData>();
				foreach (Dictionary<string, object> item in GSJson.From(json) as List<object>)
				{
					list.Add(new GSData(item));
				}
				return list;
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while converting JSON to GS data list: " + ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}

		public static List<ChallengeTimeData> CreateListFromJson(string json)
		{
			return CreateListFromGSDataRecordList(CreateGSDataListFromJson(json));
		}

		public static string GetJsonFromList(List<ChallengeTimeData> list)
		{
			List<GSData> list2 = new List<GSData>();
			foreach (ChallengeTimeData item in list)
			{
				GSData gSData = new GSData();
				gSData.BaseData.Add("time", item.time);
				List<object> value = item.playerIds.Cast<object>().ToList();
				gSData.BaseData.Add("playerIds", value);
				List<object> value2 = item.playerNames.Cast<object>().ToList();
				gSData.BaseData.Add("playerNames", value2);
				List<object> value3 = item.platformIds.Cast<object>().ToList();
				gSData.BaseData.Add("platformIds", value3);
				list2.Add(gSData);
			}
			return GSJson.To(list2);
		}
	}

	public class LeaderboardMetadata
	{
		public bool newPersonalBestNoCoins;

		public bool newPersonalBestAllCoins;

		public float personalBestNoCoins;

		public float personalBestAllCoins;

		public bool newWorldRecordNoCoins;

		public bool newWorldRecordAllCoins;

		public int scoreIndexNoCoins = -1;

		public int scoreIndexAllCoins = -1;

		public string ToJsonString()
		{
			return GSJson.To(new Dictionary<string, object>
			{
				["newPersonalBestNoCoins"] = newPersonalBestNoCoins,
				["newPersonalBestAllCoins"] = newPersonalBestAllCoins,
				["personalBestNoCoins"] = personalBestNoCoins,
				["personalBestAllCoins"] = personalBestAllCoins,
				["newWorldRecordNoCoins"] = newWorldRecordNoCoins,
				["newWorldRecordAllCoins"] = newWorldRecordAllCoins,
				["scoreIndexNoCoins"] = scoreIndexNoCoins,
				["scoreIndexAllCoins"] = scoreIndexAllCoins
			});
		}

		public static LeaderboardMetadata FromJsonString(string json)
		{
			Dictionary<string, object> dictionary = GSJson.From(json) as Dictionary<string, object>;
			return new LeaderboardMetadata
			{
				newPersonalBestNoCoins = (bool)dictionary["newPersonalBestNoCoins"],
				newPersonalBestAllCoins = (bool)dictionary["newPersonalBestAllCoins"],
				newWorldRecordNoCoins = (bool)dictionary["newWorldRecordNoCoins"],
				newWorldRecordAllCoins = (bool)dictionary["newWorldRecordAllCoins"],
				personalBestNoCoins = Convert.ToSingle(dictionary["personalBestNoCoins"]),
				personalBestAllCoins = Convert.ToSingle(dictionary["personalBestAllCoins"]),
				scoreIndexNoCoins = Convert.ToInt32(dictionary["scoreIndexNoCoins"]),
				scoreIndexAllCoins = Convert.ToInt32(dictionary["scoreIndexAllCoins"])
			};
		}
	}

	protected enum NewRecordType
	{
		NONE,
		PERSONALBESTBOTH,
		PERSONALBESTALLCOIN,
		PERSONALBESTFASTEST,
		WORLDRECORDBOTH,
		WORLDRECORDALLCOIN,
		WORLDRECORDFASTEST
	}

	public Sprite[] DeathSprites = new Sprite[Enum.GetValues(typeof(Character.Animals)).Length];

	[Header("Animation Times")]
	public float TimerSpeedMultiplier = 10f;

	public float MaxTimerTime = 5f;

	public float MinTimerTime = 1f;

	public float TimePerCoin = 0.1f;

	public float MaxCoinTime = 2f;

	[Header("Scoreboard Pieces")]
	public Image[] CharacterSprites = new Image[4];

	public Image[] CharacterSpritesBG = new Image[4];

	public Text LevelTitle;

	public Text LevelCode;

	public Text NewRecordText;

	public Text AverageText;

	public UGCNameTag authorNameTag;

	protected NewRecordType TriggerNewRecordAnimation;

	public UiElementAnimation NewRecordAnimator;

	public Text ResultClock;

	public Text BestTimeText;

	public Text BestTimeWithCoinsText;

	public Color ResultNormalColor;

	public Color ResultFailureColor;

	public Text CoinCounter;

	public Text CoinMultiple;

	public Image CoinCounterFill;

	public Image CoinBackDrop;

	public float CoinCounterFillSpeed = 1f;

	public Animator CoinExplosion;

	public Text[] FastestLeaderboard = new Text[6];

	public Text[] CoinLeaderboard = new Text[6];

	public Color SelfHighlightColor;

	public Color OtherPeopleColor;

	public Color NewRecordColor;

	public Color NewPersonalBestColor;

	public RetryButton Retry;

	public TreehouseButton Treehouse;

	public VoteButton UpVote;

	public VoteButton DownVote;

	public Transform VoteContainer;

	public Transform ReportContainer;

	public FavouriteButton Favourite;

	public FlagButton Flagged;

	public CopyCodeButton CopyCode;

	public Transform leaderboardContainer;

	protected CanvasGroup leaderboardCointainerCanvasGroup;

	public SpriteRenderer leaderboardLoadSpinner;

	public Transform fastestTimesAllCoinsContainer;

	public HighscoreDisplayEntry[] fastestTimeEntries;

	public HighscoreDisplayEntry[] fastestTimeAllCoinsEntries;

	public string EmptyPlayerString;

	public Text ratingText;

	public Transform mainButtonsContainer;

	public Transform reportButtonsContainer;

	public Transform resultBoxContainer;

	public Transform reportBoxContainer;

	public ScoreboardReportDialog scoreboardReportDialog;

	public Text connectionFailedText;

	public Text genericErrorText;

	public Image AverageLine;

	public Transform Top;

	public Transform Bottom;

	public float AverageLineFillSpeed = 1f;

	public AnimationCurve EaseCurve;

	public Text PlayerTimesText;

	public GameObject OfflineMessage;

	[Header("Prefabs")]
	public PickCursor CursorPrefab;

	public Camera UICamera;

	[Space]
	public Bounds CursorBounds;

	public Transform CursorSpawn;

	public Transform CursorSpawn2;

	[HideInInspector]
	public float TotalRoundTime;

	[HideInInspector]
	public float AverageRoundTime;

	[HideInInspector]
	public float TotalCoins;

	private ChallengeControl challengeController;

	private ChallengePlayer[] players = new ChallengePlayer[4];

	private List<PickCursor> cursors = new List<PickCursor>();

	private bool skip;

	private bool showingOnHost;

	private float bestTime;

	private float bestCoinTime;

	private bool waitingForHostData;

	private int userRating;

	private bool showingConnectionFailed;

	private bool showingGenericError;

	private float averageLineStartFillAmount;

	protected bool Paused;

	public UnityEngine.Object userInfoPopupPrefab;

	private UserInfoPopup userInfoPopup;

	protected override void Start()
	{
		base.Start();
		Controller.AddGlobalReceiver(this);
		reportBoxContainer.gameObject.SetActive(value: false);
		reportButtonsContainer.gameObject.SetActive(value: false);
		connectionFailedText.gameObject.SetActive(value: false);
		genericErrorText.gameObject.SetActive(value: false);
		leaderboardCointainerCanvasGroup = leaderboardContainer.GetComponent<CanvasGroup>();
		averageLineStartFillAmount = AverageLine.fillAmount;
	}

	public ChallengePlayer GetPlayer(int number)
	{
		return players[number - 1];
	}

	protected override void Update()
	{
		base.Update();
		foreach (PickCursor cursor in cursors)
		{
			cursor.SetBounds(CursorBounds);
		}
		PickableButton.SetMaskingLayerState(1, !challengeController.IsBookShown);
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<SoftPauseEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
	}

	public override void Hide(bool afterTally = false, bool allLocal = true)
	{
		base.Hide(afterTally, allLocal);
		foreach (PickCursor cursor in cursors)
		{
			cursor.Disable();
		}
		waitingForHostData = false;
		if (challengeController != null && challengeController.hasAuthority)
		{
			challengeController.CallRpcHideScoreboard();
		}
		removeDisconnectedPlayers();
		if (userInfoPopup != null)
		{
			UnityEngine.Object.Destroy(userInfoPopup);
			userInfoPopup = null;
		}
		NewRecordText.GetComponent<Localize>().Term = "Empty";
	}

	private void setupScoreboard()
	{
		if (LevelTitle != null)
		{
			LevelTitle.text = ScriptLocalization.Ultimate_Chicken_Horse__SingleLine;
		}
		if (LevelCode != null)
		{
			LevelCode.text = "";
		}
		authorNameTag.gameObject.SetActive(value: false);
	}

	public void SetPlayerCharacter(int playerNum, Character.Animals character)
	{
		if (playerNum >= 1 && playerNum <= 4)
		{
			players[playerNum - 1].Animal = character;
			if (character != Character.Animals.NONE)
			{
				players[playerNum - 1].LiveSprite = CharacterSpriteManager.GetInstance().GetCharaterAliveIcon(character);
				players[playerNum - 1].DeadSprite = CharacterSpriteManager.GetInstance().GetCharaterDeadIcon(character);
			}
		}
	}

	public void SetPlayerAlive(int playerNum, bool alive)
	{
		if (playerNum >= 1 && playerNum <= 4)
		{
			players[playerNum - 1].Alive = alive;
		}
	}

	public void SetPlayerCoins(int playerNum, int coins)
	{
		if (playerNum >= 1 && playerNum <= 4)
		{
			players[playerNum - 1].Coins = Mathf.Max(coins, 0);
		}
	}

	public void SetPlayerEndTime(int playerNum, float time)
	{
		if (playerNum >= 1 && playerNum <= 4)
		{
			players[playerNum - 1].EndTime = Mathf.Max(time, 0f);
		}
	}

	public void SetPlayerGSID(int playerNum, string GSID)
	{
		if (playerNum >= 1 && playerNum <= 4)
		{
			players[playerNum - 1].GSID = GSID;
		}
	}

	public void SetPlayerDisconnected(int playerNum, bool disconnected)
	{
		if (playerNum >= 1 && playerNum <= 4)
		{
			players[playerNum - 1].Disconnected = disconnected;
			if (players[playerNum - 1].EndTime <= 0f)
			{
				players[playerNum - 1].Alive = false;
			}
		}
	}

	public void SetLevelInfo(LevelSelectController.PlayedSnapshotInfo info)
	{
		if (!info.snapshotName.NullOrEmpty() && LevelTitle != null)
		{
			LevelTitle.text = info.snapshotName;
		}
		if (!info.snapshotCode.NullOrEmpty() && LevelCode != null)
		{
			LevelCode.text = GameSparksQuery.GetFormattedSnapshotCode(info.snapshotCode);
		}
		else
		{
			LevelCode.text = "";
		}
		Favourite.SetSnapshot(info.snapshotName, info.snapshotCode);
		Flagged.SetSnapshot(info.snapshotName, info.snapshotCode);
		scoreboardReportDialog.SetData(info.snapshotCode);
		if (!info.authorID.NullOrEmpty())
		{
			authorNameTag.gameObject.SetActive(value: true);
			authorNameTag.InitializeAsync(info.authorDisplayName, info.authorPlatformID, info.authorID, info.authorPlatform);
		}
		else
		{
			authorNameTag.gameObject.SetActive(value: false);
		}
	}

	public void SetLevelData(bool hasCode, int levelRating, int userRating, bool favourite, bool flagged)
	{
		this.userRating = userRating;
		VoteContainer.gameObject.SetActive(hasCode);
		ReportContainer.gameObject.SetActive(hasCode);
		UpVote.SetCanVote(hasCode);
		CopyCode.DisableCollidersOnConsole();
		if (hasCode)
		{
			CopyCode.Enable();
		}
		UpVote.SetVote(userRating > 0);
		ratingText.text = levelRating.ToString();
		Favourite.SetFavourite(favourite);
		Flagged.SetFlagged(flagged);
	}

	public void SetBestTimes(float fastestTime, float fastestTimeAllCoins)
	{
		if (fastestTime > 0f)
		{
			bestTime = fastestTime;
			BestTimeText.text = HighscoreDisplayEntry.GetTimeString(fastestTime);
		}
		else
		{
			BestTimeText.text = "--:--.--";
		}
		if (fastestTimeAllCoins > 0f)
		{
			bestCoinTime = fastestTimeAllCoins;
			BestTimeWithCoinsText.text = HighscoreDisplayEntry.GetTimeString(fastestTimeAllCoins);
		}
		else
		{
			BestTimeWithCoinsText.text = "--:--.--";
		}
	}

	public void SetShowingOnHost(bool onHost)
	{
		Retry.SetOnHost(onHost);
		Treehouse.SetOnHost(onHost);
		showingOnHost = onHost;
	}

	public void SetChallengeController(ChallengeControl challengeController)
	{
		this.challengeController = challengeController;
	}

	public void AddCursorForPlayer(GamePlayer gamePlayer)
	{
		if (gamePlayer.IsLocalPlayer)
		{
			PickCursor pickCursor = UnityEngine.Object.Instantiate(CursorPrefab);
			pickCursor.transform.parent = base.transform;
			pickCursor.transform.localPosition = CursorBounds.center;
			pickCursor.SetBounds(CursorBounds);
			pickCursor.UseCamera = UICamera;
			pickCursor.Disable();
			Transform[] componentsInChildren = pickCursor.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = 5;
			}
			pickCursor.SetLayer(5, showingOnHost);
			pickCursor.NetworknetworkNumber = gamePlayer.networkNumber;
			pickCursor.NetworklocalNumber = gamePlayer.localNumber;
			pickCursor.SetSprites(gamePlayer.PickedAnimal);
			pickCursor.ignoreDirectPauseEvents = true;
			gamePlayer.LocalPlayer.UseController.AddReceiver(pickCursor);
			SpriteRenderer[] componentsInChildren2 = pickCursor.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].sortingLayerName = "UI 3";
			}
			cursors.Add(pickCursor);
		}
	}

	public void ResetRound()
	{
		for (int i = 0; i != 4; i++)
		{
			players[i].Alive = true;
			players[i].Coins = 0;
			players[i].EndTime = 0f;
		}
		TotalRoundTime = 0f;
		ResultClock.text = "0:00.00";
		CoinCounter.text = "0";
		skip = false;
		waitingForHostData = false;
	}

	public void ShowNewResult(float roundTime, float sumOfTimes)
	{
		TotalRoundTime = roundTime;
		int num = 0;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].Animal != Character.Animals.NONE)
			{
				num++;
			}
		}
		AverageRoundTime = sumOfTimes / (float)num;
		if (!GameState.GetInstance().currentSnapshotInfo.snapshotCode.NullOrEmpty())
		{
			OfflineMessage.SetActive(value: false);
			if (showingOnHost)
			{
				waitingForHostData = false;
				UploadRoundTime(AverageRoundTime, !RunWasSuccessful());
			}
			else
			{
				waitingForHostData = true;
			}
		}
		else
		{
			NewRecordText.GetComponent<Localize>().Term = "Empty";
			ShowLoadingIndicator(onOff: false);
			OfflineMessage.SetActive(value: true);
		}
		NewRecordAnimator.Reset();
		leaderboardContainer.gameObject.SetActive(value: false);
		leaderboardCointainerCanvasGroup.alpha = 0f;
		AverageText.enabled = false;
		AverageLine.fillAmount = 0f;
		StartCoroutine(showResult());
	}

	public static IEnumerator UploadRoundGhosts(List<byte[]> ghostBytes, UnityAction<List<string>> callback)
	{
		yield break;
	}

	public void UploadRoundTime(float averageTime, bool noUpdate)
	{
		if (GameState.GetInstance().currentSnapshotInfo.snapshotCode.NullOrEmpty())
		{
			return;
		}
		bool allCoins = TotalCoins != 0f && (float)CountCoins() == TotalCoins;
		List<string> playerIds = CollectPlayerIds();
		showingConnectionFailed = false;
		showingGenericError = false;
		if (!GameSparksManager.Instance.Connected)
		{
			connectionFailedText.gameObject.SetActive(value: true);
			genericErrorText.gameObject.SetActive(value: false);
			ShowLoadingIndicator(onOff: false);
			leaderboardContainer.gameObject.SetActive(value: false);
			showingConnectionFailed = true;
			if (!noUpdate)
			{
				ChallengeTimeCache.Instance.PostDeferredChallengeTime(GameState.GetInstance().currentSnapshotInfo.snapshotCode, playerIds, averageTime, allCoins);
			}
		}
		else
		{
			uploadRoundTime(playerIds, averageTime, allCoins, noUpdate);
		}
	}

	private void uploadRoundTime(List<string> playerIds, float averageTime, bool allCoins, bool noUpdate)
	{
		if (!LobbyManager.instance.IsHost)
		{
			return;
		}
		GameState.GetInstance();
		connectionFailedText.gameObject.SetActive(value: false);
		genericErrorText.gameObject.SetActive(value: false);
		ShowLoadingIndicator(onOff: true);
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SubmitChallengeTime(GameState.GetInstance().currentSnapshotInfo.snapshotCode, playerIds, averageTime, allCoins, noUpdate);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (query.HasError)
			{
				Debug.LogError("Error with leaderboard stuff: " + query.Error);
				ShowLoadingIndicator(onOff: false);
				connectionFailedText.gameObject.SetActive(value: false);
				genericErrorText.gameObject.SetActive(value: true);
				leaderboardContainer.gameObject.SetActive(value: false);
				showingGenericError = true;
			}
			else
			{
				List<ChallengeTimeData> list = query.ResultData["recordsNoCoins"] as List<ChallengeTimeData>;
				List<ChallengeTimeData> list2 = query.ResultData["recordsAllCoins"] as List<ChallengeTimeData>;
				LeaderboardMetadata leaderboardMetadata = new LeaderboardMetadata
				{
					personalBestNoCoins = query.GetResultDataFloat("bestNoCoins", 0f),
					personalBestAllCoins = query.GetResultDataFloat("bestAllCoins", 0f),
					newPersonalBestNoCoins = query.GetResultDataBool("newBestNoCoins", defaultValue: false),
					newPersonalBestAllCoins = query.GetResultDataBool("newBestAllCoins", defaultValue: false)
				};
				for (int i = 0; i < list.Count; i++)
				{
					_ = list[i];
					if (ComparePlayerIDs(playerIds, list[i].playerIds))
					{
						leaderboardMetadata.scoreIndexNoCoins = i;
						if (i == 0 && leaderboardMetadata.newPersonalBestNoCoins)
						{
							leaderboardMetadata.newWorldRecordNoCoins = true;
						}
						break;
					}
				}
				for (int j = 0; j < list2.Count; j++)
				{
					_ = list2[j];
					if (ComparePlayerIDs(playerIds, list2[j].playerIds))
					{
						leaderboardMetadata.scoreIndexAllCoins = j;
						if (j == 0 && leaderboardMetadata.newPersonalBestAllCoins)
						{
							leaderboardMetadata.newWorldRecordAllCoins = true;
						}
						break;
					}
				}
				PopulateScoreboard(list, list2, leaderboardMetadata);
				string jsonFromList = ChallengeTimeData.GetJsonFromList(list);
				string jsonFromList2 = ChallengeTimeData.GetJsonFromList(list2);
				string metadataJson = leaderboardMetadata.ToJsonString();
				challengeController.CallRpcPassLeaderboardData(jsonFromList, jsonFromList2, metadataJson);
			}
		});
	}

	private void PopulateScoreboard(List<ChallengeTimeData> noCoinsList, List<ChallengeTimeData> allCoinsList, LeaderboardMetadata metadata)
	{
		ShowLoadingIndicator(onOff: false);
		leaderboardContainer.gameObject.SetActive(value: true);
		fastestTimesAllCoinsContainer.gameObject.SetActive(TotalCoins != 0f);
		PopulateScores(fastestTimeEntries, noCoinsList, metadata.newPersonalBestNoCoins, metadata.scoreIndexNoCoins);
		PopulateScores(fastestTimeAllCoinsEntries, allCoinsList, metadata.newPersonalBestAllCoins, metadata.scoreIndexAllCoins);
		SetBestTimes(metadata.personalBestNoCoins, metadata.personalBestAllCoins);
		SetWorldRecord(noCoinsList, allCoinsList, metadata);
	}

	public void SetWorldRecord(List<ChallengeTimeData> noCoinsList, List<ChallengeTimeData> allCoinsList, LeaderboardMetadata metadata)
	{
		NewRecordText.GetComponent<Localize>().Term = "Empty";
		BestTimeWithCoinsText.color = SelfHighlightColor;
		BestTimeText.color = SelfHighlightColor;
		TriggerNewRecordAnimation = NewRecordType.NONE;
		int num = 0;
		for (int i = 0; i != 4; i++)
		{
			if (players[i].Animal != Character.Animals.NONE)
			{
				num++;
			}
		}
		if (metadata.newPersonalBestNoCoins)
		{
			if (metadata.newPersonalBestAllCoins)
			{
				TriggerNewRecordAnimation = NewRecordType.PERSONALBESTBOTH;
				NewRecordText.GetComponent<Localize>().Term = ((num <= 1) ? "Scoreboard/Challenge/RecordType/PersonalBestBoth" : "Scoreboard/Challenge/RecordType/GroupBestBoth");
				NewRecordText.color = NewPersonalBestColor;
				BestTimeWithCoinsText.color = NewPersonalBestColor;
				BestTimeText.color = NewPersonalBestColor;
				if (metadata.newWorldRecordNoCoins && metadata.newWorldRecordAllCoins)
				{
					TriggerNewRecordAnimation = NewRecordType.WORLDRECORDBOTH;
					NewRecordText.GetComponent<Localize>().Term = "Scoreboard/Challenge/RecordType/WorldRecordBoth";
					NewRecordText.color = NewRecordColor;
					BestTimeWithCoinsText.color = NewRecordColor;
					BestTimeText.color = NewRecordColor;
				}
				else if (metadata.newWorldRecordNoCoins)
				{
					TriggerNewRecordAnimation = NewRecordType.WORLDRECORDFASTEST;
					NewRecordText.GetComponent<Localize>().Term = "Scoreboard/Challenge/RecordType/WorldRecordFastest";
					NewRecordText.color = NewRecordColor;
					BestTimeText.color = NewRecordColor;
				}
				else if (metadata.newWorldRecordAllCoins)
				{
					TriggerNewRecordAnimation = NewRecordType.WORLDRECORDALLCOIN;
					NewRecordText.GetComponent<Localize>().Term = "Scoreboard/Challenge/RecordType/WorldRecordAllCoin";
					NewRecordText.color = NewRecordColor;
					BestTimeWithCoinsText.color = NewRecordColor;
				}
			}
			else
			{
				TriggerNewRecordAnimation = NewRecordType.PERSONALBESTFASTEST;
				NewRecordText.GetComponent<Localize>().Term = ((num <= 1) ? "Scoreboard/Challenge/RecordType/PersonalBestFastest" : "Scoreboard/Challenge/RecordType/GroupBestFastest");
				BestTimeText.color = NewPersonalBestColor;
				if (metadata.newWorldRecordNoCoins)
				{
					TriggerNewRecordAnimation = NewRecordType.WORLDRECORDFASTEST;
					NewRecordText.GetComponent<Localize>().Term = "Scoreboard/Challenge/RecordType/WorldRecordFastest";
					NewRecordText.color = NewRecordColor;
					BestTimeText.color = NewRecordColor;
				}
			}
		}
		else if (metadata.newPersonalBestAllCoins)
		{
			TriggerNewRecordAnimation = NewRecordType.PERSONALBESTALLCOIN;
			NewRecordText.GetComponent<Localize>().Term = ((num <= 1) ? "Scoreboard/Challenge/RecordType/PersonalBestAllCoin" : "Scoreboard/Challenge/RecordType/GroupBestAllCoin");
			NewRecordText.color = NewPersonalBestColor;
			BestTimeWithCoinsText.color = NewPersonalBestColor;
			if (metadata.newWorldRecordAllCoins)
			{
				NewRecordText.GetComponent<Localize>().Term = "Scoreboard/Challenge/RecordType/WorldRecordAllCoin";
				TriggerNewRecordAnimation = NewRecordType.WORLDRECORDALLCOIN;
				NewRecordText.color = NewRecordColor;
				BestTimeWithCoinsText.color = NewRecordColor;
			}
		}
		if (!NewRecordText.text.Equals(" "))
		{
			NewRecordText.enabled = false;
		}
	}

	private bool ComparePlayerIDs(List<string> A, List<string> B)
	{
		if (A.Count == 0 || B.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < A.Count; i++)
		{
			if (A[i].CompareTo(B[i]) != 0)
			{
				return false;
			}
		}
		return true;
	}

	private void populateScoreEntry(HighscoreDisplayEntry entry, int rank, float time, Color color, List<UserInfoPopup.UserInfo> users)
	{
		entry.Initialize(rank, HighscoreDisplayEntry.GetTimeString(time), color, users, shownInComputer: false);
	}

	private void PopulateScores(HighscoreDisplayEntry[] timeEntries, List<ChallengeTimeData> timesList, bool newPersonalBest, int personalScoreIndex)
	{
		for (int i = 0; i < timeEntries.Length; i++)
		{
			Color color = OtherPeopleColor;
			if (timesList.Count > i)
			{
				if (i == personalScoreIndex)
				{
					color = ((!newPersonalBest) ? SelfHighlightColor : ((i != 0) ? NewPersonalBestColor : NewRecordColor));
				}
				ChallengeTimeData challengeTimeData = timesList[i];
				List<UserInfoPopup.UserInfo> userListFromChallengeTimeData = UserInfoPopup.GetUserListFromChallengeTimeData(challengeTimeData);
				populateScoreEntry(timeEntries[i], i + 1, challengeTimeData.time, color, userListFromChallengeTimeData);
			}
			else
			{
				timeEntries[i].Initialize(i + 1, HighscoreDisplayEntry.GetTimeString(0f), color, null, shownInComputer: false);
			}
		}
	}

	public static string JoinPlayerNames(List<string> ids, List<string> names)
	{
		string text = "";
		for (int i = 0; i < names.Count; i++)
		{
			text += names[i];
			int num = 1;
			for (int j = i + 1; j < ids.Count && ids[j] == ids[i]; j++)
			{
				num++;
			}
			if (num > 1)
			{
				text = text + " (" + num + ")";
				i += num - 1;
			}
			if (i + 1 < names.Count)
			{
				text += ", ";
			}
		}
		return text;
	}

	private int CountCoins()
	{
		int num = 0;
		for (int i = 0; i != 4; i++)
		{
			if (players[i].Animal != Character.Animals.NONE && players[i].Alive)
			{
				num += players[i].Coins;
			}
		}
		return num;
	}

	private bool RunWasSuccessful()
	{
		for (int i = 0; i != 4; i++)
		{
			if (players[i].Animal != Character.Animals.NONE && !players[i].Alive)
			{
				return false;
			}
		}
		return true;
	}

	public List<string> CollectPlayerIds()
	{
		if (!challengeController.hasAuthority)
		{
			Debug.LogError("CollectPlayerIds called on client. Only the host stores player IDs.");
			return null;
		}
		List<string> list = new List<string>(4);
		for (int i = 0; i != 4; i++)
		{
			if (players[i].Animal != Character.Animals.NONE)
			{
				list.Add(players[i].GSID);
			}
		}
		list.Sort();
		return list;
	}

	private IEnumerator showResult()
	{
		if (DrawingScore)
		{
			yield break;
		}
		DrawingScore = true;
		int coins = 0;
		bool successful = true;
		int numberOfPlayers = 0;
		for (int i = 0; i != 4; i++)
		{
			players[i].Showing = false;
			CharacterSprites[i].enabled = false;
			CharacterSpritesBG[i].enabled = false;
			if (players[i].Animal == Character.Animals.NONE)
			{
				continue;
			}
			numberOfPlayers++;
			if (players[i].Alive)
			{
				coins += players[i].Coins;
				if (players[i].Coins > 0)
				{
					Player player = PlayerManager.GetInstance().GetPlayer(i + 1);
					SaveFileData saveFileDataForLocalPlayer = StatTracker.Instance.GetSaveFileDataForLocalPlayer(i + 1);
					if (player != null)
					{
						saveFileDataForLocalPlayer?.GetStat<StatCount>("CoinsCollected").Increment(players[i].Coins);
					}
				}
			}
			else
			{
				successful = false;
			}
			if (players[i].EndTime > TotalRoundTime)
			{
				players[i].EndTime = TotalRoundTime;
			}
		}
		bool allCoinsCollected = (float)coins == TotalCoins;
		if (TotalCoins == 0f)
		{
			CoinCounter.text = "";
			CoinCounterFill.enabled = false;
			CoinBackDrop.enabled = false;
			CoinMultiple.enabled = false;
		}
		else
		{
			CoinCounterFill.enabled = true;
			CoinBackDrop.enabled = true;
			CoinMultiple.enabled = true;
			CoinCounter.text = "0";
			CoinCounterFill.fillAmount = 0f;
		}
		ResultClock.text = "0:00.00";
		ResultClock.color = ResultNormalColor;
		AverageText.enabled = numberOfPlayers > 1;
		PlayerTimesText.enabled = numberOfPlayers > 1;
		float finalTime = 0f;
		float timeMult = TimerSpeedMultiplier;
		if (TotalRoundTime / timeMult < MinTimerTime)
		{
			timeMult = TotalRoundTime / MinTimerTime;
		}
		else if (TotalRoundTime / timeMult > MaxTimerTime)
		{
			timeMult = TotalRoundTime / MaxTimerTime;
		}
		int coinsToAdd = 0;
		int coinsCounted = 0;
		float coinTimer = 0f;
		float coinTime = TimePerCoin;
		if ((float)coins * TimePerCoin > MaxCoinTime)
		{
			coinTime = MaxCoinTime / (float)coins;
		}
		do
		{
			yield return null;
		}
		while (!actuallyVisible);
		skip = false;
		AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_ScoreboardTimerTally_Start", base.gameObject);
		bool AverageLineStarted = false;
		while (finalTime < AverageRoundTime || coinsToAdd > 0)
		{
			finalTime += timeMult * Time.unscaledDeltaTime;
			if (finalTime > AverageRoundTime || skip)
			{
				finalTime = AverageRoundTime;
			}
			ResultClock.text = HighscoreDisplayEntry.GetTimeString(finalTime);
			if (successful)
			{
				ResultClock.color = ResultNormalColor;
			}
			else
			{
				ResultClock.color = Color.Lerp(ResultNormalColor, ResultFailureColor, finalTime / AverageRoundTime);
			}
			for (int j = 0; j != 4; j++)
			{
				ChallengePlayer challengePlayer = players[j];
				if (challengePlayer.Animal == Character.Animals.NONE || challengePlayer.Showing || !(challengePlayer.EndTime / TotalRoundTime <= finalTime / AverageRoundTime))
				{
					continue;
				}
				players[j].Showing = true;
				Image image = CharacterSprites[j];
				Image image2 = CharacterSpritesBG[j];
				if (image != null)
				{
					image.enabled = true;
					if (challengePlayer.Alive)
					{
						image.sprite = challengePlayer.LiveSprite;
						image2.enabled = numberOfPlayers > 1;
						StartCoroutine(barFillSmoothly(challengePlayer.EndTime / TotalRoundTime, image2));
						coinsToAdd += challengePlayer.Coins;
					}
					else
					{
						image.sprite = challengePlayer.DeadSprite;
						image2.enabled = false;
					}
				}
			}
			if (numberOfPlayers > 1)
			{
				if (!AverageLineStarted)
				{
					AverageLine.enabled = successful;
					AverageLineStarted = true;
					float y = Mathf.Lerp(Bottom.localPosition.y, Top.localPosition.y, AverageRoundTime / TotalRoundTime);
					AverageLine.transform.localPosition = new Vector3(AverageLine.transform.localPosition.x, y, AverageLine.transform.localPosition.z);
					StartCoroutine(FillAvereageLineSmoothly());
				}
			}
			else
			{
				AverageLine.enabled = false;
			}
			if (skip)
			{
				if (coins != 0)
				{
					CoinCounter.text = coins.ToString();
				}
				StartCoroutine(coinFillSmoothly((float)coinsToAdd / TotalCoins));
				coinsToAdd = 0;
			}
			else if (coinsToAdd > 0)
			{
				coinTimer += Time.unscaledDeltaTime;
				if (coinTimer >= coinTime)
				{
					coinTimer -= coinTime;
					int num = coinsToAdd - 1;
					coinsToAdd = num;
					num = coinsCounted + 1;
					coinsCounted = num;
					AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_Scoreboard_CountCoin", base.gameObject);
					StartCoroutine(coinFillSmoothly((float)coinsCounted / TotalCoins));
					if (CoinCounter != null)
					{
						CoinCounter.text = coinsCounted.ToString();
					}
				}
			}
			yield return null;
		}
		AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_ScoreboardTimerTally_End", base.gameObject);
		if (!successful)
		{
			AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_Failed", base.gameObject);
		}
		if (TriggerNewRecordAnimation != NewRecordType.NONE)
		{
			NewRecordText.enabled = true;
			NewRecordAnimator.Activate();
			switch (TriggerNewRecordAnimation)
			{
			case NewRecordType.NONE:
				AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_Success_NoRecord", base.gameObject);
				break;
			case NewRecordType.PERSONALBESTBOTH:
				AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_PersonalBestBoth", base.gameObject);
				break;
			case NewRecordType.PERSONALBESTALLCOIN:
				AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_PersonalBestAllCoins", base.gameObject);
				break;
			case NewRecordType.PERSONALBESTFASTEST:
				AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_PersonalBestFastest", base.gameObject);
				break;
			case NewRecordType.WORLDRECORDBOTH:
				AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_WorldRecordBoth", base.gameObject);
				break;
			case NewRecordType.WORLDRECORDALLCOIN:
				AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_WorldRecordAllCoins", base.gameObject);
				break;
			case NewRecordType.WORLDRECORDFASTEST:
				AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_WorldRecordFastest", base.gameObject);
				break;
			}
			TriggerNewRecordAnimation = NewRecordType.NONE;
		}
		StartCoroutine(FadeLeaderBoard());
		if (allCoinsCollected)
		{
			Debug.Log("All coins collected!");
		}
		else
		{
			Debug.Log("Collected " + coins + "/" + TotalCoins + " coins.");
		}
		if (successful)
		{
			if (bestTime == 0f || TotalRoundTime < bestTime)
			{
				bestTime = TotalRoundTime;
				BestTimeText.text = HighscoreDisplayEntry.GetTimeString(TotalRoundTime);
			}
			if (allCoinsCollected && (bestCoinTime == 0f || TotalRoundTime < bestCoinTime))
			{
				bestCoinTime = TotalRoundTime;
				BestTimeWithCoinsText.text = HighscoreDisplayEntry.GetTimeString(TotalRoundTime);
			}
		}
		if (GameShowingScore)
		{
			foreach (PickCursor cursor in cursors)
			{
				Vector3 position = ((!showingOnHost || successful || numberOfPlayers != 1) ? Vector3.Lerp(CursorSpawn.position, CursorSpawn2.position, ((float)cursor.networkNumber - 1f) / 3f) : CursorSpawn.position);
				cursor.transform.position = position;
				Debug.Log("Spawning cursor at local position " + cursor.transform.localPosition.ToString());
				if (!Paused)
				{
					cursor.Enable();
				}
			}
		}
		DrawingScore = false;
	}

	private void removeDisconnectedPlayers()
	{
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].Disconnected)
			{
				players[i] = default(ChallengePlayer);
			}
		}
	}

	private IEnumerator FadeLeaderBoard()
	{
		while (leaderboardCointainerCanvasGroup.alpha < 1f)
		{
			leaderboardCointainerCanvasGroup.alpha += Time.unscaledDeltaTime * 2f;
			yield return null;
		}
	}

	private IEnumerator coinFillSmoothly(float targetAmount)
	{
		while (CoinCounterFill.fillAmount < targetAmount && !skip)
		{
			CoinCounterFill.fillAmount = Mathf.MoveTowards(CoinCounterFill.fillAmount, targetAmount, Time.unscaledDeltaTime * CoinCounterFillSpeed);
			yield return null;
		}
		CoinCounterFill.fillAmount = targetAmount;
		if (targetAmount >= 1f)
		{
			CoinExplosion.SetTrigger("Play");
		}
	}

	private IEnumerator barFillSmoothly(float targetAmount, Image TargetImage)
	{
		TargetImage.fillAmount = 0f;
		while (TargetImage.fillAmount < targetAmount)
		{
			TargetImage.fillAmount = Mathf.MoveTowards(TargetImage.fillAmount, targetAmount, Time.unscaledDeltaTime * CoinCounterFillSpeed * EaseCurve.Evaluate((targetAmount - TargetImage.fillAmount) / (targetAmount - 0f)));
			yield return null;
		}
	}

	private IEnumerator FillAvereageLineSmoothly()
	{
		float initialStart = 0f;
		float fullFillAmount = averageLineStartFillAmount;
		AverageLine.fillAmount = 0f;
		yield return new WaitForSeconds(1f);
		while (AverageLine.fillAmount < fullFillAmount)
		{
			AverageLine.fillAmount = Mathf.MoveTowards(AverageLine.fillAmount, fullFillAmount, Time.unscaledDeltaTime * AverageLineFillSpeed * EaseCurve.Evaluate((fullFillAmount - AverageLine.fillAmount) / (fullFillAmount - initialStart)));
			yield return null;
		}
		AverageLine.fillAmount = fullFillAmount;
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (e.PlayerBitMask > 0 && e.Changed && e.Valueb && (e.Key == InputEvent.InputKey.Accept || e.Key == InputEvent.InputKey.Back))
		{
			skip = true;
		}
	}

	public void OnReceiveLeaderboardDataFromHost(string noCoinsListJson, string allCoinsListJson, string metadataJson)
	{
		if (!waitingForHostData)
		{
			return;
		}
		try
		{
			List<ChallengeTimeData> list = ChallengeTimeData.CreateListFromJson(noCoinsListJson);
			List<ChallengeTimeData> list2 = ChallengeTimeData.CreateListFromJson(allCoinsListJson);
			LeaderboardMetadata leaderboardMetadata = LeaderboardMetadata.FromJsonString(metadataJson);
			if (list == null || list2 == null || leaderboardMetadata == null)
			{
				throw new Exception("Some of the leaderboard data parsed to null");
			}
			PopulateScoreboard(list, list2, leaderboardMetadata);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error while parsing leaderboard data from host: " + ex.Message + "\n" + ex.StackTrace);
		}
		waitingForHostData = false;
	}

	public void CastVote(VoteButton button)
	{
		PickableButton.maskAll = true;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			if (!query.HasError)
			{
				int num = (int)query.ResultData["newRating"];
				int num2 = (int)query.ResultData["myVote"];
				if (num2 > 0)
				{
					AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_RateUp", base.gameObject);
				}
				else if (num2 < 0)
				{
					AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_RateDown", base.gameObject);
				}
				button.SetVote(num2 != 0);
				button.OppositeButton.SetVote(vote: false);
				ratingText.text = num.ToString();
				userRating = num2;
			}
			else
			{
				Debug.LogError("Error casting vote: " + query.Error);
			}
		});
		string snapshotCode = GameState.GetInstance().currentSnapshotInfo.snapshotCode;
		if (userRating == button.VoteScore)
		{
			query.CastLevelVote(snapshotCode, 0);
		}
		else
		{
			query.CastLevelVote(snapshotCode, button.VoteScore);
		}
	}

	private void ShowReportDialog(bool onOff)
	{
		reportBoxContainer.gameObject.SetActive(onOff);
		reportButtonsContainer.gameObject.SetActive(onOff);
		mainButtonsContainer.gameObject.SetActive(!onOff);
		resultBoxContainer.gameObject.SetActive(!onOff);
		connectionFailedText.gameObject.SetActive(showingConnectionFailed && !onOff);
		genericErrorText.gameObject.SetActive(showingGenericError && !onOff);
		if (!onOff)
		{
			ShowLoadingIndicator(onOff: false);
		}
	}

	public void OnClickReport()
	{
		ShowReportDialog(onOff: true);
		scoreboardReportDialog.Initialize();
	}

	public void OnClickReportDialogBack()
	{
		ShowReportDialog(onOff: false);
	}

	public void ShowLoadingIndicator(bool onOff)
	{
		leaderboardLoadSpinner.gameObject.SetActive(onOff);
	}

	public void SetFlaggedState(bool flagged)
	{
		Flagged.SetFlagged(flagged);
	}

	public void OnClickChallengeTwitterShare()
	{
		if (TabletLoadedLevelScreen.Instance != null)
		{
			TabletLoadedLevelScreen.Instance.OnClickChallengeTwitterShareButton(this);
		}
	}

	public void OnClickChallengeRedditShare()
	{
		if (TabletLoadedLevelScreen.Instance != null)
		{
			TabletLoadedLevelScreen.Instance.OnClickChallengeRedditShareButton(this);
		}
	}

	public void ShareSnapshotCodeOnTwitter(string snapshotName, string code, string imageURL)
	{
		string text = ((bestTime == 0f && bestCoinTime == 0f) ? (LocalizationManager.GetTranslation("Snapshot/ChallengeShareNoRecord") + code) : ((!(bestTime <= bestCoinTime) && bestCoinTime != 0f) ? (LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecord") + " " + BestTimeWithCoinsText.text + LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecordP2") + code) : (LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecord") + " " + BestTimeText.text + LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecordP2") + code)));
		if (!imageURL.NullOrEmpty())
		{
			int num = imageURL.IndexOf(".jpg");
			string text2 = imageURL;
			if (num >= 0)
			{
				text2 = imageURL.Remove(num, 4);
			}
			text = text + " " + text2;
		}
		OpenURLWrapper.Open("https://twitter.com/intent/tweet?text=" + WWW.EscapeURL(text));
	}

	public void ShareSnapshotCodeOnReddit(string snapshotName, string code, string imageURL)
	{
		string text = ((bestTime == 0f && bestCoinTime == 0f) ? (LocalizationManager.GetTranslation("Snapshot/ChallengeShareNoRecord") + code) : ((!(bestTime <= bestCoinTime) && bestCoinTime != 0f) ? (LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecord") + BestTimeWithCoinsText.text + LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecordP2") + code) : (LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecord") + BestTimeText.text + LocalizationManager.GetTranslation("Snapshot/ChallengeShareWithRecordP2") + code)));
		if (!imageURL.NullOrEmpty())
		{
			int num = imageURL.IndexOf(".jpg");
			string text2 = imageURL;
			if (num >= 0)
			{
				text2 = imageURL.Remove(num, 4);
			}
			text = text + " " + text2;
		}
		QuickSaver.CopyStringToClipboard(text);
		UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
		GameState.GetInstance().StartCoroutine("OpenRedditUrlInASecond");
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		Type type = e.GetType();
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				Paused = true;
				if (GameShowingScore)
				{
					foreach (PickCursor cursor in cursors)
					{
						cursor.Disable();
					}
				}
			}
			else
			{
				Paused = false;
				if (GameShowingScore)
				{
					foreach (PickCursor cursor2 in cursors)
					{
						cursor2.Enable();
					}
				}
			}
		}
		if (!(type == typeof(SoftPauseEvent)))
		{
			return;
		}
		if ((e as SoftPauseEvent).SoftPaused)
		{
			Paused = true;
			if (!GameShowingScore)
			{
				return;
			}
			{
				foreach (PickCursor cursor3 in cursors)
				{
					cursor3.Disable();
				}
				return;
			}
		}
		Paused = false;
		if (!GameShowingScore)
		{
			return;
		}
		foreach (PickCursor cursor4 in cursors)
		{
			cursor4.Enable();
		}
	}

	public void PopupNameOptions(List<UserInfoPopup.UserInfo> users)
	{
		Canvas componentInChildren = GetComponentInChildren<Canvas>();
		if (componentInChildren != null)
		{
			userInfoPopup = componentInChildren.gameObject.AddPrefabAsChild<UserInfoPopup>(userInfoPopupPrefab);
			userInfoPopup.Show(users, null);
		}
		else
		{
			Debug.LogError("PopupNameOptions: No Canvas element found...");
		}
	}
}
