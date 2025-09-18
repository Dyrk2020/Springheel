using System;
using GameEvent;
using UnityEngine;

public class LobbyPointCounter : MonoBehaviour, IGameEventListener
{
	protected int Setup;

	protected int Kicks;

	protected int Connection;

	protected int PlaySpeed;

	protected int EarlyQuit;

	protected int GamePoints;

	protected int Ping;

	protected int AFK;

	protected int PreviousScore;

	public int InitialScore;

	public int PerCharacterUnlock;

	public int PerLevelUnlock;

	public int PlayerKickedFromLobby;

	public int PlayerKickedFromGame;

	public int PlayerJoins;

	public int PlayerDisconnects;

	public int PlayerDisconnectsAfterGame;

	public int PlayerDisconectsDuringGame;

	public int QuickGameStart;

	public int SlowGameStart;

	public int PerMinute;

	public int MatchEndedByHost;

	public int PointsGameComplete;

	public int PointsRoundWithWinner;

	public int GreatAveragePing;

	public int SlowAveragePing;

	public int PoorAveragePing;

	public int AFKPenalty;

	public int MaxPreviousNegative;

	public float QuickStartTime = 60f;

	public float PingCheckInterval = 5f;

	private bool inLobby;

	private bool[] playerJoinedGame = new bool[4];

	private bool[] playerPlayedGame = new bool[4];

	private bool[] playerAFK = new bool[4];

	private float lobbyMinute;

	private float lastPlayerJoin;

	private float lastPingCheck;

	public int Score => Setup + Kicks + Connection + PlaySpeed + EarlyQuit + GamePoints + Ping + AFK + PreviousScore;

	public string DetailedScoreDebug => "S" + Setup + " K" + Kicks + " C" + Connection + " P" + PlaySpeed + " Q" + EarlyQuit + " C" + GamePoints + " PNG" + Ping + " AFK" + AFK + " P" + PreviousScore;

	private void Start()
	{
		inLobby = true;
		changeListeners(adding: true);
		Reset();
	}

	private void Update()
	{
		if (!inLobby)
		{
			return;
		}
		lastPlayerJoin += Time.unscaledDeltaTime;
		lobbyMinute += Time.unscaledDeltaTime;
		if (lobbyMinute >= 60f)
		{
			PlaySpeed += PerMinute;
			lobbyMinute -= 60f;
			UpdateLobbyInfo();
		}
		lastPingCheck += Time.unscaledDeltaTime;
		if (LobbyManager.instance != null && lastPingCheck >= PingCheckInterval)
		{
			int averageClientPing = LobbyManager.instance.AverageClientPing;
			if (averageClientPing < LobbyManager.PingGood)
			{
				Ping = LobbyManager.instance.CountClients() * GreatAveragePing;
			}
			else if (averageClientPing < LobbyManager.PingSlow)
			{
				Ping = LobbyManager.instance.CountClients() * SlowAveragePing;
			}
			else if (averageClientPing < LobbyManager.PingPoor)
			{
				Ping = LobbyManager.instance.CountClients() * PoorAveragePing;
			}
			else
			{
				Ping = LobbyManager.instance.CountClients() * PoorAveragePing;
			}
			lastPingCheck = 0f;
		}
	}

	private void OnDestroy()
	{
		changeListeners(adding: false);
	}

	public void Reset()
	{
		if (GameState.GetInstance().LastLobbyScore < 0 && GameState.GetInstance().LastLobbyScore > MaxPreviousNegative)
		{
			PreviousScore = GameState.GetInstance().LastLobbyScore;
		}
		else
		{
			PreviousScore = 0;
		}
		lobbyMinute = 0f;
		lastPingCheck = 0f;
		bool[] array = playerPlayedGame;
		bool[] array2 = playerPlayedGame;
		bool flag;
		playerPlayedGame[2] = (flag = (playerPlayedGame[3] = false));
		array2[1] = (flag = flag);
		array[0] = flag;
		Setup = (Kicks = (Connection = (PlaySpeed = (EarlyQuit = (GamePoints = (Ping = (AFK = 0)))))));
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			StatBoolArray stat = saveFileDataForMainUser.GetStat<StatBoolArray>("LevelsUnlocked");
			StatBoolArray stat2 = saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked");
			for (int i = 3; i < stat.values.Length; i++)
			{
				if (stat.values[i])
				{
					Setup += PerLevelUnlock;
				}
			}
			for (int j = 5; j < stat2.values.Length; j++)
			{
				if (stat2.values[j])
				{
					Setup += PerCharacterUnlock;
				}
			}
		}
		UpdateLobbyInfo();
	}

	public void UpdateLobbyInfo()
	{
		if (!(LobbyManager.instance == null))
		{
			if (LobbyManager.instance.IsInOnlineGame && LobbyManager.instance.IsHost)
			{
				GameState.GetInstance().LastLobbyScore = Score;
				MatchmakingLobby currentLobby = Matchmaker.Instance.CurrentLobby;
				currentLobby.SetLobbyScore(Score);
				currentLobby.SetLobbyDetailedScore(DetailedScoreDebug);
			}
			else
			{
				GameState.GetInstance().LastLobbyScore = 0;
			}
		}
	}

	private void changeListeners(bool adding)
	{
		GameEventManager.ChangeListener<NetworkPlayerConnectEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkPlayerDisconnectEvent>(this, adding);
		GameEventManager.ChangeListener<GameStartEvent>(this, adding);
		GameEventManager.ChangeListener<GameEndEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<RoundCompleteEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NetworkPlayerConnectEvent))
		{
			NetworkPlayerConnectEvent networkPlayerConnectEvent = e as NetworkPlayerConnectEvent;
			Connection += PlayerJoins;
			playerJoinedGame[networkPlayerConnectEvent.NetworkNumber - 1] = true;
			SetCurrentAFK(networkPlayerConnectEvent.NetworkNumber, isAFK: false);
			lastPlayerJoin = 0f;
		}
		if (type == typeof(NetworkPlayerDisconnectEvent))
		{
			NetworkPlayerDisconnectEvent networkPlayerDisconnectEvent = e as NetworkPlayerDisconnectEvent;
			if (networkPlayerDisconnectEvent.Kicked)
			{
				if (inLobby)
				{
					Kicks += PlayerKickedFromLobby;
				}
				else
				{
					Kicks += PlayerKickedFromGame;
				}
			}
			else if (inLobby && playerPlayedGame[networkPlayerDisconnectEvent.NetworkNumber - 1])
			{
				Connection += PlayerDisconnectsAfterGame;
			}
			else if (inLobby)
			{
				Connection += PlayerDisconnects;
			}
			else
			{
				Connection += PlayerDisconectsDuringGame;
			}
			playerPlayedGame[networkPlayerDisconnectEvent.NetworkNumber - 1] = false;
			playerJoinedGame[networkPlayerDisconnectEvent.NetworkNumber - 1] = false;
			SetCurrentAFK(networkPlayerDisconnectEvent.NetworkNumber, isAFK: false);
		}
		if (type == typeof(GameStartEvent))
		{
			if (lastPlayerJoin <= QuickStartTime)
			{
				PlaySpeed += QuickGameStart;
			}
			else
			{
				PlaySpeed += SlowGameStart;
			}
			for (int i = 0; i != 4; i++)
			{
				if (playerJoinedGame[i])
				{
					playerPlayedGame[i] = true;
				}
			}
			inLobby = false;
		}
		if (type == typeof(GameEndEvent))
		{
			if ((e as GameEndEvent).GameCompleted)
			{
				GamePoints += PointsGameComplete;
			}
			else
			{
				EarlyQuit += MatchEndedByHost;
			}
			inLobby = true;
		}
		if (e.GetType() == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.AFKPlayer)
			{
				return;
			}
			MsgAFKPlayer msgAFKPlayer = (MsgAFKPlayer)networkMessageReceivedEvent.ReadMessage;
			if (msgAFKPlayer.PlayerNetworkNumber == 0)
			{
				Debug.Log("AFK message coming from a player 0, this is invalid. IsAFK = " + msgAFKPlayer.isAFK);
				return;
			}
			SetCurrentAFK(msgAFKPlayer.PlayerNetworkNumber, msgAFKPlayer.isAFK);
		}
		if (type == typeof(RoundCompleteEvent) && (e as RoundCompleteEvent).PointsAwarded)
		{
			GamePoints += PointsRoundWithWinner;
		}
		UpdateLobbyInfo();
	}

	public void SetCurrentAFK(int playerNetworkNumber, bool isAFK)
	{
		playerAFK[playerNetworkNumber - 1] = isAFK;
		AFK = 0;
		for (int i = 0; i < playerAFK.Length; i++)
		{
			if (playerAFK[i])
			{
				AFK += AFKPenalty;
			}
		}
	}
}
