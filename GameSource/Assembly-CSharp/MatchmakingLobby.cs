using System;
using System.Collections.Generic;
using UCHServices;
using UnityEngine;
using UnityEngine.Events;

public abstract class MatchmakingLobby
{
	public enum Visibility
	{
		PUBLIC,
		FRIENDS,
		PRIVATE,
		INVISIBLE
	}

	public enum LobbyPlatform
	{
		ANY,
		NONE,
		STEAM,
		ANDROID,
		XBOXONE,
		PS4,
		SWITCH,
		ORIGIN
	}

	protected Visibility privacy;

	protected Visibility visibility;

	protected bool invisible;

	public static string data_owner = "owner";

	public static string data_lastHostHeartbeat = "lastHostHeartbeat";

	public static string data_numPlayers = "numPlayers";

	public static string data_version = "version";

	public static string data_matchProgress = "matchProgress";

	public static string data_externalIP = "externalIPAddress";

	public static string data_internalIP = "internalIPAddress";

	public static string data_port = "port";

	public static string data_limitType = "limitType";

	public static string data_limitAmount = "limitAmount";

	public static string data_pointLimit = "pointLimit";

	public static string data_gameMode = "gameMode";

	public static string data_rulePreset = "rulePreset";

	public static string data_usingUnityRelay = "usingUnityRelay";

	public static string data_unityLobbyID = "unityLobbyNetworkID";

	public static string data_unityLobbyRegion = "unityRelayRegion";

	public static string data_lobbyScore = "lobbyScore";

	public static string data_detailedLobbyScore = "detailedLobbyScore";

	public static string data_playerSkills = "playerSkills";

	public static string data_privacy = "privacy";

	public static string data_joinable = "joinable";

	public static string data_lobbyTag = "lobbyTag";

	public static string data_lobbyPlatform = "lobbyPlatform";

	public static string data_psnTainted = "psnTainted";

	public static string data_psnHidden = "psnHidden";

	public static string data_lobbyCode = "lobbyCode";

	public static string data_kickedPlayers = "kickedPlayers";

	public static string data_lobbyGUID = "LobbyGuid";

	public static string data_matchGUID = "MatchGuid";

	public static string data_isCrossplay = "IsCrossplay";

	public static string data_usingMods = "usingMods";

	public static string data_hostIsAFK = "hostIsAFK";

	public static string data_disallowCrossplay = "disallowCrossplay";

	public Visibility LobbyVisibility
	{
		get
		{
			return visibility;
		}
		set
		{
			if (value == Visibility.INVISIBLE)
			{
				Debug.LogWarning("Lobby visibility should not be set to invisible manually");
			}
			privacy = value;
			setLobbyVisibility(value);
		}
	}

	public static LobbyPlayer.SocialPlatform GetSocialPlatformFromLobbyPlatform(LobbyPlatform platform)
	{
		return platform switch
		{
			LobbyPlatform.ANDROID => LobbyPlayer.SocialPlatform.Android, 
			LobbyPlatform.ORIGIN => LobbyPlayer.SocialPlatform.Origin, 
			LobbyPlatform.PS4 => LobbyPlayer.SocialPlatform.PSN, 
			LobbyPlatform.STEAM => LobbyPlayer.SocialPlatform.Steam, 
			LobbyPlatform.SWITCH => LobbyPlayer.SocialPlatform.Nintendo, 
			LobbyPlatform.XBOXONE => LobbyPlayer.SocialPlatform.XboxLive, 
			_ => LobbyPlayer.SocialPlatform.Undefined, 
		};
	}

	protected abstract void setLobbyVisibility(Visibility visibility);

	public virtual void SetLobbyVisible(bool visible)
	{
		if (visible)
		{
			visibility = privacy;
		}
		else
		{
			visibility = Visibility.INVISIBLE;
		}
	}

	public virtual bool LobbyIsCrossplay(RuntimePlatform currentPlatform)
	{
		if (GetLobbyIsCrossplay())
		{
			return true;
		}
		bool flag = Application.platform == RuntimePlatform.PS4;
		bool flag2 = Application.platform == RuntimePlatform.Switch;
		bool flag3 = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.OSXPlayer;
		if (Matchmaker.CurrentMatchmakingLobby.GetPS4Hidden() || Matchmaker.CurrentMatchmakingLobby.GetPS4Taint())
		{
			if (flag3)
			{
				return true;
			}
		}
		else if (flag || flag2)
		{
			return true;
		}
		return false;
	}

	public abstract bool IsValid();

	public abstract string GetLobbyOwner();

	public abstract long GetLastHeartbeat();

	public abstract uint GetServerTime();

	public abstract int GetPlayerCount();

	public abstract string GetLobbyVersion();

	public abstract int GetMatchProgress();

	public abstract string GetLobbyExternalIP();

	public abstract string GetLobbyInternalIP();

	public abstract int GetLobbyPort();

	public abstract GameLimitType GetLobbyLimitType();

	public abstract int GetLobbyLimitAmount();

	public abstract int GetLobbyPointLimit();

	public abstract GameState.GameMode GetLobbyGameMode();

	public abstract string GetLobbyRulePreset();

	public abstract bool GetUsingUnityRelay();

	public abstract ulong GetUnityLobbyID();

	public abstract AvailableRegion GetLobbyRegion();

	public abstract int GetLobbyScore();

	public abstract string GetLobbyDetailedScore();

	public abstract string GetPlayerSkills();

	public abstract LobbyTags GetLobbyTag();

	public abstract LobbyPlatform GetLobbyPlatform();

	public abstract string GetLobbyCode();

	public abstract string GetCustomData(string dataKey);

	public abstract bool GetPS4Taint();

	public abstract bool GetPS4Hidden();

	public abstract List<string> GetKickedPlayers();

	public abstract Guid GetLobbyGuid();

	public abstract Guid GetMatchGuid();

	public abstract bool GetHostIsAFK();

	public abstract bool GetLobbyIsCrossplay();

	public abstract bool GetLobbyDisallowCrossplay();

	public abstract bool GetLobbyJoinable();

	public abstract void SetLobbyOwner(string owner);

	public abstract void SetLastHeartbeat(int lastHeartbeat, UnityAction<bool> callback);

	public abstract void SetPlayerCount(int playerCount);

	public abstract void SetLobbyVersion(string lobbyVersion);

	public abstract void SetMatchProgress(int matchProgress);

	public abstract void SetLobbyExternalIP(string myExternalIP);

	public abstract void SetLobbyInternalIP(string myInternalIP);

	public abstract void SetLobbyPort(int port);

	public abstract void SetLobbyLimitType(GameLimitType limitType);

	public abstract void SetLobbyLimitAmount(int limitAmount);

	public abstract void SetLobbyPointLimit(int points);

	public abstract void SetLobbyGameMode(GameState.GameMode gameMode);

	public abstract void SetLobbyRulePreset(string presetName);

	public abstract void SetLobbyJoinable(bool joinable);

	public abstract void SetUsingUnityRelay(bool usingRelay);

	public abstract void SetUnityLobbyID(ulong unityLobbyID);

	public abstract void SetLobbyRegion(AvailableRegion region);

	public abstract void SetLobbyScore(int score);

	public abstract void SetLobbyDetailedScore(string detailed);

	public abstract void SetPlayerSkills(string skillString);

	public abstract void SetLobbyTag(LobbyTags lobbyTag);

	public abstract void SetLobbyPlatform(LobbyPlatform platform);

	public abstract void SetCustomData(string key, string value);

	public abstract void AddKickedPlayer(string kickedPlayer);

	public abstract void SetLobbyGuid(Guid guid);

	public abstract void SetMatchGuid(Guid guid);

	public abstract void SetLobbyIsCrossplay(bool isCrossplay);

	public abstract void SetLobbyUsingMods(bool usingMods);

	public abstract void SetHostIsAFK(bool isAFK);

	public abstract void SetLobbyDisallowCrossplay(bool disallowCrossplay);
}
