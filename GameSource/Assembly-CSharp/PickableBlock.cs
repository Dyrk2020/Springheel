using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PickableBlock : NetworkBehaviour, IGameEventListener, IPickable
{
	private bool animating;

	public Animator animator;

	public bool playOnce;

	public Collider2D[] PickColliders;

	public List<Cursor> HoveredCursors = new List<Cursor>();

	protected float hoverTimer;

	public SortOrder spriteSortOrder;

	public InventoryBook inventoryBook;

	public int pageNumber;

	public Text buttonText;

	protected Vector3 initialScale;

	protected PickCursor lastCursor;

	public string HoverSoundEvent;

	public string ClickSoundEvent;

	[SyncVar]
	protected bool Visible;

	protected bool paused;

	protected bool deactivatedInBook;

	public Placeable placeablePrefab;

	public SpriteRenderer[] ArtSprites;

	public SortingGroup sortingGroup;

	public SpriteRenderer crossOut;

	public SpriteRenderer twitchLogo;

	public ParticleSystem[] particleSystems;

	public Material TabletParticleMaterial;

	[SyncVar]
	public bool InLobby;

	[SyncVar]
	public bool InPartybox;

	[SyncVar]
	private string spriteLayer = "Default";

	public float PartyBoxScale = 1f;

	public Transform ArtHolderTransform;

	protected float initialsAnimatorSpeed;

	public float BlockProbabilityScale = 1f;

	public Vector2 BlockProbabilityOffset;

	public bool PlayUIHoverSoundOn = true;

	public bool PlayUISelectSound = true;

	protected bool forceTint;

	protected Color forceTintColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	public bool noneDefaultColors;

	protected Color[] initialColors;

	[SyncVar]
	public Vector3 StartPosition;

	[SyncVar]
	public bool FindPartyBox;

	[SyncVar]
	public bool UseStartPosition;

	[SyncVar]
	public bool isTwitchItem;

	public bool canHaveCustomColorSet;

	public Color CustomColor;

	public int DamageLevel;

	public int blockSerializeIndex = -1;

	private static int kCmdCmdEnable;

	private static int kRpcRpcEnable;

	public List<Cursor> IHoveredCursors
	{
		get
		{
			return HoveredCursors;
		}
		set
		{
		}
	}

	public int PageNumber
	{
		get
		{
			return pageNumber;
		}
		set
		{
			pageNumber = value;
		}
	}

	public PickableBlock ThisPickableBlock => this;

	public SortOrder SpriteSortOrder => spriteSortOrder;

	public string Name => base.name;

	public uint netIdValue => base.netId.Value;

	public string SFXEventName => placeablePrefab.SFXEventName;

	public InventoryBook InventoryBook
	{
		get
		{
			return inventoryBook;
		}
		set
		{
			inventoryBook = value;
		}
	}

	public bool Paused => paused;

	public bool DeactivatedInBook => deactivatedInBook;

	public bool Available
	{
		get
		{
			if (!InLobby && GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
			{
				return true;
			}
			if (GameSettings.GetInstance().itemFilter.ContainsKey(placeablePrefab))
			{
				if (placeablePrefab.FilterOverride.Length == 0)
				{
					return GameSettings.GetInstance().itemFilter[placeablePrefab].Enabled;
				}
				for (int i = 0; i != placeablePrefab.FilterOverride.Length; i++)
				{
					if (!GameSettings.GetInstance().itemFilter[placeablePrefab.FilterOverride[i]].Enabled)
					{
						return false;
					}
				}
				return true;
			}
			if (placeablePrefab.FilterOverride.Length != 0)
			{
				for (int j = 0; j != placeablePrefab.FilterOverride.Length; j++)
				{
					if (!GameSettings.GetInstance().itemFilter[placeablePrefab.FilterOverride[j]].Enabled)
					{
						return false;
					}
				}
				return true;
			}
			if (Debug.isDebugBuild)
			{
				Debug.LogError(placeablePrefab.Name + " Error: placeable not contained in item filter");
			}
			return false;
		}
	}

	public bool NetworkVisible
	{
		get
		{
			return Visible;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref Visible, 1u);
		}
	}

	public bool NetworkInLobby
	{
		get
		{
			return InLobby;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref InLobby, 2u);
		}
	}

	public bool NetworkInPartybox
	{
		get
		{
			return InPartybox;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref InPartybox, 4u);
		}
	}

	public string NetworkspriteLayer
	{
		get
		{
			return spriteLayer;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref spriteLayer, 8u);
		}
	}

	public Vector3 NetworkStartPosition
	{
		get
		{
			return StartPosition;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref StartPosition, 16u);
		}
	}

	public bool NetworkFindPartyBox
	{
		get
		{
			return FindPartyBox;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref FindPartyBox, 32u);
		}
	}

	public bool NetworkUseStartPosition
	{
		get
		{
			return UseStartPosition;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref UseStartPosition, 64u);
		}
	}

	public bool NetworkisTwitchItem
	{
		get
		{
			return isTwitchItem;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isTwitchItem, 128u);
		}
	}

	protected void Awake()
	{
		spriteSortOrder = new SortOrder(base.gameObject);
		initialScale = base.transform.localScale;
		ChangeListener(AddRemove: true);
		initialScale = ArtHolderTransform.localScale;
		if (animator != null)
		{
			initialsAnimatorSpeed = animator.speed;
		}
		EnsureSerializeIndex();
	}

	public void EnsureSerializeIndex()
	{
		if (blockSerializeIndex < 0)
		{
			blockSerializeIndex = placeablePrefab.GetComponent<PlaceableMetadata>().blockSerializeIndex;
		}
	}

	public void ChangeListener(bool AddRemove)
	{
		GameEventManager.ChangeListener<SpecialUIEvent>(this, AddRemove);
		GameEventManager.ChangeListener<PauseEvent>(this, AddRemove);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, AddRemove);
		GameEventManager.ChangeListener<SetpieceColorChangeEvent>(this, AddRemove);
	}

	public void setInitialScale(float newScale)
	{
		base.transform.localScale = Vector3.one * newScale;
		initialScale = base.transform.localScale;
	}

	protected void Start()
	{
		Transform parent = base.gameObject.transform.parent;
		while (inventoryBook == null && parent != null)
		{
			inventoryBook = parent.gameObject.GetComponent<InventoryBook>();
			parent = parent.parent;
		}
		Enable(Visible);
		if (noneDefaultColors)
		{
			initialColors = new Color[ArtSprites.Length];
			for (int i = 0; i < ArtSprites.Length; i++)
			{
				initialColors[i] = ArtSprites[i].color;
			}
		}
		if (FindPartyBox)
		{
			VersusControl versusControl = LobbyManager.instance.CurrentGameController as VersusControl;
			if (versusControl != null && versusControl.PartyBox != null)
			{
				base.transform.parent = versusControl.PartyBox.transform;
				setInitialScale(GameSettings.GetInstance().partyBoxItemScale * PartyBoxScale);
				base.transform.localRotation = Quaternion.identity;
			}
		}
		if (inventoryBook == null)
		{
			if (UseStartPosition)
			{
				base.transform.localPosition = StartPosition;
			}
			else
			{
				NetworkStartPosition = base.transform.localPosition;
			}
		}
		else
		{
			NetworkStartPosition = base.transform.localPosition;
		}
		ChangeArtLayer(spriteLayer);
		crossOut.color = GameSettings.GetInstance().DiabledXoutColor;
	}

	protected void Update()
	{
		if (!Visible || Paused)
		{
			if (animating)
			{
				animator.SetBool("Keep Active", value: false);
				animator.SetTrigger("ImmediateHold");
				animating = false;
			}
			return;
		}
		if ((bool)inventoryBook)
		{
			if (!inventoryBook.Visible)
			{
				return;
			}
			if (!inventoryBook.ShowingOnHost && InLobby)
			{
				deactivatedInBook = true;
			}
			else
			{
				deactivatedInBook = false;
			}
		}
		if (HoveredCursors.Count > 0 && (InPartybox || Available || InLobby))
		{
			hoverTimer += Time.deltaTime;
			ArtHolderTransform.localScale = Vector3.MoveTowards(ArtHolderTransform.localScale, initialScale * GameSettings.GetInstance().hoverScaledAmount, GameSettings.GetInstance().hoverScaledSpeed * Time.deltaTime * initialScale.x);
		}
		else
		{
			hoverTimer = 0f;
			ArtHolderTransform.localScale = Vector3.MoveTowards(ArtHolderTransform.localScale, initialScale, GameSettings.GetInstance().hoverScaledSpeed * Time.deltaTime * initialScale.x);
		}
		if ((bool)animator && animator.isInitialized)
		{
			if (HoveredCursors.Count > 0 && hoverTimer >= GameSettings.GetInstance().animationDelay && Available)
			{
				if (!animating)
				{
					animator.SetTrigger("Active");
					animating = true;
				}
				if (!playOnce)
				{
					animator.SetBool("Keep Active", value: true);
				}
			}
			else
			{
				animator.SetBool("Keep Active", value: false);
				animating = false;
			}
		}
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer in artSprites)
		{
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
		}
	}

	public void Enable()
	{
		Enable(enable: true);
	}

	public void Enable(bool enable)
	{
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = enable;
		}
		NetworkVisible = enable;
		bool networkVisible = enable;
		if (enable)
		{
			Update();
		}
		NetworkVisible = networkVisible;
		SpriteRenderer[] artSprites = ArtSprites;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = enable;
		}
		if ((bool)crossOut)
		{
			if (!Available && !InPartybox)
			{
				crossOut.enabled = enable;
			}
			else
			{
				crossOut.enabled = false;
			}
		}
		else
		{
			crossOut.enabled = false;
		}
		if (buttonText != null)
		{
			buttonText.enabled = enable;
		}
		if ((bool)twitchLogo)
		{
			twitchLogo.enabled = isTwitchItem && enable;
		}
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(enable);
		}
	}

	protected void LateUpdate()
	{
		if (Visible && !Paused)
		{
			Tint();
		}
	}

	public void Disable()
	{
		Enable(enable: false);
		if ((bool)animator)
		{
			animator.SetBool("Active", value: false);
		}
	}

	public void Tint()
	{
		Color color = GameSettings.GetInstance().neutralColor;
		if (Available || InLobby || InPartybox)
		{
			if (HoveredCursors.Count == 1)
			{
				color = GameSettings.GetInstance().highlightColor;
			}
			else if (HoveredCursors.Count == 2)
			{
				color = GameSettings.GetInstance().highlightColor2;
			}
			else if (HoveredCursors.Count == 3)
			{
				color = GameSettings.GetInstance().highlightColor3;
			}
			else if (HoveredCursors.Count > 3)
			{
				color = GameSettings.GetInstance().highlightColor4;
			}
		}
		SpriteRenderer[] artSprites;
		if (noneDefaultColors)
		{
			for (int i = 0; i < ArtSprites.Length; i++)
			{
				Color color2 = initialColors[i] + color - GameSettings.GetInstance().neutralColor;
				ArtSprites[i].color = new Color(color2.r, color2.g, color2.b, ArtSprites[i].color.a);
			}
		}
		else
		{
			artSprites = ArtSprites;
			foreach (SpriteRenderer spriteRenderer in artSprites)
			{
				spriteRenderer.color = new Color(color.r, color.g, color.b, spriteRenderer.color.a);
			}
		}
		if (DeactivatedInBook)
		{
			for (int k = 0; k < ArtSprites.Length; k++)
			{
				SpriteRenderer spriteRenderer2 = ArtSprites[k];
				if (spriteRenderer2.color.a > 0f)
				{
					Color color3 = spriteRenderer2.color;
					color3.a = GameSettings.GetInstance().CantChangeBecauseNotHostAlpha;
					spriteRenderer2.color = color3;
				}
			}
		}
		if (Available || InPartybox)
		{
			return;
		}
		artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer3 in artSprites)
		{
			if (InLobby)
			{
				spriteRenderer3.color = new Color(spriteRenderer3.color.r, spriteRenderer3.color.g, spriteRenderer3.color.b, spriteRenderer3.color.a * GameSettings.GetInstance().DisabledObjectAlpha);
			}
			else
			{
				spriteRenderer3.color = new Color(spriteRenderer3.color.r, spriteRenderer3.color.g, spriteRenderer3.color.b, spriteRenderer3.color.a * GameSettings.GetInstance().DisabledObjectInGameAlpha);
			}
		}
	}

	public void PlayHoverSound()
	{
		if (HoverSoundEvent != "")
		{
			AkSoundEngine.PostEvent(HoverSoundEvent, base.gameObject);
		}
		if (PlayUIHoverSoundOn && Available)
		{
			AkSoundEngine.PostEvent("UI_Inventory_ScrollOn_" + placeablePrefab.SFXEventName, base.gameObject);
		}
	}

	public void OnAccept(PickCursor pickCursor)
	{
		OnAccept(pickCursor, playSound: true);
		if (PlayUISelectSound && Available)
		{
			AkSoundEngine.PostEvent("UI_Inventory_Select_" + placeablePrefab.SFXEventName, base.gameObject);
		}
	}

	public void OnAccept(PickCursor pickCursor, bool playSound)
	{
		lastCursor = pickCursor;
		if (playSound && ClickSoundEvent != "")
		{
			AkSoundEngine.PostEvent(ClickSoundEvent, base.gameObject);
		}
	}

	public void OnDestroy()
	{
		ChangeListener(AddRemove: false);
	}

	[Command]
	private void CmdEnable(bool enable)
	{
		NetworkVisible = enable;
		CallRpcEnable(enable);
	}

	[ClientRpc]
	private void RpcEnable(bool enable)
	{
		if (!base.hasAuthority)
		{
			Enable(enable);
		}
	}

	public void DeactivateForTwitchVotePanelDisplay()
	{
		Collider2D[] pickColliders = PickColliders;
		foreach (Collider2D obj in pickColliders)
		{
			obj.enabled = false;
			obj.gameObject.SetActive(value: false);
		}
	}

	public void DeactivateItem()
	{
		SetFrequency(0, inventoryBook != null && inventoryBook.ShowingOnHost, force: false);
	}

	public void ActivateItem()
	{
		int stepValueFromRarity = TabletBlockList.GetStepValueFromRarity(placeablePrefab.BaseRarity);
		SetFrequency(stepValueFromRarity, inventoryBook != null && inventoryBook.ShowingOnHost, force: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				paused = true;
				if (animator != null)
				{
					animator.speed = 0f;
				}
			}
			else
			{
				paused = false;
				if (animator != null)
				{
					animator.speed = initialsAnimatorSpeed;
				}
			}
		}
		if (type == typeof(SetpieceColorChangeEvent))
		{
			SetpieceColorChangeEvent setpieceColorChangeEvent = e as SetpieceColorChangeEvent;
			if (canHaveCustomColorSet)
			{
				CustomColor = setpieceColorChangeEvent.NewColor;
				for (int i = 0; i < initialColors.Length; i++)
				{
					initialColors[i] = new Color(CustomColor.r, CustomColor.g, CustomColor.b, initialColors[i].a);
				}
			}
		}
		if (!(type == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetBlockFrequency)
		{
			MsgSetBlockFrequency msgSetBlockFrequency = (MsgSetBlockFrequency)networkMessageReceivedEvent.ReadMessage;
			if (msgSetBlockFrequency.blockIndex == blockSerializeIndex)
			{
				SetFrequency(msgSetBlockFrequency.frequency, sendNetworkMessage: false, force: false);
			}
		}
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetAllBlockFrequencies)
		{
			MsgSetAllBlockFrequencies msgSetAllBlockFrequencies = (MsgSetAllBlockFrequencies)networkMessageReceivedEvent.ReadMessage;
			if (msgSetAllBlockFrequencies.frequency == -1)
			{
				int stepValueFromRarity = TabletBlockList.GetStepValueFromRarity(placeablePrefab.BaseRarity);
				SetFrequency(stepValueFromRarity, sendNetworkMessage: false, force: false);
			}
			else
			{
				SetFrequency(msgSetAllBlockFrequencies.frequency, sendNetworkMessage: false, force: false);
			}
		}
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SendAllBlockFrequencies)
		{
			MsgSendAllBlockFrequencies msgSendAllBlockFrequencies = (MsgSendAllBlockFrequencies)networkMessageReceivedEvent.ReadMessage;
			if (blockSerializeIndex >= 0 && blockSerializeIndex < msgSendAllBlockFrequencies.frequencies.Length)
			{
				SetFrequency(msgSendAllBlockFrequencies.frequencies[blockSerializeIndex], sendNetworkMessage: false, force: false);
			}
		}
	}

	public void ChangeArtLayer(string layerName)
	{
		NetworkspriteLayer = layerName;
		SpriteRenderer[] artSprites = ArtSprites;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].sortingLayerName = layerName;
		}
		if (sortingGroup != null)
		{
			sortingGroup.sortingLayerName = layerName;
		}
		if (twitchLogo != null)
		{
			twitchLogo.sortingLayerName = layerName;
		}
	}

	public void SetTextCanvasOrder(int num)
	{
		Canvas componentInChildren = base.gameObject.GetComponentInChildren<Canvas>();
		if (componentInChildren != null)
		{
			componentInChildren.sortingOrder = num;
		}
	}

	public void SetFrequency(int frequency, bool sendNetworkMessage, bool force)
	{
		int blockFrequency = GameSettings.GetInstance().GetBlockFrequency(blockSerializeIndex);
		if (force || blockFrequency != frequency)
		{
			GameSettings.GetInstance().SetBlockFrequency(blockSerializeIndex, frequency);
			if (sendNetworkMessage)
			{
				MsgSetBlockFrequency msgSetBlockFrequency = new MsgSetBlockFrequency();
				msgSetBlockFrequency.blockIndex = blockSerializeIndex;
				msgSetBlockFrequency.frequency = frequency;
				LobbyManager.instance.client.Send(NetMsgTypes.SetBlockFrequency, msgSetBlockFrequency);
			}
		}
		if ((bool)crossOut)
		{
			crossOut.enabled = frequency <= 0;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdEnable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEnable called on client.");
		}
		else
		{
			((PickableBlock)obj).CmdEnable(reader.ReadBoolean());
		}
	}

	public void CallCmdEnable(bool enable)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdEnable called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdEnable(enable);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdEnable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(enable);
		SendCommandInternal(networkWriter, 0, "CmdEnable");
	}

	protected static void InvokeRpcRpcEnable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnable called on server.");
		}
		else
		{
			((PickableBlock)obj).RpcEnable(reader.ReadBoolean());
		}
	}

	public void CallRpcEnable(bool enable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcEnable called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcEnable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(enable);
		SendRPCInternal(networkWriter, 0, "RpcEnable");
	}

	static PickableBlock()
	{
		kCmdCmdEnable = -1253954667;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PickableBlock), kCmdCmdEnable, InvokeCmdCmdEnable);
		kRpcRpcEnable = -471886101;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PickableBlock), kRpcRpcEnable, InvokeRpcRpcEnable);
		NetworkCRC.RegisterBehaviour("PickableBlock", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(Visible);
			writer.Write(InLobby);
			writer.Write(InPartybox);
			writer.Write(spriteLayer);
			writer.Write(StartPosition);
			writer.Write(FindPartyBox);
			writer.Write(UseStartPosition);
			writer.Write(isTwitchItem);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(Visible);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(InLobby);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(InPartybox);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(spriteLayer);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(StartPosition);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(FindPartyBox);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(UseStartPosition);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(isTwitchItem);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			Visible = reader.ReadBoolean();
			InLobby = reader.ReadBoolean();
			InPartybox = reader.ReadBoolean();
			spriteLayer = reader.ReadString();
			StartPosition = reader.ReadVector3();
			FindPartyBox = reader.ReadBoolean();
			UseStartPosition = reader.ReadBoolean();
			isTwitchItem = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			Visible = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			InLobby = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			InPartybox = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			spriteLayer = reader.ReadString();
		}
		if ((num & 0x10) != 0)
		{
			StartPosition = reader.ReadVector3();
		}
		if ((num & 0x20) != 0)
		{
			FindPartyBox = reader.ReadBoolean();
		}
		if ((num & 0x40) != 0)
		{
			UseStartPosition = reader.ReadBoolean();
		}
		if ((num & 0x80) != 0)
		{
			isTwitchItem = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
