using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameEvent;
using I2.Loc;
using MLAPI.Relay.Transports;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkLobbyManager, IGameEventListener
{
	public enum ConnectionQuality
	{
		POOR,
		SLOW,
		GOOD,
		GREAT
	}

	public enum KickReasons
	{
		HOST,
		VOTE,
		AFK,
		NONE
	}

	public static int PingGreat = 0;

	public static int PingGood = 125;

	public static int PingSlow = 200;

	public static int PingPoor = 300;

	public int trackerlength;

	public int lastTrackerLength;

	private bool readyToAddPlayers;

	public bool reloadingScene;

	public NetworkPlayerTracker PlayerTracker;

	public bool AllLocal;

	private bool isClientDisconnected;

	public LevelSelectController CurrentLevelSelectController;

	public GameControl CurrentGameController;

	private List<Player> localPlayerBacklog = new List<Player>();

	private KickTracker kickTracker;

	private Dictionary<NetworkConnection, float> brokenClientConnections = new Dictionary<NetworkConnection, float>();

	private Dictionary<NetworkConnection, float> connectionLifetimes = new Dictionary<NetworkConnection, float>();

	private const float brokenClientTimeout = 0.25f;

	private const float brokenClientTimeoutBuffer = 3f;

	private List<NetworkConnection> connectionsToKillCache = new List<NetworkConnection>(8);

	private Coroutine connectCoroutine;

	private float brokenClientTimer;

	public static LobbyManager instance => (LobbyManager)NetworkManager.singleton;

	public bool IsHost { get; protected set; }

	public bool IsInOnlineGame
	{
		get
		{
			if (IsHost)
			{
				if (Matchmaker.CurrentMatchmakingLobby != null)
				{
					return Matchmaker.CurrentMatchmakingLobby.IsValid();
				}
				return false;
			}
			return true;
		}
	}

	public int CurrentPing
	{
		get
		{
			if (client == null)
			{
				return 0;
			}
			return checkPing();
		}
	}

	public int AverageClientPing
	{
		get
		{
			if (!IsHost)
			{
				return 0;
			}
			return checkAveragePing();
		}
	}

	public int NumActiveConnections
	{
		get
		{
			int num = 0;
			foreach (NetworkConnection connection in NetworkServer.connections)
			{
				if (connection != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool HasPlayersLockedForLoad
	{
		get
		{
			NetworkLobbyPlayer[] array = lobbySlots;
			for (int i = 0; i < array.Length; i++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)array[i];
				if (lobbyPlayer != null && lobbyPlayer.LockedForLoad)
				{
					return true;
				}
			}
			return false;
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		base.lobbyScene = "TreeHouseLobby";
		NetworkManager.networkSceneName = null;
		base.onlineScene = null;
		base.playScene = "Empty";
		base.offlineScene = null;
		isClientDisconnected = false;
	}

	private void Start()
	{
		kickTracker = new KickTracker();
		connectCoroutine = StartCoroutine(wait3SecondsForLoad());
		LobbyManagerManager.Instance.ClearAbortReason();
	}

	private IEnumerator wait3SecondsForLoad()
	{
		if (!GameSettings.GetInstance().StartLocal)
		{
			yield return new WaitUntil(() => GameSettings.GetInstance().RelayServerConnectionData != null);
		}
		yield return new WaitForSeconds(1f);
		ChangeListener(addRemove: true);
		Connect();
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null)
			{
				localPlayerBacklog.Add(item);
			}
		}
	}

	private void Update()
	{
		if (readyToAddPlayers && localPlayerBacklog.Count > 0)
		{
			foreach (Player item in localPlayerBacklog)
			{
				if (!item.Connected)
				{
					addLobbyPlayerForLocalPlayer(item);
				}
			}
			localPlayerBacklog.Clear();
		}
		trackerlength = PlayerTracker.NumPlayers;
		if (lastTrackerLength != trackerlength)
		{
			lastTrackerLength = trackerlength;
			if (IsHost && Matchmaker.Instance.CurrentLobby != null)
			{
				Matchmaker.Instance.CurrentLobby.SetPlayerCount(trackerlength);
			}
		}
		brokenClientTimer += Time.unscaledDeltaTime;
		if (brokenClientTimer > 1f)
		{
			DisconnectBrokenClients();
			brokenClientTimer = 0f;
		}
	}

	private void OnDestroy()
	{
		if (instance == this && instance != null)
		{
			Debug.Log("LobbyManager being destroyed");
			ChangeListener(addRemove: false);
			if (NetworkManager.activeTransport is UnetRelayTransport unetRelayTransport)
			{
				unetRelayTransport.OnRelayServerDisconnected -= OnRelayDisconnect;
			}
		}
		else
		{
			Debug.Log("Destroyed duplicate LobbyManager instance");
		}
	}

	public void ChangeListener(bool addRemove)
	{
		GameEventManager.ChangeListener<NetworkStartHostEvent>(this, addRemove);
		GameEventManager.ChangeListener<NetworkStartClientEvent>(this, addRemove);
		GameEventManager.ChangeListener<NetworkClientCleanedUpEvent>(this, addRemove);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, addRemove);
		GameEventManager.ChangeListener<LocalPlayerAddedEvent>(this, addRemove);
		GameEventManager.ChangeListener<LocalPlayerRemovedEvent>(this, addRemove);
	}

	private void addLobbyPlayerForLocalPlayer(Player p)
	{
		if (p != null && !p.Connected)
		{
			if (!ClientScene.AddPlayer(client.connection, (short)p.Number))
			{
				Debug.LogWarning("Problem adding lobby player for local player " + p.Number);
			}
			else
			{
				Debug.Log("Lobby Player added for local player " + p.Number);
			}
		}
	}

	public void Connect()
	{
		base.networkAddress = GameSettings.GetInstance().NetworkAddress;
		base.networkPort = GameSettings.GetInstance().NetworkPort;
		base.connectionConfig.PacketSize = 1312;
		base.connectionConfig.MaxConnectionAttempt = 15;
		if (GameSettings.GetInstance().StartAsHost)
		{
			Debug.Log("Starting Host");
			if (GameSettings.GetInstance().StartLocal)
			{
				NetworkServer.dontListen = true;
				Matchmaker.Instance.CurrentLobby = new LocalLobby();
				Debug.Log("Not listening");
			}
			else
			{
				NetworkServer.dontListen = false;
				Debug.Log("Listening");
				if (NetworkManager.activeTransport is UnetRelayTransport unetRelayTransport)
				{
					unetRelayTransport.OnRelayServerDisconnected += OnRelayDisconnect;
				}
			}
			Debug.Log("[Net] Starting hosting using P2P on port: " + base.networkPort);
			if (StartHost() == null)
			{
				Debug.Log("[Net] Problem starting the host");
			}
			Matchmaker.Instance.CurrentLobby.SetPlayerCount(1);
			if (Matchmaker.Instance.IsLobbyOwner())
			{
				Matchmaker.Instance.CurrentLobby.SetLobbyPort(base.networkPort);
			}
		}
		else if (Matchmaker.Instance.CheckHostConnectivity())
		{
			if (StartClient() == null)
			{
				Debug.Log("[Net] L: Problem starting the client the old way P2p");
				LobbyManagerManager.Instance.AbortGameInProgress();
			}
		}
		else
		{
			LobbyManagerManager.Instance.AbortGameInProgress();
		}
		NetworkServer.RegisterHandler(NetMsgTypes.NetworkClientConnected, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.NetworkClientDisconnected, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.GameRuleSet, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetBlockFrequency, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetAllBlockFrequencies, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SendAllBlockFrequencies, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PiecePicked, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PiecePlaced, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ProjectileHit, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.TrapTriggered, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PieceDestroyed, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.CharacterSuccess, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ProjectileHit, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PointAwarded, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetNetworkSurrogateVal, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SwitchToMode, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.BookPiecePicked, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetPartyPieceID, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ClientLoadedTreehouse, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ChatSent, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PortalHasUnlock, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.UnlockAvailable, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ConnectionQuality, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.LobbyVoting, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PlayerSkillUpdated, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetGameModeLock, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PlayerHandicapSet, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.AFKPlayer, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PiecePickedUp, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SnapshotLoadingDone, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.VoteToKick, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ShowCredits, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.CommunicateCharacterOutfits, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetCustomPortalInfo, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.LobbyDataUpdated, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetCustomBackground, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetCustomMusic, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.UpdateVoteKickCounts, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PlayerWantsToRetry, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PlayerReadyToStart, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.SetCustomAmbience, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ProjectileDestroyed, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.HostEndedGame, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.AFKTimerChanged, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PunchingBlockTriggered, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ApplyRuleset, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.RulesetDirty, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ForcedPieceSpawned, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.PlatformDancing, distributeServerMessage);
		NetworkServer.RegisterHandler(NetMsgTypes.ThwompTriggered, distributeServerMessage);
		if (client != null)
		{
			client.RegisterHandler(NetMsgTypes.NetworkClientConnected, distributeMessage);
			client.RegisterHandler(NetMsgTypes.NetworkClientDisconnected, distributeMessage);
			client.RegisterHandler(NetMsgTypes.GameRuleSet, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetBlockFrequency, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetAllBlockFrequencies, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SendAllBlockFrequencies, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PiecePicked, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PiecePlaced, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PartyBoxOpen, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PartyBoxClosed, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ProjectileHit, distributeMessage);
			client.RegisterHandler(NetMsgTypes.TrapTriggered, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PieceDestroyed, distributeMessage);
			client.RegisterHandler(NetMsgTypes.CharacterSuccess, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PointAwarded, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PointsCleared, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ReadyToTallyPoints, distributeMessage);
			client.RegisterHandler(NetMsgTypes.NetworkSurrogateSpawned, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetNetworkSurrogateVal, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SwitchToMode, distributeMessage);
			client.RegisterHandler(NetMsgTypes.BookPiecePicked, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetPartyPieceID, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ClientLoadedTreehouse, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ChatSent, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PortalHasUnlock, distributeMessage);
			client.RegisterHandler(NetMsgTypes.UnlockAvailable, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ClientKicked, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ConnectionQuality, distributeMessage);
			client.RegisterHandler(NetMsgTypes.LobbyVoting, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PlayerSkillUpdated, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetGameModeLock, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PlayerHandicapSet, distributeMessage);
			client.RegisterHandler(NetMsgTypes.AFKPlayer, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PiecePickedUp, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SnapshotLoadingDone, distributeMessage);
			client.RegisterHandler(NetMsgTypes.VoteToKick, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ShowCredits, distributeMessage);
			client.RegisterHandler(NetMsgTypes.CommunicateCharacterOutfits, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetCustomPortalInfo, distributeMessage);
			client.RegisterHandler(NetMsgTypes.LobbyDataUpdated, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetCustomBackground, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetCustomMusic, distributeMessage);
			client.RegisterHandler(NetMsgTypes.UpdateVoteKickCounts, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PlayerWantsToRetry, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PlayerReadyToStart, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PrepareToReloadScene, distributeMessage);
			client.RegisterHandler(NetMsgTypes.SetCustomAmbience, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ProjectileDestroyed, distributeMessage);
			client.RegisterHandler(NetMsgTypes.HostEndedGame, distributeMessage);
			client.RegisterHandler(NetMsgTypes.AFKTimerChanged, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PunchingBlockTriggered, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ApplyRuleset, distributeMessage);
			client.RegisterHandler(NetMsgTypes.RulesetDirty, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ForcedPieceSpawned, distributeMessage);
			client.RegisterHandler(NetMsgTypes.PlatformDancing, distributeMessage);
			client.RegisterHandler(NetMsgTypes.ThwompTriggered, distributeMessage);
		}
		else
		{
			Debug.LogError("NO CLIENT");
		}
	}

	private void OnRelayDisconnect()
	{
		LobbyManagerManager.AbortGameInProgressGracefully(ScriptLocalization.Network.Disconnected);
	}

	public int CountClients()
	{
		if (IsHost)
		{
			int num = 0;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null && networkConnection.hostId != -1)
				{
					num++;
				}
			}
			return num;
		}
		return 0;
	}

	private int checkAveragePing()
	{
		if (IsHost)
		{
			if (NumActiveConnections == 1)
			{
				return 0;
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null && networkConnection.hostId != -1)
				{
					byte error;
					int currentRTT = NetworkManager.activeTransport.GetCurrentRTT(networkConnection.hostId, networkConnection.connectionId, out error);
					num += currentRTT;
					num2++;
				}
			}
			if (num2 > 0)
			{
				return num / num2;
			}
			return 0;
		}
		return client.GetRTT();
	}

	private int checkPing()
	{
		if (IsHost)
		{
			if (NumActiveConnections == 1)
			{
				return 0;
			}
			int num = int.MaxValue;
			bool flag = true;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null && networkConnection.hostId != -1)
				{
					byte error;
					int currentRTT = NetworkManager.activeTransport.GetCurrentRTT(networkConnection.hostId, networkConnection.connectionId, out error);
					if (currentRTT < num)
					{
						num = currentRTT;
					}
					flag = false;
				}
			}
			if (!flag)
			{
				return num;
			}
			return 0;
		}
		return client.GetRTT();
	}

	public void ChangeGameModeOnClient(GameState.GameMode toGameMode)
	{
		MsgSwitchToMode msgSwitchToMode = new MsgSwitchToMode();
		msgSwitchToMode.toMode = toGameMode;
		NetworkManager.singleton.client.Send(NetMsgTypes.SwitchToMode, msgSwitchToMode);
	}

	private void onError(NetworkMessage netMsg)
	{
		Debug.Log("[Net] Error msg received:" + netMsg.ToString());
	}

	private void distributeServerMessage(NetworkMessage msg)
	{
		NetworkServer.SendToAll(msg.msgType, readMessage(msg));
	}

	private void distributeMessage(NetworkMessage msg)
	{
		MessageBase messageBase = readMessage(msg);
		GameEventManager.SendEvent(new NetworkMessageReceivedEvent(msg, messageBase));
	}

	private MessageBase readMessage(NetworkMessage msg)
	{
		MessageBase messageBase = null;
		if (msg.msgType == NetMsgTypes.CharacterSuccess)
		{
			messageBase = msg.ReadMessage<MsgCharacterSuccess>();
		}
		if (msg.msgType == NetMsgTypes.PartyBoxClosed)
		{
			messageBase = msg.ReadMessage<MsgPartyBoxOpen>();
		}
		if (msg.msgType == NetMsgTypes.PartyBoxOpen)
		{
			messageBase = msg.ReadMessage<MsgPartyBoxOpen>();
		}
		if (msg.msgType == NetMsgTypes.PieceDestroyed)
		{
			messageBase = msg.ReadMessage<MsgPieceDestroyed>();
		}
		if (msg.msgType == NetMsgTypes.PiecePicked)
		{
			messageBase = msg.ReadMessage<MsgPiecePicked>();
		}
		if (msg.msgType == NetMsgTypes.PiecePlaced)
		{
			messageBase = msg.ReadMessage<MsgPiecePlaced>();
		}
		if (msg.msgType == NetMsgTypes.ProjectileHit)
		{
			messageBase = msg.ReadMessage<MsgProjectileHit>();
		}
		if (msg.msgType == NetMsgTypes.TrapTriggered)
		{
			messageBase = msg.ReadMessage<MsgTrapTriggered>();
		}
		if (msg.msgType == NetMsgTypes.PointAwarded)
		{
			messageBase = msg.ReadMessage<MsgPointAwarded>();
		}
		if (msg.msgType == NetMsgTypes.PointsCleared)
		{
			messageBase = msg.ReadMessage<MsgPointsCleared>();
		}
		if (msg.msgType == NetMsgTypes.ReadyToTallyPoints)
		{
			messageBase = msg.ReadMessage<MsgReadyToTallyPoints>();
		}
		if (msg.msgType == NetMsgTypes.NetworkSurrogateSpawned)
		{
			messageBase = msg.ReadMessage<MsgNetworkSurrogateSpawned>();
		}
		if (msg.msgType == NetMsgTypes.SetNetworkSurrogateVal)
		{
			messageBase = msg.ReadMessage<MsgSetNetworkSurrogateVal>();
		}
		if (msg.msgType == NetMsgTypes.SwitchToMode)
		{
			messageBase = msg.ReadMessage<MsgSwitchToMode>();
		}
		if (msg.msgType == NetMsgTypes.BookPiecePicked)
		{
			messageBase = msg.ReadMessage<MsgBookPiecePicked>();
		}
		if (msg.msgType == NetMsgTypes.SetPartyPieceID)
		{
			messageBase = msg.ReadMessage<MsgSetPartyPieceID>();
		}
		if (msg.msgType == NetMsgTypes.NetworkClientConnected)
		{
			messageBase = msg.ReadMessage<MsgNetworkClientConnected>();
		}
		if (msg.msgType == NetMsgTypes.NetworkClientDisconnected)
		{
			messageBase = msg.ReadMessage<MsgNetworkClientDisconnected>();
		}
		if (msg.msgType == NetMsgTypes.GameRuleSet)
		{
			messageBase = msg.ReadMessage<MsgGameRuleSet>();
		}
		if (msg.msgType == NetMsgTypes.SetBlockFrequency)
		{
			messageBase = msg.ReadMessage<MsgSetBlockFrequency>();
		}
		if (msg.msgType == NetMsgTypes.SetAllBlockFrequencies)
		{
			messageBase = msg.ReadMessage<MsgSetAllBlockFrequencies>();
		}
		if (msg.msgType == NetMsgTypes.SendAllBlockFrequencies)
		{
			messageBase = msg.ReadMessage<MsgSendAllBlockFrequencies>();
		}
		if (msg.msgType == NetMsgTypes.ClientLoadedTreehouse)
		{
			messageBase = msg.ReadMessage<MsgClientLoadedTreehouse>();
		}
		if (msg.msgType == NetMsgTypes.ChatSent)
		{
			messageBase = msg.ReadMessage<MsgChat>();
		}
		if (msg.msgType == NetMsgTypes.PortalHasUnlock)
		{
			messageBase = msg.ReadMessage<MsgPortalHasUnlock>();
		}
		if (msg.msgType == NetMsgTypes.UnlockAvailable)
		{
			messageBase = msg.ReadMessage<MsgUnlockAvailable>();
		}
		if (msg.msgType == NetMsgTypes.ClientKicked)
		{
			messageBase = msg.ReadMessage<MsgClientKicked>();
		}
		if (msg.msgType == NetMsgTypes.ConnectionQuality)
		{
			messageBase = msg.ReadMessage<MsgConnectionQuality>();
		}
		if (msg.msgType == NetMsgTypes.LobbyVoting)
		{
			messageBase = msg.ReadMessage<MsgLobbyVoting>();
		}
		if (msg.msgType == NetMsgTypes.PlayerSkillUpdated)
		{
			messageBase = msg.ReadMessage<MsgPlayerSkillUpdated>();
		}
		if (msg.msgType == NetMsgTypes.SetGameModeLock)
		{
			messageBase = msg.ReadMessage<MsgSetGameModeLock>();
		}
		if (msg.msgType == NetMsgTypes.PlayerHandicapSet)
		{
			messageBase = msg.ReadMessage<MsgPlayerHandicapSet>();
		}
		if (msg.msgType == NetMsgTypes.AFKPlayer)
		{
			messageBase = msg.ReadMessage<MsgAFKPlayer>();
		}
		if (msg.msgType == NetMsgTypes.PiecePickedUp)
		{
			messageBase = msg.ReadMessage<MsgPiecePickedUp>();
		}
		if (msg.msgType == NetMsgTypes.SnapshotLoadingDone)
		{
			messageBase = msg.ReadMessage<MsgSnapshotLoadingDone>();
		}
		if (msg.msgType == NetMsgTypes.VoteToKick)
		{
			messageBase = msg.ReadMessage<MsgVoteToKick>();
		}
		if (msg.msgType == NetMsgTypes.ShowCredits)
		{
			messageBase = msg.ReadMessage<MsgShowCredits>();
		}
		if (msg.msgType == NetMsgTypes.CommunicateCharacterOutfits)
		{
			messageBase = msg.ReadMessage<MsgCommunicateCharacterOutfits>();
		}
		if (msg.msgType == NetMsgTypes.SetCustomPortalInfo)
		{
			messageBase = msg.ReadMessage<MsgSetCustomPortalInfo>();
		}
		if (msg.msgType == NetMsgTypes.LobbyDataUpdated)
		{
			messageBase = msg.ReadMessage<MsgLobbyDataUpdated>();
		}
		if (msg.msgType == NetMsgTypes.SetCustomBackground)
		{
			messageBase = msg.ReadMessage<MsgSetCustomBackground>();
		}
		if (msg.msgType == NetMsgTypes.SetCustomMusic)
		{
			messageBase = msg.ReadMessage<MsgSetCustomMusic>();
		}
		if (msg.msgType == NetMsgTypes.UpdateVoteKickCounts)
		{
			messageBase = msg.ReadMessage<MsgUpdateVoteKickCounts>();
		}
		if (msg.msgType == NetMsgTypes.PlayerWantsToRetry)
		{
			messageBase = msg.ReadMessage<MsgPlayerWantsToRetry>();
		}
		if (msg.msgType == NetMsgTypes.PlayerReadyToStart)
		{
			messageBase = msg.ReadMessage<MsgPlayerReadyToStart>();
		}
		if (msg.msgType == NetMsgTypes.PrepareToReloadScene)
		{
			messageBase = msg.ReadMessage<MsgPrepareToReloadScene>();
		}
		if (msg.msgType == NetMsgTypes.SetCustomAmbience)
		{
			messageBase = msg.ReadMessage<MsgSetCustomAmbience>();
		}
		if (msg.msgType == NetMsgTypes.ProjectileDestroyed)
		{
			messageBase = msg.ReadMessage<MsgProjectileDestroyed>();
		}
		if (msg.msgType == NetMsgTypes.HostEndedGame)
		{
			messageBase = msg.ReadMessage<MsgHostEndedGame>();
		}
		if (msg.msgType == NetMsgTypes.AFKTimerChanged)
		{
			messageBase = msg.ReadMessage<MsgAFKTimerChanged>();
		}
		if (msg.msgType == NetMsgTypes.PunchingBlockTriggered)
		{
			messageBase = msg.ReadMessage<MsgPunchingBlockTriggered>();
		}
		if (msg.msgType == NetMsgTypes.ApplyRuleset)
		{
			messageBase = msg.ReadMessage<MsgApplyRuleset>();
		}
		if (msg.msgType == NetMsgTypes.RulesetDirty)
		{
			messageBase = msg.ReadMessage<MsgRulesetDirty>();
		}
		if (msg.msgType == NetMsgTypes.ForcedPieceSpawned)
		{
			messageBase = msg.ReadMessage<MsgForcedPieceSpawned>();
		}
		if (msg.msgType == NetMsgTypes.PlatformDancing)
		{
			messageBase = msg.ReadMessage<MsgPlatformDancing>();
		}
		if (msg.msgType == NetMsgTypes.ThwompTriggered)
		{
			messageBase = msg.ReadMessage<MsgThwompTriggered>();
		}
		if (messageBase == null)
		{
			Debug.LogWarning("Some unreadable message was sent: " + msg.msgType + " - " + NetMsgTypes.GetMessageName(msg.msgType));
		}
		return messageBase;
	}

	public override void OnStartClient(NetworkClient lobbyClient)
	{
		base.OnStartClient(lobbyClient);
		Debug.Log("Client started: " + lobbyClient.serverIp + ":" + lobbyClient.serverPort + " (" + base.networkAddress + ":" + base.networkPort + ") ");
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
		Debug.Log("Client connected: " + conn);
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		Debug.Log("Client stopped, returning to main menu");
		LobbyManagerManager.AbortGameInProgressGracefully();
	}

	public override void OnServerReady(NetworkConnection conn)
	{
		base.OnServerReady(conn);
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		Debug.Log("Server Connect");
	}

	public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
	{
		base.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
		LobbyPlayer lobbyPlayer = (LobbyPlayer)UnityEngine.Object.Instantiate(base.lobbyPlayerPrefab);
		for (int i = 0; i != lobbySlots.Length; i++)
		{
			if (lobbySlots[i] == null)
			{
				lobbyPlayer.NetworknetworkNumber = i + 1;
				lobbyPlayer.NetworkPlayerColor = GameSettings.GetInstance().PlayerColors[i];
				break;
			}
		}
		PlayerTracker.AddLobbyPlayer(lobbyPlayer);
		GameEventManager.SendEvent(new NetworkPlayerConnectEvent(conn, lobbyPlayer.networkNumber));
		return lobbyPlayer.gameObject;
	}

	private IEnumerator WaitForLobbyPlayerID(NetworkConnection conn, LobbyPlayer player)
	{
		while (player.netId.IsEmpty())
		{
			yield return null;
		}
	}

	public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
	{
		if (CurrentLevelSelectController != null)
		{
			countDownStart countDownStart2 = CurrentLevelSelectController.CountDownStart;
			if (countDownStart2 != null)
			{
				countDownStart2.RpcStopTimer();
			}
		}
		Debug.Log("Player removed");
		foreach (PlayerController playerController in conn.playerControllers)
		{
			if (playerController.playerControllerId != playerControllerId || !(playerController.gameObject != null))
			{
				continue;
			}
			LobbyPlayer component = playerController.gameObject.GetComponent<LobbyPlayer>();
			if (component != null)
			{
				Debug.Log("OnLobbyServerPlayerRemoved " + component.playerName);
				if (IsInOnlineGame)
				{
					RemovePlayerFromMPSDCustomField(component);
				}
				RemoveLobbyPlayer(component);
				continue;
			}
			GamePlayer component2 = playerController.gameObject.GetComponent<GamePlayer>();
			if (!(component2 != null))
			{
				continue;
			}
			LobbyPlayer[] array = UnityEngine.Object.FindObjectsOfType<LobbyPlayer>();
			for (int i = 0; i != array.Length; i++)
			{
				if (array[i].networkNumber == component2.networkNumber)
				{
					RemovePlayerFromMPSDCustomField(array[i]);
				}
			}
			MsgNetworkClientDisconnected msgNetworkClientDisconnected = new MsgNetworkClientDisconnected();
			msgNetworkClientDisconnected.PlayerNetworkNumber = component2.networkNumber;
			NetworkServer.SendToAll(NetMsgTypes.NetworkClientDisconnected, msgNetworkClientDisconnected);
			GameEventManager.SendEvent(new NetworkPlayerDisconnectEvent(component2.networkNumber, component2.WasKicked));
		}
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
	{
		Debug.Log("Removing player: " + player?.ToString() + " with connection " + conn);
		base.OnServerRemovePlayer(conn, player);
	}

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
	{
		return UnityEngine.Object.Instantiate(base.gamePlayerPrefab);
	}

	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
	{
		base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
		GamePlayer component = gamePlayer.GetComponent<GamePlayer>();
		LobbyPlayer component2 = lobbyPlayer.GetComponent<LobbyPlayer>();
		component2.NetworkLockedForLoad = false;
		Debug.Log("Scene loaded for player " + component2.networkNumber);
		if (component2.PlayerStatus == LobbyPlayer.Status.CURSOR || component2.PlayerStatus == LobbyPlayer.Status.INACTIVE)
		{
			component2.RemovePlayer();
			UnityEngine.Object.Destroy(gamePlayer);
			return false;
		}
		component.NetworknetworkNumber = component2.networkNumber;
		component.NetworklocalNumber = component2.localNumber;
		component.NetworkPickedAnimal = component2.PickedAnimal;
		component.characterOutfitsList.Clear();
		foreach (int characterOutfits in component2.characterOutfitsList)
		{
			component.characterOutfitsList.Add(characterOutfits);
		}
		PlayerTracker.AddGamePlayer(component);
		return true;
	}

	public override void OnServerError(NetworkConnection conn, int errorCode)
	{
		base.OnServerError(conn, errorCode);
		NetworkError networkError = (NetworkError)errorCode;
		Debug.Log("Server error " + networkError.ToString() + "(" + errorCode + ") for connection " + conn);
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		base.OnClientError(conn, errorCode);
		NetworkError networkError = (NetworkError)errorCode;
		Debug.Log("Client error " + networkError.ToString() + "(" + errorCode + ") for connection " + conn);
		if (errorCode != 0)
		{
			LobbyManagerManager.AbortGameInProgressGracefully();
		}
	}

	public override void OnClientNotReady(NetworkConnection conn)
	{
		base.OnClientNotReady(conn);
		Debug.Log($"Client not ready {conn.connectionId}");
	}

	public override void OnLobbyClientEnter()
	{
		base.OnLobbyClientEnter();
		readyToAddPlayers = true;
		Debug.Log("Lobby client entered");
	}

	public override void OnLobbyClientDisconnect(NetworkConnection conn)
	{
		base.OnLobbyClientDisconnect(conn);
		Debug.Log("Lobby Client disconnected");
		if (!GameSettings.GetInstance().StartLocal && !IsHost)
		{
			LobbyManagerManager.Instance.AbortGameInProgress(ScriptLocalization.Network.Disconnected);
		}
	}

	public override void OnLobbyServerSceneChanged(string sceneName)
	{
		base.OnLobbyServerSceneChanged(sceneName);
		Debug.Log("Lobby Server scene changed to " + sceneName);
	}

	public override void OnLobbyClientSceneChanged(NetworkConnection conn)
	{
		base.OnLobbyClientSceneChanged(conn);
		Debug.Log("Lobby Client scene changed");
		int num = 0;
		for (int i = 0; i != lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (!(lobbyPlayer != null) || (!lobbyPlayer.IsLocalPlayer && IsInOnlineGame))
			{
				continue;
			}
			if (lobbyPlayer.PickedAnimal != Character.Animals.NONE)
			{
				num++;
				continue;
			}
			Player localPlayer = lobbyPlayer.LocalPlayer;
			if (localPlayer != null && localPlayer.AssociatedGamePlayer != null && localPlayer.AssociatedGamePlayer.PickedAnimal != Character.Animals.NONE)
			{
				num++;
			}
		}
		Scene activeScene = SceneManager.GetActiveScene();
		Debug.Log("currenscene name = " + activeScene.name + " networked scene name = " + NetworkManager.networkSceneName);
		if (!(activeScene.name != "TreeHouseLobby") || num != 0)
		{
			return;
		}
		for (int j = 0; j != lobbySlots.Length; j++)
		{
			LobbyPlayer lobbyPlayer2 = (LobbyPlayer)lobbySlots[j];
			if (lobbyPlayer2 != null && lobbyPlayer2.IsLocalPlayer)
			{
				lobbyPlayer2.CallCmdIShouldNotBeHere();
			}
		}
	}

	public override void OnLobbyStartHost()
	{
		base.OnLobbyStartHost();
		Debug.Log("OnLobbyStartHost");
		IsHost = true;
		if (Matchmaker.Instance != null)
		{
			Matchmaker.Instance.OnLobbyManagerHostingStarted();
		}
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		Debug.Log("Client disconnected");
		LobbyManagerManager.AbortGameInProgressGracefully(ScriptLocalization.Network.Disconnected);
		IsHost = false;
		isClientDisconnected = true;
	}

	public override void OnLobbyClientAddPlayerFailed()
	{
		base.OnLobbyClientAddPlayerFailed();
		Debug.Log("Failed to add player, leaving lobby");
		for (int i = 1; i < 5; i++)
		{
			PlayerManager.GetInstance().RemovePlayer(i)?.Reset();
		}
		LobbyManagerManager.AbortGameInProgressGracefully(ScriptLocalization.Network.matchfull);
	}

	public new void StopClient()
	{
		base.StopClient();
	}

	public override void OnLobbyClientExit()
	{
		if (instance != null)
		{
			base.OnLobbyClientExit();
			Debug.Log("Client exit");
		}
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer(conn, playerControllerId);
		Debug.Log("Server added player");
		doCrossplatformCheck();
	}

	private bool doCrossplatformCheck()
	{
		LobbyPlayer.SocialPlatform socialPlatform = LobbyPlayer.SocialPlatform.Undefined;
		NetworkLobbyPlayer[] array = lobbySlots;
		for (int i = 0; i < array.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)array[i];
			if (lobbyPlayer != null)
			{
				if (socialPlatform == LobbyPlayer.SocialPlatform.Undefined)
				{
					socialPlatform = lobbyPlayer.platform;
				}
				else if (lobbyPlayer.platform != socialPlatform)
				{
					Matchmaker.CurrentMatchmakingLobby.SetLobbyIsCrossplay(isCrossplay: true);
					return true;
				}
			}
		}
		Matchmaker.CurrentMatchmakingLobby.SetLobbyIsCrossplay(isCrossplay: false);
		return false;
	}

	public override void OnLobbyServerConnect(NetworkConnection conn)
	{
		base.OnLobbyServerConnect(conn);
		Debug.Log("Lobby server connect " + conn);
	}

	public override void OnLobbyServerDisconnect(NetworkConnection conn)
	{
		base.OnLobbyServerDisconnect(conn);
		Debug.Log("Lobby server disconnect " + conn);
	}

	private void revokePlayerAuthority(NetworkConnection conn)
	{
		if (conn.clientOwnedObjects == null)
		{
			return;
		}
		NetworkInstanceId[] array = new NetworkInstanceId[conn.clientOwnedObjects.Count];
		conn.clientOwnedObjects.CopyTo(array);
		NetworkInstanceId[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			GameObject gameObject = ClientScene.FindLocalObject(array2[i]);
			if (gameObject != null)
			{
				gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(conn);
			}
		}
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		Debug.LogWarning("OnServerDisconnect");
		Debug.Log("Server disconnect from client.");
		if (Matchmaker.Instance.CurrentLobby.IsValid())
		{
			foreach (PlayerController playerController in conn.playerControllers)
			{
				if (!playerController.IsValid)
				{
					continue;
				}
				revokePlayerAuthority(playerController.unetView.connectionToClient);
				if (!(playerController.gameObject != null))
				{
					continue;
				}
				LobbyPlayer component = playerController.gameObject.GetComponent<LobbyPlayer>();
				if (component != null)
				{
					RemovePlayerFromMPSDCustomField(component);
					RemoveLobbyPlayer(component);
					continue;
				}
				GamePlayer component2 = playerController.gameObject.GetComponent<GamePlayer>();
				if (!(component2 != null))
				{
					continue;
				}
				PlayerTracker.RemovePlayer(component2.networkNumber);
				LobbyPlayer[] array = UnityEngine.Object.FindObjectsOfType<LobbyPlayer>();
				for (int i = 0; i != array.Length; i++)
				{
					if (array[i].networkNumber == component2.networkNumber)
					{
						RemovePlayerFromMPSDCustomField(array[i]);
						LobbyManagerManager.Instance.MarkLobbyPlayerToRemove(array[i]);
					}
				}
				MsgNetworkClientDisconnected msgNetworkClientDisconnected = new MsgNetworkClientDisconnected();
				msgNetworkClientDisconnected.PlayerNetworkNumber = component2.networkNumber;
				NetworkServer.SendToAll(NetMsgTypes.NetworkClientDisconnected, msgNetworkClientDisconnected);
				GameEventManager.SendEvent(new NetworkPlayerDisconnectEvent(component2.networkNumber, component2.WasKicked));
				UnityEngine.Object.Destroy(component2.gameObject);
			}
			int playerCount = PlayerTracker.NumPlayers;
			Matchmaker.Instance.CurrentLobby.SetPlayerCount(playerCount);
			Debug.Log("Steam players set to " + playerCount);
		}
		base.OnServerDisconnect(conn);
	}

	private void RemovePlayerFromMPSDCustomField(LobbyPlayer lobbyPlayer)
	{
		if (lobbyPlayer.platform == LobbyPlayer.SocialPlatform.XboxLive)
		{
			return;
		}
		_ = (UnityMatchmaker)Matchmaker.Instance;
		if (!string.IsNullOrEmpty(lobbyPlayer.GSID))
		{
			StringBuilder stringBuilder = new StringBuilder(lobbyPlayer.GSID);
			if (!lobbyPlayer.MainUser)
			{
				stringBuilder.Append("-");
				stringBuilder.Append(lobbyPlayer.networkNumber);
			}
			Debug.Log("Player removed" + lobbyPlayer.playerName + "Host Player ID" + lobbyPlayer.GSID + "Host Player NetID" + lobbyPlayer.netid + "Host Player NetWork Number" + lobbyPlayer.networkNumber);
		}
	}

	public ConnectionQuality GetConnectionQuality()
	{
		int currentPing = CurrentPing;
		if (currentPing <= PingPoor)
		{
			if (currentPing <= PingSlow)
			{
				if (currentPing <= PingGood)
				{
					return ConnectionQuality.GREAT;
				}
				return ConnectionQuality.GOOD;
			}
			return ConnectionQuality.SLOW;
		}
		return ConnectionQuality.POOR;
	}

	public void Disconnect(string reason = null)
	{
		base.lobbyScene = null;
		if (client != null && client.connection != null)
		{
			Debug.Log("Disconnecting all local players from game");
			for (int num = client.connection.playerControllers.Count - 1; num > 0; num--)
			{
				PlayerController playerController = client.connection.playerControllers[num];
				if (playerController.IsValid && playerController.gameObject != null)
				{
					LobbyPlayer component = playerController.gameObject.GetComponent<LobbyPlayer>();
					if (component != null)
					{
						component.RemovePlayer();
						component.PlayerStatus = LobbyPlayer.Status.INACTIVE;
					}
				}
			}
		}
		GameState gameState = GameState.GetInstance();
		Character[] componentsInChildren = gameState.GetComponentsInChildren<Character>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].transform.parent = null;
		}
		Cursor[] componentsInChildren2 = gameState.GetComponentsInChildren<Cursor>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].transform.parent = null;
		}
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null)
			{
				item.PlayerCharacter = null;
				item.PlayerCursor = null;
			}
		}
		NetworkIdentity networkIdentity = null;
		if (CurrentGameController != null)
		{
			networkIdentity = CurrentGameController.GetComponent<NetworkIdentity>();
		}
		else if (CurrentLevelSelectController != null)
		{
			networkIdentity = CurrentLevelSelectController.GetComponent<NetworkIdentity>();
		}
		if (networkIdentity != null && networkIdentity.isServer)
		{
			StopHost();
			NetworkServer.DisconnectAll();
			NetworkServer.dontListen = true;
			if (client != null)
			{
				client.Disconnect();
			}
			Matchmaker.Instance.LeaveLobby(reason);
		}
		else
		{
			if (client != null)
			{
				client.Disconnect();
			}
			if (isClientDisconnected)
			{
				isClientDisconnected = false;
				Matchmaker.Instance.LeaveLobby("HostDisconnect");
			}
			else
			{
				Matchmaker.Instance.LeaveLobby();
			}
		}
	}

	private void kickPlayer(int networkNumber, KickReasons kickReason)
	{
		if (!IsInOnlineGame)
		{
			return;
		}
		Debug.LogWarning("Player kick: " + networkNumber);
		for (int i = 0; i < PlayerTracker.NumPlayers; i++)
		{
			NetworkPlayerTracker.NetPlayerInfo playerInfoByIndex = PlayerTracker.GetPlayerInfoByIndex(i);
			if (playerInfoByIndex.NetworkNumber != networkNumber)
			{
				continue;
			}
			if (IsHost)
			{
				GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfoByIndex.LobbyNetID));
				if (gameObject != null)
				{
					LobbyPlayer component = gameObject.GetComponent<LobbyPlayer>();
					component.WasKicked = true;
					if (kickReason == KickReasons.HOST || kickReason == KickReasons.VOTE)
					{
						Matchmaker.Instance.CurrentLobby.AddKickedPlayer(component.GSID);
					}
					RemovePlayerFromMPSDCustomField(component);
					RemoveLobbyPlayer(component);
				}
				GameObject gameObject2 = ClientScene.FindLocalObject(new NetworkInstanceId(playerInfoByIndex.GameNetID));
				if (gameObject2 != null)
				{
					GamePlayer component2 = gameObject2.GetComponent<GamePlayer>();
					component2.WasKicked = true;
					if (component2.IsLocalPlayer)
					{
						UnityEngine.Object.Destroy(component2.gameObject);
					}
				}
				break;
			}
			{
				foreach (Player item in PlayerManager.GetInstance())
				{
					if (item != null && item.AssociatedLobbyPlayer != null && item.AssociatedLobbyPlayer.networkNumber == networkNumber)
					{
						switch (kickReason)
						{
						case KickReasons.HOST:
							LobbyManagerManager.AbortGameInProgressGracefully(ScriptLocalization.Network.Kicked_By_host);
							break;
						case KickReasons.VOTE:
							LobbyManagerManager.AbortGameInProgressGracefully(LocalizationManager.GetTranslation("Network/Kick By Vote"));
							break;
						case KickReasons.AFK:
							LobbyManagerManager.AbortGameInProgressGracefully(LocalizationManager.GetTranslation("Network/Kicked For Inactivity"));
							break;
						case KickReasons.NONE:
							LobbyManagerManager.AbortGameInProgressGracefully(LocalizationManager.GetTranslation("Network/XB1/LostConnection"));
							break;
						}
					}
				}
				break;
			}
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.VoteToKick)
			{
				MsgVoteToKick msgVoteToKick = (MsgVoteToKick)networkMessageReceivedEvent.ReadMessage;
				LobbyPlayer lobbyPlayer = null;
				NetworkLobbyPlayer[] array = lobbySlots;
				for (int i = 0; i < array.Length; i++)
				{
					LobbyPlayer lobbyPlayer2 = (LobbyPlayer)array[i];
					if (lobbyPlayer2 != null && lobbyPlayer2.networkNumber == msgVoteToKick.NetworkPlayerToKick)
					{
						lobbyPlayer = lobbyPlayer2;
						break;
					}
				}
				if (msgVoteToKick.VoteToKick && !lobbyPlayer.IsLocalPlayer)
				{
					GameState.ChatSystem.DisplayNewMessage(new ChatMessageDetails(Character.Animals.NONE, null, GameSettings.GetInstance().SystemAlertColor, string.Format(ScriptLocalization.Network.PlayerVotedToKick, lobbyPlayer.playerName), GameSettings.GetInstance().SystemAlertColor, EmoteMeanings.CHAT_Text, 0));
				}
				if (IsHost)
				{
					if (SceneManager.GetActiveScene().name == "TreeHouseLobby")
					{
						LobbyPlayer lobbyPlayer3 = null;
						for (int j = 0; j < lobbySlots.Length; j++)
						{
							LobbyPlayer component = lobbySlots[j].GetComponent<LobbyPlayer>();
							if (component != null && component.IsLocalPlayer && component.networkNumber == msgVoteToKick.NetworkPlayerVoting)
							{
								lobbyPlayer3 = component;
								break;
							}
						}
						if (lobbyPlayer3 != null)
						{
							IssueKickMessage(msgVoteToKick.NetworkPlayerToKick, KickReasons.HOST);
						}
					}
					else
					{
						int num = kickTracker.CountVotes(msgVoteToKick.NetworkPlayerToKick);
						List<int> list = new List<int>();
						if (lobbyPlayer != null)
						{
							array = lobbySlots;
							for (int i = 0; i < array.Length; i++)
							{
								LobbyPlayer lobbyPlayer4 = (LobbyPlayer)array[i];
								if (lobbyPlayer4 != null && lobbyPlayer4.connectionToClient == lobbyPlayer.connectionToClient)
								{
									int networkNumber = lobbyPlayer4.GetComponent<LobbyPlayer>().networkNumber;
									kickTracker.SetVote(msgVoteToKick.NetworkPlayerVoting, networkNumber, msgVoteToKick.VoteToKick);
									list.Add(networkNumber);
								}
							}
						}
						int num2 = kickTracker.CountVotes(msgVoteToKick.NetworkPlayerToKick);
						if (num != num2)
						{
							if (msgVoteToKick.VoteToKick)
							{
								Debug.Log(num2 + "/" + (NumActiveConnections - 1) + " votes to kick player " + msgVoteToKick.NetworkPlayerToKick);
								if (num2 >= NumActiveConnections - 1)
								{
									Debug.Log("Enough votes received to kick player: " + msgVoteToKick.NetworkPlayerToKick);
									IssueKickMessage(msgVoteToKick.NetworkPlayerToKick, KickReasons.VOTE);
								}
							}
							else
							{
								Debug.Log("Cancelling vote to kick player " + msgVoteToKick.NetworkPlayerToKick);
							}
							foreach (int item in list)
							{
								UpdateVoteKickCountForPlayer(item, num2);
							}
						}
					}
				}
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ClientKicked)
			{
				MsgClientKicked msgClientKicked = (MsgClientKicked)networkMessageReceivedEvent.ReadMessage;
				kickPlayer(msgClientKicked.NetworkPlayerNumber, msgClientKicked.kickReason);
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.NetworkClientDisconnected)
			{
				GameEventManager.SendEvent(new GamePlayerRemovedEvent(((MsgNetworkClientDisconnected)networkMessageReceivedEvent.ReadMessage).PlayerNetworkNumber));
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SwitchToMode)
			{
				MsgSwitchToMode msgSwitchToMode = (MsgSwitchToMode)networkMessageReceivedEvent.ReadMessage;
				if (!IsHost)
				{
					GameSettings.GetInstance().GameMode = msgSwitchToMode.toMode;
				}
			}
		}
		if (type == typeof(NetworkClientCleanedUpEvent))
		{
			NetworkServer.DestroyPlayersForConnection((e as NetworkClientCleanedUpEvent).ConnectionToClient);
		}
		if (type == typeof(LocalPlayerAddedEvent))
		{
			LocalPlayerAddedEvent localPlayerAddedEvent = e as LocalPlayerAddedEvent;
			if (readyToAddPlayers)
			{
				addLobbyPlayerForLocalPlayer(localPlayerAddedEvent.NewPlayer);
			}
			else if (!localPlayerBacklog.Contains(localPlayerAddedEvent.NewPlayer))
			{
				localPlayerBacklog.Add(localPlayerAddedEvent.NewPlayer);
			}
		}
		if (type == typeof(LocalPlayerRemovedEvent))
		{
			LocalPlayerRemovedEvent localPlayerRemovedEvent = e as LocalPlayerRemovedEvent;
			Debug.Log("LocalPlayerRemovedEvent called in LobbyManager");
			if (localPlayerBacklog.Contains(localPlayerRemovedEvent.RemovedPlayer))
			{
				localPlayerBacklog.Remove(localPlayerRemovedEvent.RemovedPlayer);
			}
		}
	}

	public LobbyPlayer GetLobbyPlayer(int networkNumber)
	{
		NetworkLobbyPlayer[] array = lobbySlots;
		for (int i = 0; i < array.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)array[i];
			if (lobbyPlayer != null && lobbyPlayer.networkNumber == networkNumber)
			{
				return lobbyPlayer;
			}
		}
		return null;
	}

	public LobbyPlayer GetLobbyPlayerByGSID(string GSID)
	{
		NetworkLobbyPlayer[] array = lobbySlots;
		for (int i = 0; i < array.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)array[i];
			if (lobbyPlayer != null && lobbyPlayer.GSID == GSID)
			{
				return lobbyPlayer;
			}
		}
		return null;
	}

	private void RemovePlayerFromKickTracker(int networkNumber)
	{
		int[] array = kickTracker.VotesFromNetworkNumber(networkNumber).ToArray();
		kickTracker.ClearPlayer(networkNumber);
		UpdateVoteKickCountForPlayer(networkNumber, 0);
		int[] array2 = array;
		foreach (int num in array2)
		{
			UpdateVoteKickCountForPlayer(num, kickTracker.CountVotes(num));
		}
	}

	private void UpdateVoteKickCountForPlayer(int networkNumber, int votes)
	{
		MsgUpdateVoteKickCounts msgUpdateVoteKickCounts = new MsgUpdateVoteKickCounts();
		msgUpdateVoteKickCounts.networkNumber = networkNumber;
		msgUpdateVoteKickCounts.votes = votes;
		if (IsHost)
		{
			NetworkServer.SendToAll(NetMsgTypes.UpdateVoteKickCounts, msgUpdateVoteKickCounts);
			return;
		}
		GameEventManager.SendEvent(new NetworkMessageReceivedEvent(new NetworkMessage
		{
			msgType = NetMsgTypes.UpdateVoteKickCounts
		}, msgUpdateVoteKickCounts));
	}

	public void IssueKickMessage(int networkNumber, KickReasons reason)
	{
		MsgClientKicked msgClientKicked = new MsgClientKicked();
		msgClientKicked.NetworkPlayerNumber = networkNumber;
		msgClientKicked.kickReason = reason;
		NetworkServer.SendByChannelToAll(NetMsgTypes.ClientKicked, msgClientKicked, 0);
	}

	public float GetAveragePingToServer()
	{
		return 1f;
	}

	public void ReloadScene(GameState.GameMode toMode)
	{
		if (IsHost)
		{
			reloadingScene = true;
			ChangeGameModeOnClient(toMode);
			ServerChangeScene(GameState.GetLevelSceneName(GameState.GetInstance().SelectedLevel));
		}
	}

	public void RemoveLobbyPlayer(LobbyPlayer lobbyPl)
	{
		LobbyManagerManager.Instance.MarkLobbyPlayerToRemove(lobbyPl);
		GameEventManager.SendEvent(new NetworkPlayerDisconnectEvent(lobbyPl.networkNumber, lobbyPl.WasKicked));
		RemovePlayerFromKickTracker(lobbyPl.networkNumber);
	}

	public void DisconnectBrokenClients()
	{
		if (!IsHost || LobbyManagerManager.Instance.IsStopping)
		{
			return;
		}
		foreach (NetworkConnection connection in NetworkServer.connections)
		{
			if (connection == null || connection.playerControllers == null)
			{
				continue;
			}
			if (!connectionLifetimes.ContainsKey(connection))
			{
				connectionLifetimes.Add(connection, 0f);
				continue;
			}
			connectionLifetimes[connection] += Time.unscaledDeltaTime;
			if (connectionLifetimes[connection] < 3f)
			{
				continue;
			}
			bool flag = false;
			foreach (PlayerController playerController in connection.playerControllers)
			{
				if (playerController.gameObject != null && (playerController.gameObject.GetComponent<LobbyPlayer>() != null || (bool)playerController.gameObject.GetComponent<GamePlayer>()))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (!brokenClientConnections.ContainsKey(connection))
				{
					brokenClientConnections.Add(connection, 0f);
				}
				else
				{
					brokenClientConnections[connection] += Time.unscaledDeltaTime;
				}
			}
			else if (brokenClientConnections.ContainsKey(connection))
			{
				brokenClientConnections.Remove(connection);
			}
		}
		connectionsToKillCache.Clear();
		List<NetworkConnection> list = connectionsToKillCache;
		foreach (KeyValuePair<NetworkConnection, float> brokenClientConnection in brokenClientConnections)
		{
			if (brokenClientConnection.Value >= 0.25f)
			{
				list.Add(brokenClientConnection.Key);
			}
		}
		foreach (NetworkConnection item in list)
		{
			brokenClientConnections.Remove(item);
			connectionLifetimes.Remove(item);
			Debug.LogError("[Net] Client with connectionId " + item.connectionId + " had no valid player controller for " + 0.25f + "s -- disconnecting.");
			item.Disconnect();
		}
		connectionsToKillCache.Clear();
	}

	public int ALocalNetworkNumber()
	{
		NetworkLobbyPlayer[] array = lobbySlots;
		for (int i = 0; i < array.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)array[i];
			if (lobbyPlayer != null && lobbyPlayer.IsLocalPlayer)
			{
				return lobbyPlayer.networkNumber;
			}
		}
		return 0;
	}

	public bool IsLocalNetworkNumber(int networkNumberToCheck)
	{
		NetworkLobbyPlayer[] array = lobbySlots;
		for (int i = 0; i < array.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)array[i];
			if (lobbyPlayer != null && lobbyPlayer.networkNumber == networkNumberToCheck)
			{
				return lobbyPlayer.IsLocalPlayer;
			}
		}
		return false;
	}

	private void DebugDumpLobbyManagerInfo()
	{
		string text = "Dumping LobbyManager info:\n";
		text = text + "lobbyScene: " + base.lobbyScene + ", playScene: " + base.playScene + ", networkSceneName: " + NetworkManager.networkSceneName + ", offlineScene: " + base.offlineScene + ", onlineScene: " + base.onlineScene + "\n";
		Debug.LogError(text);
	}

	public IEnumerable<LobbyPlayer> GetLobbyPlayers()
	{
		NetworkLobbyPlayer[] array = lobbySlots;
		for (int i = 0; i < array.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)array[i];
			if (lobbyPlayer != null)
			{
				yield return lobbyPlayer;
			}
		}
	}

	public void DumpConfig()
	{
		string text = "CONFIG DUMP:\n";
		text = text + "AckDelay: " + base.connectionConfig.AckDelay + "\n";
		text = text + "AcksType: " + base.connectionConfig.AcksType.ToString() + "\n";
		text = text + "AllCostTimeout: " + base.connectionConfig.AllCostTimeout + "\n";
		text = text + "BandwidthPeakFactor: " + base.connectionConfig.BandwidthPeakFactor + "\n";
		text = text + "ChannelCount: " + base.connectionConfig.ChannelCount + "\n";
		for (int i = 0; i < base.connectionConfig.Channels.Count; i++)
		{
			ChannelQOS channelQOS = base.connectionConfig.Channels[i];
			text = text + "  Channel #" + i + ": " + channelQOS.QOS.ToString() + "\n";
		}
		text = text + "ConnectTimeout: " + base.connectionConfig.ConnectTimeout + "\n";
		text = text + "DisconnectTimeout: " + base.connectionConfig.DisconnectTimeout + "\n";
		text = text + "FragmentSize: " + base.connectionConfig.FragmentSize + "\n";
		text = text + "InitialBandwidth: " + base.connectionConfig.InitialBandwidth + "\n";
		text = text + "MaxCombinedReliableMessageCount: " + base.connectionConfig.MaxCombinedReliableMessageCount + "\n";
		text = text + "MaxCombinedReliableMessageSize: " + base.connectionConfig.MaxCombinedReliableMessageSize + "\n";
		text = text + "MaxConnectionAttempt: " + base.connectionConfig.MaxConnectionAttempt + "\n";
		text = text + "MaxSentMessageQueueSize: " + base.connectionConfig.MaxSentMessageQueueSize + "\n";
		text = text + "MinUpdateTimeout: " + base.connectionConfig.MinUpdateTimeout + "\n";
		text = text + "NetworkDropThreshold: " + base.connectionConfig.NetworkDropThreshold + "\n";
		text = text + "OverflowDropThreshold: " + base.connectionConfig.OverflowDropThreshold + "\n";
		text = text + "PacketSize: " + base.connectionConfig.PacketSize + "\n";
		text = text + "PingTimeout: " + base.connectionConfig.PingTimeout + "\n";
		text = text + "ReducedPingTimeout: " + base.connectionConfig.ReducedPingTimeout + "\n";
		text = text + "ResendTimeout: " + base.connectionConfig.ResendTimeout + "\n";
		text = text + "SendDelay: " + base.connectionConfig.SendDelay + "\n";
		text = text + "SSLCAFilePath: " + base.connectionConfig.SSLCAFilePath + "\n";
		text = text + "SSLCertFilePath: " + base.connectionConfig.SSLCertFilePath + "\n";
		text = text + "SSLPrivateKeyFilePath: " + base.connectionConfig.SSLPrivateKeyFilePath + "\n";
		text = text + "UdpSocketReceiveBufferMaxSize: " + base.connectionConfig.UdpSocketReceiveBufferMaxSize + "\n";
		text = text + "UsePlatformSpecificProtocols: " + base.connectionConfig.UsePlatformSpecificProtocols + "\n";
		text = text + "WebSocketReceiveBufferMaxSize: " + base.connectionConfig.WebSocketReceiveBufferMaxSize + "\n";
		Debug.LogWarning(text);
	}

	public ZoomCamera GetCurrentZoomCamera()
	{
		if (CurrentLevelSelectController != null)
		{
			return CurrentLevelSelectController.MainCamera;
		}
		if (CurrentGameController != null)
		{
			return CurrentGameController.MainCamera;
		}
		Debug.LogError("Could not find current zoom camera.");
		return null;
	}

	public Camera GetCurrentUICamera()
	{
		if (CurrentLevelSelectController != null)
		{
			return CurrentLevelSelectController.UICamera;
		}
		if (CurrentGameController != null)
		{
			return CurrentGameController.UICamera;
		}
		Debug.LogError("Could not find current UI camera.");
		return null;
	}
}
