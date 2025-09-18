using System;
using System.Collections.Generic;
using System.Text;
using I2.Loc;
using UnityEngine;

public class SaveFileData
{
	public class RecentSnapshotEntry
	{
		public enum SnapshotType
		{
			Uploaded,
			Downloaded
		}

		public SnapshotType type;

		public string code;

		public string name;

		public RecentSnapshotEntry()
		{
		}

		public RecentSnapshotEntry(RecentSnapshotEntry other)
		{
			type = other.type;
			code = other.code;
			name = other.name;
		}
	}

	public class FavoriteSnapshotEntry
	{
		public string name;

		public string code;

		public FavoriteSnapshotEntry()
		{
		}

		public FavoriteSnapshotEntry(FavoriteSnapshotEntry other)
		{
			name = other.name;
			code = other.code;
		}
	}

	public class PortalSnapshotEntry
	{
		public string name;

		public string code;

		public FeaturedQuickFilter.LevelTypes levelType;

		public PortalSnapshotEntry()
		{
		}

		public PortalSnapshotEntry(PortalSnapshotEntry other)
		{
			name = other.name;
			code = other.code;
			levelType = other.levelType;
		}
	}

	public const int numLevels = 24;

	public const int numCharacters = 16;

	public Dictionary<string, StatBase> stats;

	public float SoundVolume = 0.8f;

	public float MusicVolume = 0.8f;

	public bool VSync = true;

	public bool BackgroundAudio;

	public int AFKAutoKickTime = 30;

	public bool CrossPlatformToggle = true;

	public OnlineChatEmotes OnlineChatEmotes;

	public OnlinePlayerNames OnlinePlayerNames = OnlinePlayerNames.Auto;

	public bool CameraLocalOnly;

	public int[,] DefaultKeys = new int[22, 2]
	{
		{ 0, 119 },
		{ 1, 115 },
		{ 2, 97 },
		{ 3, 100 },
		{ 8, 32 },
		{ 9, 98 },
		{ 11, 304 },
		{ 12, 114 },
		{ 13, 122 },
		{ 16, 113 },
		{ 17, 101 },
		{ 18, 13 },
		{ 19, 27 },
		{ 20, 9 },
		{ 21, 13 },
		{ 22, 8 },
		{ 24, 105 },
		{ 25, 107 },
		{ 26, 106 },
		{ 27, 108 },
		{ 33, 116 },
		{ 14, 118 }
	};

	public int[,] DefaultAltKeys = new int[22, 2]
	{
		{ 0, 273 },
		{ 1, 274 },
		{ 2, 276 },
		{ 3, 275 },
		{ 8, 0 },
		{ 9, 8 },
		{ 11, 0 },
		{ 12, 0 },
		{ 13, 0 },
		{ 16, 0 },
		{ 17, 0 },
		{ 18, 0 },
		{ 19, 0 },
		{ 20, 0 },
		{ 21, 32 },
		{ 22, 0 },
		{ 24, 264 },
		{ 25, 258 },
		{ 26, 260 },
		{ 27, 262 },
		{ 33, 0 },
		{ 14, 0 }
	};

	public bool HideVersion;

	public double SkillMean = 25.0;

	public double SkillStdDev = 8.333333333333334;

	public string language;

	public string creationDate = "Not set";

	public string lastSaveDate = "Not set";

	public const int maxCodeHistoryLength = 50;

	public List<RecentSnapshotEntry> recentSnapshotEntries = new List<RecentSnapshotEntry>();

	public List<FavoriteSnapshotEntry> favoriteSnapshots = new List<FavoriteSnapshotEntry>();

	public Dictionary<string, int> snapshotSequenceNumbers = new Dictionary<string, int>();

	public Dictionary<string, string> localSnapshotCodes = new Dictionary<string, string>();

	public List<PortalSnapshotEntry> portalSnapshotEntries = new List<PortalSnapshotEntry>();

	public bool IsCheater => GetStat<StatBool>("Cheater").value;

	public SaveFileData()
	{
		stats = new Dictionary<string, StatBase>();
		AddStatCount("GamesPlayed");
		AddStatCount("OnlineGamesPlayed");
		AddStatCount("PartyModeGamesPlayed");
		AddStatCount("CreativeModeGamesPlayed");
		AddStatCount("SandboxModeGamesPlayed");
		AddStatCount("GamesSinceLastLevelUnlocked");
		AddStatCount("GamesSinceLastCharacterLevelUnlocked");
		AddStatCountArray("LevelsPlayed", 24);
		AddStatCount("PiecesPlaced");
		AddStatCount("PiecesDestroyed");
		AddStatCount("TrapsPlaced");
		AddStatCount("TrapsDestroyed");
		AddStatCount("PlatformsPlaced");
		AddStatCount("PlatformsDestroyed");
		AddStatCount("MovingPlatformsPlaced");
		AddStatCount("MovingPlatformsDestroyed");
		AddStatCount("AttachmentsPlaced");
		AddStatCount("AttachmentsDestroyed");
		AddStatCount("BombsPlaced");
		AddStatCount("SpecialPlaced");
		AddStatCount("SpecialDestroyed");
		AddStatCount("ItemsPlaced");
		AddStatCount("ItemsDestroyed");
		AddStatCount("PiecesGlued");
		AddStatCount("LargeContraptionsMade");
		AddStatCount("Jumps");
		AddStatCount("WallJumps");
		AddStatCount("TimesTeleported");
		AddStatCount("SpringBounces");
		AddStatFloat("DistanceRun");
		AddStatFloat("DistanceSlid");
		AddStatCount("TotalDeaths");
		AddStatCount("DeathsByTrap");
		AddStatCount("DeathsBySuicide");
		AddStatCount("DeathsByFalling");
		AddStatCount("DeathsByHazard");
		AddStatCount("DeathsBySpikeBall");
		AddStatCount("DeathsByBarbedWire");
		AddStatCount("DeathsByArrow");
		AddStatCount("DeathsByTennisBall");
		AddStatCount("DeathsBySpinningSaw");
		AddStatCount("DeathsByLinearSaw");
		AddStatCount("DeathsByPropeller");
		AddStatCount("DeathsByFlippingBlock");
		AddStatCount("DeathsByBlackHole");
		AddStatCount("DeathsByHockeyPuck");
		AddStatCount("DeathsByPunchingPlant");
		AddStatCount("DeathsByPressureTriggerSpikes");
		AddStatCount("DeathsByWreckingBall");
		AddStatCountArray("CharacterDeaths", 16);
		AddStatCount("CoinsCollected");
		AddStatCount("CoinsLost");
		AddStatCount("CoinsStolen");
		AddStatCount("ComebackPointsEarned");
		AddStatCount("SoloPointsEarned");
		AddStatCount("TrapPointsEarned");
		AddStatCount("PostmortemVictories");
		AddStatCountArray("CharacterSuccess", 16);
		AddStatCountArray("CharacterWins", 16);
		AddStatFloat("TotalMatchTime");
		AddStatFloatArray("TotalLevelTime", 24);
		AddStatCount("TotalRounds");
		AddStatCount("TotalSuddenDeaths");
		AddStatCountArray("TotalLevelRounds", 24);
		AddStatBoolArray("CharactersUnlocked", 16);
		AddStatCountArray("OutfitsUnlocked", 16);
		AddStatBoolArray("LevelsUnlocked", 24);
		AddStatBool("Cheater");
		ResetStats();
	}

	public SaveFileData Clone()
	{
		SaveFileData saveFileData = new SaveFileData();
		saveFileData.creationDate = creationDate;
		saveFileData.lastSaveDate = lastSaveDate;
		foreach (KeyValuePair<string, StatBase> stat in stats)
		{
			saveFileData.stats[stat.Key] = stat.Value.Clone();
		}
		saveFileData.SoundVolume = SoundVolume;
		saveFileData.MusicVolume = MusicVolume;
		saveFileData.VSync = VSync;
		saveFileData.BackgroundAudio = BackgroundAudio;
		saveFileData.AFKAutoKickTime = AFKAutoKickTime;
		saveFileData.CrossPlatformToggle = CrossPlatformToggle;
		saveFileData.language = language;
		saveFileData.OnlineChatEmotes = OnlineChatEmotes;
		saveFileData.OnlinePlayerNames = OnlinePlayerNames;
		saveFileData.CameraLocalOnly = CameraLocalOnly;
		Array.Copy(DefaultKeys, saveFileData.DefaultKeys, DefaultKeys.Length);
		Array.Copy(DefaultAltKeys, saveFileData.DefaultAltKeys, DefaultAltKeys.Length);
		saveFileData.HideVersion = HideVersion;
		saveFileData.SkillMean = SkillMean;
		saveFileData.SkillStdDev = SkillStdDev;
		saveFileData.recentSnapshotEntries = new List<RecentSnapshotEntry>(recentSnapshotEntries.Count);
		foreach (RecentSnapshotEntry recentSnapshotEntry in recentSnapshotEntries)
		{
			saveFileData.recentSnapshotEntries.Add(new RecentSnapshotEntry(recentSnapshotEntry));
		}
		saveFileData.favoriteSnapshots = new List<FavoriteSnapshotEntry>(favoriteSnapshots.Count);
		foreach (FavoriteSnapshotEntry favoriteSnapshot in favoriteSnapshots)
		{
			saveFileData.favoriteSnapshots.Add(new FavoriteSnapshotEntry(favoriteSnapshot));
		}
		saveFileData.snapshotSequenceNumbers = new Dictionary<string, int>(snapshotSequenceNumbers.Count);
		foreach (KeyValuePair<string, int> snapshotSequenceNumber in snapshotSequenceNumbers)
		{
			saveFileData.snapshotSequenceNumbers.Add(snapshotSequenceNumber.Key, snapshotSequenceNumber.Value);
		}
		saveFileData.localSnapshotCodes = new Dictionary<string, string>(localSnapshotCodes.Count);
		foreach (KeyValuePair<string, string> localSnapshotCode in localSnapshotCodes)
		{
			saveFileData.localSnapshotCodes.Add(localSnapshotCode.Key, localSnapshotCode.Value);
		}
		saveFileData.portalSnapshotEntries = new List<PortalSnapshotEntry>(portalSnapshotEntries.Count);
		foreach (PortalSnapshotEntry portalSnapshotEntry in portalSnapshotEntries)
		{
			saveFileData.portalSnapshotEntries.Add(new PortalSnapshotEntry(portalSnapshotEntry));
		}
		return saveFileData;
	}

	public void LoadFromBytes(byte[] bytes)
	{
		byte[] bytes2 = Convert.FromBase64String(Encoding.UTF8.GetString(bytes));
		string xml = Encoding.UTF8.GetString(bytes2);
		Parsing.useCurrentCulture = false;
		try
		{
			XMLSaver.Load(xml, this);
			Debug.LogWarning("Save file loaded!");
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while loading save file: " + ex.Message + "\n" + ex.StackTrace);
			throw new Exception("Error loading file from passed bytes");
		}
	}

	public void ResetStats()
	{
		foreach (KeyValuePair<string, StatBase> stat in stats)
		{
			stat.Value.Reset();
		}
		bool[] values = (stats["LevelsUnlocked"] as StatBoolArray).values;
		values[0] = true;
		values[1] = true;
		values[10] = true;
		bool[] values2 = (stats["CharactersUnlocked"] as StatBoolArray).values;
		values2[1] = true;
		values2[2] = true;
		values2[3] = true;
		values2[4] = true;
	}

	public static void ApplySoundVolumes(float bgm, float sfx)
	{
		GameSettings.Music = bgm;
		if (!WwiseSuspender.Muted)
		{
			AkSoundEngine.SetRTPCValue("MUS_volume", bgm * 100f);
		}
		GameSettings.Sound = sfx;
		if (!WwiseSuspender.Muted)
		{
			AkSoundEngine.SetRTPCValue("SFX_volume", sfx * 100f);
		}
	}

	public void ApplySettings()
	{
		ApplySoundVolumes(MusicVolume, SoundVolume);
		GameSettings.GetInstance().AFKAutoKickTime = AFKAutoKickTime;
		GameSettings.GetInstance().CurrentLobbyAFKAutoKickTime = AFKAutoKickTime;
		GameSettings.GetInstance().OnlineChatEmotes = OnlineChatEmotes;
		GameSettings.GetInstance().OnlinePlayerNames = OnlinePlayerNames;
		ZoomCamera.LocalOnly = CameraLocalOnly;
		QualitySettings.vSyncCount = (VSync ? 1 : 0);
		if (GameSettings.PlatformCanDisableCrossPlay)
		{
			GameSettings.GetInstance().CrossPlatformToggle = CrossPlatformToggle;
		}
		else
		{
			GameSettings.GetInstance().CrossPlatformToggle = true;
		}
		KeyboardInput keyboard = GameState.GetInstance().Keyboard;
		for (int i = 0; i != DefaultKeys.GetLength(0); i++)
		{
			InputEvent.InputKey inputKey = (InputEvent.InputKey)DefaultKeys[i, 0];
			if (inputKey != InputEvent.InputKey.Back)
			{
				if (inputKey == InputEvent.InputKey.Suicide)
				{
					keyboard.RebindKey(InputEvent.InputKey.Back, (KeyCode)DefaultKeys[i, 1]);
				}
				keyboard.RebindKey(inputKey, (KeyCode)DefaultKeys[i, 1]);
			}
		}
		for (int j = 0; j != DefaultAltKeys.GetLength(0); j++)
		{
			InputEvent.InputKey inputKey2 = (InputEvent.InputKey)DefaultAltKeys[j, 0];
			if (inputKey2 != InputEvent.InputKey.Back)
			{
				if (inputKey2 == InputEvent.InputKey.Suicide)
				{
					keyboard.RebindAltKey(InputEvent.InputKey.Back, (KeyCode)DefaultAltKeys[j, 1]);
				}
				keyboard.RebindAltKey(inputKey2, (KeyCode)DefaultAltKeys[j, 1]);
			}
		}
		if (language != null)
		{
			LocalizationManager.CurrentLanguage = language;
		}
	}

	public KeyCode? GetKeyBinding(InputEvent.InputKey eventKey)
	{
		if (eventKey == InputEvent.InputKey.NoKey)
		{
			return null;
		}
		for (int i = 0; i < DefaultKeys.GetLength(0); i++)
		{
			if (DefaultKeys[i, 0] == (int)eventKey)
			{
				return (KeyCode)DefaultKeys[i, 1];
			}
		}
		return null;
	}

	public KeyCode? GetAltKeyBinding(InputEvent.InputKey eventKey)
	{
		if (eventKey == InputEvent.InputKey.NoKey)
		{
			return null;
		}
		for (int i = 0; i < DefaultAltKeys.GetLength(0); i++)
		{
			if (DefaultAltKeys[i, 0] == (int)eventKey)
			{
				return (KeyCode)DefaultAltKeys[i, 1];
			}
		}
		return null;
	}

	public void SetKeyBinding(InputEvent.InputKey inputKey, KeyCode keycode)
	{
		for (int i = 0; i < DefaultKeys.GetLength(0); i++)
		{
			if (DefaultKeys[i, 0] == (int)inputKey)
			{
				DefaultKeys[i, 1] = (int)keycode;
				break;
			}
		}
	}

	public void SetAltKeyBinding(InputEvent.InputKey inputKey, KeyCode keycode)
	{
		for (int i = 0; i < DefaultAltKeys.GetLength(0); i++)
		{
			if (DefaultAltKeys[i, 0] == (int)inputKey)
			{
				DefaultAltKeys[i, 1] = (int)keycode;
				break;
			}
		}
	}

	public StatBase GetStat(string name)
	{
		return GetStat<StatBase>(name);
	}

	public T GetStat<T>(string name) where T : StatBase
	{
		if (stats.TryGetValue(name, out var value))
		{
			T val = value as T;
			if (value != null && val == null)
			{
				Debug.LogError("WARNING: Could not cast stat \"" + name + "\" to " + typeof(T).ToString());
			}
			return value as T;
		}
		return null;
	}

	private void AddStatCount(string name)
	{
		if (!stats.ContainsKey(name))
		{
			stats.Add(name, new StatCount
			{
				name = name
			});
		}
		else
		{
			OnStatNotAdded(name);
		}
	}

	private void AddStatCountArray(string name, int elements)
	{
		if (!stats.ContainsKey(name))
		{
			stats.Add(name, new StatCountArray
			{
				name = name,
				values = new int[elements]
			});
		}
		else
		{
			OnStatNotAdded(name);
		}
	}

	private void AddStatFloat(string name)
	{
		if (!stats.ContainsKey(name))
		{
			stats.Add(name, new StatFloat
			{
				name = name
			});
		}
		else
		{
			OnStatNotAdded(name);
		}
	}

	private void AddStatFloatArray(string name, int elements)
	{
		if (!stats.ContainsKey(name))
		{
			stats.Add(name, new StatFloatArray
			{
				name = name,
				values = new float[elements]
			});
		}
		else
		{
			OnStatNotAdded(name);
		}
	}

	private void AddStatBool(string name)
	{
		if (!stats.ContainsKey(name))
		{
			stats.Add(name, new StatBool
			{
				name = name
			});
		}
		else
		{
			OnStatNotAdded(name);
		}
	}

	private void AddStatBoolArray(string name, int elements)
	{
		if (!stats.ContainsKey(name))
		{
			stats.Add(name, new StatBoolArray
			{
				name = name,
				values = new bool[elements]
			});
		}
		else
		{
			OnStatNotAdded(name);
		}
	}

	private void OnStatNotAdded(string name)
	{
		Debug.LogError("Stat not added: " + name);
	}

	public bool CheckStatCountGoalReached(string name, int threshold)
	{
		StatCount stat = GetStat<StatCount>(name);
		if (stat != null)
		{
			if (stat.count >= threshold)
			{
				if (AchievementChecker.PlatformWantsFreshStats)
				{
					return stat.dirty;
				}
				return true;
			}
			return false;
		}
		Debug.LogError("ERROR: Could not find StatCount called " + name);
		return false;
	}

	public bool CheckStatBoolArray(string name, int index)
	{
		if (index >= 100)
		{
			return false;
		}
		StatBoolArray stat = GetStat<StatBoolArray>(name);
		if (stat != null)
		{
			if (stat.values[index])
			{
				if (AchievementChecker.PlatformWantsFreshStats)
				{
					return stat.dirty;
				}
				return true;
			}
			return false;
		}
		Debug.LogError("ERROR: Could not find StatCount called " + name);
		return false;
	}

	public void IncrementStat(string name)
	{
		StatBase stat = GetStat(name);
		if (stat != null)
		{
			if (stat.type == StatBase.StatType.Count)
			{
				if (stat is StatCount statCount)
				{
					statCount.Increment(1);
				}
				else
				{
					Debug.LogError("IncrementStat (name) error: Could not cast \"" + name + "\" to Stat" + stat.type);
				}
			}
			else
			{
				Debug.LogError("IncrementStat (name) error: Cannot increment stat of type: " + stat.type);
			}
		}
		else
		{
			Debug.LogError("IncrementStat (name) error: Could not find stat \"" + name + "\"");
		}
	}

	public void DecrementStat(string name)
	{
		StatBase stat = GetStat(name);
		if (stat != null)
		{
			if (stat.type == StatBase.StatType.Count)
			{
				if (stat is StatCount statCount)
				{
					statCount.Increment(-1);
				}
				else
				{
					Debug.LogError("DecrementStat (name) error: Could not cast \"" + name + "\" to Stat" + stat.type);
				}
			}
			else
			{
				Debug.LogError("DecrementStat (name) error: Cannot decrement stat of type: " + stat.type);
			}
		}
		else
		{
			Debug.LogError("DecrementStat (name) error: Could not find stat \"" + name + "\"");
		}
	}

	public void IncrementStat(string name, int arrayIndex)
	{
		if (arrayIndex >= 100)
		{
			return;
		}
		StatBase stat = GetStat(name);
		if (stat != null)
		{
			if (stat.type == StatBase.StatType.CountArray)
			{
				if (stat is StatCountArray statCountArray)
				{
					statCountArray.Increment(arrayIndex, 1);
				}
				else
				{
					Debug.LogError("IncrementStat (name, arrayIndex) error: Could not cast \"" + name + "\" to Stat" + stat.type);
				}
			}
			else
			{
				Debug.LogError("IncrementStat (name, arrayIndex) error: Cannot increment stat of type: " + stat.type);
			}
		}
		else
		{
			Debug.LogError("IncrementStat (name, arrayIndex) error: Could not find stat \"" + name + "\"");
		}
	}

	public void DecrementStat(string name, int arrayIndex)
	{
		if (arrayIndex >= 100)
		{
			return;
		}
		StatBase stat = GetStat(name);
		if (stat != null)
		{
			if (stat.type == StatBase.StatType.CountArray)
			{
				if (stat is StatCountArray statCountArray)
				{
					statCountArray.Increment(arrayIndex, -1);
				}
				else
				{
					Debug.LogError("DecrementStat (name, arrayIndex) error: Could not cast \"" + name + "\" to Stat" + stat.type);
				}
			}
			else
			{
				Debug.LogError("DecrementStat (name, arrayIndex) error: Cannot decrement stat of type: " + stat.type);
			}
		}
		else
		{
			Debug.LogError("DecrementStat (name, arrayIndex) error: Could not find stat \"" + name + "\"");
		}
	}

	public void IncrementStat(string name, float incrementValue)
	{
		StatBase stat = GetStat(name);
		if (stat != null)
		{
			if (stat.type == StatBase.StatType.Float)
			{
				if (stat is StatFloat statFloat)
				{
					statFloat.Increment(incrementValue);
				}
				else
				{
					Debug.LogError("IncrementStat (name, incrementValue) error: Could not cast \"" + name + "\" to Stat" + stat.type);
				}
			}
			else
			{
				Debug.LogError("IncrementStat (name, incrementValue) error: Cannot increment stat of type: " + stat.type);
			}
		}
		else
		{
			Debug.LogError("IncrementStat (name, incrementValue) error: Could not find stat \"" + name + "\"");
		}
	}

	public void IncrementStat(string name, int arrayIndex, float incrementValue)
	{
		if (arrayIndex >= 100)
		{
			return;
		}
		StatBase stat = GetStat(name);
		if (stat != null)
		{
			if (stat.type == StatBase.StatType.FloatArray)
			{
				if (stat is StatFloatArray statFloatArray)
				{
					statFloatArray.Increment(arrayIndex, incrementValue);
				}
				else
				{
					Debug.LogError("IncrementStat (name, arrayIndex, incrementValue) error: Could not cast \"" + name + "\" to Stat" + stat.type);
				}
			}
			else
			{
				Debug.LogError("IncrementStat (name, arrayIndex, incrementValue) error: Cannot increment stat of type: " + stat.type);
			}
		}
		else
		{
			Debug.LogError("IncrementStat (name, arrayIndex, incrementValue) error: Could not find stat \"" + name + "\"");
		}
	}

	public void AddRecentSnapshotCode(RecentSnapshotEntry.SnapshotType type, string code, string name)
	{
		Debug.Log("Added recent snapshot code: " + code + " - " + name + " (" + type.ToString() + ")");
		int num = recentSnapshotEntries.FindIndex((RecentSnapshotEntry e) => e.code == code);
		if (num != 0 || recentSnapshotEntries[num].type != type)
		{
			if (num != -1)
			{
				recentSnapshotEntries.RemoveAt(num);
			}
			recentSnapshotEntries.Insert(0, new RecentSnapshotEntry
			{
				code = code,
				type = type,
				name = name
			});
			if (recentSnapshotEntries.Count > 50)
			{
				recentSnapshotEntries = recentSnapshotEntries.GetRange(0, 50);
			}
		}
	}

	public void DeleteRecentSnapshotCode(string code)
	{
		int num = recentSnapshotEntries.FindIndex((RecentSnapshotEntry entry) => entry.code == code);
		if (num != -1)
		{
			recentSnapshotEntries.RemoveAt(num);
		}
		else
		{
			Debug.LogWarning("Could not remove recent snapshot code: Snapshot not found (" + code + ")");
		}
	}

	public void AddFavoriteSnapshotCode(string name, string code)
	{
		code = GameSparksQuery.GetFormattedSnapshotCode(code);
		int num = -1;
		num = ((!code.NullOrEmpty()) ? favoriteSnapshots.FindIndex((FavoriteSnapshotEntry e) => e.code == code && e.name == name) : favoriteSnapshots.FindIndex((FavoriteSnapshotEntry e) => e.code.NullOrEmpty() && e.name == name));
		switch (num)
		{
		case 0:
			return;
		default:
			favoriteSnapshots.RemoveAt(num);
			break;
		case -1:
			break;
		}
		favoriteSnapshots.Insert(0, new FavoriteSnapshotEntry
		{
			code = code,
			name = name
		});
	}

	public void AddCodeToLocalFavorite(string name, string code)
	{
		code = GameSparksQuery.GetFormattedSnapshotCode(code);
		int num = favoriteSnapshots.FindIndex((FavoriteSnapshotEntry e) => e.code.NullOrEmpty() && e.name == name);
		if (num != -1)
		{
			favoriteSnapshots[num].code = code;
		}
	}

	public void RemoveFavoriteSnapshotCode(string name, string code)
	{
		code = GameSparksQuery.GetFormattedSnapshotCode(code);
		int num = -1;
		num = ((!code.NullOrEmpty()) ? favoriteSnapshots.FindIndex((FavoriteSnapshotEntry e) => e.code == code && e.name == name) : favoriteSnapshots.FindIndex((FavoriteSnapshotEntry e) => e.code.NullOrEmpty() && e.name == name));
		if (num != -1)
		{
			favoriteSnapshots.RemoveAt(num);
			return;
		}
		Debug.LogWarning("Could not remove favorite snapshot code: Snapshot not found (" + code + " - " + name + ")");
	}

	public bool IsFavorite(string name, string code)
	{
		code = GameSparksQuery.GetFormattedSnapshotCode(code);
		int num = -1;
		num = ((!code.NullOrEmpty()) ? favoriteSnapshots.FindIndex((FavoriteSnapshotEntry e) => e.code == code && e.name == name) : favoriteSnapshots.FindIndex((FavoriteSnapshotEntry e) => e.code.NullOrEmpty() && e.name == name));
		return num != -1;
	}

	public void AssociateLocalSnapshotCode(string filename, string code)
	{
		if (localSnapshotCodes.ContainsKey(filename))
		{
			localSnapshotCodes[filename] = code;
		}
		else
		{
			localSnapshotCodes.Add(filename, code);
		}
	}

	public void RemoveLocalSnapshotCodeAssociation(string filename)
	{
		if (!localSnapshotCodes.Remove(filename))
		{
			Debug.LogWarning("Could not remove local snapshot code association for " + filename);
		}
	}

	public bool IsLocalSnapshotWithCode(string filename, string code)
	{
		if (localSnapshotCodes.TryGetValue(filename, out var value))
		{
			return code == value;
		}
		return false;
	}

	public void SetPortalInfo(CustomLevelPortal.SnapshotInfo[] snapshotInfos)
	{
		portalSnapshotEntries = new List<PortalSnapshotEntry>();
		foreach (CustomLevelPortal.SnapshotInfo snapshotInfo in snapshotInfos)
		{
			if (snapshotInfo != null)
			{
				portalSnapshotEntries.Add(new PortalSnapshotEntry
				{
					name = snapshotInfo.snapshotName,
					code = snapshotInfo.code,
					levelType = snapshotInfo.levelType
				});
			}
			else
			{
				portalSnapshotEntries.Add(new PortalSnapshotEntry());
			}
		}
	}

	public void SetPortalInfo(int portalIdx, CustomLevelPortal.SnapshotInfo snapshotInfo)
	{
		while (portalSnapshotEntries.Count <= portalIdx)
		{
			portalSnapshotEntries.Add(new PortalSnapshotEntry());
		}
		portalSnapshotEntries[portalIdx].name = snapshotInfo.snapshotName;
		portalSnapshotEntries[portalIdx].code = snapshotInfo.code;
		portalSnapshotEntries[portalIdx].levelType = snapshotInfo.levelType;
	}

	public byte[] GetSaveFileBytes()
	{
		string saveText = XMLSaver.GetSaveText(this);
		string s = Convert.ToBase64String(Encoding.UTF8.GetBytes(saveText));
		return Encoding.UTF8.GetBytes(s);
	}

	public void ClearAllDirtyFlags()
	{
		foreach (KeyValuePair<string, StatBase> stat in stats)
		{
			stat.Value.dirty = false;
		}
	}
}
