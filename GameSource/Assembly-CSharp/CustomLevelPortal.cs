using System.Runtime.InteropServices;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomLevelPortal : LevelPortal, IGameEventListener
{
	public class SnapshotInfo
	{
		public GameState.LevelName targetLevel;

		public string code;

		public string snapshotName;

		public string xml;

		public FeaturedQuickFilter.LevelTypes levelType;

		public AuthorInfo authorInfo;
	}

	public class AuthorInfo
	{
		public string GSID;

		public string displayName;

		public string platformID;

		public LobbyPlayer.SocialPlatform platform;

		public AuthorInfo(string authorID, string authorDisplayName, GSData platformIDs)
		{
			GSID = authorID;
			displayName = authorDisplayName;
			platform = UGCNameTag.GetPlatformFromGSData(platformIDs);
			platformID = UGCNameTag.GetPlatformIDFromGSData(platformIDs);
		}

		public AuthorInfo(string authorID, string authorDisplayName, string authorPlatformID, LobbyPlayer.SocialPlatform authorPlatform)
		{
			GSID = authorID;
			displayName = authorDisplayName;
			platform = authorPlatform;
			platformID = authorPlatformID;
		}
	}

	public Text levelName;

	public Text levelCode;

	public Text SlotLetter;

	public UGCNameTag authorNameTag;

	public RawImage thumbnailRawImage;

	public SpriteRenderer levelImage;

	public SpriteRenderer spinnyLoadingThing;

	public Color slotLetterColorEnabled;

	public Color slotLetterColorDisabled;

	[SyncVar]
	public bool populated;

	[SyncVar]
	public bool isLoading;

	private bool empty = true;

	public SnapshotInfo snapshotInfo;

	private static int kRpcRpcSetAppearance;

	public bool Networkpopulated
	{
		get
		{
			return populated;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref populated, 2u);
		}
	}

	public bool NetworkisLoading
	{
		get
		{
			return isLoading;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isLoading, 4u);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		thumbnailRawImage.enabled = false;
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void SetPortalIndex(int idx)
	{
		SlotLetter.text = ((char)(65 + idx)).ToString();
	}

	public void ClearContents()
	{
		snapshotXml = null;
		levelCode.text = "";
		authorNameTag.gameObject.SetActive(value: false);
		levelName.text = "";
		levelName.GetComponent<ScrollyHoverText>().scrollOnce = false;
		levelImage.gameObject.SetActive(value: false);
		levelImage.sprite = null;
		SlotLetter.color = slotLetterColorDisabled;
		Networkpopulated = false;
		snapshotInfo = null;
		SoundFXHint = "";
		LevelMusicString = "";
		spinnyLoadingThing.enabled = false;
		empty = true;
		thumbnailRawImage.enabled = false;
		thumbnailRawImage.texture = null;
	}

	public void SetSnapshotInfo(GameState.LevelName targetLevel, string code, string snapshotName, string xml, FeaturedQuickFilter.LevelTypes levelType)
	{
		snapshotInfo = new SnapshotInfo();
		snapshotInfo.targetLevel = targetLevel;
		snapshotInfo.code = code;
		snapshotInfo.snapshotName = snapshotName;
		snapshotInfo.xml = xml;
		snapshotInfo.levelType = levelType;
	}

	public void SetAuthorInfo(string authorID, string authorDisplayName, GSData platformIDs)
	{
		if (!authorID.NullOrEmpty())
		{
			snapshotInfo.authorInfo = new AuthorInfo(authorID, authorDisplayName, platformIDs);
		}
		else
		{
			snapshotInfo.authorInfo = null;
		}
	}

	public void SetContents(GameState.LevelName targetLevel, string snapshotName, string code, string xml, Sprite levelImageSprite, AuthorInfo authorInfo)
	{
		empty = false;
		TargetLevel = targetLevel;
		if (authorInfo != null)
		{
			levelCode.enabled = false;
			authorNameTag.gameObject.SetActive(value: true);
			authorNameTag.InitializeAsync(authorInfo.displayName, authorInfo.platformID, authorInfo.GSID, authorInfo.platform);
		}
		else
		{
			levelCode.enabled = true;
			authorNameTag.gameObject.SetActive(value: false);
			if (!code.NullOrEmpty())
			{
				levelCode.text = GameSparksQuery.GetFormattedSnapshotCode(code);
			}
			else
			{
				levelCode.text = ScriptLocalization.Snapshot.localSnapshot;
			}
		}
		levelName.text = snapshotName;
		levelName.GetComponent<ScrollyHoverText>().scrollOnce = false;
		levelImage.gameObject.SetActive(value: true);
		levelImage.sprite = levelImageSprite;
		SlotLetter.color = slotLetterColorEnabled;
		snapshotXml = xml;
		if (base.hasAuthority)
		{
			if (authorInfo != null)
			{
				CallRpcSetAppearance(targetLevel, snapshotName, code, authorInfo.GSID, authorInfo.displayName, authorInfo.platform, authorInfo.platformID);
			}
			else
			{
				CallRpcSetAppearance(targetLevel, snapshotName, code, null, null, LobbyPlayer.SocialPlatform.Undefined, null);
			}
		}
		Networkpopulated = true;
		bool flag = false;
		LevelPortal[] portals = levelSelectController.portals;
		foreach (LevelPortal levelPortal in portals)
		{
			if (!(levelPortal is CustomLevelPortal) && levelPortal.TargetLevel == targetLevel)
			{
				SoundFXHint = levelPortal.SoundFXHint;
				LevelMusicString = levelPortal.LevelMusicString;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogError("Could not find sound info for level portal " + targetLevel);
		}
		if (!GameSparksQuery.ValidateSnapshotCode(code))
		{
			TryLoadLocalSaveThumbnailFromDisk(snapshotName);
		}
		else
		{
			TryLoadThumbnailFromCloud(code);
		}
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(LanguageChangeEvent) && snapshotInfo != null && snapshotInfo.code.NullOrEmpty())
		{
			levelCode.text = ScriptLocalization.Snapshot.localSnapshot;
		}
	}

	[ClientRpc]
	private void RpcSetAppearance(GameState.LevelName targetLevel, string snapshotName, string code, string authorGSID, string authorDisplayName, LobbyPlayer.SocialPlatform authorPlatform, string authorPlatformID)
	{
		if (!base.hasAuthority)
		{
			AuthorInfo authorInfo = null;
			if (!authorGSID.NullOrEmpty())
			{
				authorInfo = new AuthorInfo(authorGSID, authorDisplayName, authorPlatformID, authorPlatform);
			}
			SetAppearanceForClient(targetLevel, snapshotName, code, authorInfo);
		}
	}

	public void SetAppearanceForClient(GameState.LevelName targetLevel, string snapshotName, string code, AuthorInfo authorInfo)
	{
		levelSelectController.ExecuteOnRuleBookInitialized(delegate
		{
			SetContents(targetLevel, snapshotName, code, null, levelSelectController.undergroundComputer.GetSpriteForLevel(targetLevel), authorInfo);
		});
	}

	public void UpdateAppearanceForClients()
	{
		if (base.hasAuthority && populated)
		{
			if (snapshotInfo != null && snapshotInfo.authorInfo != null)
			{
				AuthorInfo authorInfo = snapshotInfo.authorInfo;
				CallRpcSetAppearance(TargetLevel, levelName.text, levelCode.text, authorInfo.GSID, authorInfo.displayName, authorInfo.platform, authorInfo.platformID);
			}
			else
			{
				CallRpcSetAppearance(TargetLevel, levelName.text, levelCode.text, null, null, LobbyPlayer.SocialPlatform.Undefined, null);
			}
		}
	}

	public void UpdateAppearanceForClient(LobbyPlayer lobbyPlayer)
	{
		if (base.hasAuthority && populated)
		{
			MsgSetCustomPortalInfo msgSetCustomPortalInfo = new MsgSetCustomPortalInfo();
			msgSetCustomPortalInfo.PortalID = PortalID;
			msgSetCustomPortalInfo.targetLevel = TargetLevel;
			msgSetCustomPortalInfo.snapshotName = levelName.text;
			if (GameSparksQuery.ValidateSnapshotCode(levelCode.text))
			{
				msgSetCustomPortalInfo.code = levelCode.text;
			}
			if (snapshotInfo != null && snapshotInfo.authorInfo != null)
			{
				msgSetCustomPortalInfo.authorGSID = snapshotInfo.authorInfo.GSID;
				msgSetCustomPortalInfo.authorDisplayName = snapshotInfo.authorInfo.displayName;
				msgSetCustomPortalInfo.authorPlatform = snapshotInfo.authorInfo.platform;
				msgSetCustomPortalInfo.authorPlatformID = snapshotInfo.authorInfo.platformID;
			}
			NetworkServer.SendToClientOfPlayer(lobbyPlayer.gameObject, NetMsgTypes.SetCustomPortalInfo, msgSetCustomPortalInfo);
		}
	}

	protected override void OnTriggerStay2D(Collider2D c)
	{
		if (populated)
		{
			base.OnTriggerStay2D(c);
			return;
		}
		ScrollyHoverText component = levelName.GetComponent<ScrollyHoverText>();
		if (!component.scrollOnce)
		{
			component.ScrollMessageOnce(LocalizationManager.GetTranslation("UndergroundComputer/NoLevelLoaded"));
		}
	}

	protected override void Update()
	{
		base.Update();
		spinnyLoadingThing.enabled = isLoading;
		if (!populated && !empty)
		{
			ClearContents();
		}
	}

	private void TryLoadLocalSaveThumbnailFromDisk(string snapshotName)
	{
		LevelThumbnailCache.Instance.LoadLocalSaveThumbnail(snapshotName, delegate(Texture2D tex)
		{
			if (tex != null)
			{
				LevelThumbnailCache.Instance.AddTextureUser(tex, this);
				if (thumbnailRawImage != null)
				{
					thumbnailRawImage.texture = tex;
					thumbnailRawImage.enabled = true;
				}
			}
			else if (thumbnailRawImage != null)
			{
				thumbnailRawImage.texture = null;
				thumbnailRawImage.enabled = false;
			}
		});
	}

	private void TryLoadThumbnailFromCloud(string snapshotCode)
	{
		LevelThumbnailCache.Instance.LoadThumbnailFromCloud(snapshotCode, delegate(Texture2D tex)
		{
			if (tex != null)
			{
				LevelThumbnailCache.Instance.AddTextureUser(tex, this);
				if (thumbnailRawImage != null)
				{
					thumbnailRawImage.texture = tex;
					thumbnailRawImage.enabled = true;
				}
			}
			else if (thumbnailRawImage != null)
			{
				thumbnailRawImage.texture = null;
				thumbnailRawImage.enabled = false;
			}
		});
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcSetAppearance(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetAppearance called on server.");
		}
		else
		{
			((CustomLevelPortal)obj).RpcSetAppearance((GameState.LevelName)reader.ReadInt32(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), (LobbyPlayer.SocialPlatform)reader.ReadInt32(), reader.ReadString());
		}
	}

	public void CallRpcSetAppearance(GameState.LevelName targetLevel, string snapshotName, string code, string authorGSID, string authorDisplayName, LobbyPlayer.SocialPlatform authorPlatform, string authorPlatformID)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetAppearance called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetAppearance);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)targetLevel);
		networkWriter.Write(snapshotName);
		networkWriter.Write(code);
		networkWriter.Write(authorGSID);
		networkWriter.Write(authorDisplayName);
		networkWriter.Write((int)authorPlatform);
		networkWriter.Write(authorPlatformID);
		SendRPCInternal(networkWriter, 0, "RpcSetAppearance");
	}

	static CustomLevelPortal()
	{
		kRpcRpcSetAppearance = -745821807;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CustomLevelPortal), kRpcRpcSetAppearance, InvokeRpcRpcSetAppearance);
		NetworkCRC.RegisterBehaviour("CustomLevelPortal", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(populated);
			writer.Write(isLoading);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(populated);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(isLoading);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			populated = reader.ReadBoolean();
			isLoading = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 2) != 0)
		{
			populated = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			isLoading = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
