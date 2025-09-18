using System;
using System.Collections.Generic;
using Steamworks;
using UCHServices;
using UnityEngine.Events;

public class LocalLobby : MatchmakingLobby
{
	private Guid matchGuid;

	private Guid lobbyGuid;

	public override long GetLastHeartbeat()
	{
		throw new NotImplementedException();
	}

	public override string GetLobbyDetailedScore()
	{
		throw new NotImplementedException();
	}

	public override string GetLobbyExternalIP()
	{
		throw new NotImplementedException();
	}

	public override GameState.GameMode GetLobbyGameMode()
	{
		throw new NotImplementedException();
	}

	public override string GetLobbyRulePreset()
	{
		throw new NotImplementedException();
	}

	public override string GetLobbyInternalIP()
	{
		throw new NotImplementedException();
	}

	public override int GetLobbyLimitAmount()
	{
		throw new NotImplementedException();
	}

	public override GameLimitType GetLobbyLimitType()
	{
		throw new NotImplementedException();
	}

	public override string GetLobbyOwner()
	{
		throw new NotImplementedException();
	}

	public override int GetLobbyPointLimit()
	{
		throw new NotImplementedException();
	}

	public override int GetLobbyPort()
	{
		throw new NotImplementedException();
	}

	public override AvailableRegion GetLobbyRegion()
	{
		throw new NotImplementedException();
	}

	public override int GetLobbyScore()
	{
		throw new NotImplementedException();
	}

	public override LobbyTags GetLobbyTag()
	{
		throw new NotImplementedException();
	}

	public override string GetLobbyVersion()
	{
		return GameSettings.GetInstance().MatchmakingNumber;
	}

	public override int GetMatchProgress()
	{
		throw new NotImplementedException();
	}

	public override int GetPlayerCount()
	{
		throw new NotImplementedException();
	}

	public override string GetPlayerSkills()
	{
		throw new NotImplementedException();
	}

	public override uint GetServerTime()
	{
		return SteamUtils.GetServerRealTime();
	}

	public override ulong GetUnityLobbyID()
	{
		throw new NotImplementedException();
	}

	public override bool GetUsingUnityRelay()
	{
		return false;
	}

	public override string GetLobbyCode()
	{
		throw new NotImplementedException();
	}

	public override bool GetPS4Taint()
	{
		throw new NotImplementedException();
	}

	public override bool GetPS4Hidden()
	{
		throw new NotImplementedException();
	}

	public override List<string> GetKickedPlayers()
	{
		throw new NotImplementedException();
	}

	public override Guid GetLobbyGuid()
	{
		return lobbyGuid;
	}

	public override Guid GetMatchGuid()
	{
		return matchGuid;
	}

	public override bool GetLobbyIsCrossplay()
	{
		return false;
	}

	public override bool GetLobbyDisallowCrossplay()
	{
		return false;
	}

	public override bool GetLobbyJoinable()
	{
		return false;
	}

	public override bool GetHostIsAFK()
	{
		return false;
	}

	public override bool IsValid()
	{
		return false;
	}

	public override void SetLastHeartbeat(int lastHeartbeat, UnityAction<bool> callback)
	{
	}

	public override void SetLobbyDetailedScore(string detailed)
	{
	}

	public override void SetLobbyExternalIP(string myExternalIP)
	{
	}

	public override void SetLobbyGameMode(GameState.GameMode gameMode)
	{
	}

	public override void SetLobbyRulePreset(string presetName)
	{
	}

	public override void SetLobbyInternalIP(string myInternalIP)
	{
	}

	public override void SetLobbyJoinable(bool joinable)
	{
	}

	public override void SetLobbyLimitAmount(int limitAmount)
	{
	}

	public override void SetLobbyLimitType(GameLimitType limitType)
	{
	}

	public override void SetLobbyOwner(string owner)
	{
	}

	public override void SetLobbyPointLimit(int points)
	{
	}

	public override void SetLobbyPort(int port)
	{
	}

	public override void SetLobbyRegion(AvailableRegion region)
	{
	}

	public override void SetLobbyScore(int score)
	{
	}

	public override void SetLobbyTag(LobbyTags lobbyTag)
	{
	}

	public override void SetLobbyVersion(string lobbyVersion)
	{
	}

	public override void SetMatchProgress(int matchProgress)
	{
	}

	public override void SetPlayerCount(int playerCount)
	{
	}

	public override void SetPlayerSkills(string skillString)
	{
	}

	public override void SetUnityLobbyID(ulong unityLobbyID)
	{
	}

	public override void SetUsingUnityRelay(bool usingRelay)
	{
	}

	protected override void setLobbyVisibility(Visibility visibility)
	{
	}

	public override string GetCustomData(string dataKey)
	{
		throw new NotImplementedException();
	}

	public override void SetCustomData(string key, string value)
	{
	}

	public override LobbyPlatform GetLobbyPlatform()
	{
		return LobbyPlatform.NONE;
	}

	public override void SetLobbyPlatform(LobbyPlatform platform)
	{
	}

	public override void AddKickedPlayer(string kickedPlayer)
	{
	}

	public override void SetLobbyGuid(Guid guid)
	{
		lobbyGuid = guid;
	}

	public override void SetMatchGuid(Guid guid)
	{
		matchGuid = guid;
	}

	public override void SetLobbyIsCrossplay(bool visible)
	{
	}

	public override void SetLobbyUsingMods(bool usingMods)
	{
	}

	public override void SetHostIsAFK(bool isAFK)
	{
	}

	public override void SetLobbyDisallowCrossplay(bool disallowCrossplay)
	{
	}
}
