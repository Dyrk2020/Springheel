using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GameEvent;
using GameSparks.Core;
using UnityEngine;
using UnityEngine.Events;

public class StatTracker : IGameEventListener
{
	public enum SaveFileStatus
	{
		NONE,
		READY,
		LOADING,
		SAVING,
		ERROR
	}

	public SaveFileData mainUserSaveFileData;

	public SaveFileStatus mainUserSaveStatus;

	public SaveFileData[] saveFiles = new SaveFileData[4];

	public SaveFileStatus[] saveStatuses = new SaveFileStatus[4];

	public bool SkipFirstSave = true;

	public const string saveFileName = "saveData.uch";

	public const string saveFileName2 = "saveData-Beta.uch";

	private static StatTracker instance;

	private static float defaultVolumeSFX = -1f;

	private static float defaultVolumeBGM = -1f;

	private static bool saveInProgress;

	private static bool loadInProgress;

	public static StatTracker Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new StatTracker();
				instance.ChangeListeners(adding: true);
			}
			return instance;
		}
	}

	public bool PlatformHasMultiSave => false;

	public static bool PlatformKnowsUserAtStart => true;

	public int TreehouseLevel
	{
		get
		{
			StatBoolArray stat = GetSaveFileDataForMainUser().GetStat<StatBoolArray>("LevelsUnlocked");
			int result = 0;
			for (int i = 0; i != stat.values.Length; i++)
			{
				if (stat.values[i] && i != 10)
				{
					result = ((i <= 10) ? i : (i - 1));
				}
			}
			return result;
		}
	}

	private bool CanSave
	{
		get
		{
			if (Application.isEditor && GameSettings.GetInstance().IgnoreSaveFileInEditor)
			{
				return false;
			}
			return true;
		}
	}

	~StatTracker()
	{
		ChangeListeners(adding: false);
	}

	public void ChangeListeners(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<GameEndEvent>(this, adding);
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, adding);
		GameEventManager.ChangeListener<DestroyPieceEvent>(this, adding);
		GameEventManager.ChangeListener<CheatUnlockEvent>(this, adding);
		GameEventManager.ChangeListener<CheatUnlockHalfEvent>(this, adding);
		GameEventManager.ChangeListener<CheatRandomGamePlayedEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<LocalPlayerAddedEvent>(this, adding);
		GameEventManager.ChangeListener<LocalPlayerRemovedEvent>(this, adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public void Initialize()
	{
	}

	private void TryLoadDefaultVolume()
	{
		Debug.Log("Loading default volume...");
		try
		{
			string path = Application.persistentDataPath + "/lastvol.config";
			if (File.Exists(path))
			{
				FileStream fileStream = File.Open(path, FileMode.Open);
				if (fileStream == null)
				{
					return;
				}
				new Parsing.EnsureInvariantCulture();
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, (int)fileStream.Length);
				fileStream.Close();
				string json = Encoding.UTF8.GetString(array);
				try
				{
					if (GSJson.From(json) is Dictionary<string, object> data)
					{
						GSData gSData = new GSData(data);
						float? num = gSData.GetFloat("bgmVolume");
						if (num.HasValue)
						{
							defaultVolumeBGM = num.Value;
						}
						float? num2 = gSData.GetFloat("sfxVolume");
						if (num2.HasValue)
						{
							defaultVolumeSFX = num2.Value;
						}
						SaveFileData.ApplySoundVolumes(defaultVolumeBGM, defaultVolumeSFX);
					}
					else
					{
						Debug.LogError("Default volume file did not parse correctly...");
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError("Error while trying to parse default volume: " + ex.Message);
					return;
				}
			}
			Debug.LogError("No previous volume file found!");
		}
		catch (Exception ex2)
		{
			Debug.LogError("Error while trying to read default volume: " + ex2.Message);
		}
	}

	private void SaveDefaultVolume(float bgmVolume, float sfxVolume)
	{
	}

	public SaveFileData CreateSaveFileDataForMainUser()
	{
		mainUserSaveFileData = new SaveFileData();
		mainUserSaveStatus = SaveFileStatus.READY;
		mainUserSaveFileData.creationDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
		return mainUserSaveFileData;
	}

	public void OnLocalPlayerAdded(int playerLocalNumber)
	{
		if (playerLocalNumber <= 0 || playerLocalNumber > 4)
		{
			Debug.LogError("ERROR: Illegal player local number (" + playerLocalNumber + ")");
			return;
		}
		Player player = PlayerManager.GetInstance().GetPlayer(playerLocalNumber);
		if (player != null)
		{
			if (ControllerMonitor.Instance.IsMainController(player.UseController))
			{
				saveFiles[playerLocalNumber - 1] = mainUserSaveFileData;
			}
			else if (PlatformHasMultiSave)
			{
				SaveFileData saveFileData = new SaveFileData();
				saveFiles[playerLocalNumber - 1] = saveFileData;
				saveStatuses[playerLocalNumber - 1] = SaveFileStatus.NONE;
				LoadGameForUser(playerLocalNumber, saveFileData);
			}
			else
			{
				saveFiles[playerLocalNumber - 1] = null;
			}
		}
		else
		{
			Debug.LogError("StatTracker.OnLocalPlayerAdded: Could not find player with local number " + playerLocalNumber);
		}
	}

	public void OnLocalPlayerRemoved(int playerLocalNumber)
	{
		saveFiles[playerLocalNumber - 1] = null;
	}

	public void OnMainControllerRemoved()
	{
		mainUserSaveFileData = null;
		for (int i = 0; i < saveFiles.Length; i++)
		{
			saveFiles[i] = null;
		}
	}

	public SaveFileData GetSaveFileDataFromNetworkNumber(int playerNetworkNumber, bool fallback = false)
	{
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && item.AssociatedLobbyPlayer != null && item.AssociatedLobbyPlayer.networkNumber == playerNetworkNumber && item.AssociatedLobbyPlayer.IsLocalPlayer)
			{
				return GetSaveFileDataForLocalPlayer(item.Number, fallback);
			}
		}
		return null;
	}

	public SaveFileData GetSaveFileDataForMainUser()
	{
		if (mainUserSaveFileData == null)
		{
			Debug.LogWarning("GetSaveFileDataForMainUser ERROR: main user save file data is null. Creating new save file data.");
			CreateSaveFileDataForMainUser();
		}
		return mainUserSaveFileData;
	}

	public SaveFileData GetSaveFileDataForLocalPlayer(int localPlayerNumber, bool fallback = false)
	{
		if (localPlayerNumber <= 0 || localPlayerNumber > 4)
		{
			return null;
		}
		if (fallback)
		{
			if (saveFiles[localPlayerNumber - 1] == null)
			{
				return GetSaveFileDataForMainUser();
			}
			return saveFiles[localPlayerNumber - 1];
		}
		return saveFiles[localPlayerNumber - 1];
	}

	public IEnumerable<SaveFileData> GetActiveUserSaveFileDatas()
	{
		HashSet<SaveFileData> returned = new HashSet<SaveFileData>();
		if (mainUserSaveFileData != null)
		{
			returned.Add(mainUserSaveFileData);
			yield return mainUserSaveFileData;
		}
		if (!PlatformHasMultiSave)
		{
			yield break;
		}
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item == null || !(item.AssociatedLobbyPlayer != null) || !item.AssociatedLobbyPlayer.IsLocalPlayer)
			{
				continue;
			}
			SaveFileData saveFileDataForLocalPlayer = GetSaveFileDataForLocalPlayer(item.Number);
			if (saveFileDataForLocalPlayer != null)
			{
				if (!returned.Contains(saveFileDataForLocalPlayer))
				{
					returned.Add(saveFileDataForLocalPlayer);
					yield return saveFileDataForLocalPlayer;
				}
			}
			else
			{
				Debug.LogError("Found null save file data for active player number " + item.Number);
			}
		}
	}

	public SaveFileData GetSaveFileDataForAnimal(Character.Animals animal, bool fallback = false)
	{
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && item.AssociatedLobbyPlayer != null && item.AssociatedLobbyPlayer.PickedAnimal == animal)
			{
				return GetSaveFileDataForLocalPlayer(item.Number, fallback);
			}
		}
		return null;
	}

	public int GetLocalNumberForSaveFileData(SaveFileData saveFileData)
	{
		for (int i = 0; i < saveFiles.Length; i++)
		{
			if (saveFiles[i] != null && saveFiles[i] == saveFileData)
			{
				return i + 1;
			}
		}
		return -1;
	}

	public void ClearKeybindings()
	{
		if (!ControllerMonitor.Instance.IsMainControllerSet)
		{
			return;
		}
		SaveFileData saveFileDataForMainUser = GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser != null)
		{
			int length = KeyboardInput.DefaultKeys.GetLength(0);
			int length2 = KeyboardInput.DefaultKeys.GetLength(1);
			saveFileDataForMainUser.DefaultKeys = new int[length, length2];
			for (int i = 0; i != length; i++)
			{
				saveFileDataForMainUser.DefaultKeys[i, 0] = KeyboardInput.DefaultKeys[i, 0];
				saveFileDataForMainUser.DefaultKeys[i, 1] = KeyboardInput.DefaultKeys[i, 1];
			}
			length = KeyboardInput.DefaultAltKeys.GetLength(0);
			length2 = KeyboardInput.DefaultAltKeys.GetLength(1);
			saveFileDataForMainUser.DefaultAltKeys = new int[length, length2];
			for (int j = 0; j != length; j++)
			{
				saveFileDataForMainUser.DefaultAltKeys[j, 0] = KeyboardInput.DefaultAltKeys[j, 0];
				saveFileDataForMainUser.DefaultAltKeys[j, 1] = KeyboardInput.DefaultAltKeys[j, 1];
			}
			saveFileDataForMainUser.ApplySettings();
		}
	}

	public void ClearStatsAndUnlocks(int localPlayerNumber = -1)
	{
		SaveFileData saveFileData = ((localPlayerNumber != -1) ? GetSaveFileDataForLocalPlayer(localPlayerNumber) : mainUserSaveFileData);
		if (saveFileData != null)
		{
			AnalyticEvent.ProgressResetEvent(saveFileData.GetStat<StatFloat>("TotalMatchTime").value, saveFileData.IsCheater);
			saveFileData.ResetStats();
			GameEventManager.SendEvent(new ResetDataEvent());
		}
		else
		{
			Debug.LogError("Could not find save file data for local player " + localPlayerNumber);
		}
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PlayerSkillUpdated)
			{
				MsgPlayerSkillUpdated msgPlayerSkillUpdated = (MsgPlayerSkillUpdated)networkMessageReceivedEvent.ReadMessage;
				foreach (Player item in PlayerManager.GetInstance())
				{
					if (item != null && item.AssociatedLobbyPlayer != null && item.AssociatedLobbyPlayer.networkNumber == msgPlayerSkillUpdated.NetworkPlayerNumber)
					{
						SaveFileData saveFileDataForLocalPlayer = GetSaveFileDataForLocalPlayer(item.Number);
						if (saveFileDataForLocalPlayer != null)
						{
							saveFileDataForLocalPlayer.SkillMean = msgPlayerSkillUpdated.SkillMean;
							saveFileDataForLocalPlayer.SkillStdDev = msgPlayerSkillUpdated.SkillStdDev;
							Debug.Log("Skill for player " + msgPlayerSkillUpdated.NetworkPlayerNumber + " updated to: [" + msgPlayerSkillUpdated.SkillMean + ", " + msgPlayerSkillUpdated.SkillStdDev + "]");
						}
						else
						{
							Debug.LogError("Could not find save file data for local player " + item.Number);
						}
					}
				}
			}
		}
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
			if (gameMode == GameState.GameMode.PARTY || gameMode == GameState.GameMode.CREATIVE)
			{
				if (startPhaseEvent.Phase == GameControl.GamePhase.PLACE)
				{
					foreach (SaveFileData activeUserSaveFileData in GetActiveUserSaveFileDatas())
					{
						activeUserSaveFileData.IncrementStat("TotalRounds");
						activeUserSaveFileData.IncrementStat("TotalLevelRounds", (int)GameState.GetInstance().SelectedLevel);
					}
				}
				else if (startPhaseEvent.Phase == GameControl.GamePhase.SUDDENDEATH)
				{
					foreach (SaveFileData activeUserSaveFileData2 in GetActiveUserSaveFileDatas())
					{
						activeUserSaveFileData2.IncrementStat("TotalSuddenDeaths");
						AchievementChecker.Instance.Clutch_Performer_AchievementCheck(activeUserSaveFileData2);
					}
				}
			}
		}
		if (type == typeof(GameEndEvent))
		{
			GameEndEvent gameEndEvent = e as GameEndEvent;
			SaveSystemProtector.Protect();
			if (!gameEndEvent.GameCompleted)
			{
				return;
			}
			foreach (SaveFileData activeUserSaveFileData3 in GetActiveUserSaveFileDatas())
			{
				activeUserSaveFileData3.IncrementStat("GamesPlayed");
				activeUserSaveFileData3.IncrementStat("GamesSinceLastLevelUnlocked");
				activeUserSaveFileData3.IncrementStat("GamesSinceLastCharacterLevelUnlocked");
				switch (gameEndEvent.GameMode)
				{
				case GameState.GameMode.CREATIVE:
					activeUserSaveFileData3.IncrementStat("CreativeModeGamesPlayed");
					break;
				case GameState.GameMode.PARTY:
					activeUserSaveFileData3.IncrementStat("PartyModeGamesPlayed");
					break;
				}
				if (gameEndEvent.Online)
				{
					activeUserSaveFileData3.IncrementStat("OnlineGamesPlayed");
				}
				activeUserSaveFileData3.IncrementStat("LevelsPlayed", (int)gameEndEvent.LevelName);
				activeUserSaveFileData3.IncrementStat("TotalMatchTime", Time.timeSinceLevelLoad);
				activeUserSaveFileData3.IncrementStat("TotalLevelTime", (int)gameEndEvent.LevelName, Time.timeSinceLevelLoad);
			}
			foreach (SaveFileData activeUserSaveFileData4 in GetActiveUserSaveFileDatas())
			{
				AchievementChecker.Instance.CheckAllAchievements(activeUserSaveFileData4);
			}
		}
		if (type == typeof(PiecePlacedEvent))
		{
			PiecePlacedEvent piecePlacedEvent = e as PiecePlacedEvent;
			if (piecePlacedEvent.PlayerNumber != 0)
			{
				SaveFileData saveFileDataFromNetworkNumber = GetSaveFileDataFromNetworkNumber(piecePlacedEvent.PlayerNumber, fallback: true);
				if (saveFileDataFromNetworkNumber != null)
				{
					if (piecePlacedEvent.PlacedBlock.Category == Placeable.PieceCategory.BOMB)
					{
						saveFileDataFromNetworkNumber.IncrementStat("BombsPlaced");
					}
					else
					{
						saveFileDataFromNetworkNumber.IncrementStat("PiecesPlaced");
					}
					switch (piecePlacedEvent.PlacedBlock.Category)
					{
					case Placeable.PieceCategory.PLATFORM:
						saveFileDataFromNetworkNumber.IncrementStat("PlatformsPlaced");
						break;
					case Placeable.PieceCategory.TRAP:
						saveFileDataFromNetworkNumber.IncrementStat("TrapsPlaced");
						AchievementChecker.Instance.Trap_AchievementCheck(saveFileDataFromNetworkNumber);
						break;
					case Placeable.PieceCategory.MOVINGPLATFORM:
						saveFileDataFromNetworkNumber.IncrementStat("MovingPlatformsPlaced");
						break;
					case Placeable.PieceCategory.ATTACHMENT:
						saveFileDataFromNetworkNumber.IncrementStat("AttachmentsPlaced");
						break;
					case Placeable.PieceCategory.SPECIAL:
						saveFileDataFromNetworkNumber.IncrementStat("SpecialPlaced");
						break;
					case Placeable.PieceCategory.ITEM:
						saveFileDataFromNetworkNumber.IncrementStat("ItemsPlaced");
						break;
					}
				}
			}
		}
		if (type == typeof(DestroyPieceEvent))
		{
			DestroyPieceEvent destroyPieceEvent = e as DestroyPieceEvent;
			SaveFileData saveFileDataFromNetworkNumber2 = GetSaveFileDataFromNetworkNumber(destroyPieceEvent.PlayerNetworkNumber, fallback: true);
			if (saveFileDataFromNetworkNumber2 != null)
			{
				if (destroyPieceEvent.Piece.Category != Placeable.PieceCategory.BOMB)
				{
					saveFileDataFromNetworkNumber2.IncrementStat("PiecesDestroyed");
				}
				switch (destroyPieceEvent.Piece.Category)
				{
				case Placeable.PieceCategory.PLATFORM:
					saveFileDataFromNetworkNumber2.IncrementStat("PlatformsDestroyed");
					break;
				case Placeable.PieceCategory.TRAP:
					saveFileDataFromNetworkNumber2.IncrementStat("TrapsDestroyed");
					break;
				case Placeable.PieceCategory.MOVINGPLATFORM:
					saveFileDataFromNetworkNumber2.IncrementStat("MovingPlatformsDestroyed");
					break;
				case Placeable.PieceCategory.ATTACHMENT:
					saveFileDataFromNetworkNumber2.IncrementStat("AttachmentsDestroyed");
					break;
				case Placeable.PieceCategory.SPECIAL:
					saveFileDataFromNetworkNumber2.IncrementStat("SpecialDestroyed");
					break;
				case Placeable.PieceCategory.ITEM:
					saveFileDataFromNetworkNumber2.IncrementStat("ItemsDestroyed");
					break;
				}
			}
		}
		if (type == typeof(CheatUnlockEvent))
		{
			SaveFileData saveFileDataForMainUser = GetSaveFileDataForMainUser();
			StatBoolArray stat = saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked");
			for (int i = 0; i != stat.values.Length; i++)
			{
				stat.values[i] = true;
			}
			StatBoolArray stat2 = saveFileDataForMainUser.GetStat<StatBoolArray>("LevelsUnlocked");
			for (int j = 0; j != stat2.values.Length; j++)
			{
				stat2.values[j] = true;
			}
			StatCountArray stat3 = saveFileDataForMainUser.GetStat<StatCountArray>("OutfitsUnlocked");
			for (int k = 0; k != stat3.values.Length; k++)
			{
				stat3.values[k] = 65535;
			}
			LevelSelectController currentLevelSelectController = LobbyManager.instance.CurrentLevelSelectController;
			if (currentLevelSelectController != null && currentLevelSelectController.hasAuthority)
			{
				currentLevelSelectController.CallCmdSetTreehouseGrowState(TreehouseLevel);
			}
			saveFileDataForMainUser.GetStat<StatBool>("Cheater").Set(value: true);
			SaveGameForUser(-1, saveFileDataForMainUser);
		}
		if (type == typeof(CheatUnlockHalfEvent))
		{
			SaveFileData saveFileDataForMainUser2 = GetSaveFileDataForMainUser();
			StatBoolArray stat4 = saveFileDataForMainUser2.GetStat<StatBoolArray>("CharactersUnlocked");
			for (int l = 0; l <= 15; l++)
			{
				stat4.values[l] = true;
			}
			StatBoolArray stat5 = saveFileDataForMainUser2.GetStat<StatBoolArray>("LevelsUnlocked");
			for (int m = 0; m <= 23; m++)
			{
				stat5.values[m] = true;
				saveFileDataForMainUser2.IncrementStat("LevelsPlayed", m);
				saveFileDataForMainUser2.IncrementStat("TotalLevelRounds", m);
				saveFileDataForMainUser2.IncrementStat("TotalRounds");
				saveFileDataForMainUser2.IncrementStat("GamesPlayed");
			}
			StatCountArray stat6 = saveFileDataForMainUser2.GetStat<StatCountArray>("OutfitsUnlocked");
			for (int n = 0; n <= 15; n++)
			{
				stat6.values[n] = 65535;
			}
			LevelSelectController currentLevelSelectController2 = LobbyManager.instance.CurrentLevelSelectController;
			if (currentLevelSelectController2 != null && currentLevelSelectController2.hasAuthority)
			{
				currentLevelSelectController2.CallCmdSetTreehouseGrowState(TreehouseLevel);
			}
			SaveGameForUser(-1, saveFileDataForMainUser2);
		}
		if (GameState.DebugMode && type == typeof(CheatRandomGamePlayedEvent))
		{
			SaveFileData saveFileDataForMainUser3 = GetSaveFileDataForMainUser();
			StatBoolArray stat7 = saveFileDataForMainUser3.GetStat<StatBoolArray>("LevelsUnlocked");
			int num;
			do
			{
				num = UnityEngine.Random.Range(0, stat7.values.Length);
			}
			while (!stat7.values[num]);
			GameState.LevelName levelName = (GameState.LevelName)num;
			SaveSystemProtector.Protect();
			foreach (SaveFileData activeUserSaveFileData5 in GetActiveUserSaveFileDatas())
			{
				activeUserSaveFileData5.IncrementStat("GamesPlayed");
				activeUserSaveFileData5.IncrementStat("GamesSinceLastLevelUnlocked");
				activeUserSaveFileData5.IncrementStat("GamesSinceLastCharacterLevelUnlocked");
				activeUserSaveFileData5.IncrementStat("LevelsPlayed", (int)levelName);
			}
			UserMessageManager.Instance.UserMessage("Fake playthrough of level: " + GameState.GetLevelSceneName(levelName) + "\n GamesSinceLastLevelUnlocked =" + saveFileDataForMainUser3.GetStat<StatCount>("GamesSinceLastLevelUnlocked").count + "\n GamesSinceLastCharacterLevelUnlocked =" + saveFileDataForMainUser3.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked").count + "\n GamesPlayed =" + saveFileDataForMainUser3.GetStat<StatCount>("GamesPlayed").count, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			foreach (SaveFileData activeUserSaveFileData6 in GetActiveUserSaveFileDatas())
			{
				AchievementChecker.Instance.CheckAllAchievements(activeUserSaveFileData6);
			}
		}
		if (type == typeof(LocalPlayerRemovedEvent))
		{
			LocalPlayerRemovedEvent localPlayerRemovedEvent = e as LocalPlayerRemovedEvent;
			OnLocalPlayerRemoved(localPlayerRemovedEvent.RemovedPlayer.Number);
		}
		if (type == typeof(LocalPlayerAddedEvent))
		{
			LocalPlayerAddedEvent localPlayerAddedEvent = e as LocalPlayerAddedEvent;
			OnLocalPlayerAdded(localPlayerAddedEvent.NewPlayer.Number);
		}
		if (type == typeof(LanguageChangeEvent))
		{
			LanguageChangeEvent languageChangeEvent = e as LanguageChangeEvent;
			SaveFileData saveFileDataForMainUser4 = GetSaveFileDataForMainUser();
			if (saveFileDataForMainUser4 != null)
			{
				saveFileDataForMainUser4.language = languageChangeEvent.LanguageString;
				SaveGameForUser(-1, saveFileDataForMainUser4);
			}
		}
	}

	public void SaveGameForAnimal(Character.Animals animal, SaveFileData saveFileData)
	{
		int playerLocalNumberForAnimal = PlayerManager.GetInstance().GetPlayerLocalNumberForAnimal(animal);
		if (playerLocalNumberForAnimal > 0 && playerLocalNumberForAnimal <= 4)
		{
			SaveGameForUser(playerLocalNumberForAnimal, saveFileData);
		}
		else
		{
			Debug.LogError("Could not save game for " + animal.ToString() + " - no local player found with that animal.");
		}
	}

	public void SaveGameForUser(int playerLocalNumber, SaveFileData saveFileData)
	{
		if (PlatformHasMultiSave)
		{
			doSaveForUser(playerLocalNumber, saveFileData);
		}
		else
		{
			if (saveInProgress)
			{
				return;
			}
			if (loadInProgress)
			{
				WorkerThreadManager.Instance.AddFileOpJob(delegate
				{
					saveInProgress = true;
					Debug.Log("There is a load attempt in progress. Waiting before trying to save.");
					while (loadInProgress)
					{
					}
				}, delegate
				{
					Debug.Log("Load done, saving file...");
					doSaveForUser(playerLocalNumber, saveFileData);
					saveInProgress = false;
				});
			}
			else
			{
				doSaveForUser(playerLocalNumber, saveFileData);
			}
		}
	}

	private void doSaveForUser(int playerLocalNumber, SaveFileData saveFileData)
	{
		if (playerLocalNumber == -1)
		{
			SaveDefaultVolume(saveFileData.MusicVolume, saveFileData.SoundVolume);
		}
		if (playerLocalNumber < 0)
		{
			mainUserSaveStatus = SaveFileStatus.SAVING;
		}
		else
		{
			saveStatuses[playerLocalNumber - 1] = SaveFileStatus.SAVING;
		}
		UnityAction unityAction = delegate
		{
			if (playerLocalNumber < 0)
			{
				mainUserSaveStatus = SaveFileStatus.READY;
			}
			else
			{
				saveStatuses[playerLocalNumber - 1] = SaveFileStatus.READY;
			}
		};
		string text = Application.persistentDataPath + "/";
		string text2 = Application.dataPath + "/";
		if (PlatformHasMultiSave && playerLocalNumber != -1 && playerLocalNumber != 1)
		{
			text = Application.persistentDataPath + "/" + playerLocalNumber;
		}
		if (PlatformHasMultiSave || playerLocalNumber == -1)
		{
			string text3 = (GameSettings.GetInstance().useSecondarySaveFile ? "saveData-Beta.uch" : "saveData.uch");
			try
			{
				if (File.Exists(text + text3))
				{
					File.Copy(text + text3, text + text3 + ".bak", overwrite: true);
				}
				else if (File.Exists(text2 + text3))
				{
					File.Copy(text2 + text3, text + text3 + ".bak", overwrite: true);
				}
				string saveText = XMLSaver.GetSaveText(saveFileData);
				string value = Convert.ToBase64String(Encoding.UTF8.GetBytes(saveText));
				StreamWriter streamWriter = File.CreateText(text + text3);
				streamWriter.WriteLine(value);
				streamWriter.Close();
				Debug.Log("Game saved to " + text + text3);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Problem saving game: (" + ex.GetType().Name + ") " + ex.Message + "\n" + ex.StackTrace);
			}
		}
		else
		{
			Debug.LogError("Tried to save data for non-main user!");
		}
		unityAction();
	}

	public void SaveGameForAllUsers()
	{
		if (!CanSave)
		{
			return;
		}
		HashSet<SaveFileData> hashSet = new HashSet<SaveFileData>();
		if (mainUserSaveFileData == null)
		{
			CreateSaveFileDataForMainUser();
		}
		if (mainUserSaveStatus == SaveFileStatus.READY)
		{
			mainUserSaveStatus = SaveFileStatus.SAVING;
			SaveGameForUser(-1, mainUserSaveFileData);
			hashSet.Add(mainUserSaveFileData);
		}
		if (PlatformHasMultiSave)
		{
			for (int i = 0; i < saveFiles.Length; i++)
			{
				if (saveFiles[i] != null && !hashSet.Contains(saveFiles[i]) && saveStatuses[i] == SaveFileStatus.READY)
				{
					SaveGameForUser(i + 1, saveFiles[i]);
					hashSet.Add(saveFiles[i]);
				}
			}
		}
		SaveDefaultVolume(mainUserSaveFileData.MusicVolume, mainUserSaveFileData.SoundVolume);
	}

	public void LoadGameForMainUser()
	{
		if (Application.isEditor && GameSettings.GetInstance().IgnoreSaveFileInEditor)
		{
			Debug.LogWarning("Skipping load for main user");
			return;
		}
		SaveFileData saveFileData = CreateSaveFileDataForMainUser();
		LoadGameForUser(-1, saveFileData);
	}

	public void LoadGameForUser(int playerLocalNumber, SaveFileData saveFileData)
	{
		if (Application.isEditor && GameSettings.GetInstance().IgnoreSaveFileInEditor)
		{
			return;
		}
		if (playerLocalNumber == -1)
		{
			if (mainUserSaveStatus == SaveFileStatus.LOADING || mainUserSaveStatus == SaveFileStatus.SAVING)
			{
				return;
			}
			mainUserSaveStatus = SaveFileStatus.LOADING;
		}
		if (playerLocalNumber >= 0)
		{
			if (saveStatuses[playerLocalNumber - 1] == SaveFileStatus.LOADING || saveStatuses[playerLocalNumber - 1] == SaveFileStatus.SAVING)
			{
				return;
			}
			saveStatuses[playerLocalNumber - 1] = SaveFileStatus.LOADING;
		}
		loadInProgress = true;
		UnityAction unityAction = delegate
		{
			Debug.Log("Save file loading finished. Checking achievements...");
			AchievementChecker.Instance.CheckAllAchievements(saveFileData);
			if (saveFileData == mainUserSaveFileData)
			{
				saveFileData.ApplySettings();
				if (RamFS.PlatformUsesRamFS)
				{
					RamFS.OnMainUserGameLoaded(delegate
					{
						UndergroundComputer.ClearMissingLocalSnapshotCodeEntries(saveFileData);
						mainUserSaveStatus = SaveFileStatus.READY;
						GameRulePreset.LoadAllSavedRulesets();
						loadInProgress = false;
					});
				}
				else
				{
					UndergroundComputer.ClearMissingLocalSnapshotCodeEntries(saveFileData);
					mainUserSaveStatus = SaveFileStatus.READY;
					GameRulePreset.LoadAllSavedRulesets();
					loadInProgress = false;
				}
			}
			else if (playerLocalNumber >= 0)
			{
				saveStatuses[playerLocalNumber - 1] = SaveFileStatus.READY;
				loadInProgress = false;
			}
			else
			{
				Debug.LogError("Likely a problem loading the main user's save file. Status is: " + mainUserSaveStatus);
				loadInProgress = false;
			}
		};
		if (PlatformHasMultiSave || playerLocalNumber == -1)
		{
			string text = (GameSettings.GetInstance().useSecondarySaveFile ? "saveData-Beta.uch" : "saveData.uch");
			string text2 = Application.persistentDataPath + "/";
			string text3 = Application.dataPath + "/";
			string text4 = text2 + text;
			if (PlatformHasMultiSave && playerLocalNumber != -1 && playerLocalNumber != 1)
			{
				text2 = Application.persistentDataPath + "/" + playerLocalNumber;
			}
			if (!File.Exists(text4))
			{
				Debug.LogWarning("Save file not found at " + text4 + ". Looking again in old path.");
				text4 = text3 + text;
			}
			Debug.Log("Loading game from: " + text4);
			try
			{
				if (!File.Exists(text4))
				{
					throw new FileNotFoundException("File not found", text4);
				}
				StreamReader streamReader = File.OpenText(text4);
				string s = streamReader.ReadToEnd();
				streamReader.Close();
				byte[] bytes = Convert.FromBase64String(s);
				string xml = Encoding.UTF8.GetString(bytes);
				try
				{
					Parsing.useCurrentCulture = false;
					XMLSaver.Load(xml, saveFileData);
				}
				catch (FormatException ex)
				{
					Debug.LogError("Could not load save file with Invariant culture, trying with current system culture instead. (" + ex.Message + ")");
					try
					{
						Parsing.useCurrentCulture = true;
						XMLSaver.Load(xml, saveFileData);
					}
					catch (Exception)
					{
						throw new Exception("Failed to load save file with both Invariant and current culture");
					}
					finally
					{
						Parsing.useCurrentCulture = false;
					}
				}
			}
			catch (Exception ex3)
			{
				Debug.LogError("Could not load save file: (" + ex3.GetType().ToString() + ") " + ex3.Message + ".\n" + ex3.StackTrace);
				Debug.Log("Trying to load backup file: " + text2 + text + ".bak");
				try
				{
					StreamReader streamReader2 = File.OpenText(text2 + text + ".bak");
					string s2 = streamReader2.ReadToEnd();
					streamReader2.Close();
					byte[] bytes2 = Convert.FromBase64String(s2);
					string xml2 = Encoding.UTF8.GetString(bytes2);
					try
					{
						Parsing.useCurrentCulture = false;
						XMLSaver.Load(xml2, saveFileData);
					}
					catch (FormatException ex4)
					{
						Debug.LogError("Could not load save file with Invariant culture, trying with current system culture instead. (" + ex4.Message + ")");
						try
						{
							Parsing.useCurrentCulture = true;
							XMLSaver.Load(xml2, saveFileData);
							goto end_IL_02bd;
						}
						catch (Exception)
						{
							throw new Exception("Failed to load save file with both Invariant and current culture");
						}
						finally
						{
							Parsing.useCurrentCulture = false;
						}
						end_IL_02bd:;
					}
				}
				catch (Exception ex6)
				{
					Debug.LogError("Could not load backup save file: (" + ex6.GetType().ToString() + ") " + ex6.Message + ".\n" + ex6.StackTrace);
				}
			}
			unityAction();
		}
		else
		{
			Debug.LogError("Tried to load save file for non-main user!");
		}
	}
}
