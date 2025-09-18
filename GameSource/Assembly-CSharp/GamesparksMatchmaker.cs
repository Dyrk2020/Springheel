using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GameEvent;
using GameSparks.Api.Responses;
using GameSparks.Core;
using I2.Loc;
using UCHServices;
using UnityEngine;
using UnityEngine.Events;

public class GamesparksMatchmaker : UnityMatchmaker, IGameEventListener
{
	private const float SET_LOBBY_TIME_INTERVAL = 5f;

	protected bool enteredSocialLobby;

	private float timeSinceLastLobbyUpdate;

	private float maxLobbyUpdateWaitTime = 60f;

	private bool forceLobbyUpdate;

	private const float maxAuthWaitTime = 10f;

	private const int maxRetries = 5;

	private int getLobbyDataAttempts;

	private const int maxGetLobbyDataAttempts = 2;

	protected bool doUpdates;

	public new bool ForceSendLobbyUpdate
	{
		set
		{
			forceLobbyUpdate = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: true);
		GameEventManager.ChangeListener<GameEndEvent>(this, adding: true);
	}

	protected override void Update()
	{
		if (CurrentLobby != null && CurrentLobby is GamesparksMatchmakingLobby && doUpdates)
		{
			GamesparksMatchmakingLobby gamesparksMatchmakingLobby = (GamesparksMatchmakingLobby)CurrentLobby;
			if (gamesparksMatchmakingLobby.DataChangedSinceLastSync)
			{
				timeSinceLastLobbyUpdate += Time.unscaledDeltaTime;
				if (timeSinceLastLobbyUpdate >= 5f || forceSendLobbyUpdate)
				{
					gamesparksMatchmakingLobby.SendLobbyData(null);
					forceSendLobbyUpdate = false;
					timeSinceLastLobbyUpdate = 0f;
				}
			}
			else if (!gamesparksMatchmakingLobby.IsOwner)
			{
				timeSinceLastLobbyUpdate += Time.unscaledDeltaTime;
				if (timeSinceLastLobbyUpdate >= maxLobbyUpdateWaitTime || forceLobbyUpdate)
				{
					getLobbyDataAttempts++;
					gamesparksMatchmakingLobby.GetLobbyData(delegate(bool success)
					{
						if (!success)
						{
							if (getLobbyDataAttempts > 2)
							{
								LobbyManagerManager.AbortGameInProgressGracefully(LocalizationManager.GetTranslation("Network/XB1/LostConnection"));
							}
							else
							{
								Debug.LogWarning("Couldn't get lobby data.");
							}
						}
						else
						{
							getLobbyDataAttempts = 0;
						}
					});
					timeSinceLastLobbyUpdate = 0f;
					forceLobbyUpdate = false;
				}
			}
		}
		base.Update();
	}

	protected virtual void OnDestroy()
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding: false);
		GameEventManager.ChangeListener<GameEndEvent>(this, adding: false);
	}

	public override void CreateLobby()
	{
		base.CreateLobby();
		if (!inPlatformLobbyNoRelay && !startingLobby)
		{
			if (GameSparksManager.Instance.Available)
			{
				StartCoroutine(createAfterGameSparksAuth());
				return;
			}
			lastLobbyResult = false;
			StartCoroutine("retryCreateGamesparksLobby");
		}
		else
		{
			startingLobby = false;
			Debug.LogError("[Net] Can't create a lobby because this client is already in one");
		}
	}

	private IEnumerator createAfterGameSparksAuth()
	{
		float timer = 10f;
		while (GameSparksManager.Instance.Connecting && !GameSparksManager.Instance.Connected && timer > 0f)
		{
			timer -= Time.unscaledDeltaTime;
			yield return null;
		}
		if (GameSparksManager.Instance.Connected)
		{
			attemptToCreateGamesparksLobby();
			yield break;
		}
		Debug.LogError("[Net] Cannot create lobby: could not authenticate with backend.");
		startingLobby = false;
	}

	private IEnumerator retryCreateGamesparksLobby()
	{
		yield return new WaitForSeconds(1f);
		int attempts = 5;
		while (!inPlatformLobbyNoRelay && attempts > 0)
		{
			if (!startingLobby)
			{
				Debug.LogWarning("Retrying  backend lobby creation " + attempts + " more times");
				int num = attempts - 1;
				attempts = num;
				Debug.Log("[Net] Trying again to create backend lobby");
				if (GameSparksManager.Instance.Available)
				{
					attemptToCreateGamesparksLobby();
					yield break;
				}
				startingLobby = false;
				yield return new WaitForSeconds(3f);
			}
		}
		if (!inPlatformLobbyNoRelay)
		{
			startingLobby = false;
			Debug.LogError("[Net] Backend is not available, cannot create lobby.");
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Problem_joining_the_lobby, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
		}
	}

	private IEnumerator retryJoinGamesparksLobby(string lobbyID, bool useCode, UnityAction<bool> callback)
	{
		yield return new WaitForSeconds(1f);
		int attempts = 5;
		while (!inPlatformLobbyNoRelay && attempts > 0)
		{
			if (!startingLobby)
			{
				Debug.LogWarning("Retrying  backend lobby creation " + attempts + " more times");
				int num = attempts - 1;
				attempts = num;
				Debug.Log("[Net] Trying again to join backend lobby");
				if (GameSparksManager.Instance.Available)
				{
					StartCoroutine(joinAfterGameSparksAuth(lobbyID, useCode, callback));
					yield break;
				}
				startingLobby = false;
				yield return new WaitForSeconds(3f);
			}
		}
		if (!inPlatformLobbyNoRelay)
		{
			startingLobby = false;
			Debug.Log("[Net] Backend is not available, cannot join lobby.");
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Problem_joining_the_lobby, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			callback(arg0: false);
		}
	}

	private void attemptToCreateGamesparksLobby()
	{
		startingLobby = true;
		Debug.Log("Starting the backend lobby");
		doUpdates = true;
		GameSparksManager.Instance.CreateQuery().CreateMatch(gamesparksMatchCreated);
	}

	public override void JoinLobby(string lobbyID, bool useCode, UnityAction<bool> callback = null)
	{
		base.JoinLobby(lobbyID, useCode, callback);
		if (!inPlatformLobbyNoRelay)
		{
			if (GameSparksManager.Instance.Available)
			{
				if (!GameSparksManager.Instance.Connecting && !GameSparksManager.Instance.Connected)
				{
					GameSparksManager.Instance.ConnectNow();
				}
				StartCoroutine(joinAfterGameSparksAuth(lobbyID, useCode, callback));
			}
			else
			{
				lastLobbyResult = false;
				StartCoroutine(retryJoinGamesparksLobby(lobbyID, useCode, callback));
			}
		}
		else
		{
			startingLobby = false;
			Debug.Log("[Net] Can't join a lobby because this client is already in one");
			callback?.Invoke(arg0: false);
		}
	}

	private IEnumerator joinAfterGameSparksAuth(string lobbyID, bool useCode, UnityAction<bool> callback = null)
	{
		float timer = 10f;
		while (GameSparksManager.Instance.Connecting && !GameSparksManager.Instance.Connected && timer > 0f)
		{
			timer -= Time.unscaledDeltaTime;
			yield return null;
		}
		if (GameSparksManager.Instance.Connected)
		{
			attemptToJoinGamesparksLobby(lobbyID, useCode, callback);
			yield break;
		}
		startingLobby = false;
		LeaveLobby("[Net] Cannot join lobby: could not authenticate with backend.");
	}

	private void attemptToJoinGamesparksLobby(string lobbyID, bool useCode, UnityAction<bool> callback)
	{
		Debug.Log("Joining the lobby...");
		doUpdates = true;
		GameSparksManager.Instance.CreateQuery().GetLobbyData(lobbyID, useCode, async delegate(LogEventResponse response)
		{
			bool joinedSuccessfully = true;
			try
			{
				await gamesparksMatchJoined(response);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error in gameparksmatchjoined: " + ex.Message + "\n" + ex.StackTrace);
				joinedSuccessfully = false;
				joiningLobby = false;
			}
			if (callback != null)
			{
				callback(joinedSuccessfully && !response.HasErrors && CheckHostConnectivity());
			}
		}, reserveSlot: true);
	}

	protected virtual void createSocialLobby()
	{
	}

	protected virtual void joinSocialLobby(string lobbyID)
	{
	}

	protected virtual void leaveSocialLobby()
	{
		enteredSocialLobby = false;
	}

	protected override void leavePlatformLobby()
	{
		base.leavePlatformLobby();
		leaveSocialLobby();
	}

	public override void FindLobbies(LobbyListingCallback callback)
	{
		base.FindLobbies(callback);
		base.Searching = true;
		GameSparksManager.Instance.CreateQuery().GetLobbyList(GameSettings.GetInstance().MatchmakingNumber, GameSettings.GetInstance().RegionFilterIndex, LobbyPlayer.LocalMachinePlatform, !GameSettings.GetInstance().CrossPlatformToggle, gamesparksLobbyList);
	}

	private void gamesparksMatchCreated(LogEventResponse response)
	{
		Debug.Log(response);
		if (response.HasErrors)
		{
			Debug.Log("[Net] Problem creating lobby: " + response.Errors.JSON);
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.ProblemConnectingToGameSparks, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			lastLobbyResult = false;
			startingLobby = false;
			onLobbyCreated(success: false);
			return;
		}
		GamesparksMatchmakingLobby gamesparksMatchmakingLobby = GamesparksMatchmakingLobby.CreateNewLobby("", isOwner: true);
		string text = "";
		if (response.ScriptData.ContainsKey("match"))
		{
			GSData gSData = response.ScriptData.GetGSData("match");
			if (gSData.ContainsKey("lobbyCode"))
			{
				gamesparksMatchmakingLobby.SetLobbyCode(gSData.GetString("lobbyCode"));
			}
			Debug.Log("\nIF\t " + gSData.GetString("lobbyCode"));
			text = gSData.GetString("ownerID");
		}
		else if (response.ScriptData.ContainsKey("ownerID"))
		{
			text = response.ScriptData.GetString("ownerID");
			Debug.Log("\nElseif \t" + response.ScriptData.GetString("lobbyCode"));
			if (response.ScriptData.ContainsKey("lobbyCode"))
			{
				gamesparksMatchmakingLobby.SetLobbyCode(response.ScriptData.GetString("lobbyCode"));
			}
		}
		else
		{
			Debug.Log("LobbyCode Not Set\n");
		}
		Debug.Log("GS MatchID: " + text);
		if (!text.NullOrEmpty())
		{
			Debug.Log("Created lobby: " + text);
			gamesparksMatchmakingLobby.MatchID = text;
			gamesparksMatchmakingLobby.SetLobbyOwner(GameSparksManager.Instance.MainUserDisplayName);
			gamesparksMatchmakingLobby.LobbyVisibility = GameSettings.GetInstance().lobbyPrivacy;
			base.LobbyAnalyticsGuid = Guid.NewGuid();
			gamesparksMatchmakingLobby.SetLobbyGuid(base.LobbyAnalyticsGuid);
			Debug.Log("[Net] Using unity relay = " + GameSettings.GetInstance().UseUnityRelay);
			CurrentLobby = gamesparksMatchmakingLobby;
			onLobbyCreated(success: true);
		}
		else
		{
			onLobbyCreated(success: false);
			startingLobby = false;
		}
		createSocialLobby();
	}

	protected virtual async UniTask gamesparksMatchJoined(LogEventResponse response)
	{
		startingLobby = false;
		if (response.HasErrors)
		{
			Debug.Log("[Net] Problem joining the lobby: " + response.Errors.JSON);
			if (response.Errors.ContainsKey("codeFind"))
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Network/NoLobbyForCode"), 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			}
			else if (response.Errors.ContainsKey("codeFull"))
			{
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.matchfull, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			}
			else
			{
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Problem_joining_the_lobby, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			}
			onLobbyJoined(success: false);
			return;
		}
		GSData matchObj = response.ScriptData.GetGSData("match");
		if (!CanJoinPlatform(matchObj.GetString(MatchmakingLobby.data_lobbyPlatform)))
		{
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Problem_joining_the_lobby, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			onLobbyJoined(success: false);
			return;
		}
		string socialLobbyIDToJoin = null;
		if (!isOwner)
		{
			GamesparksMatchmakingLobby lobby = (GamesparksMatchmakingLobby)(CurrentLobby = GamesparksMatchmakingLobby.CreateNewLobby(matchObj, isOwner: false));
			AvailableRegion lobbyRegion = lobby.GetLobbyRegion();
			GameSettings.GetInstance().SelectedRegion = lobbyRegion;
			GameSettings gameSettings = GameSettings.GetInstance();
			gameSettings.RelayServerConnectionData = await UCHOnlineConnector.Service.SendGetServerForGame(lobby.GetLobbyCode());
			string text = string.Empty;
			LobbyPlayer.SocialPlatform socialPlatform = LobbyPlayer.SocialPlatform.Undefined;
			switch (lobby.GetSocialLobbyType())
			{
			case "STEAM":
				socialPlatform = LobbyPlayer.SocialPlatform.Steam;
				break;
			case "ORIGIN":
				socialPlatform = LobbyPlayer.SocialPlatform.Origin;
				break;
			case "PSN":
				socialPlatform = LobbyPlayer.SocialPlatform.PSN;
				break;
			case "XBOXONE":
				socialPlatform = LobbyPlayer.SocialPlatform.XboxLive;
				break;
			}
			if (socialPlatform != LobbyPlayer.SocialPlatform.Undefined && socialPlatform == LobbyPlayer.LocalMachinePlatform)
			{
				text = lobby.GetSocialLobbyID();
			}
			if (!text.NullOrEmpty() && !enteredSocialLobby)
			{
				socialLobbyIDToJoin = text;
			}
			if (LobbyPlayer.LocalMachinePlatform == LobbyPlayer.SocialPlatform.XboxLive)
			{
				if (LobbyPlayer.LocalMachinePlatform == socialPlatform)
				{
					socialLobbyRequired = true;
				}
				else
				{
					socialLobbyRequired = false;
				}
			}
		}
		if (!CheckHostConnectivity())
		{
			onLobbyJoined(success: false);
			return;
		}
		onLobbyJoined(success: true);
		if (socialLobbyIDToJoin != null)
		{
			joinSocialLobby(socialLobbyIDToJoin);
		}
		Debug.Log("[Net] Entered lobby: " + matchObj.GetString("ownerID"));
	}

	private void gamesparksLobbyList(LogEventResponse response)
	{
		if (response.HasErrors)
		{
			Debug.LogError("[Net] Problem enumerating lobby list: " + response.Errors.JSON);
			return;
		}
		foreach (GSData gSData in response.ScriptData.GetGSDataList("matches"))
		{
			LobbyListInfo lobbyListInfo = GamesparksMatchmakingLobby.CreateListEntryFromGSData(gSData);
			float num = 0f;
			try
			{
				num = (float)calcMatchQuality(lobbyListInfo.PlayerSkills);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to calculate match quality from skill string \"" + lobbyListInfo.PlayerSkills + "\": " + ex.Message + "\n" + ex.StackTrace);
			}
			lobbyListInfo.CalculatedSkillMatchQuality = num;
			float num2 = 0f;
			float midMatchQuality = GameSettings.GetInstance().MidMatchQuality;
			int qualityScoreMin = GameSettings.GetInstance().QualityScoreMin;
			int qualityScoreMax = GameSettings.GetInstance().QualityScoreMax;
			num2 = ((!(num < 0f)) ? ((!(midMatchQuality <= 0f)) ? ((!(midMatchQuality >= 1f)) ? ((!(num < midMatchQuality)) ? ((float)qualityScoreMax * (num - midMatchQuality) / (1f - midMatchQuality)) : ((float)qualityScoreMin * (1f - num / midMatchQuality))) : ((float)qualityScoreMin * (1f - num))) : ((float)qualityScoreMax * num)) : 0f);
			lobbyListInfo.LobbySkillNum = Mathf.RoundToInt(num2);
			lobbyListInfo.CombinedHealthSkill = lobbyListInfo.LobbyHealthNum + lobbyListInfo.LobbySkillNum;
			currentLobbyListCallback(lobbyListInfo);
		}
		base.Searching = false;
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is NetworkMessageReceivedEvent && (e as NetworkMessageReceivedEvent).Message.msgType == NetMsgTypes.LobbyDataUpdated)
		{
			forceLobbyUpdate = true;
		}
		if (e is GameEndEvent && (e as GameEndEvent).GameCompleted)
		{
			int matchesPlayed = base.MatchesPlayed + 1;
			base.MatchesPlayed = matchesPlayed;
		}
	}

	public static string GetLobbyPlatformString(LobbyPlayer.SocialPlatform platform)
	{
		switch (platform)
		{
		case LobbyPlayer.SocialPlatform.Steam:
			return "STEAM";
		case LobbyPlayer.SocialPlatform.Origin:
			return "ORIGIN";
		case LobbyPlayer.SocialPlatform.PSN:
			return "PS4";
		case LobbyPlayer.SocialPlatform.XboxLive:
			return "XBOXONE";
		case LobbyPlayer.SocialPlatform.Nintendo:
			return "SWITCH";
		case LobbyPlayer.SocialPlatform.Android:
			return "ANDROID";
		default:
			Debug.LogWarning("Couldn't find lobby platform string for this platform; returning STEAM");
			return "STEAM";
		}
	}
}
