using I2.Loc;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public class SteamMatchmaker : GamesparksMatchmaker
{
	private static bool SteamCallBacksSetup;

	private CallResult<LobbyCreated_t> lobbyCreatedResult;

	private CallResult<LobbyEnter_t> lobbyEnteredResult;

	private Callback<LobbyKicked_t> lobbyKickedCallback;

	private Callback<GameLobbyJoinRequested_t> joinRequestedCallback;

	public SteamMatchmakingLobby SteamLobby;

	protected void SetupSteam()
	{
		if (SteamManager.Initialized)
		{
			if (!SteamCallBacksSetup)
			{
				SteamCallBacksSetup = true;
				Debug.Log("[Net] St:Callbacks setup");
				lobbyCreatedResult = CallResult<LobbyCreated_t>.Create(OnSteamLobbyCreated);
				lobbyEnteredResult = CallResult<LobbyEnter_t>.Create(OnSteamLobbyEnter);
				joinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnSteamLobbyJoinRequested);
			}
			else
			{
				Debug.Log("[Net] St:Callbacks already setup");
			}
		}
		else
		{
			Debug.Log("[Net] St:Steam Manager not initialized in Setup");
		}
	}

	protected override void Awake()
	{
		base.Awake();
		SetupSteam();
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void createSocialLobby()
	{
		base.createSocialLobby();
		ELobbyType eLobbyType = GameSettings.GetInstance().lobbyPrivacy switch
		{
			MatchmakingLobby.Visibility.PUBLIC => ELobbyType.k_ELobbyTypePublic, 
			MatchmakingLobby.Visibility.FRIENDS => ELobbyType.k_ELobbyTypeFriendsOnly, 
			MatchmakingLobby.Visibility.PRIVATE => ELobbyType.k_ELobbyTypePrivate, 
			_ => ELobbyType.k_ELobbyTypePublic, 
		};
		Debug.Log("[Net] Starting " + GameSettings.GetInstance().lobbyPrivacy.ToString() + " Steam lobby.");
		SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(eLobbyType, 4);
		lobbyCreatedResult.Set(hAPICall);
	}

	protected override void joinSocialLobby(string lobbyID)
	{
		base.joinSocialLobby(lobbyID);
		JoinSteamLobby(lobbyID);
	}

	protected override void leavePlatformLobby()
	{
		if (enteredSocialLobby)
		{
			bool flag = false;
			if (SteamManager.Initialized)
			{
				NetworkLobbyPlayer[] array = LobbyManager.instance?.lobbySlots;
				ulong steamID = SteamUser.GetSteamID().m_SteamID;
				if (steamID != 0L && array != null)
				{
					NetworkLobbyPlayer[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						LobbyPlayer lobbyPlayer = (LobbyPlayer)array2[i];
						if (lobbyPlayer != null && !lobbyPlayer.IsLocalPlayer && lobbyPlayer.SteamID == steamID)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					if (SteamLobby != null && SteamLobby.IsValid())
					{
						Debug.Log("[Net] St:Leaving Steam Lobby");
						SteamMatchmaking.LeaveLobby(SteamLobby.LobbyID);
						SteamLobby = null;
					}
					else
					{
						Debug.Log("Skipped LeaveLobby; other player with same Steam ID is present");
					}
				}
			}
		}
		base.leavePlatformLobby();
	}

	private void OnSteamLobbyCreated(LobbyCreated_t callbackResult, bool failure)
	{
		if (failure)
		{
			Debug.LogWarning("[Net] St:Problem creating steam Lobby");
			StartCoroutine("retryCreateSteamLobby");
			return;
		}
		bool flag = false;
		switch (callbackResult.m_eResult)
		{
		case EResult.k_EResultOK:
			Debug.Log("[Net] Created Steam Lobby: " + callbackResult.m_ulSteamIDLobby);
			SteamLobby = new SteamMatchmakingLobby(callbackResult.m_ulSteamIDLobby);
			flag = true;
			enteredSocialLobby = true;
			if (CurrentLobby != null && CurrentLobby is GamesparksMatchmakingLobby)
			{
				GamesparksMatchmakingLobby gamesparksMatchmakingLobby = (GamesparksMatchmakingLobby)CurrentLobby;
				gamesparksMatchmakingLobby.SetLobbyPlatform(MatchmakingLobby.LobbyPlatform.STEAM);
				gamesparksMatchmakingLobby.SetSocialLobby(callbackResult.m_ulSteamIDLobby.ToString(), "STEAM");
				SteamMatchmaking.SetLobbyData(new CSteamID(callbackResult.m_ulSteamIDLobby), "gamesparksID", gamesparksMatchmakingLobby.MatchID);
			}
			else
			{
				Debug.LogWarning("Entered Steam lobby without backend lobby");
			}
			break;
		case EResult.k_EResultNoConnection:
			Debug.LogWarning("[Net] Problem creating Steam lobby: No connection");
			break;
		case EResult.k_EResultTimeout:
			Debug.LogWarning("[Net] Problem creating Steam lobby: Timeout");
			break;
		case EResult.k_EResultFail:
			Debug.LogWarning("[Net] Problem creating Steam lobby: Internal error");
			break;
		case EResult.k_EResultAccessDenied:
			Debug.LogWarning("[Net] Problem creating Steam lobby: Access denied");
			break;
		case EResult.k_EResultLimitExceeded:
			Debug.LogWarning("[Net] Problem creating Steam lobby: Limit exceeded Too many lobbies");
			break;
		default:
			Debug.LogWarning("[Net] Problem creating Steam lobby: " + callbackResult.m_eResult);
			break;
		}
		if (!flag)
		{
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.ProblemSteam, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			StartCoroutine("retryCreateSteamLobby");
		}
	}

	private void OnSteamLobbyEnter(LobbyEnter_t callbackResult, bool failure)
	{
		startingLobby = false;
		if (failure)
		{
			Debug.Log("[Net] Problem joining the Steam lobby");
			return;
		}
		SteamLobby = new SteamMatchmakingLobby(callbackResult.m_ulSteamIDLobby);
		enteredSocialLobby = true;
		Debug.Log("[Net] St:Entered Steam Lobby: " + callbackResult.m_ulSteamIDLobby + "(" + callbackResult.m_EChatRoomEnterResponse + ")");
		if (CurrentLobby == null)
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(new CSteamID(callbackResult.m_ulSteamIDLobby), "gamesparksID");
			JoinLobby(lobbyData, useCode: false, delegate(bool success)
			{
				if (success)
				{
					AnalyticEvent.JoinMatchEvent(Matchmaker.CurrentMatchmakingLobby.GetLobbyGuid(), AnalyticEvent.JoinMethod.INVITE, Matchmaker.CurrentMatchmakingLobby.LobbyIsCrossplay(Application.platform));
				}
			});
		}
		SteamMatchmaking.SetLobbyMemberData(SteamLobby.LobbyID, "name", SteamFriends.GetPersonaName());
		SteamMatchmaking.SetLobbyMemberData(SteamLobby.LobbyID, "host", isOwner ? "1" : "0");
	}

	private void OnSteamLobbyJoinRequested(GameLobbyJoinRequested_t callbackResult)
	{
		CSteamID steamIDFriend = callbackResult.m_steamIDFriend;
		string text = steamIDFriend.ToString();
		steamIDFriend = callbackResult.m_steamIDLobby;
		Debug.Log("[Net] St:Steam Join Request received from " + text + " for " + steamIDFriend.ToString());
		GameSettings.GetInstance().StartAsHost = false;
		GameSettings.GetInstance().StartLocal = false;
		if (IsInLobby() || LobbyManager.instance != null)
		{
			if (LobbyManager.instance.IsInOnlineGame && SteamLobby != null && SteamLobby.LobbyID.m_SteamID == callbackResult.m_steamIDLobby.m_SteamID)
			{
				Debug.Log("[Net] St: Client was invited to the same lobby they are in.");
				return;
			}
			GameState.GetInstance().PreservePlayers = true;
			for (int i = 1; i < 5; i++)
			{
				PlayerManager.GetInstance().GetPlayer(i)?.Reset(full: false);
			}
			LobbyManagerManager.AbortGameInProgressGracefully();
			LeaveLobby(null);
		}
		UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.JoiningRequest + " " + SteamFriends.GetFriendPersonaName(callbackResult.m_steamIDFriend), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
		LobbyManagerManager.WaitForMainMenu(delegate
		{
			JoinSteamLobby(callbackResult.m_steamIDLobby);
		});
	}

	public void JoinSteamLobby(string lobbyID)
	{
		ulong result = 0uL;
		if (ulong.TryParse(lobbyID, out result))
		{
			JoinSteamLobby(result);
		}
		else
		{
			Debug.LogError("Problem joining Steam lobby with ID: " + lobbyID);
		}
	}

	public void JoinSteamLobby(ulong lobbyID)
	{
		JoinSteamLobby(new CSteamID(lobbyID));
	}

	public void JoinSteamLobby(CSteamID lobbyID)
	{
		if (lobbyID.IsValid())
		{
			SteamAPICall_t hAPICall = SteamMatchmaking.JoinLobby(lobbyID);
			lobbyEnteredResult.Set(hAPICall);
		}
		else
		{
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Invalid_Lobby, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
		}
	}

	public override void CheckStartupArguments(string[] args)
	{
		base.CheckStartupArguments(args);
		for (int i = 0; i != args.Length; i++)
		{
			if (args[i] == "+connect_lobby")
			{
				ulong result = 0uL;
				if (ulong.TryParse(args[i + 1], out result))
				{
					GameSettings.GetInstance().StartAsHost = false;
					GameSettings.GetInstance().StartLocal = false;
					Debug.Log("Joining game on load: " + args[i + 1]);
					JoinSteamLobby(result);
				}
			}
		}
	}
}
