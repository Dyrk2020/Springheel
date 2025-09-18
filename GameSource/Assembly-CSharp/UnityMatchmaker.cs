using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GameEvent;
using I2.Loc;
using Moserware.Skills;
using Moserware.Skills.TrueSkill;
using Relay;
using UCHServices;
using UnityEngine;
using UnityEngine.Events;

public class UnityMatchmaker : Matchmaker
{
	private class dummy
	{
	}

	protected bool isOwner;

	protected bool inUnityMatch;

	protected bool manualConnection;

	protected bool connectByEndpoint;

	protected bool inPlatformLobbyNoRelay;

	protected bool startingLobby;

	protected bool lastLobbyResult;

	public bool joiningLobby;

	protected AvailableRegion SessionLockedUnityRelayRegion;

	private UISplashScreen FadeOut;

	public PickableNetworkButton refreshSearch;

	private static int checkOnStart;

	public int failedHeartbeatQueries;

	private UniTaskCompletionSource externalIpTask;

	private void OnDestroy()
	{
		if (Matchmaker.instance == this)
		{
			Matchmaker.instance = null;
		}
	}

	protected override void Start()
	{
		if (!(Matchmaker.instance != this))
		{
			failedHeartbeatQueries = 0;
			findExternalIP();
			getStartupArguments();
		}
	}

	protected override void Update()
	{
		if (!(Matchmaker.instance != this))
		{
			if (FadeOut == null)
			{
				FadeOut = LoadingInterstitialSplash.Instance;
			}
			if (base.WaitingForLobby && IsInLobby() && !base.WaitingForSocialLobby)
			{
				base.WaitingForLobby = false;
				StartCoroutine(startGameOnInput());
			}
		}
	}

	private IEnumerator startGameOnInput()
	{
		bool hasPlayer = false;
		Debug.Log("[Net] Waiting for controller assignment");
		base.WaitingForController = true;
		while (!hasPlayer)
		{
			foreach (Player item in PlayerManager.GetInstance())
			{
				if (item != null)
				{
					hasPlayer = true;
					break;
				}
			}
			yield return null;
		}
		base.WaitingForController = false;
		Debug.Log("[Net] Attempting to load Treehouse");
		if (IsInLobby())
		{
			StartGameTreehouse();
		}
		else
		{
			LeaveLobby(ScriptLocalization.Network_XB1.LostConnection);
		}
	}

	public void StartGameTreehouse()
	{
		FadeOut = LoadingInterstitialSplash.Instance;
		if (FadeOut != null)
		{
			LobbyManagerManager.Instance.StartCoroutine(FadeOutToLoad());
			FadeOut.SkipOff();
			FadeOut.FadeIn();
		}
	}

	private IEnumerator FadeOutToLoad()
	{
		while (FadeOut.State != UISplashScreen.STATE.SHOW)
		{
			yield return null;
		}
		switch (GameSettings.GetInstance().GameMode)
		{
		case GameState.GameMode.FREEPLAY:
			AkSoundEngine.PostEvent("Lobby_Freeplay", base.gameObject);
			break;
		case GameState.GameMode.CREATIVE:
			AkSoundEngine.PostEvent("Lobby_Normal", base.gameObject);
			break;
		case GameState.GameMode.PARTY:
			AkSoundEngine.PostEvent("Lobby_PartyMode", base.gameObject);
			break;
		case GameState.GameMode.CHALLENGE:
			AkSoundEngine.PostEvent("Lobby_Challenge", base.gameObject);
			break;
		}
		IEnumerator gentleLoad = SceneManagerWrapper.DoGentleSceneLoad("TreeHouseLobby");
		while (gentleLoad.MoveNext())
		{
			yield return null;
		}
	}

	public override void CreateLobby()
	{
		SessionLockedUnityRelayRegion = GameSettings.GetInstance().SelectedRegion;
	}

	public override void FindLobbies(LobbyListingCallback callback)
	{
		currentLobbyListCallback = callback;
		base.Searching = true;
		LoadingSearchIndicator = true;
	}

	public override bool IsInLobby()
	{
		if (manualConnection)
		{
			return true;
		}
		if (GameSettings.GetInstance().UseUnityRelay)
		{
			return inUnityMatch;
		}
		return inPlatformLobbyNoRelay;
	}

	public override bool IsLobbyOwner()
	{
		return isOwner;
	}

	public override void JoinLobby(string lobbyID, bool useCode, UnityAction<bool> callback = null)
	{
		GameSettings.GetInstance().RelayServerConnectionData = null;
		joiningLobby = true;
	}

	public override void JoinGameManual()
	{
		base.WaitingForLobby = true;
		manualConnection = true;
		CurrentLobby = new LocalLobby();
	}

	public override bool LastLobbyResult()
	{
		return lastLobbyResult;
	}

	public override void LeaveLobby(string reason = null, UserMessageManager.UserMsgPriority priority = UserMessageManager.UserMsgPriority.lo)
	{
		LeaveLobbySubModule(reason, priority);
		if (!string.IsNullOrEmpty(reason) && reason.Equals("QuitGame"))
		{
			TabletQuitScreen.QuitGame();
		}
	}

	private void LeaveLobbySubModule(string reason, UserMessageManager.UserMsgPriority priority)
	{
		joiningLobby = false;
		Matchmaker.CleanUpPlayers();
		if (!reason.NullOrEmpty())
		{
			Debug.Log("[Net] " + reason);
			UserMessageManager.Instance.UserMessage(reason, 3f, priority, tiedToCurrentScene: false);
		}
		if (inPlatformLobbyNoRelay || inUnityMatch)
		{
			if (isOwner)
			{
				CurrentLobby.SetMatchProgress(-100);
				base.Joinability = false;
				StopCoroutine("hostHeartbeat");
			}
			leavePlatformLobby();
			isOwner = false;
			inUnityMatch = false;
		}
		CurrentLobby = null;
		manualConnection = false;
		base.WaitingForLobby = false;
		startingLobby = false;
		lastLobbyResult = false;
		MainMenuControl mainMenuControl = UnityEngine.Object.FindObjectOfType<MainMenuControl>();
		if (mainMenuControl != null)
		{
			mainMenuControl.OnLeaveLobbyCalled();
		}
		GameSettings.GetInstance().NetworkPort = 17778;
		Debug.LogFormat("<color=cyan>Leaving Match: Network Address ({0}), Network Port ({1}), Relay Region ({2})</color>", GameSettings.GetInstance().NetworkAddress, GameSettings.GetInstance().NetworkPort, GameSettings.GetInstance().SelectedRegion.name);
	}

	protected override void leavePlatformLobby()
	{
		inPlatformLobbyNoRelay = false;
	}

	public override bool StartingLobby()
	{
		return startingLobby;
	}

	public override void DropConnection(ushort dropNodeId)
	{
	}

	public override void OnLobbyPrivacyChanged(MatchmakingLobby.Visibility visibility)
	{
	}

	public override bool CanJoinPlatform(string platform)
	{
		return true;
	}

	public override bool CheckHostConnectivity()
	{
		bool result = true;
		if (CurrentLobby == null)
		{
			return false;
		}
		if (!isOwner)
		{
			if (manualConnection)
			{
				return true;
			}
			long lastHeartbeat = CurrentLobby.GetLastHeartbeat();
			uint serverTime = CurrentLobby.GetServerTime();
			if (lastHeartbeat + 10 < serverTime)
			{
				LeaveLobby(ScriptLocalization.Network.Host_not_available_Refreshing_search, UserMessageManager.UserMsgPriority.hi);
				GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.REFRESHSEARCH));
				result = false;
			}
			if (CurrentLobby != null)
			{
				int playerCount = CurrentLobby.GetPlayerCount();
				if (playerCount == 0)
				{
					LeaveLobby(ScriptLocalization.Network.Host_ended_game, UserMessageManager.UserMsgPriority.hi);
					GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.REFRESHSEARCH));
					result = false;
				}
				else if (playerCount >= 4)
				{
					LeaveLobby(ScriptLocalization.Network.matchfull, UserMessageManager.UserMsgPriority.hi);
					GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.REFRESHSEARCH));
					result = false;
				}
			}
			if (CurrentLobby != null)
			{
				string lobbyVersion = CurrentLobby.GetLobbyVersion();
				if (lobbyVersion != GameSettings.GetInstance().MatchmakingNumber)
				{
					Debug.Log("[Net] St:Trying to join: " + lobbyVersion + " Your version is:" + GameSettings.GetInstance().MatchmakingNumber);
					LeaveLobby(ScriptLocalization.Network.Game_Version_does_not_match, UserMessageManager.UserMsgPriority.hi);
					result = false;
				}
			}
			if (CurrentLobby != null)
			{
				int matchProgress = CurrentLobby.GetMatchProgress();
				if (matchProgress != 0)
				{
					if (matchProgress > 0)
					{
						LeaveLobby(ScriptLocalization.Network.Game_In_Progress_No_Join, UserMessageManager.UserMsgPriority.hi);
					}
					else
					{
						LeaveLobby(ScriptLocalization.Network.Host_ended_game, UserMessageManager.UserMsgPriority.hi);
					}
					GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.REFRESHSEARCH));
					result = false;
				}
				if (!CurrentLobby.GetLobbyJoinable())
				{
					Debug.Log("Client tried joining game in progress");
					LeaveLobby(ScriptLocalization.Network.Game_In_Progress_No_Join, UserMessageManager.UserMsgPriority.hi);
				}
			}
			if (CurrentLobby != null)
			{
				bool lobbyDisallowCrossplay = CurrentLobby.GetLobbyDisallowCrossplay();
				if (lobbyDisallowCrossplay && MatchmakingLobby.GetSocialPlatformFromLobbyPlatform(CurrentLobby.GetLobbyPlatform()) != LobbyPlayer.LocalMachinePlatform)
				{
					LeaveLobby(ScriptLocalization.Network.CrossPlatformJoinErrorHostSetting, UserMessageManager.UserMsgPriority.hi);
					GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.REFRESHSEARCH));
					result = false;
				}
				if (!GameSettings.GetInstance().CrossPlatformToggle && !lobbyDisallowCrossplay)
				{
					LeaveLobby(ScriptLocalization.Network.CrossPlatformJoinErrorLocalSetting, UserMessageManager.UserMsgPriority.hi);
					GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.REFRESHSEARCH));
					result = false;
				}
			}
		}
		return result;
	}

	public override void OnLobbyManagerHostingStarted()
	{
		MakeLobbyJoinable();
	}

	private async UniTaskVoid MakeLobbyJoinable()
	{
		_ = GameSettings.GetInstance().UseUnityRelay;
		Debug.Log("Before finding external IP");
		await findExternalIP();
		Debug.Log($"After finding external IP - Lobby is owner {IsLobbyOwner()}");
		if (IsLobbyOwner())
		{
			CurrentLobby.SetLobbyExternalIP(myExternalIP);
			CurrentLobby.SetLobbyInternalIP(myInternalIP);
			base.Joinability = true;
			Debug.Log("Got ip adresses and set joinability to true.");
		}
	}

	public override void UpdateLobbyRuleData()
	{
		if (isOwner)
		{
			GameSettings gameSettings = GameSettings.GetInstance();
			CurrentLobby.SetLobbyLimitType(gameSettings.GameLimitType);
			switch (gameSettings.GameLimitType)
			{
			case GameLimitType.NONE:
				CurrentLobby.SetLobbyLimitAmount(0);
				break;
			case GameLimitType.TIME:
				CurrentLobby.SetLobbyLimitAmount(gameSettings.MaxTime);
				break;
			case GameLimitType.ROUNDS:
				CurrentLobby.SetLobbyLimitAmount(gameSettings.MaxRounds);
				break;
			}
			CurrentLobby.SetLobbyPointLimit(gameSettings.MaxScore);
			CurrentLobby.SetLobbyGameMode(gameSettings.GameMode);
			CurrentLobby.SetLobbyUsingMods(Modifiers.GetInstance() != null && Modifiers.GetInstance().IsNonDefault);
			GameRulePreset currentRuleset = gameSettings.GetCurrentRuleset();
			if (gameSettings.HasDirtyRuleset || (currentRuleset != null && !currentRuleset.IsPremade))
			{
				CurrentLobby.SetLobbyRulePreset("Custom");
			}
			else
			{
				CurrentLobby.SetLobbyRulePreset(currentRuleset.Name);
			}
		}
	}

	protected override double calcMatchQuality(string playerSkillString)
	{
		if (playerSkillString == null || playerSkillString == "")
		{
			return -1.0;
		}
		int result = 0;
		string[] array = playerSkillString.Split(',');
		if (array.Length == 0 || !int.TryParse(array[0], out result))
		{
			return -1.0;
		}
		if (array.Length - 1 < 2 * result)
		{
			return -1.0;
		}
		Team<dummy>[] array2 = new Team<dummy>[result + 1];
		for (int i = 0; i < result; i++)
		{
			if (!Parsing.TryParseDouble(array[2 * i + 1], out var result2) || !Parsing.TryParseDouble(array[2 * i + 2], out var result3))
			{
				return -1.0;
			}
			array2[i] = new Team<dummy>(new dummy(), new Rating(result2, result3));
		}
		double mean = 25.0;
		double standardDeviation = 8.333333333333334;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser != null)
		{
			mean = saveFileDataForMainUser.SkillMean;
			standardDeviation = saveFileDataForMainUser.SkillStdDev;
		}
		array2[result] = new Team<dummy>(new dummy(), new Rating(mean, standardDeviation));
		return new FactorGraphTrueSkillCalculator().CalculateMatchQuality<dummy>(teams: Teams.Concat(array2), gameInfo: GameInfo.DefaultGameInfo);
	}

	protected override void onLobbyCreated(bool success)
	{
		if (!success)
		{
			lastLobbyResult = false;
			return;
		}
		if (CurrentLobby == null)
		{
			Debug.LogError("No lobby object was set up.");
			return;
		}
		lastLobbyResult = true;
		isOwner = true;
		CurrentLobby.SetPlayerCount(0);
		CurrentLobby.AddKickedPlayer("");
		CurrentLobby.SetLobbyVersion(GameSettings.GetInstance().MatchmakingNumber);
		CurrentLobby.SetMatchProgress(0);
		CurrentLobby.SetLobbyPort(GameSettings.GetInstance().NetworkPort);
		base.Joinability = false;
		CurrentLobby.SetUsingUnityRelay(GameSettings.GetInstance().UseUnityRelay);
		UpdateLobbyRuleData();
		CurrentLobby.SetLobbyTag(GameSettings.GetInstance().lobbyTag);
		CurrentLobby.SetLobbyDisallowCrossplay(!GameSettings.GetInstance().CrossPlatformToggle);
		startingLobby = false;
		base.WaitingForLobby = true;
		if (CurrentLobby.GetUsingUnityRelay())
		{
			Debug.Log("Non Xbox Player OnLobbyCreate\n");
			GameSettings.GetInstance().SelectedRegion = SessionLockedUnityRelayRegion;
			CreateMatch();
		}
		else
		{
			manualConnection = true;
			inPlatformLobbyNoRelay = true;
		}
		StartCoroutine("hostHeartbeat");
	}

	protected override void onLobbyJoined(bool success)
	{
		joiningLobby = false;
		if (!success)
		{
			return;
		}
		if (CurrentLobby == null)
		{
			Debug.LogError("No lobby object was set up. Cannot join game");
			return;
		}
		bool usingUnityRelay = CurrentLobby.GetUsingUnityRelay();
		GameSettings.GetInstance().UseUnityRelay = usingUnityRelay;
		manualConnection = !usingUnityRelay;
		base.WaitingForLobby = true;
		if (!CheckHostConnectivity())
		{
			return;
		}
		Debug.Log("[Net] Entered Lobby: Owner:" + isOwner + ", Using Unity Relay:" + GameSettings.GetInstance().UseUnityRelay + ", Running version: " + CurrentLobby.GetLobbyVersion());
		if (!usingUnityRelay)
		{
			if (connectByEndpoint)
			{
				base.WaitingForLobby = true;
				inPlatformLobbyNoRelay = true;
				return;
			}
			int lobbyPort = CurrentLobby.GetLobbyPort();
			string lobbyExternalIP = CurrentLobby.GetLobbyExternalIP();
			string lobbyInternalIP = CurrentLobby.GetLobbyInternalIP();
			tryJoinP2PGame(lobbyExternalIP, lobbyPort, lobbyInternalIP);
		}
		else if (CurrentLobby.GetPlayerCount() == 4)
		{
			Debug.LogError("Session is already full!");
			LeaveLobby(ScriptLocalization.Network.matchfull);
		}
		else
		{
			Debug.Log("Non Xbox Player OnLobbyJoined\n");
			GameSettings.GetInstance().SelectedRegion = CurrentLobby.GetLobbyRegion();
			JoinMatch();
		}
	}

	private void CreateMatch()
	{
		inUnityMatch = true;
		CurrentLobby.SetLobbyRegion(GameSettings.GetInstance().SelectedRegion = SessionLockedUnityRelayRegion);
		Debug.LogFormat("<color=cyan>Creating Match: Network Address ({0}), Network Port ({1}), Relay Region ({2})</color>", GameSettings.GetInstance().NetworkAddress, GameSettings.GetInstance().NetworkPort, GameSettings.GetInstance().SelectedRegion.name);
	}

	private void JoinMatch()
	{
		base.WaitingForLobby = true;
		inUnityMatch = true;
		GameSettings.GetInstance().SelectedRegion = CurrentLobby.GetLobbyRegion();
		GameSettings.GetInstance().NetworkPort = CurrentLobby.GetLobbyPort();
		GameSettings.GetInstance().NetworkAddress = CurrentLobby.GetLobbyExternalIP();
		Debug.LogFormat("<color=cyan>Joining Match: Network Address ({0}), Network Port ({1}), Relay Region ({2})</color>", GameSettings.GetInstance().NetworkAddress, GameSettings.GetInstance().NetworkPort, GameSettings.GetInstance().SelectedRegion.name);
	}

	private async UniTaskVoid tryJoinP2PGame(string address, int port, string lanAddress)
	{
		GameSettings.GetInstance().NetworkPort = port;
		GameSettings.GetInstance().NetworkAddress = address;
		if (myExternalIP.Contains("UNASSIGNED"))
		{
			await findExternalIP();
		}
		if (GameSettings.GetInstance().NetworkAddress.Contains(myExternalIP))
		{
			GameSettings.GetInstance().NetworkAddress = lanAddress;
			Debug.Log("[Net] using Internal Ip instead " + GameSettings.GetInstance().NetworkAddress);
		}
		inPlatformLobbyNoRelay = true;
		base.WaitingForLobby = true;
		Debug.Log("[Net] Starting P2P connection to " + GameSettings.GetInstance().NetworkAddress + ":" + GameSettings.GetInstance().NetworkPort);
	}

	private IEnumerator hostHeartbeat()
	{
		while (isOwner && IsInLobby())
		{
			CurrentLobby.SetLastHeartbeat((int)CurrentLobby.GetServerTime(), delegate(bool success)
			{
				if (this != null)
				{
					if (!success && LobbyManager.instance != null && !LobbyManager.instance.AllLocal && Matchmaker.InTreehouse)
					{
						failedHeartbeatQueries++;
						if (failedHeartbeatQueries >= GameSettings.GetInstance().maxFailedHeartbeatChecks)
						{
							Debug.LogError("Failed to set lobby heartbeat " + failedHeartbeatQueries + " times in a row -- going back to main menu.");
							failedHeartbeatQueries = 0;
							LobbyManagerManager.AbortGameInProgressGracefully(LocalizationManager.GetTranslation("Network/XB1/LostConnection"));
						}
					}
					else
					{
						failedHeartbeatQueries = 0;
					}
				}
				else
				{
					Debug.LogWarning("Ignored heartbeat response -- Matchmaker destroyed");
				}
			});
			yield return new WaitForSeconds(10f);
		}
	}

	protected override void getStartupArguments()
	{
		string[] array = Environment.GetCommandLineArgs();
		string debugLaunchArgs = GameSettings.GetInstance().DebugLaunchArgs;
		if (!debugLaunchArgs.NullOrEmpty())
		{
			array = debugLaunchArgs.Split(' ');
		}
		string text = "";
		if (array != null)
		{
			for (int i = 0; i != array.Length; i++)
			{
				text += array[i];
			}
		}
		Debug.Log("CheckOnStart: " + checkOnStart + ". Args being used: " + text);
		if (checkOnStart <= 0)
		{
			checkOnStart++;
			CheckStartupArguments(array);
		}
	}

	public override void CheckStartupArguments(string[] args)
	{
	}

	protected async UniTask findExternalIP()
	{
		if (externalIpTask != null && !externalIpTask.Task.Status.IsCompleted())
		{
			await externalIpTask.Task;
			return;
		}
		await UniTask.WaitUntil(() => UCHOnlineConnector.Instance != null && UCHOnlineConnector.Service != null && UCHOnlineConnector.Service.IsReady);
		myExternalIP = GameSettings.GetInstance().ExternalIp;
		myInternalIP = GameSettings.GetInstance().InternalIp;
		if (!myExternalIP.Contains("UNASSIGNED"))
		{
			Debug.Log("[Net] Already have IP returning " + myExternalIP);
			return;
		}
		Debug.Log("[Net] Attempting to find IP");
		try
		{
			externalIpTask = new UniTaskCompletionSource();
			float startTimeStamp = Time.unscaledTime;
			Debug.Log("[Net] Requesting external IP");
			myExternalIP = await GetExternalIPServices();
			GameSettings.GetInstance().ExternalIp = myExternalIP;
			myInternalIP = GetInternalIP();
			GameSettings.GetInstance().InternalIp = myInternalIP;
			Debug.LogFormat("<color=cyan>IP Addresses: Internal IP ({0}) External IP ({1})</color>", myInternalIP, myExternalIP.ToString());
			Debug.Log("[Net] Finding IPs took: " + (Time.unscaledTime - startTimeStamp) + " s.");
			externalIpTask.TrySetResult();
		}
		catch (Exception ex)
		{
			Debug.Log("Error while getting external IP : " + ex);
		}
		finally
		{
			externalIpTask = null;
		}
	}

	public async UniTask<string> GetExternalIPServices()
	{
		Debug.Log($"UCHOnlineConnector.Instance != null {UCHOnlineConnector.Instance != null} UCHOnlineConnector.Service != null {UCHOnlineConnector.Service != null} GameSettings.GetInstance().SelectedRegion != null {GameSettings.GetInstance().SelectedRegion != null}");
		Debug.Log("Getting external IP FOR REAL");
		for (int i = 0; i < 3; i++)
		{
			bool success = false;
			try
			{
				GetIpResponse getIpResponse = await UCHOnlineConnector.Service.SendGetIpRequest();
				if (getIpResponse != null)
				{
					success = true;
					return getIpResponse.Ip;
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Error while getting external IP: {arg}");
			}
			if (success)
			{
				break;
			}
			await UniTask.Delay(5000);
		}
		return "UNASSIGNED";
	}

	private static bool ValidateIP(string ipString)
	{
		if (ipString.NullOrEmpty())
		{
			Debug.Log("[Net] Couldn't validate IP: String null or empty.");
			return false;
		}
		if (ipString.Length < 7)
		{
			Debug.Log("[Net] Couldn't validate IP: String too short.");
			return false;
		}
		bool flag = false;
		int num = 0;
		for (int i = 0; i < ipString.Length; i++)
		{
			switch (ipString[i])
			{
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				flag = false;
				break;
			case '.':
				if (flag)
				{
					Debug.Log("[Net] Couldn't validate IP: Two dots in a row.");
					return false;
				}
				flag = true;
				num++;
				break;
			default:
				Debug.Log("[Net] Couldn't validate IP: Unexpected character: '" + ipString[i] + "'");
				return false;
			}
		}
		if (num != 3)
		{
			Debug.Log("[Net] Couldn't validate IP: Too many periods.");
			return false;
		}
		return true;
	}

	protected string GetInternalIP()
	{
		return IPMan.GetIP(IPMan.ADDRESSFAM.IPv4);
	}
}
