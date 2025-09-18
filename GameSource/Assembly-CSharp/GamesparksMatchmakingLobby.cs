using System;
using System.Collections.Generic;
using GameSparks.Api.Responses;
using GameSparks.Core;
using UCHServices;
using UnityEngine;
using UnityEngine.Events;

public class GamesparksMatchmakingLobby : MatchmakingLobby
{
	public enum DisallowCrossplayState
	{
		Uninitialized,
		True,
		False
	}

	public string MatchID;

	private GSRequestData lobbyData;

	private GSRequestData lobbyDataDelta;

	private bool isOwner;

	private bool gettingData;

	private bool settingData;

	private float serverTimeDifference;

	private int lastSentLobbyScore = -1;

	private string lastSentDetailedScore = "";

	private bool psnTainted;

	private bool psnHidden;

	private DisallowCrossplayState disallowCrossplay;

	public bool usingMods;

	public static string data_socialLobbyID = "socialLobbyID";

	public static string data_socialLobbyType = "socialLobbyType";

	public bool DataChangedSinceLastSync { get; protected set; }

	public bool PSNTainted
	{
		get
		{
			return psnTainted;
		}
		set
		{
			if (!isOwner)
			{
				Debug.LogWarning("Can't set lobby data as client");
			}
			else if (psnTainted != value)
			{
				psnTainted = value;
				lobbyDataDelta.AddString(MatchmakingLobby.data_psnTainted, value ? "1" : "0");
				DataChangedSinceLastSync = true;
			}
		}
	}

	public bool PSNHidden
	{
		get
		{
			return psnHidden;
		}
		set
		{
			if (!isOwner)
			{
				Debug.LogWarning("Can't set lobby data as client");
			}
			else if (psnHidden != value)
			{
				psnHidden = value;
				lobbyDataDelta.AddString(MatchmakingLobby.data_psnHidden, value ? "1" : "0");
				DataChangedSinceLastSync = true;
			}
		}
	}

	public bool DisallowCrossplay
	{
		get
		{
			return GetLobbyDisallowCrossplay();
		}
		set
		{
			if (!isOwner)
			{
				Debug.LogWarning("Can't set lobby data as client");
				return;
			}
			switch (disallowCrossplay)
			{
			case DisallowCrossplayState.Uninitialized:
			case DisallowCrossplayState.False:
				if (value)
				{
					SetLobbyDisallowCrossplay(value);
				}
				break;
			case DisallowCrossplayState.True:
				if (!value)
				{
					SetLobbyDisallowCrossplay(value);
				}
				break;
			}
		}
	}

	public bool IsOwner => isOwner;

	public static GamesparksMatchmakingLobby CreateNewLobby(string matchId, bool isOwner)
	{
		return new GamesparksMatchmakingLobby(matchId, isOwner);
	}

	public static GamesparksMatchmakingLobby CreateNewLobby(GSData lobbyData, bool isOwner)
	{
		return new GamesparksMatchmakingLobby(lobbyData, isOwner);
	}

	protected GamesparksMatchmakingLobby(string matchID, bool isOwner)
	{
		MatchID = matchID;
		lobbyData = new GSRequestData();
		lobbyDataDelta = new GSRequestData();
		this.isOwner = isOwner;
	}

	protected GamesparksMatchmakingLobby(GSData lobbyData, bool isOwner)
	{
		MatchID = lobbyData.GetString("ownerID");
		this.lobbyData = new GSRequestData(lobbyData);
		lobbyDataDelta = new GSRequestData();
		this.isOwner = isOwner;
	}

	public void SendLobbyData(UnityAction<bool> callback)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Cannot set lobby data as the client");
			if (callback != null)
			{
				callback(arg0: false);
			}
		}
		else
		{
			if (!IsValid() || settingData)
			{
				return;
			}
			settingData = true;
			string privacyString = null;
			if (lobbyDataDelta.ContainsKey("privacy"))
			{
				privacyString = lobbyDataDelta.GetString("privacy");
			}
			GameSparksManager.Instance.CreateQuery().SetLobbyData(MatchID, lobbyDataDelta, delegate(LogEventResponse r)
			{
				settingData = false;
				if (r.HasErrors)
				{
					Debug.LogError("Problem sending lobby data update to backend: " + r.Errors.JSON);
					Debug.Log("Trying again next time lobby data is sent");
					if (r.ScriptData != null && r.ScriptData.ContainsKey("matchDelta"))
					{
						GSData gSData = r.ScriptData.GetGSData("matchDelta");
						{
							foreach (string key in gSData.BaseData.Keys)
							{
								if (!lobbyDataDelta.ContainsKey(key))
								{
									lobbyDataDelta.Add(key, gSData.BaseData[key]);
								}
							}
							return;
						}
					}
					if (callback != null)
					{
						callback(arg0: true);
					}
				}
				else
				{
					if (r.ScriptData.ContainsKey("match"))
					{
						lobbyData = new GSRequestData(r.ScriptData.GetGSData("match"));
					}
					if (r.ScriptData.ContainsKey("time"))
					{
						long valueOrDefault = r.ScriptData.GetLong("time").GetValueOrDefault();
						if (valueOrDefault > 0)
						{
							serverTimeDifference = (float)valueOrDefault / 1000f - Time.realtimeSinceStartup;
						}
					}
					if (privacyString != null)
					{
						try
						{
							Visibility visibility = (Visibility)Enum.Parse(typeof(Visibility), privacyString);
							Matchmaker.Instance.OnLobbyPrivacyChanged(visibility);
						}
						catch (Exception)
						{
						}
					}
					if (callback != null)
					{
						callback(arg0: true);
					}
				}
			});
			DataChangedSinceLastSync = false;
			lobbyDataDelta = new GSRequestData();
		}
	}

	public void GetLobbyData(UnityAction<bool> callback)
	{
		if (!IsValid() || gettingData)
		{
			return;
		}
		gettingData = true;
		GameSparksManager.Instance.CreateQuery().GetLobbyData(MatchID, useCode: false, delegate(LogEventResponse r)
		{
			gettingData = false;
			if (r.HasErrors)
			{
				Debug.LogError("Problem getting lobby data from backend: " + r.Errors.JSON);
				if (callback != null)
				{
					callback(arg0: false);
				}
			}
			else
			{
				if (r.ScriptData.ContainsKey("match"))
				{
					lobbyData = new GSRequestData(r.ScriptData.GetGSData("match"));
					if (!settingData)
					{
						DataChangedSinceLastSync = false;
					}
				}
				if (r.ScriptData.ContainsKey("time"))
				{
					long valueOrDefault = r.ScriptData.GetLong("time").GetValueOrDefault();
					if (valueOrDefault > 0)
					{
						serverTimeDifference = (float)valueOrDefault / 1000f - Time.realtimeSinceStartup;
					}
				}
				if (callback != null)
				{
					callback(arg0: true);
				}
			}
		});
	}

	public override bool IsValid()
	{
		return !MatchID.NullOrEmpty();
	}

	public override long GetLastHeartbeat()
	{
		long result = 0L;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_lastHostHeartbeat)) ? lobbyData.GetLong(MatchmakingLobby.data_lastHostHeartbeat).GetValueOrDefault() : lobbyDataDelta.GetLong(MatchmakingLobby.data_lastHostHeartbeat).GetValueOrDefault());
		}
		return result;
	}

	public override string GetLobbyDetailedScore()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_detailedLobbyScore))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_detailedLobbyScore);
			}
			return lobbyData.GetString(MatchmakingLobby.data_detailedLobbyScore);
		}
		return "";
	}

	public override string GetLobbyExternalIP()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_externalIP))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_externalIP);
			}
			return lobbyData.GetString(MatchmakingLobby.data_externalIP);
		}
		return "0.0.0.0";
	}

	public override GameState.GameMode GetLobbyGameMode()
	{
		if (IsValid())
		{
			try
			{
				return (!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_gameMode)) ? ((GameState.GameMode)Enum.Parse(typeof(GameState.GameMode), lobbyData.GetString(MatchmakingLobby.data_gameMode))) : ((GameState.GameMode)Enum.Parse(typeof(GameState.GameMode), lobbyDataDelta.GetString(MatchmakingLobby.data_gameMode)));
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
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_rulePreset))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_rulePreset);
			}
			return lobbyData.GetString(MatchmakingLobby.data_rulePreset);
		}
		return GameSettings.GetInstance().DefaultRuleset.Name;
	}

	public override string GetLobbyInternalIP()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_internalIP))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_internalIP);
			}
			return lobbyData.GetString(MatchmakingLobby.data_internalIP);
		}
		return "0.0.0.0";
	}

	public override int GetLobbyLimitAmount()
	{
		int result = 0;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_limitAmount)) ? lobbyData.GetInt(MatchmakingLobby.data_limitAmount).GetValueOrDefault() : lobbyDataDelta.GetInt(MatchmakingLobby.data_limitAmount).GetValueOrDefault());
		}
		return result;
	}

	public override GameLimitType GetLobbyLimitType()
	{
		if (IsValid())
		{
			try
			{
				return (!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_limitType)) ? ((GameLimitType)Enum.Parse(typeof(GameLimitType), lobbyData.GetString(MatchmakingLobby.data_limitType))) : ((GameLimitType)Enum.Parse(typeof(GameLimitType), lobbyDataDelta.GetString(MatchmakingLobby.data_limitType)));
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
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_owner))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_owner);
			}
			return lobbyData.GetString(MatchmakingLobby.data_owner);
		}
		return "";
	}

	public override int GetLobbyPointLimit()
	{
		return 0;
	}

	public override int GetLobbyPort()
	{
		int result = 17778;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_port)) ? lobbyData.GetInt(MatchmakingLobby.data_port).GetValueOrDefault() : lobbyDataDelta.GetInt(MatchmakingLobby.data_port).GetValueOrDefault());
		}
		return result;
	}

	public override AvailableRegion GetLobbyRegion()
	{
		if (IsValid())
		{
			try
			{
				string regionId = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_unityLobbyRegion)) ? lobbyData.GetString(MatchmakingLobby.data_unityLobbyRegion) : lobbyDataDelta.GetString(MatchmakingLobby.data_unityLobbyRegion));
				return RelayConstants.FindRegionById(regionId);
			}
			catch (Exception)
			{
				return RelayConstants.AVAILABLE_REGIONS[0];
			}
		}
		return RelayConstants.AVAILABLE_REGIONS[0];
	}

	public override bool GetPS4Taint()
	{
		if (IsValid())
		{
			try
			{
				int num = 0;
				num = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_psnTainted)) ? int.Parse(lobbyData.GetString(MatchmakingLobby.data_psnTainted)) : int.Parse(lobbyDataDelta.GetString(MatchmakingLobby.data_psnTainted)));
				return (num != 0) ? true : false;
			}
			catch (Exception)
			{
				return false;
			}
		}
		return false;
	}

	public override bool GetPS4Hidden()
	{
		if (IsValid())
		{
			try
			{
				int num = 0;
				num = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_psnHidden)) ? int.Parse(lobbyData.GetString(MatchmakingLobby.data_psnHidden)) : int.Parse(lobbyDataDelta.GetString(MatchmakingLobby.data_psnHidden)));
				return (num != 0) ? true : false;
			}
			catch (Exception)
			{
				return false;
			}
		}
		return false;
	}

	public override int GetLobbyScore()
	{
		return 0;
	}

	public override string GetLobbyVersion()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_version))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_version);
			}
			return lobbyData.GetString(MatchmakingLobby.data_version);
		}
		return "0.0.0";
	}

	public override int GetMatchProgress()
	{
		int result = 1;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_matchProgress)) ? lobbyData.GetInt(MatchmakingLobby.data_matchProgress).GetValueOrDefault() : lobbyDataDelta.GetInt(MatchmakingLobby.data_matchProgress).GetValueOrDefault());
		}
		return result;
	}

	public override int GetPlayerCount()
	{
		int result = -1;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_numPlayers)) ? lobbyData.GetInt(MatchmakingLobby.data_numPlayers).GetValueOrDefault() : lobbyDataDelta.GetInt(MatchmakingLobby.data_numPlayers).GetValueOrDefault());
		}
		return result;
	}

	public override string GetPlayerSkills()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_playerSkills))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_playerSkills);
			}
			return lobbyData.GetString(MatchmakingLobby.data_playerSkills);
		}
		return "";
	}

	public override uint GetServerTime()
	{
		return (uint)(serverTimeDifference + Time.realtimeSinceStartup);
	}

	public override ulong GetUnityLobbyID()
	{
		ulong result = 0uL;
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_unityLobbyID))
			{
				ulong.TryParse(lobbyDataDelta.GetString(MatchmakingLobby.data_unityLobbyID), out result);
			}
			else
			{
				ulong.TryParse(lobbyData.GetString(MatchmakingLobby.data_unityLobbyID), out result);
			}
		}
		return result;
	}

	public override bool GetUsingUnityRelay()
	{
		bool result = false;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_usingUnityRelay)) ? (lobbyData.GetBoolean(MatchmakingLobby.data_usingUnityRelay) == true) : (lobbyDataDelta.GetBoolean(MatchmakingLobby.data_usingUnityRelay) == true));
		}
		return result;
	}

	public override LobbyTags GetLobbyTag()
	{
		if (IsValid())
		{
			try
			{
				return (!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_lobbyTag)) ? ((LobbyTags)Enum.Parse(typeof(LobbyTags), lobbyData.GetString(MatchmakingLobby.data_lobbyTag))) : ((LobbyTags)Enum.Parse(typeof(LobbyTags), lobbyDataDelta.GetString(MatchmakingLobby.data_lobbyTag)));
			}
			catch (Exception)
			{
				return LobbyTags.Fun;
			}
		}
		return LobbyTags.Fun;
	}

	public string GetSocialLobbyType()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(data_socialLobbyType))
			{
				return lobbyDataDelta.GetString(data_socialLobbyType);
			}
			return lobbyData.GetString(data_socialLobbyType);
		}
		return "";
	}

	public string GetSocialLobbyID()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(data_socialLobbyID))
			{
				return lobbyDataDelta.GetString(data_socialLobbyID);
			}
			return lobbyData.GetString(data_socialLobbyID);
		}
		return "";
	}

	public override LobbyPlatform GetLobbyPlatform()
	{
		if (IsValid())
		{
			try
			{
				return (!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_unityLobbyRegion)) ? ((LobbyPlatform)Enum.Parse(typeof(LobbyPlatform), lobbyData.GetString(MatchmakingLobby.data_lobbyPlatform))) : ((LobbyPlatform)Enum.Parse(typeof(LobbyPlatform), lobbyDataDelta.GetString(MatchmakingLobby.data_lobbyPlatform)));
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
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_lobbyCode))
			{
				return lobbyDataDelta.GetString(MatchmakingLobby.data_lobbyCode);
			}
			return lobbyData.GetString(MatchmakingLobby.data_lobbyCode);
		}
		return "";
	}

	public override List<string> GetKickedPlayers()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_kickedPlayers))
			{
				return lobbyDataDelta.GetStringList(MatchmakingLobby.data_kickedPlayers);
			}
			if (lobbyData.ContainsKey(MatchmakingLobby.data_kickedPlayers))
			{
				return lobbyData.GetStringList(MatchmakingLobby.data_kickedPlayers);
			}
		}
		return new List<string>();
	}

	public override Guid GetLobbyGuid()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_lobbyGUID))
			{
				return new Guid(lobbyDataDelta.GetString(MatchmakingLobby.data_lobbyGUID));
			}
			if (lobbyData.ContainsKey(MatchmakingLobby.data_lobbyGUID))
			{
				return new Guid(lobbyData.GetString(MatchmakingLobby.data_lobbyGUID));
			}
		}
		return default(Guid);
	}

	public override Guid GetMatchGuid()
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_matchGUID))
			{
				return new Guid(lobbyDataDelta.GetString(MatchmakingLobby.data_matchGUID));
			}
			if (lobbyData.ContainsKey(MatchmakingLobby.data_matchGUID))
			{
				return new Guid(lobbyData.GetString(MatchmakingLobby.data_matchGUID));
			}
		}
		return default(Guid);
	}

	public override bool GetLobbyIsCrossplay()
	{
		bool result = false;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_isCrossplay)) ? (lobbyData.GetBoolean(MatchmakingLobby.data_isCrossplay) == true) : (lobbyDataDelta.GetBoolean(MatchmakingLobby.data_isCrossplay) == true));
		}
		return result;
	}

	public override bool GetLobbyJoinable()
	{
		if (!IsValid())
		{
			return false;
		}
		if (lobbyDataDelta.ContainsKey(MatchmakingLobby.data_joinable))
		{
			return lobbyDataDelta.GetBoolean(MatchmakingLobby.data_joinable) == true;
		}
		return lobbyData.GetBoolean(MatchmakingLobby.data_joinable) == true;
	}

	public override bool GetHostIsAFK()
	{
		bool result = false;
		if (IsValid())
		{
			result = ((!lobbyDataDelta.ContainsKey(MatchmakingLobby.data_hostIsAFK)) ? (lobbyData.GetBoolean(MatchmakingLobby.data_hostIsAFK) == true) : (lobbyDataDelta.GetBoolean(MatchmakingLobby.data_hostIsAFK) == true));
		}
		return result;
	}

	public override void SetLastHeartbeat(int lastHeartbeat, UnityAction<bool> callback)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
		gameSparksQuery.SetLobbyHeartbeat();
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
		{
			if (callback != null)
			{
				callback(!q.HasError);
			}
		});
	}

	public override void SetLobbyDetailedScore(string detailed)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
		}
		else if (lastSentDetailedScore != detailed)
		{
			lobbyDataDelta.AddString(MatchmakingLobby.data_detailedLobbyScore, detailed);
			DataChangedSinceLastSync = true;
			lastSentDetailedScore = detailed;
		}
	}

	public override void SetLobbyExternalIP(string myExternalIP)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_externalIP, myExternalIP);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyGameMode(GameState.GameMode gameMode)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_gameMode, gameMode.ToString());
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyRulePreset(string presetName)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_rulePreset, presetName);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyInternalIP(string myInternalIP)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_internalIP, myInternalIP);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyJoinable(bool joinable)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddBoolean(MatchmakingLobby.data_joinable, joinable);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyUsingMods(bool usingMods)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		this.usingMods = usingMods;
		lobbyDataDelta.AddBoolean(MatchmakingLobby.data_usingMods, usingMods);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyLimitAmount(int limitAmount)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddNumber(MatchmakingLobby.data_limitAmount, limitAmount);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyLimitType(GameLimitType limitType)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_limitType, limitType.ToString());
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyOwner(string owner)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_owner, owner);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyPointLimit(int points)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddNumber(MatchmakingLobby.data_pointLimit, points);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyPort(int port)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddNumber(MatchmakingLobby.data_port, port);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyRegion(AvailableRegion region)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_unityLobbyRegion, region.id);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyScore(int score)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
		}
		else if (lastSentLobbyScore != score)
		{
			lobbyDataDelta.AddNumber(MatchmakingLobby.data_lobbyScore, score);
			DataChangedSinceLastSync = true;
			lastSentLobbyScore = score;
		}
	}

	public override void SetLobbyVersion(string lobbyVersion)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_version, lobbyVersion);
		DataChangedSinceLastSync = true;
	}

	public override void SetMatchProgress(int matchProgress)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddNumber(MatchmakingLobby.data_matchProgress, matchProgress);
		DataChangedSinceLastSync = true;
	}

	public override void SetPlayerCount(int playerCount)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		Debug.LogWarning("Setting player count to " + playerCount);
		lobbyDataDelta.AddNumber(MatchmakingLobby.data_numPlayers, playerCount);
		DataChangedSinceLastSync = true;
		DiscordListener.UpdatePresencePlayers(playerCount);
	}

	public override void SetPlayerSkills(string skillString)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_playerSkills, skillString);
		DataChangedSinceLastSync = true;
	}

	public override void SetUnityLobbyID(ulong unityLobbyID)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_unityLobbyID, unityLobbyID.ToString());
		DataChangedSinceLastSync = true;
	}

	public override void SetUsingUnityRelay(bool usingRelay)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddBoolean(MatchmakingLobby.data_usingUnityRelay, usingRelay);
		DataChangedSinceLastSync = true;
	}

	protected override void setLobbyVisibility(Visibility visibility)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_privacy, visibility.ToString());
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyTag(LobbyTags lobbyTag)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		DiscordListener.UpdateLobbyTag(lobbyTag);
		lobbyDataDelta.AddString(MatchmakingLobby.data_lobbyTag, lobbyTag.ToString());
		DataChangedSinceLastSync = true;
	}

	public void SetSocialLobby(string lobbyID, string lobbyType)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(data_socialLobbyID, lobbyID);
		lobbyDataDelta.AddString(data_socialLobbyType, lobbyType);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyPlatform(LobbyPlatform platform)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_lobbyPlatform, platform.ToString());
		DataChangedSinceLastSync = true;
	}

	public override void AddKickedPlayer(string kickedPlayer)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		List<string> list = new List<string>();
		if (!kickedPlayer.NullOrEmpty())
		{
			list.Add(kickedPlayer);
		}
		List<string> list2 = new List<string>();
		list2.AddRange(GetKickedPlayers());
		if (list2.Count > 0)
		{
			list.AddRange(GetKickedPlayers());
		}
		lobbyDataDelta.AddStringList(MatchmakingLobby.data_kickedPlayers, list);
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyGuid(Guid guid)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_lobbyGUID, guid.ToString());
		DataChangedSinceLastSync = true;
	}

	public override void SetMatchGuid(Guid guid)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(MatchmakingLobby.data_matchGUID, guid.ToString());
		DataChangedSinceLastSync = true;
	}

	public override void SetLobbyIsCrossplay(bool isCrossplay)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddBoolean(MatchmakingLobby.data_isCrossplay, isCrossplay);
		DataChangedSinceLastSync = true;
	}

	public override void SetHostIsAFK(bool isAFK)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
		}
		else if (lobbyData.GetBoolean(MatchmakingLobby.data_hostIsAFK) != isAFK)
		{
			lobbyDataDelta.AddBoolean(MatchmakingLobby.data_hostIsAFK, isAFK);
			DataChangedSinceLastSync = true;
		}
	}

	public void SetLobbyCode(string lobby)
	{
		lobbyDataDelta.AddString(MatchmakingLobby.data_lobbyCode, lobby);
		lobbyData.AddString(MatchmakingLobby.data_lobbyCode, lobby);
	}

	public static Matchmaker.LobbyListInfo CreateListEntryFromGSData(GSData data)
	{
		Matchmaker.LobbyListInfo lobbyListInfo = new Matchmaker.LobbyListInfo();
		lobbyListInfo.sLobbyID = data.GetString("ownerID");
		lobbyListInfo.LobbyOwner = data.GetString(MatchmakingLobby.data_owner);
		lobbyListInfo.InfoReceived = true;
		lobbyListInfo.UnityMatchID = (ulong)data.GetLong(MatchmakingLobby.data_unityLobbyID).GetValueOrDefault();
		lobbyListInfo.Players = data.GetInt(MatchmakingLobby.data_numPlayers) ?? 1;
		lobbyListInfo.matchProgress = data.GetInt(MatchmakingLobby.data_matchProgress) ?? 1;
		long valueOrDefault = data.GetLong(MatchmakingLobby.data_lastHostHeartbeat).GetValueOrDefault();
		lobbyListInfo.lastHearbeatTime = (uint)(valueOrDefault / 1000);
		lobbyListInfo.limitAmount = data.GetInt(MatchmakingLobby.data_limitAmount).GetValueOrDefault();
		lobbyListInfo.pointLimit = data.GetInt(MatchmakingLobby.data_pointLimit).GetValueOrDefault();
		lobbyListInfo.LobbyHealthNum = data.GetInt(MatchmakingLobby.data_lobbyScore).GetValueOrDefault();
		lobbyListInfo.error = false;
		string text = data.GetString(MatchmakingLobby.data_unityLobbyRegion);
		if (!text.NullOrEmpty())
		{
			try
			{
				lobbyListInfo.UnityServerRegion = RelayConstants.FindRegionById(text);
			}
			catch (Exception)
			{
				Debug.LogWarning("Problem parsing server region for lobby " + lobbyListInfo.sLobbyID);
				lobbyListInfo.UnityServerRegion = RelayConstants.AVAILABLE_REGIONS[0];
			}
		}
		string text2 = data.GetString(MatchmakingLobby.data_limitType);
		if (!text2.NullOrEmpty())
		{
			try
			{
				lobbyListInfo.limitType = (GameLimitType)Enum.Parse(typeof(GameLimitType), text2);
			}
			catch (Exception)
			{
				Debug.LogWarning("Problem parsing server region for lobby " + lobbyListInfo.sLobbyID);
				lobbyListInfo.limitType = GameLimitType.ROUNDS;
			}
		}
		string text3 = data.GetString(MatchmakingLobby.data_gameMode);
		if (!text3.NullOrEmpty())
		{
			try
			{
				lobbyListInfo.gameMode = (GameState.GameMode)Enum.Parse(typeof(GameState.GameMode), text3);
			}
			catch (Exception)
			{
				Debug.LogWarning("Problem parsing server region for lobby " + lobbyListInfo.sLobbyID);
				lobbyListInfo.gameMode = GameState.GameMode.PARTY;
			}
		}
		lobbyListInfo.rulePreset = data.GetString(MatchmakingLobby.data_rulePreset);
		string text4 = data.GetString(MatchmakingLobby.data_lobbyTag);
		if (!text4.NullOrEmpty())
		{
			try
			{
				lobbyListInfo.lobbyTag = (LobbyTags)Enum.Parse(typeof(LobbyTags), text4);
			}
			catch (Exception)
			{
				Debug.LogWarning("Problem parsing lobbytag" + lobbyListInfo.sLobbyID);
				lobbyListInfo.lobbyTag = LobbyTags.Fun;
			}
		}
		lobbyListInfo.usingMods = data.GetBoolean("usingMods") == true;
		lobbyListInfo.DebugString = data.GetString(MatchmakingLobby.data_detailedLobbyScore);
		lobbyListInfo.PlayerSkills = data.GetString(MatchmakingLobby.data_playerSkills);
		GSData gSData = data.GetGSData("hostPlatformIds");
		if (gSData != null)
		{
			lobbyListInfo.hostPlatform = UGCNameTag.GetPlatformFromGSData(gSData);
			lobbyListInfo.hostPlatformId = UGCNameTag.GetPlatformIDFromGSData(gSData);
		}
		lobbyListInfo.hostGSID = data.GetString("ownerID");
		lobbyListInfo.isAFK = data.GetBoolean(MatchmakingLobby.data_hostIsAFK) == true;
		return lobbyListInfo;
	}

	public override string GetCustomData(string dataKey)
	{
		if (IsValid())
		{
			if (lobbyDataDelta.ContainsKey(dataKey))
			{
				return lobbyDataDelta.GetString(dataKey);
			}
			return lobbyData.GetString(dataKey);
		}
		return "";
	}

	public override void SetCustomData(string key, string value)
	{
		if (!isOwner)
		{
			Debug.LogWarning("Can't set lobby data as client");
			return;
		}
		lobbyDataDelta.AddString(key, value);
		DataChangedSinceLastSync = true;
	}

	public override bool GetLobbyDisallowCrossplay()
	{
		return lobbyData.GetString(MatchmakingLobby.data_disallowCrossplay) == "1";
	}

	public override void SetLobbyDisallowCrossplay(bool disallowCrossplay)
	{
		this.disallowCrossplay = (disallowCrossplay ? DisallowCrossplayState.True : DisallowCrossplayState.False);
		lobbyDataDelta.AddString(MatchmakingLobby.data_disallowCrossplay, disallowCrossplay ? "1" : "0");
		DataChangedSinceLastSync = true;
	}
}
