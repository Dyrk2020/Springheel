using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using GameEvent;
using SevenZip.Compression.LZMA;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class QuickSaver : MonoBehaviour
{
	public class SaveablePiece
	{
		public int blockID;

		public int sceneID;

		public Vector3 pos;

		public Quaternion rot;

		public Vector3 scale;

		public string parentPath;

		public string parentAttachmentPoint;

		public Vector3 relativeAttachPosition;

		public SaveablePiece parentPiece;

		public SaveablePiece mainPiece;

		public string subElementName;

		public string overrideSubElementName;

		public string overrideName;

		public int placeableID = -1;

		public int teleporterDestinationID = -1;

		public bool clockwise;

		public int damageLevel;

		public bool alwaysSaveDamage;

		public Placeable placeable;

		public PlaceableMetadata metadata;

		public Color CustomColor;

		public bool IsConnectedToMainPiece
		{
			get
			{
				if (mainPiece == null)
				{
					return false;
				}
				Transform parent = placeable.transform.parent;
				while (parent != null)
				{
					if (parent == mainPiece.placeable.transform)
					{
						return true;
					}
					parent = parent.parent;
				}
				return false;
			}
		}
	}

	public delegate void LocalSaveCompleteDelegate(bool success, string filename);

	private PlaceableMetadataList metadataList;

	public static string levelPortalXml;

	public static string lastLoadedXml;

	public static int numLocalSaves = -1;

	public static bool numLocalSavesQueried = false;

	private List<SaveablePiece> initialLevelPlaceables;

	private List<string> initialDestroyedPaths;

	private static List<string> excludedLayers = new List<string> { "Player", "Particles", "UI", "Splash Screens" };

	private static List<string> includedLayers = new List<string> { "TransparentFX" };

	private static List<string> excludedLayersEditor = new List<string> { "UI", "Splash Screens" };

	private static List<string> includedLayersEditor = new List<string> { "TransparentFX" };

	public static QuickSaver lastInstance;

	public static string LocalSavesFolder
	{
		get
		{
			if (RamFS.PlatformUsesRamFS)
			{
				return "/snapshots";
			}
			return Application.persistentDataPath + "/snapshots";
		}
	}

	public static string RuleSavesFolder
	{
		get
		{
			if (RamFS.PlatformUsesRamFS)
			{
				return "/rules";
			}
			return Application.persistentDataPath + "/rules";
		}
	}

	public static string RemoteThumbnailsFolder => Application.persistentDataPath + "/snapshots/thumbnails";

	public static string LocalThumbnailsFolder
	{
		get
		{
			if (RamFS.PlatformUsesRamFS)
			{
				return "/snapshots/thumbnails";
			}
			return Application.persistentDataPath + "/snapshots/thumbnails";
		}
	}

	public static string PreferredThumbnailFormatExtension
	{
		get
		{
			switch (GameSettings.GetInstance().LevelThumbnailFormat)
			{
			case ThumbnailFormat.PNG:
				return ".png";
			case ThumbnailFormat.JPG:
				return ".jpg";
			default:
				UnityEngine.Debug.LogError("Unknown image format");
				return ".image";
			}
		}
	}

	public static string PreferredThumbnailFormatMimeType
	{
		get
		{
			switch (GameSettings.GetInstance().LevelThumbnailFormat)
			{
			case ThumbnailFormat.PNG:
				return "image/png";
			case ThumbnailFormat.JPG:
				return "image/jpeg";
			default:
				UnityEngine.Debug.LogError("Unknown image format");
				return "application/octet-stream";
			}
		}
	}

	private void Awake()
	{
		lastInstance = this;
	}

	private void Start()
	{
		metadataList = PlaceableMetadataList.Instance;
		CheckSaveFolders();
	}

	private void Update()
	{
	}

	public static XmlDocument TryLoadSnapshotXMLFromPath(string fullpath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		FileStream fileStream = null;
		try
		{
			fileStream = File.OpenRead(fullpath);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Exception while trying to open file " + fullpath + ": " + ex.Message);
			return null;
		}
		if (fileStream == null)
		{
			UnityEngine.Debug.LogError("Stream is null!?");
			return null;
		}
		if (fileStream.Length > 20971520)
		{
			UnityEngine.Debug.LogError("Could not open snapshot file: File too big!");
			fileStream.Close();
			return null;
		}
		int num = fileStream.ReadByte();
		fileStream.Seek(0L, SeekOrigin.Begin);
		if (num != -1)
		{
			if ((ushort)num == 60)
			{
				try
				{
					xmlDocument.Load(fileStream);
				}
				catch (Exception ex2)
				{
					UnityEngine.Debug.LogError("Could not open snapshot file " + fullpath + ": " + ex2.Message);
					fileStream.Close();
					return null;
				}
			}
			else
			{
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, (int)fileStream.Length);
				array = SevenZipHelper.Decompress(array);
				try
				{
					string xml = Encoding.UTF8.GetString(array);
					xmlDocument.LoadXml(xml);
				}
				catch (Exception ex3)
				{
					UnityEngine.Debug.LogError("Could not open snapshot file " + fullpath + ": " + ex3.Message);
					fileStream.Close();
					return null;
				}
			}
			fileStream.Close();
			return xmlDocument;
		}
		UnityEngine.Debug.LogError("Snapshot file is empty: " + fullpath);
		fileStream.Close();
		return null;
	}

	public static XmlDocument TryLoadSnapshotXMLFromBytes(byte[] bytes)
	{
		if (bytes.Length > 20971520)
		{
			UnityEngine.Debug.LogError("Could not open snapshot file: File too big!");
			return null;
		}
		XmlDocument xmlDocument = new XmlDocument();
		if (bytes.Length != 0)
		{
			if (bytes[0] == 60)
			{
				try
				{
					string xml = Encoding.UTF8.GetString(bytes);
					xmlDocument.LoadXml(xml);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Could not open snapshot file: " + ex.Message);
					return null;
				}
			}
			else
			{
				bytes = SevenZipHelper.Decompress(bytes);
				try
				{
					string xml2 = Encoding.UTF8.GetString(bytes);
					xmlDocument.LoadXml(xml2);
				}
				catch (Exception ex2)
				{
					UnityEngine.Debug.LogError("Could not open snapshot file: " + ex2.Message);
					return null;
				}
			}
			return xmlDocument;
		}
		UnityEngine.Debug.LogError("Snapshot file is empty");
		return null;
	}

	public static XmlDocument GetXmlDocFromString(string str)
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.LoadXml(str);
			return xmlDocument;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Error reading XML string: " + ex.Message);
			return null;
		}
	}

	public static XmlDocument GetXmlDocFromBytes(byte[] data)
	{
		string xmlStringFromBytes = GetXmlStringFromBytes(data);
		if (xmlStringFromBytes == null)
		{
			return null;
		}
		return GetXmlDocFromString(xmlStringFromBytes);
	}

	public static string GetXmlStringFromBytes(byte[] data)
	{
		if (data.Length == 0)
		{
			return null;
		}
		if (data[0] != 60)
		{
			data = SevenZipHelper.Decompress(data);
		}
		return Encoding.UTF8.GetString(data);
	}

	public void QuickClear(bool restoreAll = false)
	{
		PlaceableMetadata[] array = UnityEngine.Object.FindObjectsOfType<PlaceableMetadata>();
		foreach (PlaceableMetadata placeableMetadata in array)
		{
			if (placeableMetadata != null)
			{
				Placeable placeable = ((placeableMetadata.placeableRef != null) ? placeableMetadata.placeableRef : placeableMetadata.GetComponent<Placeable>());
				if (placeable != null && (placeable.IsSaveable || !placeableMetadata.isLevelGeometry) && placeable.Placed && (!placeable.IsSubElement || placeable.ParentPiece == null) && !placeable.MarkedForDestruction)
				{
					placeable.DestroySelf(destroyChildren: false, useSmoke: false);
				}
			}
		}
		Dictionary<int, SaveablePiece> dictionary = new Dictionary<int, SaveablePiece>(initialLevelPlaceables.Count);
		foreach (SaveablePiece initialLevelPlaceable in initialLevelPlaceables)
		{
			if (initialLevelPlaceable.metadata == null || !initialLevelPlaceable.metadata.isLevelGeometry)
			{
				dictionary.Add(initialLevelPlaceable.sceneID, initialLevelPlaceable);
			}
		}
		RestoreSaveables(dictionary, restoreAsUnsaveable: true);
		if (restoreAll)
		{
			initialDestroyedPaths.Clear();
		}
		else
		{
			ClearInitialDestroyedObjects();
		}
	}

	private void ClearInitialDestroyedObjects()
	{
		foreach (string initialDestroyedPath in initialDestroyedPaths)
		{
			Transform transformFromHierarchyPath = GetTransformFromHierarchyPath(initialDestroyedPath);
			if (transformFromHierarchyPath != null)
			{
				Placeable component = transformFromHierarchyPath.GetComponent<Placeable>();
				if (!component.MarkedForDestruction)
				{
					component.DestroySelf(destroyChildren: false, useSmoke: false);
				}
			}
		}
		LobbyManager.instance.CurrentGameController.DestroyMarkedPiecesNow();
	}

	public string GetNewLocalSaveName(IEnumerable<string> existingFiles = null)
	{
		string sceneName = SceneManager.GetActiveScene().name;
		string localizedLevelName = LevelSelectController.GetLocalizedLevelName(LevelSelectController.GetLevelNameEnumFromSceneName(sceneName));
		string input = "noName";
		if (SteamManager.Initialized)
		{
			input = SteamFriends.GetPersonaName();
		}
		string text = Regex.Replace(input, "[<>:\"/\\\\\\|\\?\\*]", "");
		int num = 0;
		while (num++ < 999999)
		{
			string zeroPaddedNumberString = GetZeroPaddedNumberString(GetNextSequenceNumber(sceneName), 3);
			string text2 = localizedLevelName + "." + ((!text.NullOrEmpty()) ? (text + ".") : "") + zeroPaddedNumberString;
			if (existingFiles == null)
			{
				if (!CheckLocalSaveExists(text2))
				{
					return text2;
				}
			}
			else if (!existingFiles.Contains(text2))
			{
				return text2;
			}
		}
		return null;
	}

	public static bool CheckLocalSaveExists(string localSaveName)
	{
		return CheckLocalSaveExistsThreadSafe(LocalSavesFolder, localSaveName);
	}

	public static bool CheckLocalSaveExistsThreadSafe(string localSavesFolder, string localSaveName)
	{
		string text = localSavesFolder + "/" + localSaveName;
		if (File.Exists(text + ".snapshot"))
		{
			return true;
		}
		if (File.Exists(text + ".c.snapshot"))
		{
			return true;
		}
		if (File.Exists(text + ".v.snapshot"))
		{
			return true;
		}
		return false;
	}

	public string GetAlternateNewLocalSaveName(string[] existingFiles = null)
	{
		string text = SceneManager.GetActiveScene().name;
		string localizedLevelName = LevelSelectController.GetLocalizedLevelName(LevelSelectController.GetLevelNameEnumFromSceneName(text));
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (!saveFileDataForMainUser.snapshotSequenceNumbers.TryGetValue(text, out var value))
		{
			value = 1;
			saveFileDataForMainUser.snapshotSequenceNumbers.Add(text, 1);
		}
		string zeroPaddedNumberString = GetZeroPaddedNumberString(value, 3);
		string text2 = localizedLevelName + "." + zeroPaddedNumberString;
		if (existingFiles == null)
		{
			if (!CheckLocalSaveExists(text2))
			{
				return text2;
			}
		}
		else if (!existingFiles.Contains(text2))
		{
			return text2;
		}
		return null;
	}

	private static int GetNextSequenceNumber(string sceneName)
	{
		Dictionary<string, int> snapshotSequenceNumbers = StatTracker.Instance.GetSaveFileDataForMainUser().snapshotSequenceNumbers;
		if (!snapshotSequenceNumbers.TryGetValue(sceneName, out var value))
		{
			snapshotSequenceNumbers.Add(sceneName, 1);
			return 1;
		}
		snapshotSequenceNumbers[sceneName]++;
		return value + 1;
	}

	private static string GetZeroPaddedNumberString(int value, int minDigits)
	{
		string text = value.ToString();
		if (text.Length < minDigits)
		{
			string text2 = "";
			for (int i = 0; i < minDigits - text.Length; i++)
			{
				text2 += "0";
			}
			text = text2 + text;
		}
		return text;
	}

	public void DoLocalSave(string suggestedFilename, FeaturedQuickFilter.LevelTypes levelType, int playerLocalNumber, bool omitModifiers, LocalSaveCompleteDelegate callback)
	{
		CheckSaveFolders();
		XmlDocument currentXmlSnapshot = GetCurrentXmlSnapshot(omitModifiers);
		byte[] compressedBytes = GetCompressedBytesFromXmlDoc(currentXmlSnapshot);
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddGetExistingFilenamesOperation("/snapshots/", null, ordered: false, delegate(IEnumerable<string> filenames)
			{
				if (filenames == null)
				{
					UnityEngine.Debug.LogError("Problem enumerating files");
					callback(success: false, null);
				}
				string newFilename = GetValidFilename(suggestedFilename, levelType, filenames);
				if (newFilename.NullOrEmpty())
				{
					UnityEngine.Debug.LogError("Problem getting filename for file");
					callback(success: false, null);
				}
				else
				{
					RamFS.AddAddFileOperation(newFilename, compressedBytes, delegate(RamFS.FSOperationReturnCode returnCode)
					{
						if (returnCode == RamFS.FSOperationReturnCode.OK)
						{
							callback(success: true, newFilename);
						}
						else
						{
							UnityEngine.Debug.LogError("Error adding file \"" + newFilename + "\" (" + returnCode.ToString() + ")");
							callback(success: false, null);
						}
					});
				}
			});
			return;
		}
		string validFilename = GetValidFilename(suggestedFilename, levelType);
		if (validFilename.NullOrEmpty())
		{
			callback(success: false, null);
			return;
		}
		FileStream fileStream = null;
		try
		{
			fileStream = File.OpenWrite(validFilename);
			fileStream.Write(compressedBytes, 0, compressedBytes.Length);
			fileStream.Close();
			callback(success: true, validFilename);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Couldn't save file: " + ex.Message);
			fileStream?.Close();
			callback(success: false, null);
		}
	}

	private string GetValidFilename(string suggestedFilename, FeaturedQuickFilter.LevelTypes levelType, IEnumerable<string> existingFiles = null)
	{
		suggestedFilename = suggestedFilename?.Trim();
		string text = null;
		if (suggestedFilename.NullOrEmpty())
		{
			bool flag = true;
			while (flag)
			{
				flag = false;
				text = GetNewLocalSaveName(existingFiles);
				string localSaveSuffixForLevelType = GetLocalSaveSuffixForLevelType(levelType);
				text = LocalSavesFolder + "/" + text + localSaveSuffixForLevelType + ".snapshot";
				FileStream fileStream = null;
				try
				{
					fileStream = File.OpenWrite(text);
				}
				catch (Exception ex)
				{
					text = GetAlternateNewLocalSaveName();
					if (text == null)
					{
						flag = true;
					}
					else
					{
						text = LocalSavesFolder + "/" + text + localSaveSuffixForLevelType + ".snapshot";
						try
						{
							fileStream = File.OpenWrite(text);
						}
						catch (Exception ex2)
						{
							UnityEngine.Debug.LogError("Couldn't open file for writing:\n Ex1: " + ex.Message + "\nEx2: " + ex2.Message);
							return null;
						}
					}
				}
				fileStream?.Close();
			}
		}
		else
		{
			text = EnsureUniqueLocalLevelName(LocalSavesFolder + "/" + suggestedFilename, existingFiles);
			if (text != null)
			{
				text = text + GetLocalSaveSuffixForLevelType(levelType) + ".snapshot";
			}
		}
		return text;
	}

	public static byte[] GetCompressedBytesFromXmlDoc(XmlDocument doc)
	{
		return GetCompressedBytesFromXmlString(doc.OuterXml);
	}

	public static byte[] GetCompressedBytesFromXmlString(string xmlString)
	{
		return SevenZipHelper.Compress(Encoding.UTF8.GetBytes(xmlString));
	}

	public XmlDocument GetCurrentXmlSnapshot(bool omitModifiers = false)
	{
		new Parsing.EnsureInvariantCulture();
		List<PlaceableMetadata> allPlaceables = UnityEngine.Object.FindObjectsOfType<PlaceableMetadata>().ToList();
		List<SaveablePiece> saveablesFromMetadata = GetSaveablesFromMetadata(allPlaceables);
		Level levelLayout = LobbyManager.instance.CurrentGameController.LevelLayout;
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("scene");
		AddAttribute(xmlDocument, xmlElement, "levelSceneName", SceneManager.GetActiveScene().name);
		AddAttribute(xmlDocument, xmlElement, "saveFormatVersion", "1");
		if (SceneManager.GetActiveScene().name == "BlankLevel")
		{
			int background;
			if (levelLayout.currentCustomBackground != null)
			{
				background = (int)levelLayout.currentCustomBackground.background;
				AddAttribute(xmlDocument, xmlElement, "customLevelBackground", background.ToString());
			}
			background = (int)levelLayout.currentCustomMusic;
			AddAttribute(xmlDocument, xmlElement, "customLevelMusic", background.ToString());
			background = (int)levelLayout.currentCustomAmbience;
			AddAttribute(xmlDocument, xmlElement, "customLevelAmbience", background.ToString());
		}
		if (Modifiers.GetInstance().ModsApplied && !omitModifiers)
		{
			XmlElement xmlElement2 = xmlDocument.CreateElement("mods");
			ModSource modSource = new ModSource();
			modSource.ReadFromModSettings();
			modSource.WriteToXmlNode(xmlDocument, xmlElement2);
			xmlElement.AppendChild(xmlElement2);
		}
		xmlDocument.AppendChild(xmlElement);
		foreach (SaveablePiece item in saveablesFromMetadata)
		{
			if (item.placeable.IsSaveable && !item.metadata.isLevelGeometry)
			{
				if (metadataList.GetPrefabForPlaceableIndex(item.blockID) != null)
				{
					XmlElement xmlElement3 = xmlDocument.CreateElement("block");
					AddAttribute(xmlDocument, xmlElement3, "sceneID", item.sceneID.ToString());
					AddAttribute(xmlDocument, xmlElement3, "blockID", item.blockID.ToString());
					AddAttribute(xmlDocument, xmlElement3, "pX", item.pos.x.ToString());
					AddAttribute(xmlDocument, xmlElement3, "pY", item.pos.y.ToString());
					AddAttribute(xmlDocument, xmlElement3, "pZ", item.pos.z.ToString());
					AddAttribute(xmlDocument, xmlElement3, "rX", item.rot.eulerAngles.x.ToString());
					AddAttribute(xmlDocument, xmlElement3, "rY", item.rot.eulerAngles.y.ToString());
					AddAttribute(xmlDocument, xmlElement3, "rZ", item.rot.eulerAngles.z.ToString());
					AddAttribute(xmlDocument, xmlElement3, "sX", item.scale.x.ToString());
					AddAttribute(xmlDocument, xmlElement3, "sY", item.scale.y.ToString());
					AddAttribute(xmlDocument, xmlElement3, "sZ", item.scale.z.ToString());
					if (item.parentPiece != null)
					{
						AddAttribute(xmlDocument, xmlElement3, "parentID", item.parentPiece.sceneID.ToString());
						if (!item.parentAttachmentPoint.NullOrEmpty())
						{
							AddAttribute(xmlDocument, xmlElement3, "parentAttachmentPoint", item.parentAttachmentPoint);
						}
					}
					if (item.mainPiece != null)
					{
						AddAttribute(xmlDocument, xmlElement3, "mainID", item.mainPiece.sceneID.ToString());
						AddAttribute(xmlDocument, xmlElement3, "subElementName", item.subElementName);
					}
					if (!item.parentPath.NullOrEmpty())
					{
						AddAttribute(xmlDocument, xmlElement3, "parentPath", item.parentPath);
						AddAttribute(xmlDocument, xmlElement3, "relX", item.relativeAttachPosition.x.ToString());
						AddAttribute(xmlDocument, xmlElement3, "relY", item.relativeAttachPosition.y.ToString());
						AddAttribute(xmlDocument, xmlElement3, "relZ", item.relativeAttachPosition.z.ToString());
					}
					if (!item.overrideName.NullOrEmpty())
					{
						AddAttribute(xmlDocument, xmlElement3, "overrideName", item.overrideName);
					}
					AddAttribute(xmlDocument, xmlElement3, "placeableID", item.placeableID.ToString());
					if (item.blockID == metadataList.NameToIndexMap[metadataList.teleporterPrefabMetadata.GetComponent<Placeable>().Name])
					{
						Teleporter teleporter = item.placeable.GetComponent<Teleporter>();
						if (teleporter.Destination != null)
						{
							SaveablePiece saveablePiece = saveablesFromMetadata.Find((SaveablePiece otherSaveable) => otherSaveable.placeable == teleporter.Destination);
							if (saveablePiece != null)
							{
								AddAttribute(xmlDocument, xmlElement3, "teleporterDestinationID", saveablePiece.sceneID.ToString());
							}
						}
					}
					if (item.placeable.RotationDirection != Placeable.RotationDirections.None)
					{
						AddAttribute(xmlDocument, xmlElement3, "clockwise", (item.placeable.RotationDirection == Placeable.RotationDirections.Clockwise) ? "1" : "0");
					}
					if (item.placeable.canSetCustomColor)
					{
						AddAttribute(xmlDocument, xmlElement3, "colR", item.placeable.CustomColor.r.ToString());
						AddAttribute(xmlDocument, xmlElement3, "colG", item.placeable.CustomColor.g.ToString());
						AddAttribute(xmlDocument, xmlElement3, "colB", item.placeable.CustomColor.b.ToString());
					}
					if (item.damageLevel > 0 || item.alwaysSaveDamage)
					{
						AddAttribute(xmlDocument, xmlElement3, "damageLevel", item.damageLevel.ToString());
						AddAttribute(xmlDocument, xmlElement3, "alwaysSaveDamage", item.alwaysSaveDamage.ToString());
					}
					xmlElement.AppendChild(xmlElement3);
				}
				else
				{
					UnityEngine.Debug.Log("Couldn't find prefab for index " + item.blockID);
				}
				continue;
			}
			foreach (SaveablePiece initialLevelPlaceable in initialLevelPlaceables)
			{
				if (!(initialLevelPlaceable.placeable == item.placeable))
				{
					continue;
				}
				if (TransformChanged(initialLevelPlaceable, item) || item.damageLevel > 0 || item.alwaysSaveDamage)
				{
					XmlElement xmlElement4 = xmlDocument.CreateElement("moved");
					AddAttribute(xmlDocument, xmlElement4, "placeableID", item.placeableID.ToString());
					AddAttribute(xmlDocument, xmlElement4, "path", GetHierarchyPath(item.placeable.transform, includeLeaf: true));
					AddAttribute(xmlDocument, xmlElement4, "pX", item.pos.x.ToString());
					AddAttribute(xmlDocument, xmlElement4, "pY", item.pos.y.ToString());
					AddAttribute(xmlDocument, xmlElement4, "pZ", item.pos.z.ToString());
					AddAttribute(xmlDocument, xmlElement4, "rX", item.rot.eulerAngles.x.ToString());
					AddAttribute(xmlDocument, xmlElement4, "rY", item.rot.eulerAngles.y.ToString());
					AddAttribute(xmlDocument, xmlElement4, "rZ", item.rot.eulerAngles.z.ToString());
					AddAttribute(xmlDocument, xmlElement4, "sX", item.scale.x.ToString());
					AddAttribute(xmlDocument, xmlElement4, "sY", item.scale.y.ToString());
					AddAttribute(xmlDocument, xmlElement4, "sZ", item.scale.z.ToString());
					if (item.damageLevel > 0 || item.alwaysSaveDamage)
					{
						AddAttribute(xmlDocument, xmlElement4, "damageLevel", item.damageLevel.ToString());
					}
					xmlElement.AppendChild(xmlElement4);
				}
				break;
			}
		}
		foreach (string initialDestroyedPath in initialDestroyedPaths)
		{
			XmlElement xmlElement5 = xmlDocument.CreateElement("destroyed");
			AddAttribute(xmlDocument, xmlElement5, "path", initialDestroyedPath);
			xmlElement.AppendChild(xmlElement5);
		}
		return xmlDocument;
	}

	public static void AddAttribute(XmlDocument doc, XmlNode node, string attributeName, string attributeValue)
	{
		XmlAttribute xmlAttribute = doc.CreateAttribute(attributeName);
		xmlAttribute.Value = attributeValue;
		node.Attributes.Append(xmlAttribute);
	}

	public static string GetHierarchyPath(Transform t, bool includeLeaf = false)
	{
		string text = (includeLeaf ? t.gameObject.name : "");
		while (t != null)
		{
			t = t.parent;
			if (t != null)
			{
				text = ((text.Length <= 0) ? t.gameObject.name : (t.gameObject.name + "/" + text));
			}
		}
		return text;
	}

	public static void CheckSaveFolders()
	{
		if (!RamFS.PlatformUsesRamFS)
		{
			if (!Directory.Exists(LocalSavesFolder))
			{
				UnityEngine.Debug.Log("Creating Local Saves folder at " + LocalSavesFolder);
				Directory.CreateDirectory(LocalSavesFolder);
			}
			if (!Directory.Exists(LocalThumbnailsFolder))
			{
				UnityEngine.Debug.Log("Creating Local Thumbnails folder at " + LocalThumbnailsFolder);
				Directory.CreateDirectory(LocalThumbnailsFolder);
			}
			if (!Directory.Exists(RuleSavesFolder))
			{
				UnityEngine.Debug.Log("Creating Rule Preset folder at " + RuleSavesFolder);
				Directory.CreateDirectory(RuleSavesFolder);
			}
		}
		if (!Directory.Exists(RemoteThumbnailsFolder))
		{
			UnityEngine.Debug.Log("Creating Remote Thumbnails folder at " + RemoteThumbnailsFolder);
			Directory.CreateDirectory(RemoteThumbnailsFolder);
		}
	}

	public bool LoadSnapshotFromXmlDocument(XmlDocument doc)
	{
		XmlElement documentElement = doc.DocumentElement;
		string text = ParseAttrStr(documentElement, "levelSceneName");
		string text2 = SceneManager.GetActiveScene().name;
		if (text == "BlankLevel")
		{
			Level levelLayout = LobbyManager.instance.CurrentGameController.LevelLayout;
			int num = ParseAttrID(documentElement, "customLevelBackground");
			if (num > 0)
			{
				levelLayout.SetBackground((BackgroundType)num);
			}
			else
			{
				levelLayout.SetBackground(BackgroundType.BlueSky);
			}
			int num2 = ParseAttrID(documentElement, "customLevelMusic");
			if (num2 >= 0)
			{
				GameEventManager.SendEvent(new SetCustomMusicEvent((GameState.LevelName)num2));
			}
			int num3 = ParseAttrID(documentElement, "customLevelAmbience");
			if (num3 >= 0)
			{
				GameEventManager.SendEvent(new SetCustomAmbienceEvent((GameState.LevelName)num3));
			}
		}
		if (text != text2)
		{
			UnityEngine.Debug.LogError("Couldn't load snapshot: save file is for level \"" + text + "\"");
			return false;
		}
		Modifiers instance = Modifiers.GetInstance();
		bool isNonDefault = instance.IsNonDefault;
		if (!instance.forceLobbyModifiers || GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
		{
			XmlNodeList xmlNodeList = documentElement.SelectNodes("mods");
			if (xmlNodeList.Count > 0)
			{
				XmlNode child = xmlNodeList[0];
				ModSource modSource = new ModSource();
				modSource.ReadFromXmlNode(child);
				modSource.WriteToModSettings(includeTreehouseSettings: false);
			}
			else
			{
				new ModSource().WriteToModSettings(includeTreehouseSettings: false);
			}
			Modifiers.GetInstance().OnModifiersDynamicChange();
			if (instance.IsNonDefault != isNonDefault)
			{
				GameEventManager.SendEvent(new ModifiersChangedEvent(TabletRule.None));
			}
		}
		Dictionary<int, SaveablePiece> dictionary = new Dictionary<int, SaveablePiece>();
		Dictionary<int, List<int>> dictionary2 = new Dictionary<int, List<int>>();
		Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name == "block")
			{
				SaveablePiece saveablePiece = new SaveablePiece();
				saveablePiece.sceneID = ParseAttrID(childNode, "sceneID");
				if (saveablePiece.sceneID != -1)
				{
					saveablePiece.blockID = ParseAttrID(childNode, "blockID");
					saveablePiece.parentAttachmentPoint = ParseAttrStr(childNode, "parentAttachmentPoint");
					saveablePiece.pos = new Vector3(ParseAttrFloat(childNode, "pX"), ParseAttrFloat(childNode, "pY"), ParseAttrFloat(childNode, "pZ"));
					saveablePiece.rot = Quaternion.Euler(ParseAttrFloat(childNode, "rX"), ParseAttrFloat(childNode, "rY"), ParseAttrFloat(childNode, "rZ"));
					saveablePiece.scale = new Vector3(ParseAttrFloat(childNode, "sX", 1f), ParseAttrFloat(childNode, "sY", 1f), ParseAttrFloat(childNode, "sZ", 1f));
					int num4 = ParseAttrID(childNode, "parentID");
					if (num4 != -1)
					{
						if (!dictionary2.ContainsKey(num4))
						{
							dictionary2.Add(num4, new List<int>());
						}
						dictionary2[num4].Add(saveablePiece.sceneID);
					}
					int num5 = ParseAttrID(childNode, "mainID");
					if (num5 != -1)
					{
						dictionary3.Add(saveablePiece.sceneID, num5);
					}
					saveablePiece.subElementName = ParseAttrStr(childNode, "subElementName");
					saveablePiece.overrideName = ParseAttrStr(childNode, "overrideName");
					saveablePiece.parentAttachmentPoint = ParseAttrStr(childNode, "parentAttachmentPoint");
					saveablePiece.parentPath = ParseAttrStr(childNode, "parentPath");
					saveablePiece.relativeAttachPosition = new Vector3(ParseAttrFloat(childNode, "relX"), ParseAttrFloat(childNode, "relY"), ParseAttrFloat(childNode, "relZ"));
					saveablePiece.placeableID = ParseAttrID(childNode, "placeableID");
					if (saveablePiece.blockID == metadataList.NameToIndexMap[metadataList.teleporterPrefabMetadata.GetComponent<Placeable>().Name])
					{
						saveablePiece.teleporterDestinationID = ParseAttrID(childNode, "teleporterDestinationID");
					}
					int num6 = ParseAttrInt(childNode, "clockwise");
					saveablePiece.clockwise = num6 == 1;
					saveablePiece.CustomColor = new Color(ParseAttrFloat(childNode, "colR"), ParseAttrFloat(childNode, "colG"), ParseAttrFloat(childNode, "colB"));
					saveablePiece.damageLevel = ParseAttrInt(childNode, "damageLevel");
					dictionary.Add(saveablePiece.sceneID, saveablePiece);
				}
				else
				{
					UnityEngine.Debug.LogError("Couldn't parse scene ID for block");
				}
			}
			else if (childNode.Name == "destroyed")
			{
				initialDestroyedPaths.Add(ParseAttrStr(childNode, "path"));
			}
			else
			{
				if (!(childNode.Name == "moved"))
				{
					continue;
				}
				string text3 = ParseAttrStr(childNode, "path");
				Transform transformFromHierarchyPath = GetTransformFromHierarchyPath(text3);
				if (transformFromHierarchyPath != null)
				{
					int num7 = ParseAttrID(childNode, "placeableID");
					Placeable component = transformFromHierarchyPath.GetComponent<Placeable>();
					if (component != null && (component.ID == num7 || component.name.CompareTo("Ceiling") == 0 || component.name.CompareTo("DeathPit") == 0 || component.name.CompareTo("GoalBlock") == 0 || component.name.CompareTo("LeftWall") == 0 || component.name.CompareTo("StartPlank") == 0 || component.name.CompareTo("RightWall") == 0))
					{
						transformFromHierarchyPath.position = new Vector3(ParseAttrFloat(childNode, "pX"), ParseAttrFloat(childNode, "pY"), ParseAttrFloat(childNode, "pZ"));
						transformFromHierarchyPath.rotation = Quaternion.Euler(ParseAttrFloat(childNode, "rX"), ParseAttrFloat(childNode, "rY"), ParseAttrFloat(childNode, "rZ"));
						transformFromHierarchyPath.localScale = new Vector3(ParseAttrFloat(childNode, "sX", 1f), ParseAttrFloat(childNode, "sY", 1f), ParseAttrFloat(childNode, "sZ", 1f));
						if (transformFromHierarchyPath.localScale.magnitude < 0.1f)
						{
							transformFromHierarchyPath.localScale = Vector3.one;
						}
						component.OriginalPosition = transformFromHierarchyPath.position;
						component.OriginalRotation = transformFromHierarchyPath.rotation;
						component.OriginalScale = transformFromHierarchyPath.localScale;
						int num8 = ParseAttrInt(childNode, "damageLevel");
						bool flag = ParseAttrBool(childNode, "alwaysSaveDamage");
						if (num8 > 0 || flag || GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
						{
							component.SetInitialDamageLevel(num8, allowDamageReset: true);
						}
					}
					else
					{
						UnityEngine.Debug.LogError("Could not find placeable with ID " + num7 + " at " + text3);
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Could not restore transform for initial level placeable with hierarchy path: " + text3);
				}
			}
		}
		foreach (KeyValuePair<int, List<int>> item in dictionary2)
		{
			SaveablePiece value = null;
			if (!dictionary.TryGetValue(item.Key, out value))
			{
				continue;
			}
			foreach (int item2 in item.Value)
			{
				SaveablePiece value2 = null;
				if (dictionary.TryGetValue(item2, out value2))
				{
					value2.parentPiece = value;
				}
			}
		}
		foreach (KeyValuePair<int, int> item3 in dictionary3)
		{
			SaveablePiece value3 = null;
			if (dictionary.TryGetValue(item3.Key, out value3))
			{
				SaveablePiece value4 = null;
				if (dictionary.TryGetValue(item3.Value, out value4))
				{
					value3.mainPiece = value4;
					if (value3.subElementName.NullOrEmpty())
					{
						UnityEngine.Debug.LogError("Found sub-element with no subElementName");
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Could not find main block " + item3.Value + " for sub-element " + item3.Key);
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Could not find sub-element " + item3.Key);
			}
		}
		ClearInitialDestroyedObjects();
		RestoreSaveables(dictionary);
		return true;
	}

	private void RestoreSaveables(Dictionary<int, SaveablePiece> saveables, bool restoreAsUnsaveable = false)
	{
		int num = 0;
		HashSet<Placeable> hashSet = new HashSet<Placeable>();
		foreach (KeyValuePair<int, SaveablePiece> saveable in saveables)
		{
			SaveablePiece value = saveable.Value;
			if (!value.subElementName.NullOrEmpty())
			{
				continue;
			}
			UnityEngine.Object prefabForPlaceableIndex = metadataList.GetPrefabForPlaceableIndex(value.blockID);
			if (prefabForPlaceableIndex != null)
			{
				try
				{
					GameObject gameObject = null;
					foreach (SaveablePiece initialLevelPlaceable in initialLevelPlaceables)
					{
						if (initialLevelPlaceable.placeableID == value.placeableID)
						{
							gameObject = initialLevelPlaceable.placeable.gameObject;
							break;
						}
					}
					if (gameObject == null)
					{
						gameObject = (GameObject)UnityEngine.Object.Instantiate(prefabForPlaceableIndex);
					}
					value.metadata = gameObject.GetComponent<PlaceableMetadata>();
					value.placeable = ((value.metadata.placeableRef != null) ? value.metadata.placeableRef : gameObject.GetComponent<Placeable>());
					value.placeable.transform.position = value.pos;
					Vector3 eulerAngles = value.rot.eulerAngles;
					eulerAngles.z = Mathf.Round(eulerAngles.z / 90f) * 90f;
					value.placeable.transform.rotation = Quaternion.Euler(eulerAngles);
					value.placeable.transform.localScale = value.scale;
					if (value.placeableID != -1)
					{
						value.placeable.ID = value.placeableID;
						num = Mathf.Max(value.placeable.GetOriginalSequenceID(), num);
					}
					if (!value.overrideName.NullOrEmpty())
					{
						value.placeable.gameObject.name = value.overrideName;
					}
					if (restoreAsUnsaveable)
					{
						value.placeable.IsSaveable = false;
					}
					if (value.placeable.IsNetworked && value.placeable.NetSurrogate == null && LobbyManager.instance.CurrentGameController.hasAuthority)
					{
						LobbyManager.instance.CurrentGameController.SpawnNetSurrogate(value.placeable.ID);
					}
					Teleporter component = value.placeable.GetComponent<Teleporter>();
					if (component != null && value.teleporterDestinationID == -1)
					{
						component.preventAutoConnectOnSpawn = true;
					}
					value.placeable.Place(0, sendEvent: true, force: true);
					if (value.placeable.canSetCustomColor)
					{
						value.placeable.SetColor(value.CustomColor);
					}
					if (value.damageLevel != 0 || value.placeable.alwaysSaveDamage || GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
					{
						value.placeable.SetInitialDamageLevel(value.damageLevel, allowDamageReset: true);
					}
					if (value.metadata.subElements.Count <= 0)
					{
						continue;
					}
					foreach (MultipiecePart subElement in value.metadata.subElements)
					{
						hashSet.Add(subElement);
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Exception while instantiating object " + value.sceneID + " (" + prefabForPlaceableIndex.name + "): " + ex.Message);
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Could not find prefab for block ID " + value.blockID + " (scene ID " + value.sceneID + ")");
			}
		}
		foreach (KeyValuePair<int, SaveablePiece> saveable2 in saveables)
		{
			SaveablePiece value2 = saveable2.Value;
			if (!(value2.placeable != null))
			{
				continue;
			}
			if (value2.teleporterDestinationID != -1 && saveables.TryGetValue(value2.teleporterDestinationID, out var value3))
			{
				Teleporter component2 = value2.placeable.GetComponent<Teleporter>();
				Teleporter component3 = value3.placeable.GetComponent<Teleporter>();
				if (component2 != null && component3 != null && component2 != component3)
				{
					component2.startupLinkedTeleporter = component3;
				}
			}
			if (value2.placeable.RotationDirection != Placeable.RotationDirections.None)
			{
				if (value2.clockwise)
				{
					value2.placeable.RotationDirection = Placeable.RotationDirections.Clockwise;
				}
				else
				{
					value2.placeable.RotationDirection = Placeable.RotationDirections.CounterClockwise;
				}
			}
		}
		foreach (KeyValuePair<int, SaveablePiece> saveable3 in saveables)
		{
			SaveablePiece value4 = saveable3.Value;
			if (!value4.subElementName.NullOrEmpty())
			{
				bool flag = false;
				foreach (MultipiecePart subElement2 in value4.mainPiece.metadata.subElements)
				{
					if (subElement2.name == value4.subElementName)
					{
						value4.placeable = subElement2;
						value4.metadata = value4.mainPiece.metadata;
						flag = true;
						if (value4.placeableID != -1)
						{
							value4.placeable.ID = value4.placeableID;
							num = Mathf.Max(value4.placeable.GetOriginalSequenceID(), num);
						}
						subElement2.IsSaveable = value4.mainPiece.placeable.IsSaveable;
						if (!value4.overrideSubElementName.NullOrEmpty())
						{
							subElement2.name = value4.overrideSubElementName;
						}
						if (value4.placeable.canSetCustomColor)
						{
							value4.placeable.SetColor(value4.CustomColor);
						}
						break;
					}
				}
				if (!flag)
				{
					UnityEngine.Debug.LogError("Could not find sub-element for saveable " + value4.sceneID);
				}
			}
			if (value4.placeable != null)
			{
				value4.placeable.transform.position = value4.pos;
				Vector3 eulerAngles2 = value4.rot.eulerAngles;
				eulerAngles2.z = Mathf.Round(eulerAngles2.z / 90f) * 90f;
				value4.placeable.transform.rotation = Quaternion.Euler(eulerAngles2);
				value4.placeable.OriginalRotation = value4.placeable.transform.rotation;
				value4.placeable.transform.localScale = value4.scale;
			}
			else
			{
				UnityEngine.Debug.LogError("Could not set transforms for saveable " + value4.sceneID + " because placeable was not set.");
			}
		}
		foreach (KeyValuePair<int, SaveablePiece> saveable4 in saveables)
		{
			SaveablePiece value5 = saveable4.Value;
			if (!value5.parentPath.NullOrEmpty())
			{
				Transform transformFromHierarchyPath = GetTransformFromHierarchyPath(value5.parentPath);
				if (transformFromHierarchyPath != null)
				{
					Placeable component4 = transformFromHierarchyPath.GetComponent<Placeable>();
					if (component4 != null)
					{
						LinkPlaceables(component4, value5.placeable);
					}
					else
					{
						value5.placeable.transform.SetParent(transformFromHierarchyPath, worldPositionStays: true);
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Parent path could not be resolved: " + value5.parentPath);
				}
			}
			else if (value5.parentPiece != null)
			{
				if (value5.parentAttachmentPoint.NullOrEmpty())
				{
					LinkPlaceables(value5.parentPiece.placeable, value5.placeable);
				}
				else
				{
					bool flag2 = false;
					foreach (MultipiecePart attachmentPoint in value5.parentPiece.metadata.attachmentPoints)
					{
						if (attachmentPoint.name == value5.parentAttachmentPoint)
						{
							LinkPlaceables(attachmentPoint, value5.placeable);
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						UnityEngine.Debug.LogError("Could not find parent attachment point " + value5.parentAttachmentPoint);
					}
				}
			}
			if (value5.mainPiece != null)
			{
				hashSet.Remove(value5.placeable);
			}
		}
		foreach (KeyValuePair<int, SaveablePiece> saveable5 in saveables)
		{
			SaveablePiece value6 = saveable5.Value;
			if (!value6.parentPath.NullOrEmpty())
			{
				value6.placeable.transform.localPosition = value6.relativeAttachPosition;
				value6.placeable.OriginalPosition = value6.placeable.transform.position;
				value6.placeable.relativeAttachPosition = value6.relativeAttachPosition;
			}
		}
		bool flag3 = false;
		foreach (Placeable item in hashSet)
		{
			MultipiecePart component5 = item.GetComponent<MultipiecePart>();
			if (!(component5 != null) || !(component5.MainBlock != null))
			{
				continue;
			}
			foreach (MultipiecePart subElement3 in component5.MainBlock.GetComponent<PlaceableMetadata>().subElements)
			{
				if (subElement3.name == component5.name)
				{
					UnityEngine.Debug.Log("Destroying unused sub-element " + subElement3.name);
					flag3 = true;
					subElement3.ID = -9999999;
					if (!subElement3.MarkedForDestruction)
					{
						subElement3.DestroySelf(destroyChildren: false, useSmoke: false);
					}
					break;
				}
			}
		}
		if (flag3)
		{
			LobbyManager.instance.CurrentGameController.DestroyMarkedPiecesNow();
		}
		foreach (KeyValuePair<int, SaveablePiece> saveable6 in saveables)
		{
			SaveablePiece value7 = saveable6.Value;
			if (value7.placeable != null)
			{
				if (!value7.placeable.Placed)
				{
					if (value7.mainPiece == null)
					{
						value7.placeable.Place(0, sendEvent: true, force: true);
					}
					else
					{
						value7.placeable.Place(0, sendEvent: false, force: true);
					}
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Error while trying to restore saveable: placeable is null (block ID: " + value7.blockID + ", placeable ID: " + value7.placeableID + ")");
			}
		}
		Placeable.SetInitialSequenceID(num);
		GameEventManager.SendEvent(new QuicksaverLevelFinishedLoading());
	}

	private void LinkPlaceables(Placeable top, Placeable bottom)
	{
		bool flag = top == null;
		bool flag2 = bottom == null;
		if (flag && flag2)
		{
			UnityEngine.Debug.LogError("ERROR: Tried to link two null placeables.");
			return;
		}
		if (flag)
		{
			UnityEngine.Debug.LogError("ERROR: Tried to link placeables: Top = null, Bottom = " + bottom.name + " (ID " + bottom.ID + ")");
			return;
		}
		if (flag2)
		{
			UnityEngine.Debug.LogError("ERROR: Tried to link placeables: Top = " + top.name + " (ID " + top.ID + "), Bottom = null");
			return;
		}
		AttachmentGroup attachmentGroup = null;
		if (bottom.Group != null)
		{
			attachmentGroup = bottom.Group;
		}
		if (top.Group != null)
		{
			if (!top.Group.AddLink(top, bottom))
			{
				UnityEngine.Debug.LogError("Could not add link");
			}
			else if (attachmentGroup != null)
			{
				AttachmentGroup.MergeGroups(top.Group, attachmentGroup);
			}
		}
		else
		{
			AttachmentGroup g = new AttachmentGroup(top, bottom);
			if (attachmentGroup != null)
			{
				AttachmentGroup.MergeGroups(g, attachmentGroup);
			}
		}
		if (bottom.transform.parent != top.transform)
		{
			top.AttachPiece(bottom);
		}
	}

	public static bool ParseAttrBool(XmlNode node, string attributeName, bool defaultValue = false)
	{
		XmlAttribute xmlAttribute = node.Attributes[attributeName];
		if (xmlAttribute != null)
		{
			return bool.Parse(xmlAttribute.Value);
		}
		return defaultValue;
	}

	public static T ParseAttrEnum<T>(XmlNode node, string attributeName, T defaultValue)
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enum");
		}
		XmlAttribute xmlAttribute = node.Attributes[attributeName];
		if (xmlAttribute == null)
		{
			return defaultValue;
		}
		return (T)Enum.Parse(typeof(T), xmlAttribute.Value);
	}

	public static int ParseAttrID(XmlNode node, string attributeName, int defaultValue = -1)
	{
		return ParseAttrInt(node, attributeName, defaultValue);
	}

	public static int ParseAttrInt(XmlNode node, string attributeName, int defaultValue = 0)
	{
		XmlAttribute xmlAttribute = node.Attributes[attributeName];
		if (xmlAttribute != null)
		{
			try
			{
				return int.Parse(xmlAttribute.Value);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("ParseAttrInt (" + xmlAttribute.Value + "): " + ex.Message + "\n" + ex.StackTrace);
				return defaultValue;
			}
		}
		return defaultValue;
	}

	public static long ParseAttrLong(XmlNode node, string attributeName, long defaultValue = -1L)
	{
		XmlAttribute xmlAttribute = node.Attributes[attributeName];
		if (xmlAttribute != null)
		{
			return long.Parse(xmlAttribute.Value);
		}
		return defaultValue;
	}

	public static float ParseAttrFloat(XmlNode node, string attributeName, float defaultValue = 0f)
	{
		XmlAttribute xmlAttribute = node.Attributes[attributeName];
		if (xmlAttribute != null)
		{
			return Parsing.ParseFloat_InvariantCulture(xmlAttribute.Value);
		}
		return defaultValue;
	}

	public static string ParseAttrStr(XmlNode node, string attributeName, string defaultValue = "")
	{
		XmlAttribute xmlAttribute = node.Attributes[attributeName];
		if (xmlAttribute != null)
		{
			return xmlAttribute.Value;
		}
		return defaultValue;
	}

	private Placeable FindInitialLevelPlaceable(Placeable placeable)
	{
		foreach (SaveablePiece initialLevelPlaceable in initialLevelPlaceables)
		{
			if (initialLevelPlaceable.placeable != null && initialLevelPlaceable.placeable == placeable)
			{
				return initialLevelPlaceable.placeable;
			}
		}
		return null;
	}

	public void OnUnsaveablePieceDestroyed(Placeable placeable)
	{
		if (FindInitialLevelPlaceable(placeable) != null)
		{
			initialDestroyedPaths.Add(GetHierarchyPath(placeable.transform, includeLeaf: true));
		}
	}

	public void OnPieceDestroyed(Placeable placeable)
	{
		if (FindInitialLevelPlaceable(placeable) != null && (placeable is GoalBlock || placeable is CrumblingBlock))
		{
			initialDestroyedPaths.Add(GetHierarchyPath(placeable.transform, includeLeaf: true));
		}
	}

	public void OnSetupStartLevel(UnityAction onAllClientsLoadedSnapshot)
	{
		if (!LobbyManager.instance.CurrentGameController.hasAuthority)
		{
			return;
		}
		MemorizeInitialLevelPlaceables();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (SaveablePiece initialLevelPlaceable in initialLevelPlaceables)
		{
			string hierarchyPath = GetHierarchyPath(initialLevelPlaceable.placeable.transform, includeLeaf: true);
			if (!dictionary.ContainsKey(hierarchyPath))
			{
				dictionary.Add(hierarchyPath, initialLevelPlaceable.placeable.ID);
			}
			else
			{
				UnityEngine.Debug.LogError("Could not add path/ID - duplicate entry detected for " + hierarchyPath);
			}
		}
		string[] paths = dictionary.Keys.ToArray();
		int[] iDs = dictionary.Values.ToArray();
		LobbyManager.instance.CurrentGameController.CallRpcPropagateBlockIDs(paths, iDs);
		if (levelPortalXml != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(levelPortalXml);
			LobbyManager.instance.CurrentGameController.CompressAndSendSnapshotBytes(bytes, onAllClientsLoadedSnapshot);
			LoadXmlSnapshotFromString(levelPortalXml);
			lastLoadedXml = levelPortalXml;
			levelPortalXml = null;
			string snapshotCode = GameState.GetInstance().currentSnapshotInfo.snapshotCode;
			if (!snapshotCode.NullOrEmpty())
			{
				string formattedSnapshotCode = GameSparksQuery.GetFormattedSnapshotCode(snapshotCode);
				if (formattedSnapshotCode.NullOrEmpty() || File.Exists(GetThumbnailFilenameForCode(formattedSnapshotCode)))
				{
					return;
				}
				byte[] currentSceneThumbnailBytes = GetCurrentSceneThumbnailBytes();
				if (currentSceneThumbnailBytes != null)
				{
					GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
					gameSparksQuery.UploadLevelThumbnail(GameSparksQuery.SanitizeSnapshotCode(formattedSnapshotCode), currentSceneThumbnailBytes);
					gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
					{
						UnityEngine.Debug.Log("Thumbnail successfully uploaded");
						ThumbnailGenerator.ThumbnailLoaded();
					});
				}
				return;
			}
			string thumbnailFilename = GetThumbnailFilenameForLocalSave(GameState.GetInstance().currentSnapshotInfo.snapshotName);
			if (RamFS.PlatformUsesRamFS)
			{
				RamFS.AddFileExistsOperation(thumbnailFilename, delegate(RamFS.FSOperationReturnCode existsReturnCode)
				{
					if (existsReturnCode == RamFS.FSOperationReturnCode.FileNotFound)
					{
						byte[] currentSceneThumbnailBytes3 = GetCurrentSceneThumbnailBytes();
						if (currentSceneThumbnailBytes3 != null)
						{
							RamFS.AddAddFileOperation(thumbnailFilename, currentSceneThumbnailBytes3, delegate(RamFS.FSOperationReturnCode returnCode)
							{
								if (returnCode == RamFS.FSOperationReturnCode.OK)
								{
									UnityEngine.Debug.Log("Thumbnail successfully created at " + thumbnailFilename);
								}
							});
						}
					}
					else
					{
						UnityEngine.Debug.Log("No new thumbnail created -> Thumbnail already exists at " + thumbnailFilename);
					}
				});
			}
			else if (!File.Exists(thumbnailFilename))
			{
				byte[] currentSceneThumbnailBytes2 = GetCurrentSceneThumbnailBytes();
				if (currentSceneThumbnailBytes2 != null && SaveBytesToFile(currentSceneThumbnailBytes2, thumbnailFilename))
				{
					UnityEngine.Debug.Log("Thumbnail successfully created at " + thumbnailFilename);
				}
			}
		}
		else
		{
			onAllClientsLoadedSnapshot();
		}
	}

	private void MemorizeInitialLevelPlaceables()
	{
		List<PlaceableMetadata> allPlaceables = UnityEngine.Object.FindObjectsOfType<PlaceableMetadata>().ToList();
		initialLevelPlaceables = GetSaveablesFromMetadata(allPlaceables);
		initialDestroyedPaths = new List<string>();
		UnityEngine.Debug.Log("Memorized " + initialLevelPlaceables.Count + " placeables that are part of the level.");
	}

	public void OnClientBlockIDsPropagated()
	{
		MemorizeInitialLevelPlaceables();
	}

	public static Transform GetTransformFromHierarchyPath(string parentPath)
	{
		string[] array = parentPath.Split('/');
		if (array.Length != 0)
		{
			Transform transform = null;
			GameObject gameObject = GameObject.Find(array[0]);
			if (gameObject != null)
			{
				transform = gameObject.transform;
				for (int i = 1; i < array.Length; i++)
				{
					transform = transform.Find(array[i]);
					if (transform == null)
					{
						UnityEngine.Debug.LogError("Could not find " + array[i] + " in path " + parentPath);
						break;
					}
				}
			}
			if (transform != null)
			{
				return transform;
			}
			UnityEngine.Debug.LogError("Parent path could not be resolved: " + parentPath);
		}
		else
		{
			UnityEngine.Debug.LogError("Could not split parent path");
		}
		return null;
	}

	private List<SaveablePiece> GetSaveablesFromMetadata(List<PlaceableMetadata> allPlaceables)
	{
		List<SaveablePiece> saveables = new List<SaveablePiece>();
		HashSet<Placeable> seenPlaceables = new HashSet<Placeable>();
		Dictionary<GameObject, SaveablePiece> gameObjects = new Dictionary<GameObject, SaveablePiece>();
		UnityAction<Placeable, PlaceableMetadata, int> ProcessPlaceable = null;
		ProcessPlaceable = delegate(Placeable placeable3, PlaceableMetadata metadata, int level)
		{
			if (seenPlaceables.Add(placeable3) && !placeable3.MarkedForDestruction)
			{
				SaveablePiece saveablePiece = new SaveablePiece
				{
					blockID = metadata.blockSerializeIndex,
					sceneID = saveables.Count,
					placeable = placeable3,
					metadata = metadata,
					pos = placeable3.OriginalPosition,
					rot = placeable3.OriginalRotation,
					scale = placeable3.OriginalScale
				};
				if (placeable3.IsSubElement && !placeable3.originalSubElementName.NullOrEmpty() && placeable3.gameObject.name != placeable3.originalSubElementName)
				{
					saveablePiece.overrideName = placeable3.gameObject.name;
				}
				saveablePiece.placeableID = placeable3.ID;
				bool flag2 = true;
				switch (GameSettings.GetInstance().GameMode)
				{
				case GameState.GameMode.CREATIVE:
				case GameState.GameMode.PARTY:
					if (placeable3.alwaysSaveDamage && placeable3.damageLevel >= placeable3.DestroyedDamageLevel)
					{
						flag2 = false;
					}
					break;
				case GameState.GameMode.FREEPLAY:
				case GameState.GameMode.CHALLENGE:
					placeable3.damageLevel = placeable3.initialDamage;
					break;
				}
				saveablePiece.damageLevel = placeable3.damageLevel;
				saveablePiece.alwaysSaveDamage = placeable3.alwaysSaveDamage;
				if (flag2)
				{
					saveables.Add(saveablePiece);
					gameObjects.Add(placeable3.gameObject, saveablePiece);
					if (placeable3.ChildPieces.Count > 0)
					{
						foreach (Placeable childPiece in placeable3.ChildPieces)
						{
							if (childPiece != null)
							{
								PlaceableMetadata component4 = childPiece.GetComponent<PlaceableMetadata>();
								if (component4 != null)
								{
									ProcessPlaceable(childPiece, component4, level + 1);
								}
								else
								{
									MultipiecePart multipiecePart = childPiece as MultipiecePart;
									if ((bool)multipiecePart)
									{
										if (multipiecePart.MainBlock != null)
										{
											component4 = multipiecePart.MainBlock.GetComponent<PlaceableMetadata>();
											if (component4 != null)
											{
												ProcessPlaceable(childPiece, component4, level + 1);
											}
											else
											{
												UnityEngine.Debug.LogWarning("Could not find metadata for multipart child block");
											}
										}
										else
										{
											UnityEngine.Debug.LogWarning("Found multipart block without main block.");
										}
									}
									else
									{
										UnityEngine.Debug.LogWarning("Could not find metadata for child block");
									}
								}
							}
						}
					}
				}
			}
		};
		foreach (PlaceableMetadata allPlaceable in allPlaceables)
		{
			Placeable placeable = ((allPlaceable.placeableRef != null) ? allPlaceable.placeableRef : allPlaceable.GetComponent<Placeable>());
			if (placeable != null && !placeable.PickedUp)
			{
				ProcessPlaceable(placeable, allPlaceable, 0);
			}
		}
		foreach (SaveablePiece item in saveables)
		{
			if (!item.placeable.IsSubElement)
			{
				continue;
			}
			MultipiecePart component = item.placeable.GetComponent<MultipiecePart>();
			if (component != null && component.MainBlock != null)
			{
				SaveablePiece value = null;
				if (gameObjects.TryGetValue(component.MainBlock.gameObject, out value))
				{
					item.mainPiece = value;
					item.subElementName = component.name;
					if (!item.placeable.originalSubElementName.NullOrEmpty() && item.placeable.originalSubElementName != component.name)
					{
						item.subElementName = item.placeable.originalSubElementName;
						item.overrideSubElementName = component.name;
					}
					else
					{
						item.subElementName = component.name;
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Could not find main block for sub-element " + component.name);
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Sub-element is not MultipiecePart.");
			}
		}
		foreach (SaveablePiece item2 in saveables)
		{
			SaveablePiece value2 = null;
			Transform parent = item2.placeable.transform.parent;
			if (!(parent != null))
			{
				continue;
			}
			bool flag = false;
			Placeable placeable2 = FindComponentInParents<Placeable>(item2.placeable.transform);
			if (placeable2 != null && (placeable2.Category == Placeable.PieceCategory.PLATFORM || placeable2.Category == Placeable.PieceCategory.MOVINGPLATFORM))
			{
				MultipiecePart component2 = placeable2.GetComponent<MultipiecePart>();
				if (component2 != null && component2.MainBlock != null)
				{
					PlaceableMetadata component3 = component2.MainBlock.GetComponent<PlaceableMetadata>();
					if (component3 != null && component3.attachmentPoints.Count > 0)
					{
						for (int num = 0; num < component3.attachmentPoints.Count; num++)
						{
							if (component3.attachmentPoints[num] == component2)
							{
								if (gameObjects.TryGetValue(component2.MainBlock.gameObject, out value2) && value2.placeable.IsSaveable)
								{
									item2.parentPiece = value2;
									item2.parentAttachmentPoint = component2.name;
									flag = true;
								}
								break;
							}
						}
					}
				}
			}
			if (flag)
			{
				continue;
			}
			if (gameObjects.TryGetValue(parent.gameObject, out value2))
			{
				if (value2.placeable.IsSaveable)
				{
					item2.parentPiece = value2;
				}
				else if (!item2.placeable.IsSubElement || !item2.IsConnectedToMainPiece)
				{
					item2.parentPath = GetHierarchyPath(item2.placeable.transform);
					item2.relativeAttachPosition = item2.placeable.transform.localPosition;
				}
			}
			else if (placeable2 == null || (item2.placeable.IsSubElement && !item2.IsConnectedToMainPiece))
			{
				item2.parentPath = GetHierarchyPath(item2.placeable.transform);
				item2.relativeAttachPosition = item2.placeable.transform.localPosition;
			}
		}
		return saveables;
	}

	private T FindComponentInParents<T>(Transform self)
	{
		Transform parent = self.parent;
		while (parent != null)
		{
			T component = parent.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			parent = parent.parent;
		}
		return default(T);
	}

	public bool LoadXmlSnapshotFromString(string xml)
	{
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.LoadXml(xml);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Could not parse XML snapshot from string: " + ex.Message);
			return false;
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("Loaded XML Snapshot from string in: " + stopwatch.ElapsedMilliseconds + " ms");
		bool flag = false;
		try
		{
			flag = LoadSnapshotFromXmlDocument(xmlDocument);
		}
		catch (Exception ex2)
		{
			flag = false;
			UnityEngine.Debug.LogError("Exception while loading snapshot: " + ex2.Message + "\n" + ex2.StackTrace);
		}
		return flag;
	}

	public static string GetHashForFile(byte[] fileContents)
	{
		return HexStringFromBytes(new SHA1CryptoServiceProvider().ComputeHash(fileContents)) + "+" + fileContents.Length;
	}

	public static string GetHashForFile(string fileContents)
	{
		return GetHashForFile(Encoding.UTF8.GetBytes(fileContents));
	}

	public static string HexStringFromBytes(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in bytes)
		{
			string value = b.ToString("x2");
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public static void CopyStringToClipboard(string str)
	{
		TextEditor textEditor = new TextEditor();
		textEditor.text = str;
		textEditor.SelectAll();
		textEditor.Copy();
	}

	public static string EnsureUniqueLocalLevelName(string localLevelBasePath, IEnumerable<string> existingFiles = null)
	{
		Func<string, bool> func;
		if (existingFiles == null)
		{
			func = FileExists;
		}
		else
		{
			string[] files = (existingFiles as string[]) ?? existingFiles.ToArray();
			func = (string s) => files.Contains(s);
		}
		if (!func(localLevelBasePath + ".c.snapshot") && !func(localLevelBasePath + ".v.snapshot") && !func(localLevelBasePath + ".snapshot"))
		{
			return localLevelBasePath;
		}
		for (int num = 0; num < 100; num++)
		{
			string text = $"{localLevelBasePath}_{num + 1}";
			if (!func(text + ".c.snapshot") && !func(text + ".v.snapshot") && !func(text + ".snapshot"))
			{
				return text;
			}
		}
		return null;
	}

	private static bool FileExists(string filename)
	{
		return File.Exists(filename);
	}

	public static string EnsureUniqueFilename(string inputFilename, int loopNum = 0, IEnumerable<string> existingFiles = null)
	{
		if (loopNum > 100)
		{
			return null;
		}
		if (existingFiles != null)
		{
			if (!existingFiles.Contains(inputFilename))
			{
				return inputFilename;
			}
		}
		else if (!File.Exists(inputFilename))
		{
			return inputFilename;
		}
		string directoryName = Path.GetDirectoryName(inputFilename);
		string text = Path.GetFileNameWithoutExtension(inputFilename);
		string extension = Path.GetExtension(inputFilename);
		string localSaveExtraSuffix = GetLocalSaveExtraSuffix(text);
		if (localSaveExtraSuffix != null)
		{
			text = text.Substring(0, text.Length - localSaveExtraSuffix.Length);
		}
		if (text.EndsWith(")"))
		{
			int num = text.Length - 2;
			int num2 = num;
			while (num >= 2 && char.IsDigit(text[num]))
			{
				num--;
			}
			if (num != num2 && num >= 0 && text[num] == '(' && num > 0 && text[num - 1] == ' ')
			{
				int num3 = num2 - num;
				int num4 = int.Parse(text.Substring(num + 1, num3));
				num4++;
				text = text.Substring(0, text.Length - (num3 + 1));
				text = text + num4 + ")";
				string text2 = directoryName + "/" + text;
				if (!localSaveExtraSuffix.NullOrEmpty())
				{
					text2 += localSaveExtraSuffix;
				}
				if (!extension.NullOrEmpty())
				{
					text2 += extension;
				}
				return EnsureUniqueFilename(text2, loopNum + 1, existingFiles);
			}
		}
		string text3 = directoryName + "/" + text + " (1)";
		if (!localSaveExtraSuffix.NullOrEmpty())
		{
			text3 += localSaveExtraSuffix;
		}
		if (!extension.NullOrEmpty())
		{
			text3 += extension;
		}
		return EnsureUniqueFilename(text3, loopNum + 1, existingFiles);
	}

	public static string GetLocalSaveExtraSuffix(string filenameWithoutExt)
	{
		if (filenameWithoutExt.EndsWith(".c"))
		{
			return ".c";
		}
		if (filenameWithoutExt.EndsWith(".v"))
		{
			return ".v";
		}
		return null;
	}

	public int CalculateLevelFullness()
	{
		int num = 0;
		foreach (PlaceableMetadata item in UnityEngine.Object.FindObjectsOfType<PlaceableMetadata>().ToList())
		{
			if (!item.isLevelGeometry)
			{
				Placeable placeable = ((item.placeableRef != null) ? item.placeableRef : item.GetComponent<Placeable>());
				if (placeable != null && placeable.Placed && placeable.IsSaveable)
				{
					num += placeable.placementCost;
				}
			}
		}
		return num;
	}

	public int CalculateLevelFullnessFromXML(XmlDocument doc)
	{
		return CalculateLevelFullnessFromXML(doc, metadataList);
	}

	public static int CalculateLevelFullnessFromXML(XmlDocument doc, PlaceableMetadataList metadataList)
	{
		XmlElement documentElement = doc.DocumentElement;
		int num = 0;
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name == "block")
			{
				int num2 = ParseAttrID(childNode, "blockID");
				if (num2 != -1)
				{
					Placeable component = (metadataList.GetPrefabForPlaceableIndex(num2) as GameObject).GetComponent<Placeable>();
					num += component.placementCost;
				}
			}
		}
		return num;
	}

	public static bool CheckNonDefaultModsFromXML(XmlDocument doc)
	{
		XmlNodeList xmlNodeList = doc.DocumentElement.SelectNodes("mods");
		if (xmlNodeList.Count > 0)
		{
			ModSource modSource = new ModSource();
			modSource.ReadFromXmlNode(xmlNodeList[0]);
			return !modSource.HasDefaultValues();
		}
		return false;
	}

	public void LoadCompressedSnapshotThreaded(byte[] compressedBytes, UnityAction<XmlDocument> onXmlLoaded)
	{
		XmlDocument resultDocument = null;
		WorkerThreadManager.Instance.AddFileOpJob(delegate
		{
			byte[] bytes = SevenZipHelper.Decompress(compressedBytes);
			string xml = Encoding.UTF8.GetString(bytes);
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(xml);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Could not parse XML snapshot from string: " + ex.Message);
				xmlDocument = null;
				return;
			}
			if (xmlDocument != null)
			{
				resultDocument = xmlDocument;
			}
		}, delegate
		{
			onXmlLoaded(resultDocument);
		});
	}

	private bool TransformChanged(SaveablePiece initialSaveable, SaveablePiece saveable)
	{
		if ((initialSaveable.pos - saveable.pos).magnitude > 0.01f)
		{
			return true;
		}
		if (Mathf.Abs(initialSaveable.rot.eulerAngles.z - saveable.rot.eulerAngles.z) > 0.01f)
		{
			return true;
		}
		if ((initialSaveable.scale - saveable.scale).magnitude > 0.01f)
		{
			return true;
		}
		return false;
	}

	public static string GetThumbnailFilenameForLocalSave(string localSaveName)
	{
		return LocalThumbnailsFolder + "/l_" + localSaveName + PreferredThumbnailFormatExtension;
	}

	public static string GetThumbnailFilenameForCode(string formattedCode)
	{
		return RemoteThumbnailsFolder + "/c_" + formattedCode + PreferredThumbnailFormatExtension;
	}

	public static byte[] EncodeTextureToPreferredFormat(Texture2D tex)
	{
		switch (GameSettings.GetInstance().LevelThumbnailFormat)
		{
		case ThumbnailFormat.PNG:
			return tex.EncodeToPNG();
		case ThumbnailFormat.JPG:
			return tex.EncodeToJPG(50);
		default:
			UnityEngine.Debug.LogError("Unknown image format");
			return null;
		}
	}

	public void SaveLocalThumbnail(string savename)
	{
		Texture2D currentSceneThumbnailTexture = GetCurrentSceneThumbnailTexture();
		byte[] bytes = EncodeTextureToPreferredFormat(currentSceneThumbnailTexture);
		if (bytes != null && bytes.Length != 0)
		{
			string fullpath = GetThumbnailFilenameForLocalSave(savename);
			if (RamFS.PlatformUsesRamFS)
			{
				RamFS.AddAddFileOperation(fullpath, bytes, delegate(RamFS.FSOperationReturnCode returnCode)
				{
					if (returnCode != RamFS.FSOperationReturnCode.OK)
					{
						UnityEngine.Debug.LogError("Could not save level thumbnail to " + fullpath);
					}
				});
			}
			else
			{
				WorkerThreadManager.Instance.AddFileOpJob(delegate
				{
					SaveBytesToFile(bytes, fullpath);
				});
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Could not save level thumbnail - image encoding failed!");
		}
		UnityEngine.Object.Destroy(currentSceneThumbnailTexture);
	}

	public static bool SaveBytesToFile(byte[] bytes, string filename)
	{
		try
		{
			FileStream fileStream = File.OpenWrite(filename);
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Close();
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Exception while saving file: " + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
		return true;
	}

	public Texture2D GetCurrentSceneScreenshot(int targetSizeX, int targetSizeY, bool showGraphpaper = false)
	{
		Bounds screenshotCameraThumbnailBounds = GetScreenshotCameraThumbnailBounds();
		float num = (float)targetSizeX / (float)targetSizeY;
		float camOrthoSize;
		if (screenshotCameraThumbnailBounds.size.y < screenshotCameraThumbnailBounds.size.x)
		{
			float num2 = screenshotCameraThumbnailBounds.size.y * num;
			float num3 = screenshotCameraThumbnailBounds.size.x / num2;
			camOrthoSize = screenshotCameraThumbnailBounds.size.y / 2f * num3;
		}
		else
		{
			camOrthoSize = screenshotCameraThumbnailBounds.size.y / 2f;
		}
		return GetScreenshotTexture(screenshotCameraThumbnailBounds, targetSizeX, targetSizeY, camOrthoSize, showGraphpaper);
	}

	public Texture2D GetCurrentSceneScreenshotWidthPriority(int targetSizeX, bool showGraphpaper = false)
	{
		Bounds screenshotCameraThumbnailBounds = GetScreenshotCameraThumbnailBounds();
		int num = (int)(screenshotCameraThumbnailBounds.size.y * (float)targetSizeX / screenshotCameraThumbnailBounds.size.x);
		if (num > 2000)
		{
			return GetCurrentSceneScreenshot(targetSizeX, 2000);
		}
		if (num < 500)
		{
			return GetCurrentSceneScreenshot(targetSizeX, 300);
		}
		float camOrthoSize = screenshotCameraThumbnailBounds.size.y / 2f;
		return GetScreenshotTexture(screenshotCameraThumbnailBounds, targetSizeX, num, camOrthoSize, showGraphpaper);
	}

	public Texture2D GetCurrentSceneThumbnailTexture()
	{
		int levelThumbnailWidth = GameSettings.GetInstance().LevelThumbnailWidth;
		int levelThumbnailHeight = GameSettings.GetInstance().LevelThumbnailHeight;
		return GetCurrentSceneScreenshot(levelThumbnailWidth, levelThumbnailHeight);
	}

	public byte[] GetCurrentSceneThumbnailBytes()
	{
		Texture2D currentSceneThumbnailTexture = GetCurrentSceneThumbnailTexture();
		byte[] result = EncodeTextureToPreferredFormat(currentSceneThumbnailTexture);
		UnityEngine.Object.Destroy(currentSceneThumbnailTexture);
		return result;
	}

	private Bounds GetScreenshotCameraThumbnailBounds()
	{
		Level levelLayout = LobbyManager.instance.CurrentGameController.LevelLayout;
		if (levelLayout != null && levelLayout.ThumbnailBounds != null)
		{
			return levelLayout.GetThumbnailBounds();
		}
		return new Bounds(Vector3.zero, new Vector3(10f, 10f, 0f));
	}

	public void SaveHighResScreenShot(float pixelsPerWorldUnit, bool withPlayer = false, int maxSizeX = 0, int maxSizeY = 0)
	{
		byte[] array = null;
		try
		{
			Texture2D currentSceneScreenshotHighRes = GetCurrentSceneScreenshotHighRes(pixelsPerWorldUnit, showGraphpaper: false, withPlayer: false, maxSizeX, maxSizeY);
			array = currentSceneScreenshotHighRes.EncodeToJPG(90);
			Directory.CreateDirectory(Application.dataPath + "/../Screenshot/");
			File.WriteAllBytes(Application.dataPath + "/../Screenshot/" + DateTime.Now.ToString("yyyy-MM-dd-hh.mm.ss") + ".jpg", array);
			UnityEngine.Object.Destroy(currentSceneScreenshotHighRes);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Failed to generate screenshot: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public Texture2D GetCurrentSceneScreenshotHighRes(float pixelsPerWorldUnit, bool showGraphpaper = false, bool withPlayer = false, int maxSizeX = 0, int maxSizeY = 0)
	{
		Bounds screenshotCameraThumbnailBounds = GetScreenshotCameraThumbnailBounds();
		int num = Mathf.CeilToInt(screenshotCameraThumbnailBounds.size.x * pixelsPerWorldUnit);
		int num2 = Mathf.CeilToInt(screenshotCameraThumbnailBounds.size.y * pixelsPerWorldUnit);
		if (maxSizeX > 0 && num > maxSizeX)
		{
			float num3 = (float)maxSizeX / (float)num;
			num = Mathf.CeilToInt((float)num * num3);
			num2 = Mathf.CeilToInt((float)num2 * num3);
			pixelsPerWorldUnit *= num3;
		}
		if (maxSizeY > 0 && num2 > maxSizeY)
		{
			float num4 = (float)maxSizeY / (float)num2;
			num = Mathf.CeilToInt((float)num * num4);
			num2 = Mathf.CeilToInt((float)num2 * num4);
			pixelsPerWorldUnit *= num4;
		}
		num += num % 2;
		num2 += num2 % 2;
		float camOrthoSize = (float)(num2 / 2) / pixelsPerWorldUnit;
		return GetScreenshotTexture(screenshotCameraThumbnailBounds, num, num2, camOrthoSize, showGraphpaper, withPlayer);
	}

	public Texture2D GetScreenshotTexture(Bounds bounds, int pixelSizeX, int pixelSizeY, float camOrthoSize, bool showGraphpaper, bool WithPlayer = false)
	{
		Graphpaper graphpaper = UnityEngine.Object.FindObjectOfType<Graphpaper>();
		float alpha = 0f;
		if (graphpaper != null)
		{
			alpha = graphpaper.CanvasGroup.alpha;
			if (showGraphpaper)
			{
				graphpaper.CanvasGroup.alpha = graphpaper.maxAlpha;
			}
			else
			{
				graphpaper.CanvasGroup.alpha = 0f;
			}
		}
		GameEventManager.SendEvent(new PrepareBlankLevelForScreenShot(hidden: true));
		Vector3 center = bounds.center;
		GameObject gameObject = new GameObject("Screenshot Camera");
		Camera camera = gameObject.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = camOrthoSize;
		camera.aspect = (float)pixelSizeX / (float)pixelSizeY;
		camera.cullingMask = GetScreenshotCullingMask(WithPlayer);
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.white;
		camera.transform.position = center + new Vector3(0f, 0f, -250f);
		RenderTexture renderTexture = (camera.targetTexture = new RenderTexture(pixelSizeX, pixelSizeY, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default));
		camera.Render();
		camera.targetTexture = null;
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(pixelSizeX, pixelSizeY, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, pixelSizeX, pixelSizeY), 0, 0);
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		UnityEngine.Object.Destroy(gameObject);
		if (graphpaper != null)
		{
			graphpaper.CanvasGroup.alpha = alpha;
		}
		GameEventManager.SendEvent(new PrepareBlankLevelForScreenShot(hidden: false));
		return texture2D;
	}

	public Texture2D GetScreenshotChallengeScoreboardTexture(int pixelSizeX = 1000, int pixelSizeY = 456)
	{
		GameControl gameControl = UnityEngine.Object.FindObjectOfType<GameControl>();
		GameEventManager.SendEvent(new PrepareBlankLevelForScreenShot(hidden: true));
		RenderTexture renderTexture = new RenderTexture(pixelSizeX, pixelSizeY, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		GameObject gameObject = new GameObject("Screenshot Camera");
		Camera camera = gameObject.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 4.2f;
		camera.aspect = (float)pixelSizeX / (float)pixelSizeY;
		camera.cullingMask = gameControl.UICamera.cullingMask;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.blue;
		camera.transform.position = new Vector3(1000.1f, 998.35f, 0f);
		camera.transform.rotation = gameControl.UICamera.transform.rotation;
		camera.nearClipPlane = gameControl.UICamera.nearClipPlane;
		camera.farClipPlane = gameControl.UICamera.farClipPlane;
		camera.targetTexture = renderTexture;
		camera.Render();
		camera.targetTexture = null;
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(pixelSizeX, pixelSizeY, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, pixelSizeX, pixelSizeY), 0, 0);
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		UnityEngine.Object.Destroy(gameObject);
		GameEventManager.SendEvent(new PrepareBlankLevelForScreenShot(hidden: false));
		return texture2D;
	}

	public Texture2D GetMergeLevelAndScoreboard()
	{
		Texture2D screenshotChallengeScoreboardTexture = GetScreenshotChallengeScoreboardTexture();
		Texture2D currentSceneScreenshotWidthPriority = GetCurrentSceneScreenshotWidthPriority(screenshotChallengeScoreboardTexture.width);
		Texture2D texture2D = new Texture2D(screenshotChallengeScoreboardTexture.width, screenshotChallengeScoreboardTexture.height + currentSceneScreenshotWidthPriority.height, TextureFormat.RGB24, mipChain: false);
		Color[] pixels = screenshotChallengeScoreboardTexture.GetPixels();
		Color[] pixels2 = currentSceneScreenshotWidthPriority.GetPixels();
		UnityEngine.Object.Destroy(screenshotChallengeScoreboardTexture);
		UnityEngine.Object.Destroy(currentSceneScreenshotWidthPriority);
		texture2D.SetPixels(0, 0, screenshotChallengeScoreboardTexture.width, screenshotChallengeScoreboardTexture.height, pixels);
		texture2D.SetPixels(0, screenshotChallengeScoreboardTexture.height, currentSceneScreenshotWidthPriority.width, currentSceneScreenshotWidthPriority.height, pixels2);
		return texture2D;
	}

	private static int GetScreenshotCullingMask(bool withPlayer = false)
	{
		int num = -1;
		if (!withPlayer)
		{
			foreach (string includedLayer in includedLayers)
			{
				num |= 1 << LayerMask.NameToLayer(includedLayer);
			}
			foreach (string excludedLayer in excludedLayers)
			{
				num &= ~(1 << LayerMask.NameToLayer(excludedLayer));
			}
		}
		else
		{
			foreach (string item in includedLayersEditor)
			{
				num |= 1 << LayerMask.NameToLayer(item);
			}
			foreach (string item2 in excludedLayersEditor)
			{
				num &= ~(1 << LayerMask.NameToLayer(item2));
			}
		}
		return num;
	}

	public static void FindLocalSaveFilenameWithoutExt(string snapshotName, UnityAction<string> callback)
	{
		string text = LocalSavesFolder + "/" + snapshotName;
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddClassifySnapshotOperation(text, delegate(RamFS.FSOperationReturnCode returnCode, string suffix)
			{
				if (returnCode == RamFS.FSOperationReturnCode.OK)
				{
					callback(snapshotName + suffix);
				}
				else
				{
					callback(snapshotName);
				}
			});
		}
		else if (File.Exists(text + ".snapshot"))
		{
			callback(snapshotName);
		}
		else if (File.Exists(text + ".c.snapshot"))
		{
			callback(snapshotName + ".c");
		}
		else if (File.Exists(text + ".v.snapshot"))
		{
			callback(snapshotName + ".v");
		}
		else
		{
			callback(snapshotName);
		}
	}

	public static string GetLocalSaveSuffixForLevelType(FeaturedQuickFilter.LevelTypes levelType)
	{
		return levelType switch
		{
			FeaturedQuickFilter.LevelTypes.Challenge => ".c", 
			FeaturedQuickFilter.LevelTypes.Versus => ".v", 
			_ => "", 
		};
	}

	public static FeaturedQuickFilter.LevelTypes InferLevelTypeFromFilename(string filenameWithoutExt)
	{
		string localSaveExtraSuffix = GetLocalSaveExtraSuffix(filenameWithoutExt);
		if (localSaveExtraSuffix.NullOrEmpty())
		{
			return FeaturedQuickFilter.LevelTypes.Any;
		}
		if (localSaveExtraSuffix == ".c")
		{
			return FeaturedQuickFilter.LevelTypes.Challenge;
		}
		if (localSaveExtraSuffix == ".v")
		{
			return FeaturedQuickFilter.LevelTypes.Versus;
		}
		return FeaturedQuickFilter.LevelTypes.Any;
	}

	public static string GetSnapshotNameWithoutSuffix(string filenameWithoutExt)
	{
		if (filenameWithoutExt.EndsWith(".c") || filenameWithoutExt.EndsWith(".v"))
		{
			return filenameWithoutExt.Substring(0, filenameWithoutExt.Length - 2);
		}
		return filenameWithoutExt;
	}

	public static void GetLocalSaveFilenamesWithoutExtensions(string extensionFilter, Action<IEnumerable<string>> OnFinish)
	{
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddGetExistingFilenamesOperation("/snapshots/", extensionFilter, ordered: true, delegate(IEnumerable<string> returnedFiles)
			{
				List<string> list = (List<string>)returnedFiles;
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = Path.GetFileNameWithoutExtension(list[i]);
				}
				OnFinish(list);
			});
			return;
		}
		string saveFolder = LocalSavesFolder;
		List<string> result = new List<string>();
		WorkerThreadManager.Instance.AddFileOpJob(delegate
		{
			foreach (FileInfo item in from f in new DirectoryInfo(saveFolder).GetFiles("*" + extensionFilter)
				orderby f.CreationTime descending
				select f)
			{
				result.Add(Path.GetFileNameWithoutExtension(item.Name));
			}
		}, delegate
		{
			OnFinish(result);
		});
	}

	public static void RecountLocalSaves(Action OnCountUpdated)
	{
		numLocalSavesQueried = true;
		CheckSaveFolders();
		if (RamFS.PlatformUsesRamFS)
		{
			RamFS.AddCountFilesOperation(LocalSavesFolder, delegate(RamFS.FSOperationReturnCode resultCode, int count)
			{
				numLocalSaves = count;
				OnCountUpdated();
			});
		}
		else
		{
			numLocalSaves = Directory.GetFiles(LocalSavesFolder + "/", "*.snapshot", SearchOption.TopDirectoryOnly).Length;
			OnCountUpdated();
		}
	}

	public static string SanitizePath(string path, string replacement = "_")
	{
		return string.Join(replacement, path.Split(Path.GetInvalidFileNameChars()));
	}
}
