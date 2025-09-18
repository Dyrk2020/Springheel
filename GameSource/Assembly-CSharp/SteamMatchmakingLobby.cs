using System;
using System.Collections.Generic;
using Steamworks;
using UCHServices;
using UnityEngine;
using UnityEngine.Events;

public class SteamMatchmakingLobby : MatchmakingLobby
{
	public CSteamID LobbyID;

	public SteamMatchmakingLobby(CSteamID lobbyID)
	{
		LobbyID = lobbyID;
	}

	public SteamMatchmakingLobby(ulong ulLobbyID)
	{
		LobbyID = new CSteamID(ulLobbyID);
	}

	protected override void setLobbyVisibility(Visibility visibility)
	{
		if (LobbyID.IsValid())
		{
			switch (visibility)
			{
			case Visibility.PUBLIC:
				SteamMatchmaking.SetLobbyType(LobbyID, ELobbyType.k_ELobbyTypePublic);
				break;
			case Visibility.FRIENDS:
				SteamMatchmaking.SetLobbyType(LobbyID, ELobbyType.k_ELobbyTypeFriendsOnly);
				break;
			case Visibility.PRIVATE:
				SteamMatchmaking.SetLobbyType(LobbyID, ELobbyType.k_ELobbyTypePrivate);
				break;
			case Visibility.INVISIBLE:
				SteamMatchmaking.SetLobbyType(LobbyID, ELobbyType.k_ELobbyTypeInvisible);
				break;
			}
		}
	}

	public override bool IsValid()
	{
		if (SteamManager.Initialized)
		{
			return LobbyID.IsValid();
		}
		return false;
	}

	public override long GetLastHeartbeat()
	{
		long result = 0L;
		if (IsValid())
		{
			long.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_lastHostHeartbeat), out result);
		}
		return result;
	}

	public override string GetLobbyDetailedScore()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_detailedLobbyScore);
		}
		return "";
	}

	public override string GetLobbyExternalIP()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_externalIP);
		}
		return "0.0.0.0";
	}

	public override GameState.GameMode GetLobbyGameMode()
	{
		if (IsValid())
		{
			try
			{
				return (GameState.GameMode)Enum.Parse(typeof(GameState.GameMode), SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_gameMode));
			}
			catch (Exception)
			{
				return GameState.GameMode.PARTY;
			}
		}
		return GameState.GameMode.PARTY;
	}

	public override string GetLobbyRulePreset()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_rulePreset);
		}
		return GameSettings.GetInstance().DefaultRuleset.Name;
	}

	public override string GetLobbyInternalIP()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_internalIP);
		}
		return "0.0.0.0";
	}

	public override int GetLobbyLimitAmount()
	{
		int result = 0;
		if (IsValid())
		{
			int.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_limitAmount), out result);
		}
		return result;
	}

	public override GameLimitType GetLobbyLimitType()
	{
		if (IsValid())
		{
			try
			{
				return (GameLimitType)Enum.Parse(typeof(GameLimitType), SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_limitType));
			}
			catch (Exception)
			{
				return GameLimitType.ROUNDS;
			}
		}
		return GameLimitType.ROUNDS;
	}

	public override string GetLobbyOwner()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_owner);
		}
		return "";
	}

	public override int GetLobbyPointLimit()
	{
		int result = 0;
		if (IsValid())
		{
			int.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_pointLimit), out result);
		}
		return result;
	}

	public override int GetLobbyPort()
	{
		int result = 17778;
		if (IsValid())
		{
			int.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_port), out result);
		}
		return result;
	}

	public override AvailableRegion GetLobbyRegion()
	{
		if (IsValid())
		{
			try
			{
				return RelayConstants.FindRegionById(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_unityLobbyRegion));
			}
			catch (Exception)
			{
				return RelayConstants.AVAILABLE_REGIONS[0];
			}
		}
		return RelayConstants.AVAILABLE_REGIONS[0];
	}

	public override int GetLobbyScore()
	{
		int result = 0;
		if (IsValid())
		{
			int.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_lobbyScore), out result);
		}
		return result;
	}

	public override string GetLobbyVersion()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_version);
		}
		return "0.0.0";
	}

	public override int GetMatchProgress()
	{
		int result = 1;
		if (IsValid())
		{
			int.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_matchProgress), out result);
		}
		return result;
	}

	public override int GetPlayerCount()
	{
		int result = -1;
		if (IsValid())
		{
			int.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_numPlayers), out result);
		}
		return result;
	}

	public override string GetPlayerSkills()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_playerSkills);
		}
		return "";
	}

	public override uint GetServerTime()
	{
		if (IsValid())
		{
			return SteamUtils.GetServerRealTime();
		}
		return 0u;
	}

	public override ulong GetUnityLobbyID()
	{
		ulong result = 0uL;
		if (IsValid())
		{
			ulong.TryParse(SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_unityLobbyID), out result);
		}
		return result;
	}

	public override bool GetUsingUnityRelay()
	{
		bool result = false;
		if (IsValid())
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_usingUnityRelay);
			if (lobbyData.NullOrEmpty())
			{
				Debug.LogWarning("usingUnityRelay on lobby was not set");
			}
			bool.TryParse(lobbyData, out result);
		}
		return result;
	}

	public override LobbyTags GetLobbyTag()
	{
		if (IsValid())
		{
			try
			{
				return (LobbyTags)Enum.Parse(typeof(LobbyTags), SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_lobbyTag));
			}
			catch (Exception)
			{
				return LobbyTags.Fun;
			}
		}
		return LobbyTags.Fun;
	}

	public override LobbyPlatform GetLobbyPlatform()
	{
		if (IsValid())
		{
			try
			{
				return (LobbyPlatform)Enum.Parse(typeof(LobbyPlatform), SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_lobbyPlatform));
			}
			catch (Exception)
			{
				return LobbyPlatform.NONE;
			}
		}
		return LobbyPlatform.NONE;
	}

	public override string GetLobbyCode()
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, MatchmakingLobby.data_lobbyCode);
		}
		return "";
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
		throw new NotImplementedException();
	}

	public override Guid GetMatchGuid()
	{
		throw new NotImplementedException();
	}

	public override bool GetLobbyIsCrossplay()
	{
		throw new NotImplementedException();
	}

	public override bool GetLobbyDisallowCrossplay()
	{
		throw new NotImplementedException();
	}

	public override bool GetLobbyJoinable()
	{
		throw new NotImplementedException();
	}

	public override bool GetHostIsAFK()
	{
		throw new NotImplementedException();
	}

	public override void SetLobbyOwner(string owner)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_owner, owner);
		}
	}

	public override void SetLastHeartbeat(int lastHeartbeat, UnityAction<bool> callback)
	{
		bool arg = false;
		if (IsValid())
		{
			arg = SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_lastHostHeartbeat, lastHeartbeat.ToString());
		}
		callback?.Invoke(arg);
	}

	public override void SetLobbyVersion(string lobbyVersion)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_version, lobbyVersion);
		}
	}

	public override void SetMatchProgress(int matchProgress)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_matchProgress, matchProgress.ToString());
		}
	}

	public override void SetPlayerCount(int playerCount)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_numPlayers, playerCount.ToString());
		}
	}

	public override void SetLobbyExternalIP(string myExternalIP)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_externalIP, myExternalIP);
		}
	}

	public override void SetLobbyInternalIP(string myInternalIP)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_internalIP, myInternalIP);
		}
	}

	public override void SetLobbyPort(int port)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_port, port.ToString());
		}
	}

	public override void SetLobbyLimitType(GameLimitType limitType)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_limitType, limitType.ToString());
		}
	}

	public override void SetLobbyLimitAmount(int limitAmount)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_limitAmount, limitAmount.ToString());
		}
	}

	public override void SetLobbyPointLimit(int points)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_pointLimit, points.ToString());
		}
	}

	public override void SetLobbyGameMode(GameState.GameMode gameMode)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_gameMode, gameMode.ToString());
		}
	}

	public override void SetLobbyRulePreset(string presetName)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_rulePreset, presetName);
		}
	}

	public override void SetLobbyJoinable(bool joinable)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyJoinable(LobbyID, joinable);
		}
	}

	public override void SetUsingUnityRelay(bool usingRelay)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_usingUnityRelay, usingRelay.ToString());
		}
	}

	public override void SetUnityLobbyID(ulong unityLobbyID)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_unityLobbyID, unityLobbyID.ToString());
		}
	}

	public override void SetLobbyRegion(AvailableRegion region)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_unityLobbyRegion, region.id);
		}
	}

	public override void SetLobbyScore(int score)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_lobbyScore, score.ToString());
		}
	}

	public override void SetLobbyDetailedScore(string detailedScore)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_detailedLobbyScore, detailedScore);
		}
	}

	public override void SetPlayerSkills(string skillString)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_playerSkills, skillString);
		}
	}

	public override void SetLobbyPlatform(LobbyPlatform platform)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_lobbyPlatform, platform.ToString());
		}
	}

	public override void SetLobbyTag(LobbyTags lobbyTag)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_lobbyTag, lobbyTag.ToString());
		}
	}

	public override void SetLobbyUsingMods(bool usingMods)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, MatchmakingLobby.data_usingMods, usingMods.ToString());
		}
	}

	public override void SetLobbyGuid(Guid guid)
	{
		throw new NotImplementedException();
	}

	public override void SetMatchGuid(Guid guid)
	{
		throw new NotImplementedException();
	}

	public override void SetLobbyIsCrossplay(bool isCrossplay)
	{
		throw new NotImplementedException();
	}

	public override void SetHostIsAFK(bool isAFK)
	{
		throw new NotImplementedException();
	}

	public override bool Equals(object obj)
	{
		if (obj is SteamMatchmakingLobby)
		{
			return ((SteamMatchmakingLobby)obj).LobbyID == LobbyID;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string GetCustomData(string dataKey)
	{
		if (IsValid())
		{
			return SteamMatchmaking.GetLobbyData(LobbyID, dataKey);
		}
		return null;
	}

	public override void SetCustomData(string key, string value)
	{
		if (IsValid())
		{
			SteamMatchmaking.SetLobbyData(LobbyID, key, value);
		}
	}

	public override void AddKickedPlayer(string kickedPlayer)
	{
		throw new NotImplementedException();
	}

	public override void SetLobbyDisallowCrossplay(bool disallowCrossplay)
	{
		throw new NotImplementedException();
	}
}
