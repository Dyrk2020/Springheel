using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;

public class GameRulePreset : ScriptableObject
{
	[Serializable]
	public struct PointData
	{
		public PointBlock.pointBlockType Type;

		public bool Enabled;

		public int Value;

		public bool AlwaysAward;

		public bool RequiresWin;

		public bool HotseatAllowed;
	}

	[Serializable]
	public struct BlockData
	{
		public Placeable BlockPlaceable;

		public int Frequency;

		public const int DefaultFrequency = 5;

		public const int MaxFrequency = 9;

		public bool Enabled => Frequency > 0;

		public BlockData(Placeable placeable)
		{
			BlockPlaceable = placeable;
			Frequency = TabletBlockList.GetStepValueFromRarity(placeable.BaseRarity);
		}
	}

	public string Name = "Default";

	public string Description = "5 points, 12 rounds, all blocks. No points for failure. No points for cooperation.";

	public bool IsPremade;

	public bool NameHasLocalization;

	public string PathToFile = "";

	[Header("Rules")]
	public int MaxScore = 5;

	public int MaxRounds = 12;

	public int MaxTime = 60;

	public float PlaceTime = 30f;

	public bool UsePlaceTimer = true;

	public GameLimitType GameLimitType = GameLimitType.ROUNDS;

	public DoublePartyBox DoublePartyBox = DoublePartyBox.TwoPlayers;

	public int RunTimerLimit;

	public int CreativePiecesPerRound = 1;

	public RespawnMode respawnMode;

	public int numRespawns;

	public PartyBoxMode partyBoxMode;

	public bool competitiveRandomizer;

	protected Dictionary<PointBlock.pointBlockType, PointData> points;

	[SerializeField]
	[Header("Points")]
	protected PointData winDefault;

	[SerializeField]
	protected PointData winSoloDefault;

	[SerializeField]
	protected PointData trapDefault;

	[SerializeField]
	protected PointData comebackDefault;

	[SerializeField]
	protected PointData coinDefault;

	[SerializeField]
	protected PointData postmortemDefault;

	[SerializeField]
	protected PointData firstDefault;

	[SerializeField]
	protected PointData secondDefault;

	[SerializeField]
	protected PointData thirdDefault;

	[SerializeField]
	protected PointData fourthDefault;

	[Header("Blocks")]
	public BlockData[] Blocks;

	[Header("Modifiers")]
	public ModSource mods = new ModSource();

	public string LocalizedName => "RuleBook/Presets/" + Name;

	public string LocalizedDescription => "RuleBook/Presets/" + Name + "Description";

	public IEnumerable<PointBlock.pointBlockType> PointTypes
	{
		get
		{
			if (points == null)
			{
				setupPoints();
			}
			return points.Keys;
		}
	}

	public int BlockFrequency(Placeable block)
	{
		for (int i = 0; i != Blocks.Length; i++)
		{
			if (Blocks[i].BlockPlaceable == block)
			{
				return Blocks[i].Frequency;
			}
		}
		return 0;
	}

	private void setupPoints()
	{
		points = new Dictionary<PointBlock.pointBlockType, PointData>();
		points.Add(PointBlock.pointBlockType.win, winDefault);
		points.Add(PointBlock.pointBlockType.soloWin, winSoloDefault);
		points.Add(PointBlock.pointBlockType.trap, trapDefault);
		points.Add(PointBlock.pointBlockType.comeback, comebackDefault);
		points.Add(PointBlock.pointBlockType.coin, coinDefault);
		points.Add(PointBlock.pointBlockType.winDead, postmortemDefault);
		points.Add(PointBlock.pointBlockType.first, firstDefault);
		points.Add(PointBlock.pointBlockType.second, secondDefault);
		points.Add(PointBlock.pointBlockType.third, thirdDefault);
		points.Add(PointBlock.pointBlockType.fourth, fourthDefault);
	}

	public bool PointTypeEnabled(PointBlock.pointBlockType type)
	{
		if (points == null)
		{
			setupPoints();
		}
		return points[type].Enabled;
	}

	public bool AlwaysAwardPointType(PointBlock.pointBlockType type)
	{
		if (points == null)
		{
			setupPoints();
		}
		return points[type].AlwaysAward;
	}

	public int PointTypeValue(PointBlock.pointBlockType type)
	{
		if (points == null)
		{
			setupPoints();
		}
		return points[type].Value;
	}

	public bool AnyWinnerPointsEnabled()
	{
		if (points == null)
		{
			setupPoints();
		}
		foreach (KeyValuePair<PointBlock.pointBlockType, PointData> point in points)
		{
			if (point.Value.Enabled && point.Value.RequiresWin && point.Value.AlwaysAward)
			{
				return true;
			}
		}
		return false;
	}

	public bool PointTypeRequiresWin(PointBlock.pointBlockType type)
	{
		if (points == null)
		{
			setupPoints();
		}
		return points[type].RequiresWin;
	}

	public bool PointTypeAllowedForHotseat(PointBlock.pointBlockType type)
	{
		if (points == null)
		{
			setupPoints();
		}
		return points[type].HotseatAllowed;
	}

	public void LoadRulesFromSettings()
	{
		GameSettings instance = GameSettings.GetInstance();
		MaxScore = instance.MaxScore;
		MaxRounds = instance.MaxRounds;
		MaxTime = instance.MaxTime;
		PlaceTime = instance.PlaceTime;
		UsePlaceTimer = instance.UsePlaceTimer;
		GameLimitType = instance.GameLimitType;
		DoublePartyBox = instance.DoublePartyBox;
		RunTimerLimit = instance.RunTimerLimit;
		CreativePiecesPerRound = instance.CreativePiecesPerRound;
		respawnMode = instance.respawnMode;
		numRespawns = instance.numRespawns;
		partyBoxMode = instance.partyBoxMode;
		competitiveRandomizer = instance.competitiveRandomizer;
		points = new Dictionary<PointBlock.pointBlockType, PointData>();
		foreach (PointBlock.pointBlockType pointType in instance.PointTypes)
		{
			PointData value = new PointData
			{
				AlwaysAward = instance.AlwaysAwardPointType(pointType),
				Enabled = instance.PointTypeEnabled(pointType),
				HotseatAllowed = instance.PointTypeAllowedForHotseat(pointType),
				RequiresWin = instance.PointTypeRequiresWin(pointType),
				Type = pointType,
				Value = instance.PointTypeValue(pointType)
			};
			points.Add(pointType, value);
		}
		Blocks = new BlockData[instance.itemFilter.Count];
		instance.itemFilter.Values.CopyTo(Blocks, 0);
		mods.ReadFromModSettings();
	}

	public bool IsCurrentlyApplied(bool checkRules, bool checkPoints, bool checkBlocks, bool checkMods)
	{
		GameSettings instance = GameSettings.GetInstance();
		if (checkRules)
		{
			if (MaxScore != instance.MaxScore)
			{
				return false;
			}
			if (GameLimitType == GameLimitType.ROUNDS && MaxRounds != instance.MaxRounds)
			{
				return false;
			}
			if (GameLimitType == GameLimitType.TIME && MaxTime != instance.MaxTime)
			{
				return false;
			}
			if (PlaceTime != instance.PlaceTime)
			{
				return false;
			}
			if (UsePlaceTimer != instance.UsePlaceTimer)
			{
				return false;
			}
			if (GameLimitType != instance.GameLimitType)
			{
				return false;
			}
			if (DoublePartyBox != instance.DoublePartyBox)
			{
				return false;
			}
			if (RunTimerLimit != instance.RunTimerLimit)
			{
				return false;
			}
			if (CreativePiecesPerRound != instance.CreativePiecesPerRound)
			{
				return false;
			}
			if (respawnMode != instance.respawnMode)
			{
				return false;
			}
			if (numRespawns != instance.numRespawns)
			{
				return false;
			}
			if (partyBoxMode != instance.partyBoxMode)
			{
				return false;
			}
			if (competitiveRandomizer != instance.competitiveRandomizer)
			{
				return false;
			}
		}
		if (checkPoints)
		{
			if (points == null)
			{
				setupPoints();
			}
			foreach (KeyValuePair<PointBlock.pointBlockType, PointData> point in points)
			{
				PointData value = point.Value;
				if (instance.PointTypeEnabled(point.Key) != value.Enabled)
				{
					return false;
				}
				if (instance.PointTypeValue(point.Key) != value.Value)
				{
					return false;
				}
				if (instance.AlwaysAwardPointType(point.Key) != value.AlwaysAward)
				{
					return false;
				}
			}
		}
		if (checkBlocks)
		{
			if (Blocks == null)
			{
				return false;
			}
			BlockData[] blocks = Blocks;
			for (int i = 0; i < blocks.Length; i++)
			{
				BlockData blockData = blocks[i];
				if (!(blockData.BlockPlaceable == null))
				{
					if (!instance.itemFilter.ContainsKey(blockData.BlockPlaceable))
					{
						return false;
					}
					BlockData blockData2 = instance.itemFilter[blockData.BlockPlaceable];
					if (blockData.Frequency != blockData2.Frequency)
					{
						return false;
					}
				}
			}
		}
		if (checkMods && !mods.IsCurrentlyApplied())
		{
			return false;
		}
		return true;
	}

	public void LoadRulesetFromXML(XmlDocument xml)
	{
		GameSettings instance = GameSettings.GetInstance();
		XmlElement documentElement = xml.DocumentElement;
		Name = QuickSaver.ParseAttrStr(documentElement, "Name", "???");
		Description = QuickSaver.ParseAttrStr(documentElement, "Description", "???");
		IsPremade = QuickSaver.ParseAttrBool(documentElement, "Premade");
		NameHasLocalization = IsPremade;
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name == "rules")
			{
				MaxScore = QuickSaver.ParseAttrInt(childNode, "MaxScore", 5);
				MaxRounds = QuickSaver.ParseAttrInt(childNode, "MaxRounds", 12);
				MaxTime = QuickSaver.ParseAttrInt(childNode, "MaxTime", 60);
				PlaceTime = QuickSaver.ParseAttrFloat(childNode, "PlaceTime", 30f);
				UsePlaceTimer = QuickSaver.ParseAttrBool(childNode, "UsePlaceTimer", defaultValue: true);
				GameLimitType = QuickSaver.ParseAttrEnum(childNode, "GameLimitType", GameLimitType.ROUNDS);
				RunTimerLimit = QuickSaver.ParseAttrInt(childNode, "RunTimerLimit");
				CreativePiecesPerRound = QuickSaver.ParseAttrInt(childNode, "CreativePiecesPerRound", 1);
				respawnMode = QuickSaver.ParseAttrEnum(childNode, "RespawnMode", RespawnMode.Off);
				numRespawns = QuickSaver.ParseAttrInt(childNode, "NumRespawns");
				partyBoxMode = QuickSaver.ParseAttrEnum(childNode, "PartyBoxMode", PartyBoxMode.Standard);
				competitiveRandomizer = QuickSaver.ParseAttrBool(childNode, "CompetitiveRandomizer");
				XmlAttribute xmlAttribute = childNode.Attributes["DoublePartyBox"];
				if (xmlAttribute != null)
				{
					try
					{
						if (bool.Parse(xmlAttribute.Value))
						{
							DoublePartyBox = DoublePartyBox.TwoPlayers;
						}
						else
						{
							DoublePartyBox = DoublePartyBox.Off;
						}
					}
					catch (FormatException)
					{
						DoublePartyBox = QuickSaver.ParseAttrEnum(childNode, "DoublePartyBox", DoublePartyBox.TwoPlayers);
					}
				}
				MaxScore = Mathf.Clamp(MaxScore, instance.minMaxScore, instance.maxMaxScore);
				MaxRounds = Mathf.Clamp(MaxRounds, instance.minMaxRounds, instance.maxMaxRounds);
				MaxTime = Mathf.Clamp(MaxTime, instance.minMaxTime, instance.maxMaxTime);
				PlaceTime = Mathf.Clamp(PlaceTime, 0f, instance.MaxPlaceTime);
				RunTimerLimit = Mathf.Clamp(RunTimerLimit, instance.minRunTimer, instance.maxRunTimer);
				CreativePiecesPerRound = Mathf.Clamp(CreativePiecesPerRound, 1, instance.MaxCreativePieces);
				numRespawns = Mathf.Clamp(numRespawns, instance.minRespawns, instance.maxRespawns);
			}
			if (childNode.Name == "points")
			{
				points = new Dictionary<PointBlock.pointBlockType, PointData>();
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					PointData value = default(PointData);
					value.Type = QuickSaver.ParseAttrEnum(childNode2, "Type", PointBlock.pointBlockType.win);
					value.AlwaysAward = QuickSaver.ParseAttrBool(childNode2, "AlwaysAward");
					value.Enabled = QuickSaver.ParseAttrBool(childNode2, "Enabled", defaultValue: true);
					value.HotseatAllowed = QuickSaver.ParseAttrBool(childNode2, "HotseatAllowed", defaultValue: true);
					value.RequiresWin = QuickSaver.ParseAttrBool(childNode2, "RequiresWin", value.Type != PointBlock.pointBlockType.trap);
					int value2 = QuickSaver.ParseAttrInt(childNode2, "Value", 50);
					value.Value = Mathf.Clamp(value2, instance.minPointValue, instance.maxPointValue);
					if (!points.ContainsKey(value.Type))
					{
						points.Add(value.Type, value);
					}
				}
			}
			if (childNode.Name == "blocks")
			{
				Dictionary<int, BlockData> dictionary = new Dictionary<int, BlockData>();
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					int num = QuickSaver.ParseAttrID(childNode3, "BlockID");
					if (num < 0)
					{
						continue;
					}
					Placeable placeableByIndex = PlaceableMetadataList.Instance.GetPlaceableByIndex(num);
					if (!(placeableByIndex == null))
					{
						BlockData value3 = new BlockData
						{
							BlockPlaceable = placeableByIndex
						};
						int stepValueFromRarity = TabletBlockList.GetStepValueFromRarity(placeableByIndex.BaseRarity);
						int value4 = QuickSaver.ParseAttrInt(childNode3, "Frequency", stepValueFromRarity);
						value3.Frequency = Mathf.Clamp(value4, 0, 9);
						if (!dictionary.ContainsKey(num))
						{
							dictionary.Add(num, value3);
						}
						else
						{
							Debug.LogError("Duplicate entry in preset data for block ID " + num);
						}
					}
				}
				int num2 = PlaceableMetadataList.Instance.AllBlockListLength();
				List<BlockData> list = new List<BlockData>(num2);
				for (int i = 0; i < num2; i++)
				{
					if (dictionary.TryGetValue(i, out var value5))
					{
						list.Add(value5);
						continue;
					}
					if (instance.DefaultRuleset != null && instance.DefaultRuleset.Blocks != null && i < instance.DefaultRuleset.Blocks.Length)
					{
						list.Add(instance.DefaultRuleset.Blocks[i]);
						continue;
					}
					Placeable placeableByIndex2 = PlaceableMetadataList.Instance.GetPlaceableByIndex(i);
					if (placeableByIndex2 != null)
					{
						list.Add(new BlockData(placeableByIndex2));
					}
				}
				Blocks = list.ToArray();
			}
			if (childNode.Name == "mods")
			{
				mods.ReadFromXmlNode(childNode);
			}
		}
	}

	public string GetRulesetXmlString()
	{
		XmlDocument rulesetXml = GetRulesetXml();
		StringWriter stringWriter = new StringWriter();
		XmlTextWriter w = new XmlTextWriter(stringWriter);
		rulesetXml.WriteTo(w);
		return stringWriter.ToString();
	}

	public XmlDocument GetRulesetXml()
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("Ruleset");
		QuickSaver.AddAttribute(xmlDocument, xmlElement, "Name", Name);
		QuickSaver.AddAttribute(xmlDocument, xmlElement, "Description", Description);
		QuickSaver.AddAttribute(xmlDocument, xmlElement, "Premade", IsPremade.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement, "saveFormatVersion", "1");
		XmlElement xmlElement2 = xmlDocument.CreateElement("rules");
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "MaxScore", MaxScore.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "MaxRounds", MaxRounds.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "MaxTime", MaxTime.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "PlaceTime", PlaceTime.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "UsePlaceTimer", UsePlaceTimer.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "GameLimitType", GameLimitType.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "DoublePartyBox", DoublePartyBox.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "RunTimerLimit", RunTimerLimit.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "CreativePiecesPerRound", CreativePiecesPerRound.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "RespawnMode", respawnMode.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "NumRespawns", numRespawns.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "PartyBoxMode", partyBoxMode.ToString());
		QuickSaver.AddAttribute(xmlDocument, xmlElement2, "CompetitiveRandomizer", competitiveRandomizer.ToString());
		xmlElement.AppendChild(xmlElement2);
		XmlElement xmlElement3 = xmlDocument.CreateElement("points");
		foreach (PointData value in points.Values)
		{
			XmlElement xmlElement4 = xmlDocument.CreateElement("point");
			QuickSaver.AddAttribute(xmlDocument, xmlElement4, "Type", value.Type.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement4, "AlwaysAward", value.AlwaysAward.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement4, "Enabled", value.Enabled.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement4, "HotseatAllowed", value.HotseatAllowed.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement4, "RequiresWin", value.RequiresWin.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement4, "Value", value.Value.ToString());
			xmlElement3.AppendChild(xmlElement4);
		}
		xmlElement.AppendChild(xmlElement3);
		XmlElement xmlElement5 = xmlDocument.CreateElement("blocks");
		for (int i = 0; i < Blocks.Length; i++)
		{
			XmlElement xmlElement6 = xmlDocument.CreateElement("block");
			QuickSaver.AddAttribute(xmlDocument, xmlElement6, "BlockID", i.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement6, "Enabled", Blocks[i].Enabled.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement6, "Frequency", Blocks[i].Frequency.ToString());
			xmlElement5.AppendChild(xmlElement6);
		}
		xmlElement.AppendChild(xmlElement5);
		XmlElement xmlElement7 = xmlDocument.CreateElement("mods");
		mods.WriteToXmlNode(xmlDocument, xmlElement7);
		xmlElement.AppendChild(xmlElement7);
		xmlDocument.AppendChild(xmlElement);
		return xmlDocument;
	}

	public static void LoadAllSavedRulesets(bool clearExisting = true)
	{
		if (clearExisting)
		{
			GameSettings.GetInstance().ResetToPremadeRulesets();
		}
		getRulesFilenames(".ruleset", delegate(IEnumerable<string> files)
		{
			foreach (string file in files)
			{
				LoadRules(file, applyOnLoad: false);
			}
		});
	}

	public static void LoadRules(string filename, bool applyOnLoad = true)
	{
		string fullPath = QuickSaver.RuleSavesFolder + "/" + filename;
		UnityAction<byte[]> onRulesLoaded = delegate(byte[] data)
		{
			XmlDocument xmlDocFromBytes = QuickSaver.GetXmlDocFromBytes(data);
			GameRulePreset gameRulePreset = ScriptableObject.CreateInstance<GameRulePreset>();
			gameRulePreset.LoadRulesetFromXML(xmlDocFromBytes);
			gameRulePreset.PathToFile = fullPath;
			gameRulePreset.name = gameRulePreset.Name;
			GameSettings.GetInstance().AddRulePreset(gameRulePreset, applyOnLoad);
		};
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddReadFileOperation(fullPath, delegate(RamFS.FSOperationReturnCode resultCode, byte[] arg)
			{
				if (resultCode == RamFS.FSOperationReturnCode.OK)
				{
					onRulesLoaded(arg);
				}
			});
			return;
		}
		byte[] fileData = null;
		bool success = false;
		WorkerThreadManager.Instance.AddFileOpJob(delegate
		{
			if (File.Exists(fullPath))
			{
				try
				{
					FileStream fileStream = File.OpenRead(fullPath);
					fileData = new byte[fileStream.Length];
					fileStream.Read(fileData, 0, (int)fileStream.Length);
					fileStream.Close();
					success = true;
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception while reading ruleset: " + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}, delegate
		{
			if (success)
			{
				onRulesLoaded(fileData);
			}
		});
	}

	private static void getRulesFilenames(string extensionFilter, Action<IEnumerable<string>> OnFinish)
	{
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddGetExistingFilenamesOperation("/rules/", ".ruleset", ordered: true, delegate(IEnumerable<string> returnedFiles)
			{
				List<string> list = (List<string>)returnedFiles;
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = Path.GetFileName(list[i]);
				}
				OnFinish(list);
			});
			return;
		}
		string saveFolder = QuickSaver.RuleSavesFolder;
		List<string> result = new List<string>();
		WorkerThreadManager.Instance.AddFileOpJob(delegate
		{
			foreach (FileInfo item in from f in new DirectoryInfo(saveFolder).GetFiles("*" + extensionFilter)
				orderby f.CreationTime descending
				select f)
			{
				result.Add(item.Name);
			}
		}, delegate
		{
			OnFinish(result);
		});
	}

	public static void DeleteRuleset(GameRulePreset ruleset, UnityAction<bool> OnFinish)
	{
		if (ruleset.IsPremade)
		{
			return;
		}
		string fullPath = ruleset.PathToFile;
		UnityAction<bool> onDelete = delegate(bool success)
		{
			if (success)
			{
				GameSettings.GetInstance().RemoveRulePreset(ruleset);
			}
			else
			{
				Debug.LogError("Could not delete ruleset at " + fullPath);
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.CouldNotDelete, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
				AkSoundEngine.PostEvent("UI_Snapshot_Error", GameState.GetInstance().gameObject);
			}
			OnFinish(success);
		};
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddDeleteFileOperation(fullPath, delegate(RamFS.FSOperationReturnCode returnCode)
			{
				if (returnCode == RamFS.FSOperationReturnCode.OK)
				{
					onDelete(arg0: true);
				}
				else
				{
					Debug.LogError("RamFS error deleting file: " + returnCode);
				}
			});
			return;
		}
		try
		{
			File.Delete(fullPath);
			onDelete(arg0: true);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not delete rule file: " + ex.Message);
			onDelete(arg0: false);
		}
	}

	public static GameRulePreset GetRulesetFromCurrentRules()
	{
		GameRulePreset gameRulePreset = ScriptableObject.CreateInstance<GameRulePreset>();
		gameRulePreset.LoadRulesFromSettings();
		return gameRulePreset;
	}

	public static void SaveRules(string nameInput, string descriptionInput, UnityAction<string> OnNameSanitized, UnityAction<GameRulePreset> OnRulesetCreated)
	{
		GameRulePreset ruleset = GetRulesetFromCurrentRules();
		if (nameInput.NullOrEmpty())
		{
			ruleset.Name = LocalizationManager.GetTranslation("RuleBook/Presets/Custom");
		}
		else
		{
			ruleset.Name = nameInput;
		}
		ruleset.Description = descriptionInput;
		ruleset.IsPremade = false;
		UnityAction OnSaveFailed = delegate
		{
			UnityEngine.Object.Destroy(ruleset);
			OnRulesetCreated(null);
		};
		QuickSaver.CheckSaveFolders();
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddGetExistingFilenamesOperation("/rules/", null, ordered: false, delegate(IEnumerable<string> filenames)
			{
				if (filenames == null)
				{
					Debug.LogError("Problem enumerating files");
					OnSaveFailed();
				}
				else
				{
					string newFilename = getValidFilename(ruleset.Name, filenames);
					string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(newFilename);
					ruleset.Name = fileNameWithoutExtension2;
					if (fileNameWithoutExtension2 != nameInput)
					{
						OnNameSanitized(fileNameWithoutExtension2);
					}
					GameSettings.GetInstance().AddRulePreset(ruleset);
					byte[] compressedBytesFromXmlDoc2 = QuickSaver.GetCompressedBytesFromXmlDoc(ruleset.GetRulesetXml());
					if (newFilename.NullOrEmpty())
					{
						Debug.LogError("Problem getting filename for file");
						OnSaveFailed();
					}
					else
					{
						ruleset.PathToFile = newFilename;
						RamFS.AddAddFileOperation(newFilename, compressedBytesFromXmlDoc2, delegate(RamFS.FSOperationReturnCode returnCode)
						{
							if (returnCode == RamFS.FSOperationReturnCode.OK)
							{
								OnRulesetCreated(ruleset);
							}
							else
							{
								Debug.LogError("Error adding file \"" + newFilename + "\" (" + returnCode.ToString() + ")");
								OnSaveFailed();
							}
						});
					}
				}
			});
			return;
		}
		string validFilename = getValidFilename(ruleset.Name);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(validFilename);
		if (fileNameWithoutExtension.NullOrEmpty())
		{
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.InvalidCharacters, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			OnSaveFailed();
			return;
		}
		ruleset.Name = fileNameWithoutExtension;
		if (fileNameWithoutExtension != nameInput)
		{
			OnNameSanitized(fileNameWithoutExtension);
		}
		GameSettings.GetInstance().AddRulePreset(ruleset);
		byte[] compressedBytesFromXmlDoc = QuickSaver.GetCompressedBytesFromXmlDoc(ruleset.GetRulesetXml());
		if (validFilename.NullOrEmpty())
		{
			OnSaveFailed();
			return;
		}
		FileStream fileStream = null;
		try
		{
			ruleset.PathToFile = validFilename;
			fileStream = File.OpenWrite(validFilename);
			fileStream.Write(compressedBytesFromXmlDoc, 0, compressedBytesFromXmlDoc.Length);
			fileStream.Close();
			OnRulesetCreated(ruleset);
		}
		catch (Exception ex)
		{
			Debug.LogError("Couldn't save file: " + ex.Message);
			if (ex is IOException)
			{
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.InvalidCharacters, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			}
			fileStream?.Close();
			OnSaveFailed();
		}
	}

	private static string getValidFilename(string suggestedFilename, IEnumerable<string> existingFiles = null)
	{
		suggestedFilename = suggestedFilename?.Trim();
		string result = null;
		if (!suggestedFilename.NullOrEmpty())
		{
			result = QuickSaver.EnsureUniqueFilename(QuickSaver.RuleSavesFolder + "/" + suggestedFilename + ".ruleset", 0, existingFiles);
		}
		return result;
	}

	public string GetNameString()
	{
		if (NameHasLocalization)
		{
			return LocalizationManager.GetTranslation(LocalizedName);
		}
		return Name;
	}

	public string GetDescriptionString()
	{
		if (NameHasLocalization)
		{
			return LocalizationManager.GetTranslation(LocalizedDescription);
		}
		return Description;
	}

	public MsgApplyRuleset GenerateApplyRulesetMessage(bool loadRules, bool loadPoints, bool loadBlocks, bool loadMods)
	{
		GameSettings instance = GameSettings.GetInstance();
		MsgApplyRuleset msgApplyRuleset = new MsgApplyRuleset();
		msgApplyRuleset.premadeIdx = (IsPremade ? instance.GetRulesetIndex(this) : (-1));
		if (!IsPremade)
		{
			msgApplyRuleset.rulesetXML = GetRulesetXmlString();
		}
		msgApplyRuleset.applyRules = loadRules;
		msgApplyRuleset.applyPoints = loadPoints;
		msgApplyRuleset.applyBlocks = loadBlocks;
		msgApplyRuleset.applyMods = loadMods;
		return msgApplyRuleset;
	}
}
