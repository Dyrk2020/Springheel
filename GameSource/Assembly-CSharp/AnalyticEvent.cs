using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnalyticEvent
{
	public enum JoinMethod
	{
		LIST,
		CODE,
		INVITE,
		MANUAL,
		DISCORD
	}

	public enum SocialLink
	{
		Announcement,
		Update,
		FunReport,
		BugReport,
		Twitter,
		Discord,
		Reddit,
		Shop,
		Twitch
	}

	public enum ShareSite
	{
		Twitter,
		Reddit
	}

	private static string version = GameSettings.GetInstance().VersionNumber;

	private static int count;

	protected static void sendEvent(string key, Dictionary<string, object> data)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			data?.Add("version", version);
			Debug.LogWarning("Event " + key + ": " + ++count);
			AnalyticsWrapper.CustomEvent(key, data);
		}
	}

	protected static void sendBatchedEvent(string key, Dictionary<string, object> data, string idKey, object idValue)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { idKey, idValue } };
		bool flag = false;
		foreach (string key2 in data.Keys)
		{
			dictionary.Add(key2, data[key2]);
			flag = false;
			if (dictionary.Count == 9)
			{
				sendEvent(key, dictionary);
				flag = true;
				dictionary = new Dictionary<string, object> { { idKey, idValue } };
			}
		}
		if (!flag)
		{
			sendEvent(key, dictionary);
		}
	}

	public static void JoinMatchEvent(Guid lobbyGuid, JoinMethod joinedBy, bool crossPlatform)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("JoinMatch", new Dictionary<string, object>
			{
				{
					"LobbyGuid",
					lobbyGuid.ToString()
				},
				{ "JoinedBy", joinedBy },
				{ "CrossPlatform", crossPlatform }
			});
		}
	}

	public static void MatchStartHostEvent(Guid matchGuid, bool online, GameState.LevelName level, GameState.GameMode gameMode, int numPlayers, string levelCode, LobbyTags lobbyTag, bool twitchIntegration)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("MatchStart_Host", new Dictionary<string, object>
			{
				{
					"MatchGuid",
					matchGuid.ToString()
				},
				{ "Online", online },
				{
					"Level",
					level.ToString()
				},
				{
					"GameMode",
					gameMode.ToString()
				},
				{ "NumPlayers", numPlayers },
				{ "LevelCode", levelCode },
				{
					"LobbyTag",
					lobbyTag.ToString()
				},
				{ "Twitch", twitchIntegration }
			});
		}
	}

	public static void MatchStartClientEvent(Guid matchGuid, int localPlayers, float globalCamTime, float localCamTime)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("MatchStart_Client", new Dictionary<string, object>
			{
				{ "MatchGuid", matchGuid },
				{ "LocalPlayers", localPlayers },
				{ "TreehouseGlobalCamTime", globalCamTime },
				{ "TreehouseLocalCamTime", localCamTime }
			});
		}
	}

	public static void CharacterPickedEvent(Guid matchGuid, Character.Animals character, int outfits, int handicap)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("CharacterPicked", new Dictionary<string, object>
			{
				{ "MatchGuid", matchGuid },
				{
					"Character",
					character.ToString()
				},
				{ "Outfits", outfits },
				{ "Handicap", handicap }
			});
		}
	}

	public static void MatchModifiersEvent(Guid matchGuid, Modifiers currentMods)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("ModsApplied", currentMods.modsApplied);
			if (currentMods.modsApplied)
			{
				addChangedModifiersToDict(currentMods, dictionary);
			}
			sendBatchedEvent("MatchModifiers", dictionary, "MatchGuid", matchGuid);
		}
	}

	private static void addChangedModifiersToDict(Modifiers currentMods, Dictionary<string, object> parameters)
	{
		if (currentMods.GravityMode > 0)
		{
			parameters.Add("GravityMode", currentMods.GravityMode);
		}
		if (currentMods.JumpSpeedMode > 0)
		{
			parameters.Add("JumpSpeedMode", currentMods.JumpSpeedMode);
		}
		if (currentMods.SprintSpeedMode > 0)
		{
			parameters.Add("SprintSpeedMode", currentMods.SprintSpeedMode);
		}
		if (currentMods.WallJumpsDisabled)
		{
			parameters.Add("WallJumpsDisabled", currentMods.wallJumpsDisabled);
		}
		if (currentMods.GameSpeedMode > 0)
		{
			parameters.Add("GameSpeedMode", currentMods.GameSpeedMode);
		}
		if (currentMods.DanceInvincibility)
		{
			parameters.Add("DanceInvincibility", currentMods.danceInvincibility);
		}
		if (currentMods.invisibilityMode != 0)
		{
			parameters.Add("InvisibilityMode", currentMods.invisibilityMode);
		}
		if (currentMods.MirrorControls)
		{
			parameters.Add("MirrorControls", currentMods.mirrorControls);
		}
		if (currentMods.PlatformSpeedMode > 0)
		{
			parameters.Add("PlatformSpeedMode", currentMods.PlatformSpeedMode);
		}
		if (currentMods.RateOfFireMode > 0)
		{
			parameters.Add("RateOfFireMode", currentMods.RateOfFireMode);
		}
		if (currentMods.MultiJumpMode > 0)
		{
			parameters.Add("MultiJumpMode", currentMods.MultiJumpMode);
		}
		if (currentMods.ProjectileExplosionMode > 0)
		{
			parameters.Add("ProjectileExplosionMode", currentMods.ProjectileExplosionMode);
		}
		if (currentMods.CharacterSizeMode > 0)
		{
			parameters.Add("CharacterSizeMode", currentMods.CharacterSizeMode);
		}
		if (currentMods.jetpackMode)
		{
			parameters.Add("JetpackMode", currentMods.jetpackMode);
		}
		if (currentMods.PostDeathBehaviorMode > 0)
		{
			parameters.Add("PostDeathBehaviourMode", currentMods.PostDeathBehaviorMode);
		}
		if (currentMods.CameraFlipMode > 0)
		{
			parameters.Add("CameraFlipMode", currentMods.CameraFlipMode);
		}
		if (currentMods.DoomsdayMeteorsMode > 0)
		{
			parameters.Add("DoomsdayMeteorsMode", currentMods.DoomsdayMeteorsMode);
		}
		if (currentMods.DoomsdayLavaMode > 0)
		{
			parameters.Add("DoomsdayLavaMode", currentMods.DoomsdayLavaMode);
		}
		if (currentMods.PlayerPlayerCollisions)
		{
			parameters.Add("PlayerPlayerCollisions", currentMods.playerPlayerCollisions);
		}
		if (currentMods.ProjectileSpeedMode > 0)
		{
			parameters.Add("ProjectileSpeedMode", currentMods.ProjectileSpeedMode);
		}
		if (currentMods.frictionless)
		{
			parameters.Add("Frictionless", currentMods.frictionless);
		}
	}

	public static void MatchRulesEvent(Guid matchGuid, GameRulePreset currentRules)
	{
		if (!AnalyticsWrapper.EnabledOnPlatform)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("IsPremade", currentRules.IsPremade);
		if (!currentRules.IsPremade)
		{
			GameRulePreset defaultRuleset = GameSettings.GetInstance().DefaultRuleset;
			dictionary.Add("Ruleset", "Custom");
			if (currentRules.MaxScore != defaultRuleset.MaxScore)
			{
				dictionary.Add("MaxScore", currentRules.MaxScore);
			}
			if (currentRules.MaxRounds != defaultRuleset.MaxRounds)
			{
				dictionary.Add("MaxRounds", currentRules.MaxRounds);
			}
			if (currentRules.MaxTime != defaultRuleset.MaxTime)
			{
				dictionary.Add("MaxTime", currentRules.MaxTime);
			}
			if (currentRules.PlaceTime != defaultRuleset.PlaceTime)
			{
				dictionary.Add("PlaceTime", currentRules.PlaceTime);
			}
			if (currentRules.UsePlaceTimer != defaultRuleset.UsePlaceTimer)
			{
				dictionary.Add("UsePlaceTimer", currentRules.UsePlaceTimer);
			}
			if (currentRules.GameLimitType != defaultRuleset.GameLimitType)
			{
				dictionary.Add("GameLimitType", currentRules.GameLimitType);
			}
			if (currentRules.DoublePartyBox != defaultRuleset.DoublePartyBox)
			{
				dictionary.Add("DoublePartyBox", currentRules.DoublePartyBox);
			}
			if (currentRules.RunTimerLimit != defaultRuleset.RunTimerLimit)
			{
				dictionary.Add("RunTimerLimit", currentRules.RunTimerLimit);
			}
			if (currentRules.CreativePiecesPerRound != defaultRuleset.CreativePiecesPerRound)
			{
				dictionary.Add("CreativePiecesPerRound", currentRules.CreativePiecesPerRound);
			}
			if (currentRules.respawnMode != defaultRuleset.respawnMode)
			{
				dictionary.Add("RespawnMode", currentRules.respawnMode);
			}
			if (currentRules.numRespawns != defaultRuleset.numRespawns)
			{
				dictionary.Add("NumRespawns", currentRules.numRespawns);
			}
			if (currentRules.partyBoxMode != defaultRuleset.partyBoxMode)
			{
				dictionary.Add("PartyBoxMod", currentRules.partyBoxMode);
			}
		}
		else
		{
			dictionary.Add("Ruleset", currentRules.Name);
		}
		sendBatchedEvent("MatchRules", dictionary, "MatchGuid", matchGuid);
	}

	public static void MatchPointsEvent(Guid matchGuid, GameRulePreset currentRules)
	{
		if (!AnalyticsWrapper.EnabledOnPlatform)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		GameRulePreset defaultRuleset = GameSettings.GetInstance().DefaultRuleset;
		foreach (PointBlock.pointBlockType pointType in currentRules.PointTypes)
		{
			if (currentRules.PointTypeEnabled(pointType) != defaultRuleset.PointTypeEnabled(pointType) || currentRules.PointTypeValue(pointType) != defaultRuleset.PointTypeValue(pointType))
			{
				dictionary.Add(pointType.ToString(), currentRules.PointTypeEnabled(pointType) ? currentRules.PointTypeValue(pointType) : 0);
			}
			if (currentRules.AlwaysAwardPointType(pointType) != defaultRuleset.AlwaysAwardPointType(pointType))
			{
				dictionary.Add("AlwaysAward" + pointType, currentRules.AlwaysAwardPointType(pointType));
			}
		}
		sendBatchedEvent("MatchPoints", dictionary, "MatchGuid", matchGuid);
	}

	public static void MatchBlocksEvent(Guid matchGuid, GameRulePreset currentRules)
	{
		if (!AnalyticsWrapper.EnabledOnPlatform)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		GameRulePreset defaultRuleset = GameSettings.GetInstance().DefaultRuleset;
		GameRulePreset.BlockData[] blocks = currentRules.Blocks;
		for (int i = 0; i < blocks.Length; i++)
		{
			GameRulePreset.BlockData blockData = blocks[i];
			int num = defaultRuleset.BlockFrequency(blockData.BlockPlaceable);
			if (blockData.Frequency != num)
			{
				if (!dictionary.ContainsKey(blockData.BlockPlaceable.name))
				{
					dictionary.Add(blockData.BlockPlaceable.Name, blockData.Frequency);
				}
				else
				{
					Debug.LogError("Error in MatchBlocksEvent: " + blockData.BlockPlaceable.name + " already in dictionary");
				}
			}
		}
		sendBatchedEvent("MatchBlocks", dictionary, "MatchGuid", matchGuid);
	}

	public static void MatchEndHostEvent(Guid matchGuid, int pointSpread, int playersKicked, int playersQuit, float duration, int rounds, bool complete)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("MatchEnd_Host", new Dictionary<string, object>
			{
				{
					"MatchGuid",
					matchGuid.ToString()
				},
				{ "PointSpread", pointSpread },
				{ "PlayersKicked", playersKicked },
				{ "PlayersQuit", playersQuit },
				{ "Duration", duration },
				{ "Rounds", rounds },
				{ "MatchComplete", complete }
			});
		}
	}

	public static void MatchEndClientEvent(Guid matchGuid, float globalCamTime, float localCamTime)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("MatchEnd_Client", new Dictionary<string, object>
			{
				{
					"MatchGuid",
					matchGuid.ToString()
				},
				{ "MatchGlobalCamTime", globalCamTime },
				{ "MatchLocalCamTime", localCamTime }
			});
		}
	}

	public static void PlayerRankingEvent(Guid matchGuid, int rank, int score, double skillMean, double skillStdDev)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("PlayerRanking", new Dictionary<string, object>
			{
				{
					"MatchGuid",
					matchGuid.ToString()
				},
				{ "Rank", rank },
				{ "Score", score },
				{ "SkillMean", skillMean },
				{ "SkillStdDev", skillStdDev }
			});
		}
	}

	public static void PlayerLeftMatchEvent(Guid matchGuid, int playerRank, int pointSpread, bool kicked)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("PlayerLeftMatch", new Dictionary<string, object>
			{
				{
					"MatchGuid",
					matchGuid.ToString()
				},
				{ "PlayerRank", playerRank },
				{ "PointSpread", pointSpread },
				{ "PlayerWasKicked", kicked }
			});
		}
	}

	public static void PlayerLeftTreehouseEvent(Guid lobbyGuid, int matchesPlayed, float timeWaited, bool kicked)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("PlayerLeftTreehouse", new Dictionary<string, object>
			{
				{ "LobbyGuid", lobbyGuid },
				{ "MatchesPlayed", matchesPlayed },
				{ "TimeWaited", timeWaited },
				{ "PlayerWasKicked", kicked }
			});
		}
	}

	public static void LevelSavedEvent(Guid matchGuid, FeaturedQuickFilter.LevelTypes savedMode, bool savedOnline, bool isPublic, float percentFull, BackgroundType background, GameState.LevelName levelMusic, GameState.LevelName levelAmbience, int numCoins)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("LevelSaved", new Dictionary<string, object>
			{
				{
					"MatchGuid",
					matchGuid.ToString()
				},
				{
					"SavedMode",
					savedMode.ToString()
				},
				{ "SavedOnline", savedOnline },
				{ "IsPublic", isPublic },
				{ "PercentFull", percentFull },
				{
					"Background",
					background.ToString()
				},
				{
					"LevelMusic",
					levelMusic.ToString()
				},
				{
					"LevelAmbience",
					levelAmbience.ToString()
				},
				{ "NumCoins", numCoins }
			});
		}
	}

	public static void LevelModifiersEvent(Guid matchGuid, Modifiers currentMods)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("ModsApplied", currentMods.modsApplied);
			if (currentMods.modsApplied)
			{
				addChangedModifiersToDict(currentMods, dictionary);
			}
			sendBatchedEvent("LevelModifiers", dictionary, "MatchGuid", matchGuid);
		}
	}

	public static void CharacterUnlockedEvent(Character.Animals character, float totalPlaytime)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("CharacterUnlocked", new Dictionary<string, object>
			{
				{
					"Character",
					character.ToString()
				},
				{ "TotalPlaytime", totalPlaytime }
			});
		}
	}

	public static void LevelUnlockedEvent(GameState.LevelName level, float totalPlaytime)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("LevelUnlocked", new Dictionary<string, object>
			{
				{
					"Level",
					level.ToString()
				},
				{ "TotalPlaytime", totalPlaytime }
			});
		}
	}

	public static void OutfitUnlocked(Character.Animals forCharacter, int outfit, int totalOutfits, float totalPlaytime)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("OutfitUnlocked", new Dictionary<string, object>
			{
				{
					"ForCharacter",
					forCharacter.ToString()
				},
				{ "Outfit", outfit },
				{ "TotalOutfits", totalOutfits },
				{ "TotalPlaytime", totalPlaytime }
			});
		}
	}

	public static void CheatCodeUsedEvent(int treehouseState, float totalPlaytime)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("CheatCodeUsed", new Dictionary<string, object>
			{
				{ "TreehouseState", treehouseState },
				{ "TotalPlaytime", totalPlaytime }
			});
		}
	}

	public static void ProgressResetEvent(float totalPlaytime, bool hadCheated)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("ProgressReset", new Dictionary<string, object>
			{
				{ "TotalPlaytime", totalPlaytime },
				{ "HadCheated", hadCheated }
			});
		}
	}

	public static void LinkClickedEvent(SocialLink linkClicked, string url)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("LinkClicked", new Dictionary<string, object>
			{
				{
					"Website",
					linkClicked.ToString()
				},
				{ "URL", url }
			});
		}
	}

	public static void LevelSharedEvent(ShareSite sharedOn)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("LevelShared", new Dictionary<string, object> { 
			{
				"SharedOn",
				sharedOn.ToString()
			} });
		}
	}

	public static void GameStartEvent(string language)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("GameStart", new Dictionary<string, object> { { "Language", language } });
		}
	}

	public static void GameQuitEvent(float runtime)
	{
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			sendEvent("GameQuit", new Dictionary<string, object> { { "Runtime", runtime } });
		}
	}
}
