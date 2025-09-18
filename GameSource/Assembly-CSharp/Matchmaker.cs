using System;
using UCHServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public abstract class Matchmaker : MonoBehaviour
{
	public class LobbyListInfo
	{
		public string sLobbyID;

		public string LobbyOwner;

		public int Players;

		public bool InfoReceived;

		public ulong UnityMatchID;

		public AvailableRegion UnityServerRegion;

		public int matchProgress;

		public uint lastHearbeatTime;

		public GameState.GameMode gameMode;

		public string rulePreset;

		public int pointLimit;

		public GameLimitType limitType;

		public int limitAmount;

		public float CalculatedSkillMatchQuality;

		public int LobbyHealthNum;

		public int LobbySkillNum;

		public int CombinedHealthSkill;

		public string PlayerSkills;

		public string DebugString;

		public LobbyPlayer.SocialPlatform hostPlatform;

		public string hostPlatformId;

		public string hostGSID;

		public bool error;

		public LobbyTags lobbyTag;

		public bool usingMods;

		public bool isAFK;

		public ulong ulLobbyID
		{
			get
			{
				ulong result = 0uL;
				if (ulong.TryParse(sLobbyID, out result))
				{
					return result;
				}
				return 0uL;
			}
			set
			{
				sLobbyID = value.ToString();
			}
		}
	}

	public delegate void LobbyListingCallback(LobbyListInfo info);

	protected static Matchmaker instance;

	protected bool forceSendLobbyUpdate;

	public bool LoadingSearchIndicator;

	protected bool socialLobbyRequired;

	protected bool inSocialLobby;

	private bool joinability;

	public MatchmakingLobby CurrentLobby;

	public string myExternalIP = "UNASSIGNED";

	public string myInternalIP = "UNASSIGNED";

	protected LobbyListingCallback currentLobbyListCallback;

	public static Matchmaker Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("SteamMatchmaker", typeof(SteamMatchmaker)).GetComponent<SteamMatchmaker>();
				Debug.Log("[Net] Steam MatchMaker Instantiated " + instance.GetInstanceID());
			}
			UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
			return instance;
		}
	}

	public static MatchmakingLobby CurrentMatchmakingLobby
	{
		get
		{
			if (instance != null)
			{
				return instance.CurrentLobby;
			}
			return null;
		}
	}

	public bool ForceSendLobbyUpdate
	{
		set
		{
			forceSendLobbyUpdate = value;
		}
	}

	public static bool IsInstantiated => instance != null;

	public bool Searching { get; protected set; }

	public bool WaitingForLobby { get; protected set; }

	public bool WaitingForController { get; protected set; }

	public bool WaitingForSocialLobby
	{
		get
		{
			if (!inSocialLobby)
			{
				return socialLobbyRequired;
			}
			return false;
		}
	}

	public bool Joinability
	{
		get
		{
			return joinability;
		}
		set
		{
			if (CurrentLobby.IsValid())
			{
				joinability = value;
				CurrentLobby.SetLobbyJoinable(value);
				forceSendLobbyUpdate = true;
			}
			else
			{
				Debug.Log("[Net] Lobby is not valid, can't set joinability");
			}
		}
	}

	public static bool InTreehouse
	{
		get
		{
			if (LobbyManager.instance != null)
			{
				return LobbyManager.instance.CurrentLevelSelectController != null;
			}
			return false;
		}
	}

	public Guid LobbyAnalyticsGuid { get; protected set; }

	public int MatchesPlayed { get; protected set; }

	public static Matchmaker InstantiateNow()
	{
		return instance;
	}

	public virtual void ResetMatchesPlayed()
	{
		MatchesPlayed = 0;
	}

	protected virtual void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
	}

	public abstract bool CheckHostConnectivity();

	public abstract void CreateLobby();

	public abstract void FindLobbies(LobbyListingCallback callback);

	public abstract bool IsInLobby();

	public abstract bool IsLobbyOwner();

	public abstract void JoinLobby(string lobbyID, bool useCode, UnityAction<bool> callback = null);

	public abstract void JoinGameManual();

	public abstract bool LastLobbyResult();

	public abstract void LeaveLobby(string reason = "", UserMessageManager.UserMsgPriority priority = UserMessageManager.UserMsgPriority.lo);

	public abstract bool StartingLobby();

	public abstract void OnLobbyManagerHostingStarted();

	public abstract void UpdateLobbyRuleData();

	public abstract void DropConnection(ushort dropNodeId);

	public abstract void CheckStartupArguments(string[] args);

	public abstract void OnLobbyPrivacyChanged(MatchmakingLobby.Visibility visibility);

	public abstract bool CanJoinPlatform(string platform);

	protected abstract void getStartupArguments();

	protected abstract void leavePlatformLobby();

	protected abstract double calcMatchQuality(string playerSkillString);

	protected abstract void onLobbyCreated(bool success);

	protected abstract void onLobbyJoined(bool success);

	public virtual void OnReadyToAutoJoin(MainMenuControl mainMenuControl)
	{
	}

	public static void ReturnToMainMenu(string reason = null)
	{
		LobbyManagerManager.Instance.SetAbortReason(reason);
		if (LobbyManager.instance != null)
		{
			if (LobbyManager.instance.IsHost)
			{
				NetworkServer.SendToAll(NetMsgTypes.HostEndedGame, new MsgHostEndedGame());
			}
			CleanUpPlayers();
		}
		LobbyManagerManager.AbortGameInProgressGracefully(reason);
	}

	public static void CleanUpPlayers()
	{
		if (!(LobbyManager.instance != null))
		{
			return;
		}
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null)
			{
				LobbyManagerManager.Instance.MarkLobbyPlayerToRemove(lobbyPlayer);
			}
		}
		for (int j = 1; j < 5; j++)
		{
			PlayerManager.GetInstance().RemovePlayer(j)?.Reset();
		}
	}
}
