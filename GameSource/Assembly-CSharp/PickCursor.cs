using System;
using System.Collections;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class PickCursor : Cursor
{
	public Collider2D SelectionCollider;

	public InventoryBook InventoryBookMenu;

	public Transform cursorPoint;

	[SyncVar]
	protected GameObject pickedPiece;

	public bool Picking;

	public bool PauseMode;

	[SyncVar]
	public Character.Animals PlayerAnimal;

	[SyncVar]
	private int onLayer;

	protected bool lockSet;

	protected static bool placementLock;

	public bool ignoreDirectPauseEvents;

	private NetworkInstanceId ___pickedPieceNetId;

	private static int kCmdCmdPickPiece;

	public IPickable Pick
	{
		get
		{
			if (pickedPiece == null)
			{
				return null;
			}
			if (pickedPiece.GetComponent<PickableBlock>() != null)
			{
				return pickedPiece.GetComponent<PickableBlock>();
			}
			if (pickedPiece.GetComponent<PickableButton>() != null)
			{
				return pickedPiece.GetComponent<PickableButton>();
			}
			return null;
		}
	}

	public GameObject NetworkpickedPiece
	{
		get
		{
			return pickedPiece;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref pickedPiece, 2048u, ref ___pickedPieceNetId);
		}
	}

	public Character.Animals NetworkPlayerAnimal
	{
		get
		{
			return PlayerAnimal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref PlayerAnimal, 4096u);
		}
	}

	public int NetworkonLayer
	{
		get
		{
			return onLayer;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref onLayer, 8192u);
		}
	}

	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
		if (PlayerAnimal > Character.Animals.NONE)
		{
			SetSprites(PlayerAnimal);
		}
		SetLayer(onLayer);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (lockSet)
		{
			placementLock = false;
			lockSet = false;
		}
		if (disabled)
		{
			return;
		}
		if (hoveredPick != null)
		{
			if (lastHoveredPick != hoveredPick)
			{
				lastHoveredPick = hoveredPick;
				lastHoveredPick.PlayHoverSound();
			}
			hoveredPick.IHoveredCursors.Remove(this);
			hoveredPick = null;
		}
		else
		{
			lastHoveredPick = null;
		}
	}

	public override void ReceiveEvent(InputEvent e)
	{
		base.ReceiveEvent(e);
		if (!(InventoryBookMenu != null) || !InventoryBookMenu.ScreenMode || !(InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.TabletPage))
		{
			return;
		}
		Tablet component = InventoryBookMenu.TabletPage.GetComponent<Tablet>();
		if (component != null)
		{
			switch (e.Key)
			{
			case InputEvent.InputKey.Up2:
				component.AccumulateCursorScroll(this, 0f, e.Valuef);
				break;
			case InputEvent.InputKey.Down2:
				component.AccumulateCursorScroll(this, 0f, 0f - e.Valuef);
				break;
			case InputEvent.InputKey.Left2:
				component.AccumulateCursorScroll(this, 0f - e.Valuef, 0f);
				break;
			case InputEvent.InputKey.Right2:
				component.AccumulateCursorScroll(this, e.Valuef, 0f);
				break;
			}
		}
	}

	protected override void OnRotateLeft()
	{
		ClearHoverPick();
		if (!(InventoryBookMenu != null))
		{
			return;
		}
		if (InventoryBookMenu.ScreenMode)
		{
			if (!(InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.TabletPage))
			{
				return;
			}
			Tablet component = InventoryBookMenu.TabletPage.GetComponent<Tablet>();
			if (component != null)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					component.OnRotateRight(this);
				}
				else
				{
					component.OnRotateLeft(this);
				}
			}
		}
		else if (Modifiers.GetInstance().CameraFlippedOnX)
		{
			InventoryBookMenu.NextPage(1, allowClearBackPage: true, pageTurnSound: true);
		}
		else
		{
			InventoryBookMenu.PreviousPage(allowClearBackPage: true, pageTurnSounds: true);
		}
	}

	protected override void OnRotateRight()
	{
		ClearHoverPick();
		if (!(InventoryBookMenu != null))
		{
			return;
		}
		if (InventoryBookMenu.ScreenMode)
		{
			if (!(InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.TabletPage))
			{
				return;
			}
			Tablet component = InventoryBookMenu.TabletPage.GetComponent<Tablet>();
			if (component != null)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					component.OnRotateLeft(this);
				}
				else
				{
					component.OnRotateRight(this);
				}
			}
		}
		else if (Modifiers.GetInstance().CameraFlippedOnX)
		{
			InventoryBookMenu.PreviousPage(allowClearBackPage: true, pageTurnSounds: true);
		}
		else
		{
			InventoryBookMenu.NextPage(1, allowClearBackPage: true, pageTurnSound: true);
		}
	}

	protected override void OnBack()
	{
		ClearHoverPick();
		if (!(InventoryBookMenu == null))
		{
			if (InventoryBookMenu.backEnabled)
			{
				int backPage = InventoryBookMenu.backPage;
				InventoryBookMenu.GotoPage(backPage);
			}
			else
			{
				OnInventory();
			}
		}
	}

	protected override void OnStart()
	{
		if (!(InventoryBookMenu == null) && !Controller.InputFieldWasActiveRecently)
		{
			OnBack();
		}
	}

	protected override void OnInventory()
	{
		if (InventoryBookMenu == null || InventoryBookMenu.FrozenOnPage)
		{
			return;
		}
		if (InventoryBookMenu.CurrentScreenpage != null && InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.TabletPage)
		{
			Tablet component = InventoryBookMenu.TabletPage.GetComponent<Tablet>();
			if (component != null && component.OnPressBack(this))
			{
				return;
			}
		}
		if (PauseMode)
		{
			ClearHoverPick();
			if (!InventoryBookMenu.FrozenOnPage && !InventoryBookMenu.FrozenWhileOpening)
			{
				if (!LobbyManager.instance.IsInOnlineGame)
				{
					GameState.GetInstance().Paused = false;
					GameEventManager.SendEvent(new PauseEvent(pause: false, networkNumber));
				}
				else
				{
					GameEventManager.SendEvent(new SoftPauseEvent(softpause: false, networkNumber, GetComponent<NetworkIdentity>().isServer));
				}
			}
		}
		else
		{
			Freeze();
			Disable();
			GameEventManager.SendEvent(new PlayerInventoryEvent(entered: false, networkNumber));
		}
	}

	protected override void OnAccept()
	{
		if (startDown)
		{
			return;
		}
		base.OnAccept();
		if (Tablet.PropagateClick(this))
		{
			return;
		}
		if (!hoverStatePick())
		{
			GameEventManager.SendEvent(new PickCursorClickedBackgroundEvent());
			return;
		}
		if (lastHoveredPick is PickableBlock && !PauseMode)
		{
			dealWithPickable();
		}
		else if (lastHoveredPick != null)
		{
			lastHoveredPick.OnAccept(this);
		}
		ClearHoverPick();
	}

	public virtual void dealWithPickable()
	{
		if ((lastHoveredPick as PickableBlock).Available)
		{
			StartCoroutine(tryPickPiece());
		}
		else
		{
			AkSoundEngine.PostEvent("UI_Inventory_CannotPlace", base.gameObject);
		}
	}

	public void SetLayer(int layer, bool isServer = false)
	{
		if (isServer)
		{
			NetworkonLayer = layer;
			return;
		}
		NetworkonLayer = layer;
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	public override void Enable()
	{
		base.Enable();
		ClearHoverPick();
		NetworkpickedPiece = null;
		SelectionCollider.enabled = true;
		Color white = Color.white;
		if (PlayerAnimal == Character.Animals.NONE)
		{
			white = base.CursorColor;
		}
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.UIPOOF, base.transform.position, 0.8f, white);
		if (!PauseMode)
		{
			Picking = true;
		}
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		checkHoveredPickAdd(c);
	}

	public void checkHoveredPickAdd(Collider2D c)
	{
		if (disabled)
		{
			return;
		}
		IPickable pickable = c.GetComponent(typeof(IPickable)) as IPickable;
		if (pickable == null)
		{
			pickable = c.transform.parent.GetComponent(typeof(IPickable)) as IPickable;
		}
		if (pickable == null || pickable.Paused || pickable.DeactivatedInBook)
		{
			return;
		}
		PickableButton pickableButton = pickable as PickableButton;
		if ((!(pickableButton != null) || (pickableButton.IsButtonAllowed() && pickableButton.CursorInsideMaskedArea(cursorPoint.position))) && (!(InventoryBookMenu != null) || !(pickable.InventoryBook != null) || InventoryBookMenu.currentPage == pickable.PageNumber))
		{
			if (hoveredPick != null && pickable != hoveredPick)
			{
				hoveredPick.IHoveredCursors.Remove(this);
			}
			hoveredPick = pickable;
			if (!hoveredPick.IHoveredCursors.Contains(this))
			{
				hoveredPick.IHoveredCursors.Add(this);
			}
		}
	}

	protected virtual IEnumerator tryPickPiece()
	{
		while (placementLock)
		{
			yield return null;
		}
		placementLock = true;
		lockSet = true;
		if (pickedPiece == null && hoveredPick != null)
		{
			Picking = false;
			AkSoundEngine.PostEvent("UI_Inventory_Select_" + hoveredPick.SFXEventName, base.gameObject);
			AkSoundEngine.PostEvent("UI_InGame_PartyBox_Select_Item", base.gameObject);
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.UIPOOF, base.transform.position, 0.7f);
			if (InventoryBookMenu != null)
			{
				GameEventManager.SendEvent(new PlayerInventoryEvent(entered: false, networkNumber));
				GameEventManager.SendEvent(new PickBlockEvent(networkNumber, hoveredPick.ThisPickableBlock));
			}
			Freeze();
			Disable();
		}
	}

	public override void Disable(bool sound = true, bool showNotebookSprite = false)
	{
		if (!disabled && sound)
		{
			Color white = Color.white;
			if (PlayerAnimal == Character.Animals.NONE)
			{
				white = base.CursorColor;
			}
			SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.UIPOOF, base.transform.position, 0.8f, white);
		}
		base.Disable(sound, showNotebookSprite);
		SelectionCollider.enabled = false;
		ClearHoverPick();
		Picking = false;
	}

	protected void ClearHoverPick()
	{
		if (hoveredPick != null)
		{
			hoveredPick.IHoveredCursors.Remove(this);
			hoveredPick = null;
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<PlacementTimerDoneEvent>(this, adding);
		GameEventManager.ChangeListener<PrepareBlankLevelForScreenShot>(this, adding);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		Type type = e.GetType();
		if (type == typeof(PauseEvent))
		{
			PauseEvent pauseEvent = e as PauseEvent;
			if (ignoreDirectPauseEvents)
			{
				return;
			}
			if (pauseEvent.Paused && pauseEvent.PlayerNumber == networkNumber)
			{
				Unpause();
				base.Networkpaused = false;
			}
			else if (!pauseEvent.Paused && pauseEvent.PlayerNumber == networkNumber)
			{
				PauseMode = false;
				if (!Picking)
				{
					Freeze();
					Disable();
					GameEventManager.SendEvent(new PlayerInventoryEvent(entered: false, networkNumber, leavingPauseMenu: true));
				}
			}
		}
		if (GetType() != typeof(PartyPickCursor))
		{
			if (type == typeof(SoftPauseEvent))
			{
				SoftPauseEvent softPauseEvent = e as SoftPauseEvent;
				if (softPauseEvent.SoftPaused && softPauseEvent.PlayerNumber == networkNumber && InventoryBookMenu != null)
				{
					Unpause();
					base.Networkpaused = false;
				}
				else if (!softPauseEvent.SoftPaused && softPauseEvent.PlayerNumber == networkNumber)
				{
					PauseMode = false;
					if (!Picking)
					{
						Freeze();
						Disable();
						GameEventManager.SendEvent(new PlayerInventoryEvent(entered: false, networkNumber, leavingPauseMenu: true));
					}
				}
			}
			if (type == typeof(PlacementTimerDoneEvent) && base.Enabled && InventoryBookMenu.currentPage < InventoryBookMenu.OptionPageNumber)
			{
				OnInventory();
			}
		}
		if (type == typeof(PrepareBlankLevelForScreenShot))
		{
			if ((e as PrepareBlankLevelForScreenShot).Hidden)
			{
				base.transform.Translate(new Vector3(200f, 200f, 0f));
			}
			else
			{
				base.transform.Translate(new Vector3(-200f, -200f, 0f));
			}
		}
	}

	[Command]
	protected void CmdPickPiece(GameObject piece)
	{
		NetworkpickedPiece = piece;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdPickPiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickPiece called on client.");
		}
		else
		{
			((PickCursor)obj).CmdPickPiece(reader.ReadGameObject());
		}
	}

	public void CallCmdPickPiece(GameObject piece)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPickPiece called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPickPiece(piece);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPickPiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(piece);
		SendCommandInternal(networkWriter, 0, "CmdPickPiece");
	}

	static PickCursor()
	{
		kCmdCmdPickPiece = 830111702;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PickCursor), kCmdCmdPickPiece, InvokeCmdCmdPickPiece);
		NetworkCRC.RegisterBehaviour("PickCursor", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(pickedPiece);
			writer.Write((int)PlayerAnimal);
			writer.WritePackedUInt32((uint)onLayer);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 0x800) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(pickedPiece);
		}
		if ((base.syncVarDirtyBits & 0x1000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)PlayerAnimal);
		}
		if ((base.syncVarDirtyBits & 0x2000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)onLayer);
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
			___pickedPieceNetId = reader.ReadNetworkId();
			PlayerAnimal = (Character.Animals)reader.ReadInt32();
			onLayer = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 0x800) != 0)
		{
			pickedPiece = reader.ReadGameObject();
		}
		if ((num & 0x1000) != 0)
		{
			PlayerAnimal = (Character.Animals)reader.ReadInt32();
		}
		if ((num & 0x2000) != 0)
		{
			onLayer = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
		if (!___pickedPieceNetId.IsEmpty())
		{
			NetworkpickedPiece = ClientScene.FindLocalObject(___pickedPieceNetId);
		}
	}
}
