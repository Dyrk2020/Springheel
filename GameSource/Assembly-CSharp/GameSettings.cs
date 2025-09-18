using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UCHServices;
using UnityEngine;
using UnityEngine.Networking.Match;

[Serializable]
public class GameSettings : ScriptableObject
{
	[Serializable]
	public struct animalColors
	{
		public Character.Animals type;

		public Color mainColor;

		public Color secondaryColor;

		public float JetpackHue;

		public float JetpackSat;

		public float JetpackVal;
	}

	[Serializable]
	public class UserAttributes
	{
		public string frozenLobbyCode;
	}

	public static float Music = 1f;

	public static float Sound = 1f;

	public static bool Debug = true;

	private string parsedMatchmakingNumber;

	private string parsedVersionNumberProd;

	private string parsedVersionNumberToShow;

	public string buildStamp;

	[SerializeField]
	private string versionNumber;

	[SerializeField]
	private bool fakeProdInEditor;

	public int UploadLevelVersion;

	public bool UseGameSparksFQAServer;

	[NonSerialized]
	[Header("DebugOptions")]
	public float RadiusModifier = 0.5f;

	public bool DebugOutsideEditor;

	[NonSerialized]
	public bool newDiagonalMapping = true;

	public bool IgnoreSaveFileInEditor;

	public bool SaveDebugXML;

	public bool useSecondarySaveFile;

	public Font onlineBetaMessageFont;

	public bool UseDebugUnlock;

	public UnLockInfo DebugUnlock;

	public bool UseDebugUnlockPosition;

	public int DebugUnlockPosition;

	public string DebugLaunchArgs;

	public bool useEditorMultiplayerFormSave;

	public float safeAreaScaleRatio = 1f;

	public int DebugStutterAmount = 200;

	public bool DebugChallengeGhosts;

	public AnimationCurve SmoothManualCameraMove;

	[Header("Game Rules")]
	public bool WasUsingCustomRules;

	protected GameState.GameMode gameMode;

	public GameState.GameMode DefaultGameMode = GameState.GameMode.PARTY;

	public bool ModeLocked;

	[Header("Rulesets")]
	public GameRulePreset DefaultRuleset;

	public GameRulePreset[] RulePresets;

	public List<GameRulePreset> rulePresetList = new List<GameRulePreset>();

	private int rulesetInd;

	[Header("Game Basics")]
	protected int maxScore;

	public int maxMaxScore = 100;

	public int minMaxScore = 1;

	protected int maxTime;

	public int maxMaxTime = 100;

	public int minMaxTime = 1;

	protected int maxRounds;

	public int maxMaxRounds = 100;

	public int minMaxRounds = 1;

	protected GameLimitType gameLimitType;

	protected Dictionary<PointBlock.pointBlockType, GameRulePreset.PointData> points = new Dictionary<PointBlock.pointBlockType, GameRulePreset.PointData>();

	public float PlacementWarnTime = 5f;

	public float PlaceTime = 30f;

	public bool UsePlaceTimer = true;

	public int MaxPlaceTime = 60;

	public int CreativePiecesPerRound = 1;

	public int MaxCreativePieces = 5;

	public int PartyBoxesPerRound = 1;

	public DoublePartyBox DoublePartyBox = DoublePartyBox.TwoPlayers;

	public int RunTimerLimit;

	public int minRunTimer;

	public int maxRunTimer = 300;

	public RespawnMode respawnMode;

	public int numRespawns = 3;

	public int minRespawns = 1;

	public int maxRespawns = 20;

	public PartyBoxMode partyBoxMode;

	public bool competitiveRandomizer;

	public int numLastRounds = 3;

	public int MaxPlayers = 4;

	public bool LockPartyButton;

	[Header("Player Colors")]
	public Color[] PlayerColors;

	public Color SystemColor = Color.white;

	public Color SystemAlertColor;

	[Header("Character Colors")]
	public animalColors[] characterColors;

	[Header("Ghost Alpha")]
	public float ghostAlpha = 0.45f;

	[Header("Object Highlight Colors")]
	public Color highlightColor;

	[Header("Object Highlight Colors")]
	public Color highlightColor2;

	[Header("Object Highlight Colors")]
	public Color highlightColor3;

	[Header("Object Highlight Colors")]
	public Color highlightColor4;

	public Color negativeColor;

	public Color neutralColor;

	[Header("Character Highlight Colors")]
	public Color CharacterHighlightColor;

	public Color CharacterNegativeColor;

	[Header("Pickup Item Color Pulse")]
	public Color ItemPickupHighlightColor;

	public Color ItemPickupHighlightColor2;

	public float ItemPickupHighlightTime;

	public Color ItemPickupDeHighlightColor;

	[Header("Item Pickup Variables")]
	public float pickupSpeed = 1f;

	public float putbackSpeed = 2f;

	public float inventoryShuffleTiming = 0.5f;

	public float inventoryShuffleExtraDelay = 0.2f;

	public float respawnItemShuffleTime = 0.5f;

	public float respawnItemDelay = 0.5f;

	[Header("End of Puzzle level Text Color")]
	public Color beatColor;

	public Color unbeatColor;

	[Header("Blackhole Settings")]
	public float blackholePull;

	public AnimationCurve blackholeRange;

	public float blackholeBurstScale;

	[Header("Scoring Point Values")]
	public int minPointValue = 10;

	public int maxPointValue = 100;

	public int pointValueIncrement = 10;

	public int ComebackStreak = 2;

	[Header("Tips")]
	public int RoundsTillSprintTip;

	public int minSprints;

	public int RoundsTillRotateTip;

	public int minRotates;

	public int minRetrys = 1;

	public int RoundsTillRetryTip = 3;

	public float hintDisplayTime;

	public float hintMinDisplayTime;

	public int roundsBetweenHints;

	[Header("Inventory")]
	public float animationDelay;

	public float hoverScaledAmount;

	public float hoverScaledSpeed;

	public float hoverHighlightSpeed;

	public float CantChangeBecauseNotHostAlpha = 0.8f;

	public float DisabledObjectAlpha = 0.4f;

	public float DisabledObjectInGameAlpha = 0.2f;

	public Color DiabledXoutColor;

	public Color pickableButtonDefaultHoverColor;

	[Header("PartyBox")]
	public float partyBoxItemScale;

	public float twoPlayerCoinProbability;

	[Header("Misc")]
	public float bombExplodeDistanceDelay = 3f;

	public AnimationCurve teleporterTimeCurve;

	public AnimationCurve MaxFollowSpeedBasedOnZoomModifier;

	public AnimationCurve boostEffectCurve;

	public AnimationCurve boostEffectLeftRightCurve;

	public float boostEffectDuration = 0.5f;

	public float cannonRentryTimeOutPeriod = 0.3f;

	public float cannonAirJumpTimeOut = 0.4f;

	[NonSerialized]
	public bool SplashScreenOnce;

	public Sprite FavStarFilled;

	public Sprite FavStarEmpty;

	public Dictionary<Placeable, GameRulePreset.BlockData> itemFilter = new Dictionary<Placeable, GameRulePreset.BlockData>();

	public string NetworkAddress = "localhost";

	public int NetworkPort;

	public bool StartAsHost = true;

	public bool StartLocal;

	public bool UseUnityRelay;

	public MatchmakingLobby.Visibility lobbyPrivacy;

	[NonSerialized]
	public LobbyTags lobbyTag;

	public AvailableRegion ClosestRegion;

	public AvailableRegion SelectedRegion;

	public string ExternalIp = "UNASSIGNED";

	public string InternalIp = "UNASSIGNED";

	public ServerConnectionData RelayServerConnectionData;

	public int RegionFilterIndex = -1;

	public bool CrossPlatformToggle = true;

	[Header("Twitch Options")]
	public bool enableTwitchVoting;

	public bool showTwitchChat;

	public string twitchChannelName = "";

	public float twitchItemAnimateSpeed = 3f;

	[Header("UI Networking")]
	public float SteamNameHideSpeed = 2f;

	public float NameBoxTime = 1f;

	public float messageComponentFadeSpeed = 1f;

	public OnlinePlayerNames OnlinePlayerNames;

	public int QualityScoreMin = -250;

	public int QualityScoreMax = 250;

	public float MidMatchQuality = 0.5f;

	public int OkMatchScore = 100;

	public int GoodMatchScore = 200;

	public float SkillMatchQualityThreshold = 0.5f;

	public float AFKThreshold = 10f;

	public static string USRelayURI = "https://us1-mm.unet.unity3d.com";

	public static string EURelayURI = "https://eu1-mm.unet.unity3d.com";

	public static string APRelayURI = "https://ap1-mm.unet.unity3d.com";

	[Header("Chat System")]
	public OnlineChatEmotes OnlineChatEmotes;

	public int maxCharactersPerMessage = 140;

	public int maxVisibleMessages = 8;

	public float ChatMessagingFadeTime = 5f;

	public float ChatMessagingFadeSpeed = 1f;

	public int ChatMessageFontSize = 20;

	public float emoteUIDisplayTime = 3f;

	public float emoteUIFadeSpeed = 1f;

	public int MaxEmotesPerMinute = 20;

	[Header("Snapshots")]
	public int LevelFullnessScoreLimit = 500;

	public int LevelThumbnailWidth = 512;

	public int LevelThumbnailHeight = 366;

	public ThumbnailFormat LevelThumbnailFormat = ThumbnailFormat.JPG;

	public float hardCompletionRate = 0.3f;

	public float mediumCompletionRate = 0.7f;

	public int maxLocalSnapshots = 8192;

	[Header("Online AFK Warning")]
	public int AFKAutoKickTime = 30;

	public int CurrentLobbyAFKAutoKickTime = 30;

	public int AFKWarningTime = 10;

	public int AFKLobbyFilterTime = 600;

	[Header("Lobby Health")]
	public int maxFailedHeartbeatChecks = 3;

	[Header("Flags for Special Builds")]
	public bool BitSummitBuild;

	[Header("Force Controller Type Visuals")]
	public bool forcedControllerVisual;

	public MultiControllerUIManager.ControllerType forcedControllerType;

	[Header("Second Display Smooth Camera Controls")]
	public AnimationCurve SecondaryCameraSpeedVsFOV;

	public string frozenLobbyCode;

	public MatchInfo matchInfo;

	protected static GameSettings instance;

	private static bool itemFilterBuilt;

	public string MatchmakingNumber
	{
		get
		{
			if (string.IsNullOrEmpty(parsedMatchmakingNumber))
			{
				string[] array = versionNumber.Split('.');
				parsedMatchmakingNumber = array[0] + "." + array[1];
			}
			if (Debug && DebugOutsideEditor)
			{
				return parsedMatchmakingNumber + "d";
			}
			return parsedMatchmakingNumber;
		}
	}

	public string VersionNumberProd
	{
		get
		{
			if (string.IsNullOrEmpty(parsedVersionNumberProd))
			{
				string[] array = versionNumber.Split('.');
				parsedVersionNumberProd = array[0] + "." + array[1] + "." + array[2];
			}
			return parsedVersionNumberProd;
		}
	}

	public string VersionNumberDev => VersionNumberProd + "d";

	public string VersionNumberToShow
	{
		get
		{
			if (string.IsNullOrEmpty(parsedVersionNumberToShow))
			{
				if (string.IsNullOrEmpty(versionNumber))
				{
					return "0.0.0";
				}
				string[] array = versionNumber.Split('.');
				string text = ((array.Length != 0) ? array[0] : "0");
				string text2 = ((array.Length > 1) ? array[1] : "0");
				string text3 = ((array.Length > 2) ? array[2] : "0");
				string text4 = ((array.Length > 3) ? array[3] : "0");
				parsedVersionNumberToShow = text + "." + text2 + "." + text3 + "." + text4;
			}
			return parsedVersionNumberToShow;
		}
	}

	public string FullVersionNumber => versionNumber;

	public string VersionNumber
	{
		get
		{
			if (Debug && DebugOutsideEditor)
			{
				return VersionNumberDev;
			}
			return VersionNumberProd;
		}
	}

	public GameState.GameMode GameMode
	{
		get
		{
			return gameMode;
		}
		set
		{
			gameMode = value;
			if (Application.isPlaying)
			{
				Matchmaker.Instance.UpdateLobbyRuleData();
			}
		}
	}

	public bool HasDirtyRuleset => rulesetInd == -1;

	public GameRulePreset NextPreset
	{
		get
		{
			int index = (rulesetInd + 1) % rulePresetList.Count;
			return rulePresetList[index];
		}
	}

	public GameRulePreset PrevPreset
	{
		get
		{
			int index = (rulesetInd + rulePresetList.Count - 1) % rulePresetList.Count;
			return rulePresetList[index];
		}
	}

	public int MaxScore
	{
		get
		{
			return maxScore;
		}
		set
		{
			maxScore = value;
			if (maxScore >= maxMaxScore)
			{
				maxScore = maxMaxScore;
			}
			if (maxScore <= minMaxScore)
			{
				maxScore = minMaxScore;
			}
			if (Application.isPlaying)
			{
				Matchmaker.Instance.UpdateLobbyRuleData();
			}
		}
	}

	public int MaxTime
	{
		get
		{
			return maxTime;
		}
		set
		{
			maxTime = value;
			if (maxTime > maxMaxTime)
			{
				maxTime = maxMaxTime;
			}
			if (maxTime < minMaxTime)
			{
				maxTime = minMaxTime;
			}
			if (Application.isPlaying)
			{
				Matchmaker.Instance.UpdateLobbyRuleData();
			}
		}
	}

	public int MaxRounds
	{
		get
		{
			return maxRounds;
		}
		set
		{
			maxRounds = value;
			if (maxRounds > maxMaxRounds)
			{
				maxRounds = maxMaxRounds;
			}
			if (maxRounds < minMaxRounds)
			{
				maxRounds = minMaxRounds;
			}
			if (Application.isPlaying)
			{
				Matchmaker.Instance.UpdateLobbyRuleData();
			}
		}
	}

	public GameLimitType GameLimitType
	{
		get
		{
			return gameLimitType;
		}
		set
		{
			gameLimitType = value;
			if (Application.isPlaying)
			{
				Matchmaker.Instance.UpdateLobbyRuleData();
			}
		}
	}

	public IEnumerable<PointBlock.pointBlockType> PointTypes => points.Keys;

	public int AvailableBlocks => availableBlocks();

	public static bool PlatformCanDisableCrossPlay => false;

	public string CurrentRelayRegionName => SelectedRegion.LocalizedName;

	public string LobbyPrivacyString => lobbyPrivacy switch
	{
		MatchmakingLobby.Visibility.PUBLIC => ScriptLocalization.Network.Public, 
		MatchmakingLobby.Visibility.FRIENDS => ScriptLocalization.Network.FriendsOnly, 
		MatchmakingLobby.Visibility.PRIVATE => ScriptLocalization.Network.InviteOnly, 
		_ => "", 
	};

	public bool HaveNonDefaultRules
	{
		get
		{
			bool num = LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null;
			bool flag = GameMode == GameState.GameMode.CREATIVE || GameMode == GameState.GameMode.PARTY;
			if ((num || flag) && !GetInstance().DefaultRuleset.IsCurrentlyApplied(checkRules: true, checkPoints: false, checkBlocks: false, checkMods: false))
			{
				return true;
			}
			return false;
		}
	}

	public void OnRulesDirty()
	{
		rulesetInd = -1;
	}

	public GameRulePreset GetRulesetByIndex(int idx)
	{
		if (idx >= 0 && idx < rulePresetList.Count)
		{
			return rulePresetList[idx];
		}
		return null;
	}

	public GameRulePreset GetCurrentRuleset()
	{
		if (HasDirtyRuleset)
		{
			return null;
		}
		if (rulesetInd < 0 || rulesetInd >= rulePresetList.Count)
		{
			UnityEngine.Debug.LogError("GetCurrentRuleset could not grab the ruleset with id " + rulesetInd);
			return null;
		}
		return rulePresetList[rulesetInd];
	}

	public int GetRulesetIndex(GameRulePreset preset)
	{
		for (int i = 0; i < rulePresetList.Count; i++)
		{
			if (rulePresetList[i] == preset)
			{
				return i;
			}
		}
		return -1;
	}

	public void PartialLoadPreset(int idx, bool loadRules, bool loadPoints, bool loadBlocks, bool loadModifiers)
	{
		GameRulePreset otherPreset = rulePresetList[idx];
		if (loadRules)
		{
			ReadGameRules(otherPreset);
		}
		if (loadPoints)
		{
			ReadPointSettings(otherPreset);
		}
		if (loadBlocks)
		{
			ReadBlockSettings(otherPreset);
		}
		if (loadModifiers)
		{
			ReadModifiers(otherPreset);
		}
	}

	public GameRulePreset ToPreset(int idx)
	{
		rulesetInd = idx;
		GameRulePreset gameRulePreset = rulePresetList[idx];
		ReadAllSettings(gameRulePreset);
		return gameRulePreset;
	}

	public GameRulePreset ToNextPreset()
	{
		return ToPreset((rulesetInd + 1) % rulePresetList.Count);
	}

	public GameRulePreset ToPrevPreset()
	{
		return ToPreset((rulesetInd + rulePresetList.Count - 1) % rulePresetList.Count);
	}

	public GameRulePreset ToDefaultPreset()
	{
		return ToPreset(0);
	}

	public GameRulePreset ToLastPreset()
	{
		return ToPreset(rulePresetList.Count - 1);
	}

	public void AddRulePreset(GameRulePreset ruleset, bool applyNow = true)
	{
		rulePresetList.Add(ruleset);
		if (applyNow)
		{
			ReadAllSettings(ruleset);
			rulesetInd = rulePresetList.Count() - 1;
		}
	}

	public void ApplyTemporaryRuleset(GameRulePreset ruleset, bool loadRules, bool loadPoints, bool loadBlocks, bool loadModifiers)
	{
		if (ruleset.IsPremade)
		{
			UnityEngine.Debug.LogError("Tried to apply a premade ruleset as temporary");
			return;
		}
		if (loadRules)
		{
			ReadGameRules(ruleset);
		}
		if (loadPoints)
		{
			ReadPointSettings(ruleset);
		}
		if (loadBlocks)
		{
			ReadBlockSettings(ruleset);
		}
		if (loadModifiers)
		{
			ReadModifiers(ruleset);
		}
		rulesetInd = -1;
	}

	public void RemoveRulePreset(GameRulePreset ruleset)
	{
		if (ruleset != DefaultRuleset && !ruleset.IsPremade)
		{
			GameRulePreset currentRuleset = GetCurrentRuleset();
			bool num = ruleset == currentRuleset;
			rulePresetList.Remove(ruleset);
			UnityEngine.Object.Destroy(ruleset);
			if (num)
			{
				rulesetInd = -1;
			}
			else
			{
				if (!(currentRuleset != null))
				{
					return;
				}
				for (int i = 0; i < rulePresetList.Count; i++)
				{
					if (rulePresetList[i] == currentRuleset)
					{
						rulesetInd = i;
					}
				}
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Tried to remove premade rule preset.");
		}
	}

	public void ResetToPremadeRulesets()
	{
		instance.rulePresetList = new List<GameRulePreset>(instance.RulePresets);
		ToDefaultPreset();
	}

	public static string ConvertLobbyTag(LobbyTags tag)
	{
		return tag switch
		{
			LobbyTags.Fun => ScriptLocalization.Network_Tag.Fun, 
			LobbyTags.Competitive => ScriptLocalization.Network_Tag.Competitive, 
			LobbyTags.Beginner => ScriptLocalization.Network_Tag.Beginner, 
			LobbyTags.CustomLevels => ScriptLocalization.Network_Tag.CustomLevels, 
			_ => null, 
		};
	}

	public static GameSettings GetInstance()
	{
		if (instance == null)
		{
			UnityEngine.Debug.Log("Loading GameWideSettings HERE");
			instance = (GameSettings)Resources.Load("GameWideSettings");
			instance.rulePresetList = new List<GameRulePreset>(instance.RulePresets);
			instance.rulesetInd = 0;
			instance.ReadGameRules(instance.DefaultRuleset);
			instance.ReadPointSettings(instance.DefaultRuleset);
			instance.ReadModifiers(instance.DefaultRuleset);
			PickableButton.gameSettings = instance;
			EnsureItemFilterBuilt();
		}
		return instance;
	}

	private static void buildItemFilter()
	{
		Placeable[] array = null;
		instance.WasUsingCustomRules = false;
		GameObject[] allBlockPrefabs = PlaceableMetadataList.Instance.allBlockPrefabs;
		array = new Placeable[allBlockPrefabs.Length];
		for (int i = 0; i < allBlockPrefabs.Length; i++)
		{
			Placeable placeable = (array[i] = allBlockPrefabs[i].GetComponent<Placeable>());
			if (placeable.FilterOverride.Length == 0)
			{
				if (!instance.itemFilter.ContainsKey(placeable))
				{
					instance.itemFilter.Add(placeable, new GameRulePreset.BlockData(placeable));
				}
				continue;
			}
			for (int j = 0; j != placeable.FilterOverride.Length; j++)
			{
				Placeable placeable2 = placeable.FilterOverride[j];
				if (placeable2 != null && !instance.itemFilter.ContainsKey(placeable2))
				{
					instance.itemFilter.Add(placeable2, new GameRulePreset.BlockData(placeable2));
				}
			}
		}
		itemFilterBuilt = true;
	}

	public bool PointTypeEnabled(PointBlock.pointBlockType type)
	{
		if (points.ContainsKey(type))
		{
			return points[type].Enabled;
		}
		UnityEngine.Debug.LogWarning("Cannot check point type: " + type.ToString() + ". Type does not exist in dictionary. If this is not one of the unused types, verify that the points are being set up properly.");
		return false;
	}

	public void SetPointTypeEnabled(PointBlock.pointBlockType type, bool enabled)
	{
		if (!points.ContainsKey(type))
		{
			UnityEngine.Debug.LogWarning("Cannot enable point type: " + type.ToString() + ". Type does not exist in dictionary. If this is not one of the unused types, verify that the points are being set up properly.");
			return;
		}
		GameRulePreset.PointData value = points[type];
		value.Enabled = enabled;
		points[type] = value;
	}

	public bool AlwaysAwardPointType(PointBlock.pointBlockType type)
	{
		if (points.ContainsKey(type))
		{
			return points[type].AlwaysAward;
		}
		UnityEngine.Debug.LogWarning("Cannot check point type: " + type.ToString() + ". Type does not exist in dictionary. If this is not one of the unused types, verify that the points are being set up properly.");
		return false;
	}

	public void SetAlwaysAwardPointType(PointBlock.pointBlockType type, bool alwaysAward)
	{
		if (!points.ContainsKey(type))
		{
			UnityEngine.Debug.LogWarning("Cannot modify point type: " + type.ToString() + ". Type does not exist in dictionary. If this is not one of the unused types, verify that the points are being set up properly.");
			return;
		}
		GameRulePreset.PointData value = points[type];
		value.AlwaysAward = alwaysAward;
		points[type] = value;
	}

	public int PointTypeValue(PointBlock.pointBlockType type)
	{
		if (points.ContainsKey(type))
		{
			return points[type].Value;
		}
		UnityEngine.Debug.LogWarning("Cannot get value of point type: " + type.ToString() + ". Type does not exist in dictionary. If this is not one of the unused types, verify that the points are being set up properly.");
		return minPointValue;
	}

	public void SetPointTypeValue(PointBlock.pointBlockType type, int value)
	{
		if (!points.ContainsKey(type))
		{
			UnityEngine.Debug.LogWarning("Cannot change value of point type: " + type.ToString() + ". Type does not exist in dictionary. If this is not one of the unused types, verify that the points are being set up properly.");
			return;
		}
		GameRulePreset.PointData value2 = points[type];
		value2.Value = value;
		points[type] = value2;
	}

	public bool AnyWinnerPointsEnabled()
	{
		foreach (KeyValuePair<PointBlock.pointBlockType, GameRulePreset.PointData> point in points)
		{
			if (point.Value.Enabled && point.Value.RequiresWin && point.Value.AlwaysAward)
			{
				return true;
			}
		}
		return false;
	}

	public bool PointTypeRequiresWin(PointBlock.pointBlockType type)
	{
		if (points.ContainsKey(type))
		{
			return points[type].RequiresWin;
		}
		return false;
	}

	public bool PointTypeAllowedForHotseat(PointBlock.pointBlockType type)
	{
		if (points.ContainsKey(type))
		{
			return points[type].HotseatAllowed;
		}
		return false;
	}

	public int GetBlockFrequency(int serializeIndex)
	{
		EnsureItemFilterBuilt();
		if (serializeIndex >= 0 && serializeIndex < PlaceableMetadataList.Instance.allBlockPrefabs.Length)
		{
			Placeable component = PlaceableMetadataList.Instance.allBlockPrefabs[serializeIndex].GetComponent<Placeable>();
			return itemFilter[component].Frequency;
		}
		UnityEngine.Debug.LogError("Could not get block frequency for serialize index " + serializeIndex);
		return 0;
	}

	public void SetBlockFrequency(int serializeIndex, int freq)
	{
		EnsureItemFilterBuilt();
		Placeable component = PlaceableMetadataList.Instance.allBlockPrefabs[serializeIndex].GetComponent<Placeable>();
		GameRulePreset.BlockData value = itemFilter[component];
		value.Frequency = freq;
		itemFilter[component] = value;
	}

	public int availableBlocks()
	{
		int num = 0;
		foreach (Placeable key in itemFilter.Keys)
		{
			if (itemFilter[key].Enabled)
			{
				num++;
			}
		}
		return num;
	}

	public void ReadGameRules(GameRulePreset otherPreset)
	{
		MaxScore = otherPreset.MaxScore;
		MaxRounds = otherPreset.MaxRounds;
		MaxTime = otherPreset.MaxTime;
		PlaceTime = otherPreset.PlaceTime;
		UsePlaceTimer = otherPreset.UsePlaceTimer;
		GameLimitType = otherPreset.GameLimitType;
		DoublePartyBox = otherPreset.DoublePartyBox;
		RunTimerLimit = otherPreset.RunTimerLimit;
		CreativePiecesPerRound = otherPreset.CreativePiecesPerRound;
		respawnMode = otherPreset.respawnMode;
		numRespawns = otherPreset.numRespawns;
		partyBoxMode = otherPreset.partyBoxMode;
		competitiveRandomizer = otherPreset.competitiveRandomizer;
		WasUsingCustomRules = false;
	}

	public void ReadModifiers(GameRulePreset otherPreset)
	{
		otherPreset.mods.WriteToModSettings();
	}

	public void ReadPointSettings(GameRulePreset otherPreset)
	{
		points.Clear();
		foreach (PointBlock.pointBlockType pointType in otherPreset.PointTypes)
		{
			GameRulePreset.PointData value = new GameRulePreset.PointData
			{
				AlwaysAward = otherPreset.AlwaysAwardPointType(pointType),
				Enabled = otherPreset.PointTypeEnabled(pointType),
				HotseatAllowed = otherPreset.PointTypeAllowedForHotseat(pointType),
				RequiresWin = otherPreset.PointTypeRequiresWin(pointType),
				Type = pointType,
				Value = otherPreset.PointTypeValue(pointType)
			};
			points[pointType] = value;
		}
	}

	public static void EnsureItemFilterBuilt()
	{
		if (!itemFilterBuilt)
		{
			if (PlaceableMetadataList.Instance != null)
			{
				buildItemFilter();
			}
			else
			{
				UnityEngine.Debug.LogWarning("Error: Could not build item filter because Placeable Metadata List has not been instantiated yet.");
			}
		}
	}

	public void ReadBlockSettings(GameRulePreset otherPreset)
	{
		EnsureItemFilterBuilt();
		for (int i = 0; i < otherPreset.Blocks.Length; i++)
		{
			Placeable blockPlaceable = otherPreset.Blocks[i].BlockPlaceable;
			GameRulePreset.BlockData value = itemFilter[blockPlaceable];
			value.Frequency = otherPreset.Blocks[i].Frequency;
			itemFilter[blockPlaceable] = value;
		}
	}

	public void SetAllDefaults()
	{
		ReadAllSettings(DefaultRuleset);
	}

	public void ReadAllSettings(GameRulePreset otherPreset)
	{
		ReadGameRules(otherPreset);
		ReadPointSettings(otherPreset);
		ReadBlockSettings(otherPreset);
		ReadModifiers(otherPreset);
	}

	public void ApplySaveFileOverrides()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser != null)
		{
			AFKAutoKickTime = saveFileDataForMainUser.AFKAutoKickTime;
			CurrentLobbyAFKAutoKickTime = AFKAutoKickTime;
			OnlineChatEmotes = saveFileDataForMainUser.OnlineChatEmotes;
			OnlinePlayerNames = saveFileDataForMainUser.OnlinePlayerNames;
			ZoomCamera.LocalOnly = saveFileDataForMainUser.CameraLocalOnly;
		}
	}

	public void OnGameStart()
	{
		Modifiers.GetInstance().ModsApplied = true;
		Time.timeScale = Modifiers.GetInstance().GameSpeed;
	}

	public void OnTreehouseStart()
	{
		Modifiers modifiers = Modifiers.GetInstance();
		modifiers.ModsApplied = modifiers.modsPreview;
		GameRulePreset currentRuleset = GetInstance().GetCurrentRuleset();
		if (currentRuleset != null)
		{
			currentRuleset.mods.WriteToModSettings();
		}
		Time.timeScale = modifiers.GameSpeed;
	}

	public void ResetModsToDefaults()
	{
		new ModSource().WriteToModSettings();
	}

	public static string GetPointLimitValueString(int maxScore)
	{
		return (maxScore / 50).ToString();
	}

	public static string GetLengthLimitValueString(GameLimitType gameLimitType, int maxRounds, int maxTime)
	{
		try
		{
			switch (gameLimitType)
			{
			case GameLimitType.NONE:
				return LocalizationManager.GetTranslation("RuleBook/NoLimit");
			case GameLimitType.ROUNDS:
				return string.Format(LocalizationManager.GetTranslation("RuleBook/Round" + ((maxRounds == 1) ? "Singular" : "Plural")), maxRounds);
			case GameLimitType.TIME:
			{
				int num = maxTime / 60;
				return string.Format(LocalizationManager.GetTranslation("RuleBook/Minute" + ((num == 1) ? "Singular" : "Plural")), num);
			}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Problem getting value string: " + ex);
		}
		return null;
	}

	public static string GetPlacementTimerValueString(bool usePlaceTimer, float placeTime)
	{
		if (usePlaceTimer)
		{
			return placeTime + " " + ScriptLocalization.RuleBook.secondsAbbreviation;
		}
		return ScriptLocalization.RuleBook.Off;
	}

	public static string GetRunTimerValueString(int runTimerLimit)
	{
		if (runTimerLimit > 0)
		{
			return runTimerLimit + " " + ScriptLocalization.RuleBook.secondsAbbreviation;
		}
		return ScriptLocalization.RuleBook.Off;
	}

	public static string GetDoublePartyBoxValueString(DoublePartyBox doublePartyBox)
	{
		return doublePartyBox switch
		{
			DoublePartyBox.Off => ScriptLocalization.RuleBook.Off, 
			DoublePartyBox.TwoPlayers => ScriptLocalization.RuleBook.TwoPlayers, 
			DoublePartyBox.Always => ScriptLocalization.RuleBook.Always, 
			_ => null, 
		};
	}

	public static string GetRespawnModeValueString(RespawnMode respawnMode, int numRespawns)
	{
		return respawnMode switch
		{
			RespawnMode.Off => ScriptLocalization.RuleBook.Off, 
			RespawnMode.LivesPerRound => string.Format("{0} " + ScriptLocalization.RuleBook_Presets.LivesPerRound, numRespawns), 
			RespawnMode.RespawnsPerRound => string.Format("{0} " + ScriptLocalization.RuleBook_Presets.RespawnsPerRound, numRespawns), 
			RespawnMode.RespawnsPerMatch => string.Format("{0} " + ScriptLocalization.RuleBook_Presets.RespawnsPermatch, numRespawns), 
			_ => null, 
		};
	}

	public static string GetPartyBoxModeValueString(PartyBoxMode partyBoxMode)
	{
		return partyBoxMode switch
		{
			PartyBoxMode.Standard => ScriptLocalization.RuleBook_Presets.Standard, 
			PartyBoxMode.Disabled => ScriptLocalization.RuleBook_Presets.Disabled, 
			PartyBoxMode.AutoRandom => ScriptLocalization.RuleBook_Presets.AutoPick, 
			_ => null, 
		};
	}

	public string GetRulesListString(bool inLobby)
	{
		Modifiers.BeginModString();
		int num;
		int num2;
		if (LobbyManager.instance != null)
		{
			num = ((LobbyManager.instance.CurrentGameController != null) ? 1 : 0);
			if (num != 0)
			{
				num2 = ((GameMode == GameState.GameMode.PARTY || GameMode == GameState.GameMode.CREATIVE) ? 1 : 0);
				goto IL_0040;
			}
		}
		else
		{
			num = 0;
		}
		num2 = 1;
		goto IL_0040;
		IL_0040:
		bool flag = (byte)num2 != 0;
		bool flag2 = num == 0 || GameMode == GameState.GameMode.PARTY;
		bool flag3 = num == 0 || GameMode == GameState.GameMode.CREATIVE;
		if (flag && MaxScore != DefaultRuleset.MaxScore)
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook.Points_to_Win, GetPointLimitValueString(MaxScore));
		}
		if (flag && (GameLimitType != DefaultRuleset.GameLimitType || (GameLimitType == GameLimitType.ROUNDS && MaxRounds != DefaultRuleset.MaxRounds)))
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook.Length_Limit, GetLengthLimitValueString(GameLimitType, MaxRounds, MaxTime));
		}
		if (flag && (UsePlaceTimer != DefaultRuleset.UsePlaceTimer || PlaceTime != DefaultRuleset.PlaceTime))
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook.Placement_Timer, GetPlacementTimerValueString(UsePlaceTimer, PlaceTime));
		}
		if (flag && RunTimerLimit != DefaultRuleset.RunTimerLimit)
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook.RunTimerLimit, GetRunTimerValueString(RunTimerLimit));
		}
		if (flag && respawnMode != DefaultRuleset.respawnMode)
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook_Presets.Respawn, GetRespawnModeValueString(respawnMode, numRespawns));
		}
		if (flag2 && partyBoxMode != DefaultRuleset.partyBoxMode)
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook_Presets.PartyBoxMode, GetPartyBoxModeValueString(partyBoxMode));
		}
		if (flag2 && DoublePartyBox != DefaultRuleset.DoublePartyBox)
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook.DoublePartyBoxText, GetDoublePartyBoxValueString(DoublePartyBox));
		}
		if (flag3 && CreativePiecesPerRound != DefaultRuleset.CreativePiecesPerRound)
		{
			Modifiers.AddModString(ScriptLocalization.RuleBook.PiecePerRound, CreativePiecesPerRound.ToString());
		}
		if (flag2 && competitiveRandomizer != DefaultRuleset.competitiveRandomizer)
		{
			Modifiers.AddModString(LocalizationManager.GetTranslation("Modifiers/CompetitiveRandomizerModifiers"));
		}
		string result = null;
		if (Modifiers.anyModsPrinted)
		{
			result = Modifiers.stringBuilder.ToString();
		}
		Modifiers.EndModString();
		return result;
	}
}
