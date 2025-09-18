using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public static class XMLSaver
{
	private static XmlDocument xmlDoc;

	private static Dictionary<string, string> extraData = new Dictionary<string, string>();

	private static XmlNode CreateSimpleNode(string nodeName, object value)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlElement xmlElement = xmlDoc.CreateElement(nodeName);
		xmlElement.InnerText = value.ToString();
		return xmlElement;
	}

	private static string ReadExtraData(string key)
	{
		if (xmlDoc == null)
		{
			return "";
		}
		string text = "UCHSave/Misc/";
		text += key;
		XmlNode xmlNode = xmlDoc.SelectSingleNode(text);
		if (xmlNode == null)
		{
			foreach (string key2 in extraData.Keys)
			{
				if (key2.Equals(key))
				{
					return extraData[key2];
				}
			}
			return "";
		}
		return xmlNode.InnerText;
	}

	private static void WriteExtraData(string key, string value)
	{
		if (extraData.ContainsKey(key))
		{
			extraData[key] = value;
		}
		else
		{
			extraData.Add(key, value);
		}
	}

	private static void Deserialize(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return;
		}
		xmlDoc.DocumentElement.GetAttribute("version");
		string attribute = xmlDoc.DocumentElement.GetAttribute("creationDate");
		if (!attribute.NullOrEmpty())
		{
			saveFileData.creationDate = attribute;
		}
		string attribute2 = xmlDoc.DocumentElement.GetAttribute("lastSaveDate");
		if (!attribute2.NullOrEmpty())
		{
			saveFileData.lastSaveDate = attribute2;
		}
		foreach (XmlNode childNode in xmlDoc.DocumentElement.ChildNodes)
		{
			switch (childNode.Name)
			{
			case "settings":
				deserializeSettings(childNode, saveFileData);
				break;
			case "stats":
				deserializeStats(childNode, saveFileData);
				break;
			case "unlocks":
				deserializeUnlocks(childNode, saveFileData);
				break;
			case "snapshotCodeHistory":
				deserializeSnapshotCodeHistory(childNode, saveFileData);
				break;
			case "favoriteSnapshots":
				deserializeFavoriteSnapshots(childNode, saveFileData);
				break;
			case "snapshotSequenceNumbers":
				deserializeSnapshotSequenceNumbers(childNode, saveFileData);
				break;
			case "localSnapshotCodes":
				deserializeLocalSnapshotCodes(childNode, saveFileData);
				break;
			case "portalSnapshotEntries":
				deserializePortalSnapshotEntries(childNode, saveFileData);
				break;
			}
		}
	}

	private static void deserializeSettings(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "sound")
			{
				saveFileData.SoundVolume = Parsing.ParseFloat(childNode.InnerText);
			}
			else if (childNode.Name == "music")
			{
				saveFileData.MusicVolume = Parsing.ParseFloat(childNode.InnerText);
			}
			else if (childNode.Name == "vsync")
			{
				saveFileData.VSync = int.Parse(childNode.InnerText) == 1;
			}
			else if (childNode.Name == "backgroundAudio")
			{
				saveFileData.BackgroundAudio = bool.Parse(childNode.InnerText);
			}
			else if (childNode.Name == "hideVersion")
			{
				saveFileData.HideVersion = bool.Parse(childNode.InnerText);
			}
			else if (childNode.Name == "keyboard")
			{
				deserializeKeyboard(childNode, saveFileData);
			}
			else if (childNode.Name == "language")
			{
				saveFileData.language = childNode.InnerText;
			}
			else if (childNode.Name == "AFKAutoKickTime")
			{
				saveFileData.AFKAutoKickTime = int.Parse(childNode.InnerText);
			}
			else if (childNode.Name == "CrossPlatformToggle")
			{
				saveFileData.CrossPlatformToggle = bool.Parse(childNode.InnerText);
			}
			else if (childNode.Name == "OnlineChatEmotes")
			{
				saveFileData.OnlineChatEmotes = (OnlineChatEmotes)int.Parse(childNode.InnerText);
			}
			else if (childNode.Name == "OnlinePlayerNames")
			{
				saveFileData.OnlinePlayerNames = (OnlinePlayerNames)int.Parse(childNode.InnerText);
			}
			else if (childNode.Name == "CameraLocalOnly")
			{
				saveFileData.CameraLocalOnly = bool.Parse(childNode.InnerText);
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: settings\\" + childNode.Name);
			}
		}
	}

	private static void deserializeKeyboard(XmlNode keyboard, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in keyboard.ChildNodes)
		{
			bool flag = false;
			if (childNode.Name == "altBindings")
			{
				flag = true;
			}
			else if (childNode.Name != "bindings")
			{
				Debug.LogWarning("Unknown serialized field: settings\\keyboard\\" + childNode.Name);
				continue;
			}
			foreach (XmlNode childNode2 in childNode.ChildNodes)
			{
				InputEvent.InputKey inputKey = InputEvent.InputKey.NoKey;
				foreach (XmlAttribute attribute in childNode2.Attributes)
				{
					if (attribute.Name == "inputkey")
					{
						inputKey = (InputEvent.InputKey)Enum.Parse(typeof(InputEvent.InputKey), attribute.Value);
						break;
					}
					Debug.LogWarning("Unknown serialized field: settings\\keyboard\\" + (flag ? "altBindings." : "bindings.") + attribute.Name);
				}
				if (inputKey != InputEvent.InputKey.NoKey)
				{
					KeyCode keycode = (KeyCode)Enum.Parse(typeof(KeyCode), childNode2.InnerText);
					if (flag)
					{
						saveFileData.SetAltKeyBinding(inputKey, keycode);
					}
					else
					{
						saveFileData.SetKeyBinding(inputKey, keycode);
					}
				}
			}
		}
	}

	private static void deserializeStats(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "gamesPlayed")
			{
				deserializeGamesPlayed(childNode, saveFileData);
			}
			else if (childNode.Name == "piecesPlaced")
			{
				deserializePiecesPlaced(childNode, saveFileData);
			}
			else if (childNode.Name == "playerActions")
			{
				deserializePlayerActions(childNode, saveFileData);
			}
			else if (childNode.Name == "deaths")
			{
				deserializeDeaths(childNode, saveFileData);
			}
			else if (childNode.Name == "points")
			{
				deserializePoints(childNode, saveFileData);
			}
			else if (childNode.Name == "timePlayed")
			{
				deserializeTimePlayed(childNode, saveFileData);
			}
			else if (childNode.Name == "SkillMean")
			{
				saveFileData.SkillMean = Parsing.ParseDouble(childNode.InnerText);
				if (double.IsNaN(saveFileData.SkillMean))
				{
					saveFileData.SkillMean = 25.0;
				}
			}
			else if (childNode.Name == "SkillStdDev")
			{
				saveFileData.SkillStdDev = Parsing.ParseDouble(childNode.InnerText);
				if (double.IsNaN(saveFileData.SkillStdDev))
				{
					saveFileData.SkillStdDev = 8.333333333333334;
				}
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: stats\\" + childNode.Name);
			}
		}
	}

	private static void deserializeGamesPlayed(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "totalGames")
			{
				saveFileData.GetStat<StatCount>("GamesPlayed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "onlineGames")
			{
				saveFileData.GetStat<StatCount>("OnlineGamesPlayed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "partyGames")
			{
				saveFileData.GetStat<StatCount>("PartyModeGamesPlayed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "creativeGames")
			{
				saveFileData.GetStat<StatCount>("CreativeModeGamesPlayed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "sandboxGames")
			{
				saveFileData.GetStat<StatCount>("SandboxModeGamesPlayed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "gamesSinceLevel")
			{
				saveFileData.GetStat<StatCount>("GamesSinceLastLevelUnlocked").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "gamesSinceCharacterLevel")
			{
				saveFileData.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "gamesPerLevel")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "levelCount")
					{
						foreach (XmlAttribute attribute in childNode2.Attributes)
						{
							if (attribute.Name == "level")
							{
								saveFileData.GetStat<StatCountArray>("LevelsPlayed").Set(int.Parse(attribute.Value), int.Parse(childNode2.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: stats\\gamesPlayed\\gamesPerLevel\\levelCount." + attribute.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: stats\\gamesPlayed\\gamesPerLevel\\" + childNode2.Name);
					}
				}
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: stats\\gamesPlayed\\" + childNode.Name);
			}
		}
	}

	private static void deserializePiecesPlaced(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "totalPieces")
			{
				saveFileData.GetStat<StatCount>("PiecesPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "piecesDestroyed")
			{
				saveFileData.GetStat<StatCount>("PiecesDestroyed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "trapsPlaced")
			{
				saveFileData.GetStat<StatCount>("TrapsPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "trapsDestroyed")
			{
				saveFileData.GetStat<StatCount>("TrapsDestroyed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "platformsPlaced")
			{
				saveFileData.GetStat<StatCount>("PlatformsPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "platformsDestroyed")
			{
				saveFileData.GetStat<StatCount>("PlatformsDestroyed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "movingPlatformsPlaced")
			{
				saveFileData.GetStat<StatCount>("MovingPlatformsPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "movingPlatformsDestroyed")
			{
				saveFileData.GetStat<StatCount>("MovingPlatformsDestroyed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "attachmentsPlaced")
			{
				saveFileData.GetStat<StatCount>("AttachmentsPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "attachmentsDestroyed")
			{
				saveFileData.GetStat<StatCount>("AttachmentsDestroyed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "bombsPlaced")
			{
				saveFileData.GetStat<StatCount>("BombsPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "specialPlaced")
			{
				saveFileData.GetStat<StatCount>("SpecialPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "specialDestroyed")
			{
				saveFileData.GetStat<StatCount>("SpecialDestroyed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "itemsPlaced")
			{
				saveFileData.GetStat<StatCount>("ItemsPlaced").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "itemsDestroyed")
			{
				saveFileData.GetStat<StatCount>("ItemsDestroyed").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "piecesGlued")
			{
				saveFileData.GetStat<StatCount>("PiecesGlued").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "largeContraptionsPlaced")
			{
				saveFileData.GetStat<StatCount>("LargeContraptionsMade").Set(int.Parse(childNode.InnerText));
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: stats\\piecesPlaced\\" + childNode.Name);
			}
		}
	}

	private static void deserializePlayerActions(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "jumps")
			{
				saveFileData.GetStat<StatCount>("Jumps").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "wallJumps")
			{
				saveFileData.GetStat<StatCount>("WallJumps").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "timesTeleported")
			{
				saveFileData.GetStat<StatCount>("TimesTeleported").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "springBounces")
			{
				saveFileData.GetStat<StatCount>("SpringBounces").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "distanceRun")
			{
				saveFileData.GetStat<StatFloat>("DistanceRun").Set(Parsing.ParseFloat(childNode.InnerText));
			}
			else if (childNode.Name == "distanceSlid")
			{
				saveFileData.GetStat<StatFloat>("DistanceSlid").Set(Parsing.ParseFloat(childNode.InnerText));
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: stats\\playerActions\\" + childNode.Name);
			}
		}
	}

	private static void deserializeDeaths(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "totalDeaths")
			{
				saveFileData.GetStat<StatCount>("TotalDeaths").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByTrap")
			{
				saveFileData.GetStat<StatCount>("DeathsByTrap").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsBySuicide")
			{
				saveFileData.GetStat<StatCount>("DeathsBySuicide").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByFalling")
			{
				saveFileData.GetStat<StatCount>("DeathsByFalling").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByHazard")
			{
				saveFileData.GetStat<StatCount>("DeathsByHazard").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsBySpikeBall")
			{
				saveFileData.GetStat<StatCount>("DeathsBySpikeBall").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByBarbedWire")
			{
				saveFileData.GetStat<StatCount>("DeathsByBarbedWire").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByArrow")
			{
				saveFileData.GetStat<StatCount>("DeathsByArrow").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByTennisBall")
			{
				saveFileData.GetStat<StatCount>("DeathsByTennisBall").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsBySpinningSaw")
			{
				saveFileData.GetStat<StatCount>("DeathsBySpinningSaw").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByLinearSaw")
			{
				saveFileData.GetStat<StatCount>("DeathsByLinearSaw").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByPropeller")
			{
				saveFileData.GetStat<StatCount>("DeathsByPropeller").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByFlippingBlock")
			{
				saveFileData.GetStat<StatCount>("DeathsByFlippingBlock").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByBlackhole")
			{
				saveFileData.GetStat<StatCount>("DeathsByBlackHole").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByHockeyPuck")
			{
				saveFileData.GetStat<StatCount>("DeathsByHockeyPuck").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByPunchingPlant")
			{
				saveFileData.GetStat<StatCount>("DeathsByPunchingPlant").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByPressureTriggerSpikes")
			{
				saveFileData.GetStat<StatCount>("DeathsByPressureTriggerSpikes").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "deathsByWreckingBall")
			{
				saveFileData.GetStat<StatCount>("DeathsByWreckingBall").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "characterDeaths")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "charDeaths")
					{
						foreach (XmlAttribute attribute in childNode2.Attributes)
						{
							if (attribute.Name == "character")
							{
								saveFileData.GetStat<StatCountArray>("CharacterDeaths").Set(int.Parse(attribute.Value), int.Parse(childNode2.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: stats\\deaths\\characterDeaths\\charDeaths." + attribute.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: stats\\deaths\\characterDeaths\\" + childNode2.Name);
					}
				}
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: stats\\deaths\\" + childNode.Name);
			}
		}
	}

	private static void deserializePoints(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "coinsCollected")
			{
				saveFileData.GetStat<StatCount>("CoinsCollected").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "coinsLost")
			{
				saveFileData.GetStat<StatCount>("CoinsLost").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "coinsStolen")
			{
				saveFileData.GetStat<StatCount>("CoinsStolen").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "comebackPoints")
			{
				saveFileData.GetStat<StatCount>("ComebackPointsEarned").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "soloPoints")
			{
				saveFileData.GetStat<StatCount>("SoloPointsEarned").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "trapPoints")
			{
				saveFileData.GetStat<StatCount>("TrapPointsEarned").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "postmortemVictories")
			{
				saveFileData.GetStat<StatCount>("PostmortemVictories").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "characterWins")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "charWins")
					{
						foreach (XmlAttribute attribute in childNode2.Attributes)
						{
							if (attribute.Name == "character")
							{
								saveFileData.GetStat<StatCountArray>("CharacterWins").Set(int.Parse(attribute.Value), int.Parse(childNode2.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: stats\\points\\charWins." + attribute.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: stats\\points\\" + childNode2.Name);
					}
				}
			}
			else if (childNode.Name == "characterSuccess")
			{
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					if (childNode3.Name == "charSuccess")
					{
						foreach (XmlAttribute attribute2 in childNode3.Attributes)
						{
							if (attribute2.Name == "character")
							{
								saveFileData.GetStat<StatCountArray>("CharacterSuccess").Set(int.Parse(attribute2.Value), int.Parse(childNode3.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: stats\\points\\charSuccess." + attribute2.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: stats\\points\\" + childNode3.Name);
					}
				}
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: stats\\points\\" + childNode.Name);
			}
		}
	}

	private static void deserializeTimePlayed(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "totalMatchTime")
			{
				saveFileData.GetStat<StatFloat>("TotalMatchTime").Set(Parsing.ParseFloat(childNode.InnerText));
			}
			else if (childNode.Name == "totalRounds")
			{
				saveFileData.GetStat<StatCount>("TotalRounds").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "totalSuddenDeaths")
			{
				saveFileData.GetStat<StatCount>("TotalSuddenDeaths").Set(int.Parse(childNode.InnerText));
			}
			else if (childNode.Name == "totalLevelTime")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "levelTime")
					{
						foreach (XmlAttribute attribute in childNode2.Attributes)
						{
							if (attribute.Name == "level")
							{
								saveFileData.GetStat<StatFloatArray>("TotalLevelTime").Set(int.Parse(attribute.Value), Parsing.ParseFloat(childNode2.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: stats\\timePlayed\\totalLevelTime\\levelTime." + attribute.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: stats\\timePlayed\\levelTime" + childNode2.Name);
					}
				}
			}
			else if (childNode.Name == "totalLevelRounds")
			{
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					if (childNode3.Name == "levelRounds")
					{
						foreach (XmlAttribute attribute2 in childNode3.Attributes)
						{
							if (attribute2.Name == "level")
							{
								saveFileData.GetStat<StatCountArray>("TotalLevelRounds").Set(int.Parse(attribute2.Value), int.Parse(childNode3.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: stats\\timePlayed\\totalLevelRounds\\levelRounds." + attribute2.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: stats\\timePlayed\\totalLevelRounds" + childNode3.Name);
					}
				}
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: stats\\timePlayed\\" + childNode.Name);
			}
		}
	}

	private static void deserializeUnlocks(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "charactersUnlocked")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "charUnlock")
					{
						foreach (XmlAttribute attribute in childNode2.Attributes)
						{
							if (attribute.Name == "character")
							{
								saveFileData.GetStat<StatBoolArray>("CharactersUnlocked").Set(int.Parse(attribute.Value), bool.Parse(childNode2.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: unlocks\\charactersUnlocked\\charUnlock." + childNode.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: unlocks\\charactersUnlocked\\" + childNode.Name);
					}
				}
			}
			else if (childNode.Name == "outfitsUnlocked")
			{
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					if (childNode3.Name == "charOutfit")
					{
						foreach (XmlAttribute attribute2 in childNode3.Attributes)
						{
							if (attribute2.Name == "character")
							{
								saveFileData.GetStat<StatCountArray>("OutfitsUnlocked").Set(int.Parse(attribute2.Value), int.Parse(childNode3.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: unlocks\\charactersUnlocked\\charOutfit." + childNode.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: unlocks\\outfitsUnlocked\\" + childNode.Name);
					}
				}
			}
			else if (childNode.Name == "levelsUnlocked")
			{
				foreach (XmlNode childNode4 in childNode.ChildNodes)
				{
					if (childNode4.Name == "levelUnlock")
					{
						foreach (XmlAttribute attribute3 in childNode4.Attributes)
						{
							if (attribute3.Name == "level")
							{
								saveFileData.GetStat<StatBoolArray>("LevelsUnlocked").Set(int.Parse(attribute3.Value), bool.Parse(childNode4.InnerText));
							}
							else
							{
								Debug.LogWarning("Unknown serialized field: unlocks\\charactersUnlocked\\levelUnlock." + childNode.Name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Unknown serialized field: unlocks\\levelsUnlocked\\" + childNode.Name);
					}
				}
			}
			else if (childNode.Name == "CodeUsed")
			{
				saveFileData.GetStat<StatBool>("Cheater").Set(bool.Parse(childNode.InnerText));
			}
			else
			{
				Debug.LogWarning("Unknown serialized field: unlocks\\" + childNode.Name);
			}
		}
	}

	private static void deserializeExtraData(XmlNode node, SaveFileData saveFileData)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			WriteExtraData(childNode.Name, childNode.InnerText);
		}
	}

	private static void deserializeSnapshotCodeHistory(XmlNode node, SaveFileData saveFileData)
	{
		saveFileData.recentSnapshotEntries = new List<SaveFileData.RecentSnapshotEntry>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "code")
			{
				saveFileData.recentSnapshotEntries.Add(new SaveFileData.RecentSnapshotEntry
				{
					type = (SaveFileData.RecentSnapshotEntry.SnapshotType)int.Parse(childNode.Attributes["type"].Value),
					code = childNode.Attributes["value"].Value,
					name = childNode.Attributes["name"].Value
				});
			}
		}
	}

	private static void deserializeFavoriteSnapshots(XmlNode node, SaveFileData saveFileData)
	{
		saveFileData.favoriteSnapshots = new List<SaveFileData.FavoriteSnapshotEntry>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "favorite")
			{
				saveFileData.favoriteSnapshots.Add(new SaveFileData.FavoriteSnapshotEntry
				{
					code = GameSparksQuery.GetFormattedSnapshotCode(childNode.Attributes["code"].Value),
					name = childNode.Attributes["name"].Value
				});
			}
		}
	}

	private static void deserializeSnapshotSequenceNumbers(XmlNode node, SaveFileData saveFileData)
	{
		saveFileData.snapshotSequenceNumbers = new Dictionary<string, int>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "snapshotSequenceNumber")
			{
				saveFileData.snapshotSequenceNumbers.Add(childNode.Attributes["sceneName"].Value, int.Parse(childNode.Attributes["number"].Value));
			}
		}
	}

	private static void deserializeLocalSnapshotCodes(XmlNode node, SaveFileData saveFileData)
	{
		saveFileData.localSnapshotCodes = new Dictionary<string, string>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "localSnapshotCode")
			{
				saveFileData.localSnapshotCodes.Add(childNode.Attributes["filename"].Value, childNode.Attributes["code"].Value);
			}
		}
	}

	private static void deserializePortalSnapshotEntries(XmlNode node, SaveFileData saveFileData)
	{
		saveFileData.portalSnapshotEntries = new List<SaveFileData.PortalSnapshotEntry>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "portalSnapshotEntry")
			{
				XmlNode namedItem = childNode.Attributes.GetNamedItem("levelType");
				FeaturedQuickFilter.LevelTypes levelType = ((namedItem != null) ? ((FeaturedQuickFilter.LevelTypes)Enum.Parse(typeof(FeaturedQuickFilter.LevelTypes), namedItem.Value)) : FeaturedQuickFilter.LevelTypes.Any);
				saveFileData.portalSnapshotEntries.Add(new SaveFileData.PortalSnapshotEntry
				{
					name = childNode.Attributes["name"].Value,
					code = childNode.Attributes["code"].Value,
					levelType = levelType
				});
			}
		}
	}

	private static void Serialize(SaveFileData saveFileData)
	{
		new Parsing.EnsureInvariantCulture();
		xmlDoc = new XmlDocument();
		XmlNode xmlNode = xmlDoc.CreateElement("UCHSave");
		XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("version");
		xmlAttribute.Value = GameSettings.GetInstance().VersionNumber;
		xmlNode.Attributes.Append(xmlAttribute);
		XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("creationDate");
		xmlAttribute2.Value = saveFileData.creationDate;
		xmlNode.Attributes.Append(xmlAttribute2);
		XmlAttribute xmlAttribute3 = xmlDoc.CreateAttribute("lastSaveDate");
		xmlAttribute3.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
		xmlNode.Attributes.Append(xmlAttribute3);
		xmlDoc.AppendChild(xmlNode);
		xmlNode.AppendChild(serializeSettings(saveFileData));
		xmlNode.AppendChild(serializeStats(saveFileData));
		xmlNode.AppendChild(serializeUnlocks(saveFileData));
		xmlNode.AppendChild(serializeExtraData(saveFileData));
		xmlNode.AppendChild(serializeSnapshotHistory(saveFileData));
		xmlNode.AppendChild(serializeFavoriteSnapshots(saveFileData));
		xmlNode.AppendChild(serializeSnapshotSequenceNumbers(saveFileData));
		xmlNode.AppendChild(serializeLocalSnapshotCodes(saveFileData));
		xmlNode.AppendChild(serializePortalSnapshotEntries(saveFileData));
	}

	private static XmlNode serializeSettings(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("settings");
		xmlNode.AppendChild(CreateSimpleNode("sound", saveFileData.SoundVolume));
		xmlNode.AppendChild(CreateSimpleNode("music", saveFileData.MusicVolume));
		xmlNode.AppendChild(CreateSimpleNode("vsync", saveFileData.VSync ? 1 : 0));
		xmlNode.AppendChild(CreateSimpleNode("backgroundAudio", saveFileData.BackgroundAudio));
		xmlNode.AppendChild(CreateSimpleNode("hideVersion", saveFileData.HideVersion));
		xmlNode.AppendChild(CreateSimpleNode("AFKAutoKickTime", saveFileData.AFKAutoKickTime));
		xmlNode.AppendChild(CreateSimpleNode("CrossPlatformToggle", saveFileData.CrossPlatformToggle));
		xmlNode.AppendChild(CreateSimpleNode("OnlineChatEmotes", (int)saveFileData.OnlineChatEmotes));
		xmlNode.AppendChild(CreateSimpleNode("OnlinePlayerNames", (int)saveFileData.OnlinePlayerNames));
		xmlNode.AppendChild(CreateSimpleNode("CameraLocalOnly", saveFileData.CameraLocalOnly));
		if (saveFileData.language != null)
		{
			xmlNode.AppendChild(CreateSimpleNode("language", saveFileData.language));
		}
		XmlNode xmlNode2 = xmlDoc.CreateElement("keyboard");
		XmlNode xmlNode3 = xmlDoc.CreateElement("bindings");
		XmlNode xmlNode4 = xmlDoc.CreateElement("altBindings");
		InputEvent.InputKey[] array = (InputEvent.InputKey[])Enum.GetValues(typeof(InputEvent.InputKey));
		for (int i = 0; i != array.Length; i++)
		{
			InputEvent.InputKey inputKey = array[i];
			KeyCode? keyBinding = saveFileData.GetKeyBinding(array[i]);
			if (keyBinding.HasValue)
			{
				XmlNode xmlNode5 = xmlDoc.CreateElement("binding");
				XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("inputkey");
				xmlAttribute.Value = inputKey.ToString();
				xmlNode5.Attributes.Append(xmlAttribute);
				xmlNode5.InnerText = ((int)keyBinding.Value).ToString();
				xmlNode3.AppendChild(xmlNode5);
			}
			keyBinding = saveFileData.GetAltKeyBinding(array[i]);
			if (keyBinding.HasValue)
			{
				XmlNode xmlNode6 = xmlDoc.CreateElement("binding");
				XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("inputkey");
				xmlAttribute2.Value = inputKey.ToString();
				xmlNode6.Attributes.Append(xmlAttribute2);
				xmlNode6.InnerText = ((int)keyBinding.Value).ToString();
				xmlNode4.AppendChild(xmlNode6);
			}
		}
		xmlNode2.AppendChild(xmlNode3);
		xmlNode2.AppendChild(xmlNode4);
		xmlNode.AppendChild(xmlNode2);
		return xmlNode;
	}

	private static XmlNode serializeStats(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlElement xmlElement = xmlDoc.CreateElement("stats");
		xmlElement.AppendChild(serializeGamesPlayed(saveFileData));
		xmlElement.AppendChild(serializePiecesPlaced(saveFileData));
		xmlElement.AppendChild(serializePlayerActions(saveFileData));
		xmlElement.AppendChild(serializeDeaths(saveFileData));
		xmlElement.AppendChild(serializePoints(saveFileData));
		xmlElement.AppendChild(serializeTimePlayed(saveFileData));
		xmlElement.AppendChild(CreateSimpleNode("SkillMean", saveFileData.SkillMean));
		xmlElement.AppendChild(CreateSimpleNode("SkillStdDev", saveFileData.SkillStdDev));
		return xmlElement;
	}

	private static XmlNode serializeUnlocks(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("unlocks");
		GameState.LevelName[] array = (GameState.LevelName[])Enum.GetValues(typeof(GameState.LevelName));
		Character.Animals[] array2 = (Character.Animals[])Enum.GetValues(typeof(Character.Animals));
		XmlNode xmlNode2 = xmlDoc.CreateElement("charactersUnlocked");
		for (int i = 0; i != array2.Length; i++)
		{
			XmlNode xmlNode3 = xmlDoc.CreateElement("charUnlock");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("character");
			int num = (int)array2[i];
			xmlAttribute.Value = num.ToString();
			xmlNode3.Attributes.Append(xmlAttribute);
			xmlNode3.InnerText = saveFileData.GetStat<StatBoolArray>("CharactersUnlocked").values[(int)array2[i]].ToString();
			xmlNode2.AppendChild(xmlNode3);
		}
		xmlNode.AppendChild(xmlNode2);
		XmlNode xmlNode4 = xmlDoc.CreateElement("outfitsUnlocked");
		for (int j = 0; j != array2.Length; j++)
		{
			XmlNode xmlNode5 = xmlDoc.CreateElement("charOutfit");
			XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("character");
			int num = (int)array2[j];
			xmlAttribute2.Value = num.ToString();
			xmlNode5.Attributes.Append(xmlAttribute2);
			xmlNode5.InnerText = saveFileData.GetStat<StatCountArray>("OutfitsUnlocked").values[(int)array2[j]].ToString();
			xmlNode4.AppendChild(xmlNode5);
		}
		xmlNode.AppendChild(xmlNode4);
		XmlNode xmlNode6 = xmlDoc.CreateElement("levelsUnlocked");
		for (int k = 0; k != array.Length; k++)
		{
			int num2 = (int)array[k];
			if (num2 < 100)
			{
				XmlNode xmlNode7 = xmlDoc.CreateElement("levelUnlock");
				XmlAttribute xmlAttribute3 = xmlDoc.CreateAttribute("level");
				xmlAttribute3.Value = num2.ToString();
				xmlNode7.Attributes.Append(xmlAttribute3);
				xmlNode7.InnerText = saveFileData.GetStat<StatBoolArray>("LevelsUnlocked").values[num2].ToString();
				xmlNode6.AppendChild(xmlNode7);
			}
		}
		xmlNode.AppendChild(xmlNode6);
		xmlNode.AppendChild(CreateSimpleNode("CodeUsed", saveFileData.IsCheater));
		return xmlNode;
	}

	private static XmlNode serializeExtraData(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("extra");
		foreach (string key in extraData.Keys)
		{
			xmlNode.AppendChild(CreateSimpleNode(key, extraData[key]));
		}
		return xmlNode;
	}

	private static XmlNode serializeSnapshotHistory(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("snapshotCodeHistory");
		foreach (SaveFileData.RecentSnapshotEntry recentSnapshotEntry in saveFileData.recentSnapshotEntries)
		{
			XmlElement xmlElement = xmlDoc.CreateElement("code");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("value");
			xmlAttribute.Value = recentSnapshotEntry.code;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("type");
			int type = (int)recentSnapshotEntry.type;
			xmlAttribute2.Value = type.ToString();
			xmlElement.Attributes.Append(xmlAttribute2);
			XmlAttribute xmlAttribute3 = xmlDoc.CreateAttribute("name");
			xmlAttribute3.Value = recentSnapshotEntry.name;
			xmlElement.Attributes.Append(xmlAttribute3);
			xmlNode.AppendChild(xmlElement);
		}
		return xmlNode;
	}

	private static XmlNode serializeFavoriteSnapshots(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("favoriteSnapshots");
		foreach (SaveFileData.FavoriteSnapshotEntry favoriteSnapshot in saveFileData.favoriteSnapshots)
		{
			XmlElement xmlElement = xmlDoc.CreateElement("favorite");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("code");
			xmlAttribute.Value = favoriteSnapshot.code;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("name");
			xmlAttribute2.Value = favoriteSnapshot.name;
			xmlElement.Attributes.Append(xmlAttribute2);
			xmlNode.AppendChild(xmlElement);
		}
		return xmlNode;
	}

	private static XmlNode serializeSnapshotSequenceNumbers(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("snapshotSequenceNumbers");
		foreach (KeyValuePair<string, int> snapshotSequenceNumber in saveFileData.snapshotSequenceNumbers)
		{
			XmlElement xmlElement = xmlDoc.CreateElement("snapshotSequenceNumber");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("sceneName");
			xmlAttribute.Value = snapshotSequenceNumber.Key;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("number");
			xmlAttribute2.Value = snapshotSequenceNumber.Value.ToString();
			xmlElement.Attributes.Append(xmlAttribute2);
			xmlNode.AppendChild(xmlElement);
		}
		return xmlNode;
	}

	private static XmlNode serializeLocalSnapshotCodes(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("localSnapshotCodes");
		foreach (KeyValuePair<string, string> localSnapshotCode in saveFileData.localSnapshotCodes)
		{
			XmlElement xmlElement = xmlDoc.CreateElement("localSnapshotCode");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("filename");
			xmlAttribute.Value = localSnapshotCode.Key;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("code");
			xmlAttribute2.Value = localSnapshotCode.Value;
			xmlElement.Attributes.Append(xmlAttribute2);
			xmlNode.AppendChild(xmlElement);
		}
		return xmlNode;
	}

	private static XmlNode serializePortalSnapshotEntries(SaveFileData saveFileData)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlNode xmlNode = xmlDoc.CreateElement("portalSnapshotEntries");
		foreach (SaveFileData.PortalSnapshotEntry portalSnapshotEntry in saveFileData.portalSnapshotEntries)
		{
			XmlElement xmlElement = xmlDoc.CreateElement("portalSnapshotEntry");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("name");
			xmlAttribute.Value = portalSnapshotEntry.name;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("code");
			xmlAttribute2.Value = portalSnapshotEntry.code;
			xmlElement.Attributes.Append(xmlAttribute2);
			xmlNode.AppendChild(xmlElement);
		}
		return xmlNode;
	}

	private static XmlNode serializeGamesPlayed(SaveFileData saveFileData)
	{
		_ = StatTracker.Instance;
		XmlNode xmlNode = xmlDoc.CreateElement("gamesPlayed");
		xmlNode.AppendChild(CreateSimpleNode("totalGames", saveFileData.GetStat<StatCount>("GamesPlayed").count));
		xmlNode.AppendChild(CreateSimpleNode("onlineGames", saveFileData.GetStat<StatCount>("OnlineGamesPlayed").count));
		xmlNode.AppendChild(CreateSimpleNode("partyGames", saveFileData.GetStat<StatCount>("PartyModeGamesPlayed").count));
		xmlNode.AppendChild(CreateSimpleNode("creativeGames", saveFileData.GetStat<StatCount>("CreativeModeGamesPlayed").count));
		xmlNode.AppendChild(CreateSimpleNode("sandboxGames", saveFileData.GetStat<StatCount>("SandboxModeGamesPlayed").count));
		xmlNode.AppendChild(CreateSimpleNode("gamesSinceLevel", saveFileData.GetStat<StatCount>("GamesSinceLastLevelUnlocked").count));
		xmlNode.AppendChild(CreateSimpleNode("gamesSinceCharacterLevel", saveFileData.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked").count));
		XmlNode xmlNode2 = xmlDoc.CreateElement("gamesPerLevel");
		GameState.LevelName[] array = (GameState.LevelName[])Enum.GetValues(typeof(GameState.LevelName));
		for (int i = 0; i != array.Length; i++)
		{
			int num = (int)array[i];
			if (num < 100)
			{
				XmlNode xmlNode3 = xmlDoc.CreateElement("levelCount");
				XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("level");
				xmlAttribute.Value = num.ToString();
				xmlNode3.Attributes.Append(xmlAttribute);
				xmlNode3.InnerText = saveFileData.GetStat<StatCountArray>("LevelsPlayed").values[num].ToString();
				xmlNode2.AppendChild(xmlNode3);
			}
		}
		xmlNode.AppendChild(xmlNode2);
		return xmlNode;
	}

	private static XmlNode serializePiecesPlaced(SaveFileData saveFileData)
	{
		_ = StatTracker.Instance;
		XmlElement xmlElement = xmlDoc.CreateElement("piecesPlaced");
		xmlElement.AppendChild(CreateSimpleNode("totalPieces", saveFileData.GetStat<StatCount>("PiecesPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("piecesDestroyed", saveFileData.GetStat<StatCount>("PiecesDestroyed").count));
		xmlElement.AppendChild(CreateSimpleNode("trapsPlaced", saveFileData.GetStat<StatCount>("TrapsPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("trapsDestroyed", saveFileData.GetStat<StatCount>("TrapsDestroyed").count));
		xmlElement.AppendChild(CreateSimpleNode("platformsPlaced", saveFileData.GetStat<StatCount>("PlatformsPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("platformsDestroyed", saveFileData.GetStat<StatCount>("PlatformsDestroyed").count));
		xmlElement.AppendChild(CreateSimpleNode("movingPlatformsPlaced", saveFileData.GetStat<StatCount>("MovingPlatformsPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("movingPlatformsDestroyed", saveFileData.GetStat<StatCount>("MovingPlatformsDestroyed").count));
		xmlElement.AppendChild(CreateSimpleNode("attachmentsPlaced", saveFileData.GetStat<StatCount>("AttachmentsPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("attachmentsDestroyed", saveFileData.GetStat<StatCount>("AttachmentsDestroyed").count));
		xmlElement.AppendChild(CreateSimpleNode("bombsPlaced", saveFileData.GetStat<StatCount>("BombsPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("specialPlaced", saveFileData.GetStat<StatCount>("SpecialPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("specialDestroyed", saveFileData.GetStat<StatCount>("SpecialDestroyed").count));
		xmlElement.AppendChild(CreateSimpleNode("itemsPlaced", saveFileData.GetStat<StatCount>("ItemsPlaced").count));
		xmlElement.AppendChild(CreateSimpleNode("itemsDestroyed", saveFileData.GetStat<StatCount>("ItemsDestroyed").count));
		xmlElement.AppendChild(CreateSimpleNode("piecesGlued", saveFileData.GetStat<StatCount>("PiecesGlued").count));
		xmlElement.AppendChild(CreateSimpleNode("largeContraptionsPlaced", saveFileData.GetStat<StatCount>("LargeContraptionsMade").count));
		return xmlElement;
	}

	private static XmlNode serializePlayerActions(SaveFileData saveFileData)
	{
		_ = StatTracker.Instance;
		XmlElement xmlElement = xmlDoc.CreateElement("playerActions");
		xmlElement.AppendChild(CreateSimpleNode("jumps", saveFileData.GetStat<StatCount>("Jumps").count));
		xmlElement.AppendChild(CreateSimpleNode("wallJumps", saveFileData.GetStat<StatCount>("WallJumps").count));
		xmlElement.AppendChild(CreateSimpleNode("timesTeleported", saveFileData.GetStat<StatCount>("TimesTeleported").count));
		xmlElement.AppendChild(CreateSimpleNode("springBounces", saveFileData.GetStat<StatCount>("SpringBounces").count));
		xmlElement.AppendChild(CreateSimpleNode("distanceRun", saveFileData.GetStat<StatFloat>("DistanceRun").value));
		xmlElement.AppendChild(CreateSimpleNode("distanceSlid", saveFileData.GetStat<StatFloat>("DistanceSlid").value));
		return xmlElement;
	}

	private static XmlNode serializeDeaths(SaveFileData saveFileData)
	{
		_ = StatTracker.Instance;
		XmlNode xmlNode = xmlDoc.CreateElement("deaths");
		xmlNode.AppendChild(CreateSimpleNode("totalDeaths", saveFileData.GetStat<StatCount>("TotalDeaths").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByTrap", saveFileData.GetStat<StatCount>("DeathsByTrap").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsBySuicide", saveFileData.GetStat<StatCount>("DeathsBySuicide").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByFalling", saveFileData.GetStat<StatCount>("DeathsByFalling").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByHazard", saveFileData.GetStat<StatCount>("DeathsByHazard").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsBySpikeBall", saveFileData.GetStat<StatCount>("DeathsBySpikeBall").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByBarbedWire", saveFileData.GetStat<StatCount>("DeathsByBarbedWire").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByArrow", saveFileData.GetStat<StatCount>("DeathsByArrow").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByTennisBall", saveFileData.GetStat<StatCount>("DeathsByTennisBall").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsBySpinningSaw", saveFileData.GetStat<StatCount>("DeathsBySpinningSaw").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByLinearSaw", saveFileData.GetStat<StatCount>("DeathsByLinearSaw").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByPropeller", saveFileData.GetStat<StatCount>("DeathsByPropeller").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByFlippingBlock", saveFileData.GetStat<StatCount>("DeathsByFlippingBlock").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByBlackhole", saveFileData.GetStat<StatCount>("DeathsByBlackHole").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByHockeyPuck", saveFileData.GetStat<StatCount>("DeathsByHockeyPuck").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByPunchingPlant", saveFileData.GetStat<StatCount>("DeathsByPunchingPlant").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByPressureTriggerSpikes", saveFileData.GetStat<StatCount>("DeathsByPressureTriggerSpikes").count));
		xmlNode.AppendChild(CreateSimpleNode("deathsByWreckingBall", saveFileData.GetStat<StatCount>("DeathsByWreckingBall").count));
		XmlNode xmlNode2 = xmlDoc.CreateElement("characterDeaths");
		Character.Animals[] array = (Character.Animals[])Enum.GetValues(typeof(Character.Animals));
		for (int i = 0; i != array.Length; i++)
		{
			XmlNode xmlNode3 = xmlDoc.CreateElement("charDeaths");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("character");
			int num = (int)array[i];
			xmlAttribute.Value = num.ToString();
			xmlNode3.Attributes.Append(xmlAttribute);
			xmlNode3.InnerText = saveFileData.GetStat<StatCountArray>("CharacterDeaths").values[(int)array[i]].ToString();
			xmlNode2.AppendChild(xmlNode3);
		}
		xmlNode.AppendChild(xmlNode2);
		return xmlNode;
	}

	private static XmlNode serializePoints(SaveFileData saveFileData)
	{
		_ = StatTracker.Instance;
		XmlNode xmlNode = xmlDoc.CreateElement("points");
		xmlNode.AppendChild(CreateSimpleNode("coinsCollected", saveFileData.GetStat<StatCount>("CoinsCollected").count));
		xmlNode.AppendChild(CreateSimpleNode("coinsLost", saveFileData.GetStat<StatCount>("CoinsLost").count));
		xmlNode.AppendChild(CreateSimpleNode("coinsStolen", saveFileData.GetStat<StatCount>("CoinsStolen").count));
		xmlNode.AppendChild(CreateSimpleNode("comebackPoints", saveFileData.GetStat<StatCount>("ComebackPointsEarned").count));
		xmlNode.AppendChild(CreateSimpleNode("soloPoints", saveFileData.GetStat<StatCount>("SoloPointsEarned").count));
		xmlNode.AppendChild(CreateSimpleNode("trapPoints", saveFileData.GetStat<StatCount>("TrapPointsEarned").count));
		xmlNode.AppendChild(CreateSimpleNode("postmortemVictories", saveFileData.GetStat<StatCount>("PostmortemVictories").count));
		XmlNode xmlNode2 = xmlDoc.CreateElement("characterWins");
		Character.Animals[] array = (Character.Animals[])Enum.GetValues(typeof(Character.Animals));
		for (int i = 0; i != array.Length; i++)
		{
			XmlNode xmlNode3 = xmlDoc.CreateElement("charWins");
			XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("character");
			int num = (int)array[i];
			xmlAttribute.Value = num.ToString();
			xmlNode3.Attributes.Append(xmlAttribute);
			xmlNode3.InnerText = saveFileData.GetStat<StatCountArray>("CharacterWins").values[(int)array[i]].ToString();
			xmlNode2.AppendChild(xmlNode3);
		}
		xmlNode.AppendChild(xmlNode2);
		XmlNode xmlNode4 = xmlDoc.CreateElement("characterSuccess");
		for (int j = 0; j != array.Length; j++)
		{
			XmlNode xmlNode5 = xmlDoc.CreateElement("charSuccess");
			XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("character");
			int num = (int)array[j];
			xmlAttribute2.Value = num.ToString();
			xmlNode5.Attributes.Append(xmlAttribute2);
			xmlNode5.InnerText = saveFileData.GetStat<StatCountArray>("CharacterSuccess").values[(int)array[j]].ToString();
			xmlNode4.AppendChild(xmlNode5);
		}
		xmlNode.AppendChild(xmlNode4);
		return xmlNode;
	}

	private static XmlNode serializeTimePlayed(SaveFileData saveFileData)
	{
		_ = StatTracker.Instance;
		XmlNode xmlNode = xmlDoc.CreateElement("timePlayed");
		xmlNode.AppendChild(CreateSimpleNode("totalMatchTime", saveFileData.GetStat<StatFloat>("TotalMatchTime").value));
		xmlNode.AppendChild(CreateSimpleNode("totalRounds", saveFileData.GetStat<StatCount>("TotalRounds").count));
		xmlNode.AppendChild(CreateSimpleNode("totalSuddenDeaths", saveFileData.GetStat<StatCount>("TotalSuddenDeaths").count));
		XmlNode xmlNode2 = xmlDoc.CreateElement("totalLevelTime");
		GameState.LevelName[] array = (GameState.LevelName[])Enum.GetValues(typeof(GameState.LevelName));
		for (int i = 0; i != array.Length; i++)
		{
			int num = (int)array[i];
			if (num < 100)
			{
				XmlNode xmlNode3 = xmlDoc.CreateElement("levelTime");
				XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("level");
				xmlAttribute.Value = num.ToString();
				xmlNode3.Attributes.Append(xmlAttribute);
				xmlNode3.InnerText = saveFileData.GetStat<StatFloatArray>("TotalLevelTime").values[num].ToString();
				xmlNode2.AppendChild(xmlNode3);
			}
		}
		xmlNode.AppendChild(xmlNode2);
		XmlNode xmlNode4 = xmlDoc.CreateElement("totalLevelRounds");
		for (int j = 0; j != array.Length; j++)
		{
			int num2 = (int)array[j];
			if (num2 < 100)
			{
				XmlNode xmlNode5 = xmlDoc.CreateElement("levelRounds");
				XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("level");
				xmlAttribute2.Value = num2.ToString();
				xmlNode5.Attributes.Append(xmlAttribute2);
				xmlNode5.InnerText = saveFileData.GetStat<StatCountArray>("TotalLevelRounds").values[num2].ToString();
				xmlNode4.AppendChild(xmlNode5);
			}
		}
		xmlNode.AppendChild(xmlNode4);
		return xmlNode;
	}

	public static string GetSaveText(SaveFileData saveFileData)
	{
		Serialize(saveFileData);
		if (Application.isEditor && GameSettings.GetInstance().SaveDebugXML)
		{
			xmlDoc.Save(Application.dataPath + "/save.xml");
		}
		return xmlDoc.InnerXml;
	}

	public static void Load(string xml, SaveFileData saveFileData)
	{
		xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(xml);
		Deserialize(saveFileData);
		if (AchievementChecker.PlatformWantsFreshStats)
		{
			saveFileData.ClearAllDirtyFlags();
		}
	}

	private static void printNodeChildren(XmlNode node, int depth)
	{
		if (depth < 0)
		{
			depth = 0;
		}
		string text = node.Name;
		for (int i = 0; i != depth; i++)
		{
			text = "  " + text;
		}
		foreach (XmlAttribute attribute in node.Attributes)
		{
			text = text + "  " + attribute.Name + ": " + attribute.Value;
		}
		Debug.Log(text);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Text)
			{
				string text2 = childNode.InnerText;
				for (int j = 0; j != depth + 1; j++)
				{
					text2 = "  " + text2;
				}
				Debug.Log(text2);
			}
			else
			{
				printNodeChildren(childNode, depth + 1);
			}
		}
	}
}
