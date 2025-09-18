using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PiecePlacementCursor : Cursor
{
	public InventoryBook InventoryBookMenu;

	public CursorControlHints cursorControlHints;

	public NetworkSurrogate NetSurrogatePrefab;

	public Collider2D SelectionCollider;

	public HoldBToGiveUp holdBIndicatorInstance;

	public HoldBToGiveUp altHoldBIndicatorInstance;

	public bool MultiplePlacement;

	public float SwitchTime;

	public bool KeepPiece;

	public bool WaitingForPlaceMessageResponse;

	protected SpriteRenderer[] pieceSpriteRenderers;

	protected float switchTimer;

	private bool backJustPressed;

	private bool tryingToCancel;

	private bool SelectionColliderLastState = true;

	[SyncVar]
	private Vector3 heldPositionOffset = Vector3.zero;

	private bool placementPhysicsLock;

	public Canvas controlsCanvas;

	public Canvas placementsLeftCanvas;

	public Text placementsLeftText;

	private Placeable acceptDownHoveredPiece;

	private Vector3 acceptDownPositionOffset;

	private Placeable sprintDownHoveredPiece;

	private Vector3 sprintDownPositionOffset;

	private Modifiers.CameraFlipModes currentCameraFlipMode;

	public static RaycastHit2D[] raycastResultCache;

	public LayerMask selectionLayerMask;

	public Vector2 selectionBoxSize;

	public Vector3 selectionOffset;

	public int numHits;

	private List<Placeable> potentialSelections = new List<Placeable>();

	private List<CursorControlHintButton> ButtonsNotToHide = new List<CursorControlHintButton>();

	private static int kCmdCmdRotatePiece;

	private static int kRpcRpcRotatePiece;

	private static int kCmdCmdSwitchFreeMode;

	private static int kRpcRpcSwitchFreeMode;

	private static int kCmdCmdSpawnNetSurrogate;

	private static int kCmdCmdClearPiece;

	private static int kRpcRpcClearPiece;

	private static int kRpcRpcSetPlacementsLeftText;

	private static int kCmdCmdSetHeldPositionOffset;

	public Placeable Piece { get; protected set; }

	public bool Placed { get; protected set; }

	public float holdingPieceOffset
	{
		get
		{
			if (Piece != null)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public Vector3 NetworkheldPositionOffset
	{
		get
		{
			return heldPositionOffset;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref heldPositionOffset, 2048u);
		}
	}

	protected override void SetSprites(CharacterSpriteLibrary spriteLib)
	{
		base.SetSprites(spriteLib);
		cursorSpriteRenderer.transform.localPosition = new Vector3(0f - PieceOffset.x, 0f - PieceOffset.y, 0f);
		placementsLeftCanvas.sortingLayerID = cursorSpriteRenderer.sortingLayerID;
		placementsLeftCanvas.sortingOrder = cursorSpriteRenderer.sortingOrder + 20;
		controlsCanvas.sortingLayerID = cursorSpriteRenderer.sortingLayerID;
		controlsCanvas.sortingOrder = cursorSpriteRenderer.sortingOrder + 30;
	}

	public override void Start()
	{
		SelectionColliderLastState = true;
		base.Start();
		placementsLeftText.text = "";
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<PickBlockEvent>(this, adding);
		GameEventManager.ChangeListener<PlayerInventoryEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public virtual void SetPiece(Placeable piece, bool destroyPrevious = false, bool pickup = true)
	{
		if (Piece != null && destroyPrevious)
		{
			Piece.DestroySelf(destroyChildren: true, useSmoke: false);
		}
		Piece = piece;
		if (Piece != null)
		{
			Piece.GetComponent<Rigidbody2D>().isKinematic = false;
			CheckColliding[] componentsInChildren = Piece.gameObject.GetComponentsInChildren<CheckColliding>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].CheckBounds = boundingCollider;
			}
			if (pickup)
			{
				Piece.PickedUp = true;
				Piece.DetachAllChildren(keepAttachments: true);
			}
			pieceSpriteRenderers = Piece.gameObject.GetComponentsInChildren<SpriteRenderer>();
			piece.SwitchColliderTo(ColliderModeEnum.PlacementPhase);
			SpriteRenderer[] array = pieceSpriteRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sortingLayerName = "UI 1";
			}
			if (pickup)
			{
				foreach (Placeable childPiece in Piece.ChildPieces)
				{
					if (childPiece != null)
					{
						childPiece.PickedUp = true;
					}
				}
				if (Piece.ParentPiece != null)
				{
					Piece.ParentPiece.DetachPiece(Piece, removeFromGroup: false);
				}
			}
			float z = Piece.transform.eulerAngles.z;
			z = Mathf.Round(z / 90f) * 90f;
			Piece.transform.rotation = Quaternion.Euler(0f, 0f, z);
			Vector2 self = Piece.GetTransformedPlacementOffset();
			if (base.hasAuthority)
			{
				if (Piece.Placed)
				{
					if (acceptDownHoveredPiece != null && acceptDownHoveredPiece == Piece)
					{
						NetworkheldPositionOffset = acceptDownPositionOffset;
					}
					else
					{
						NetworkheldPositionOffset = Piece.transform.position - base.transform.position - self.ToVector3();
					}
				}
				else
				{
					NetworkheldPositionOffset = Vector3.zero;
				}
				CallCmdSetHeldPositionOffset(heldPositionOffset);
			}
			gridPosition.Set(Mathf.Round(base.transform.position.x + heldPositionOffset.x) + self.x, Mathf.Round(base.transform.position.y + heldPositionOffset.y) + self.y, 0f);
			if (Piece.ConstrainX)
			{
				Piece.transform.position = new Vector2(Piece.OriginalPosition.x, gridPosition.y);
			}
			else if (Piece.ConstrainY)
			{
				Piece.transform.position = new Vector2(gridPosition.x, Piece.OriginalPosition.y);
			}
			else
			{
				Piece.transform.position = new Vector2(gridPosition.x, gridPosition.y);
			}
			SelectionCollider.enabled = false;
			SelectionColliderLastState = false;
			if (hoveredPiece != null)
			{
				hoveredPiece.HoveredCursors.Remove(this);
				hoveredPiece.Tint();
				hoveredPiece = null;
			}
		}
		else
		{
			SelectionCollider.enabled = true;
			SelectionColliderLastState = true;
			pieceSpriteRenderers = null;
		}
	}

	protected override void Update()
	{
		base.Update();
		Modifiers instance = Modifiers.GetInstance();
		if (instance.CameraFlipping != currentCameraFlipMode)
		{
			currentCameraFlipMode = instance.CameraFlipping;
			switch (currentCameraFlipMode)
			{
			case Modifiers.CameraFlipModes.None:
				base.transform.localScale = new Vector3(1f, 1f, 1f);
				break;
			case Modifiers.CameraFlipModes.FlipX:
				base.transform.localScale = new Vector3(-1f, 1f, 1f);
				break;
			case Modifiers.CameraFlipModes.FlipY:
				base.transform.localScale = new Vector3(1f, -1f, 1f);
				break;
			case Modifiers.CameraFlipModes.FlipXY:
				base.transform.localScale = new Vector3(-1f, -1f, 1f);
				break;
			}
		}
		if (holdBIndicatorInstance != null && Piece == null)
		{
			if (!disabled && !frozen && !paused)
			{
				holdBIndicatorInstance.Show();
				if (back)
				{
					if (backJustPressed || switchTimer != 0f)
					{
						switchTimer += Time.unscaledDeltaTime;
					}
					else
					{
						holdBIndicatorInstance.SetFillAmount(1f);
					}
					if (switchTimer != 0f)
					{
						if (switchTimer >= SwitchTime)
						{
							cursorControlHints.SetButtonVisible(CursorControlHints.Button.Switch, visible: false);
							holdBIndicatorInstance.Hide();
							switchToPlay();
						}
						else
						{
							if (!holdBIndicatorInstance.Visible)
							{
								holdBIndicatorInstance.Show();
							}
							holdBIndicatorInstance.SetFillAmount(1f - switchTimer / SwitchTime);
						}
					}
				}
				else
				{
					if (switchTimer > 0f)
					{
						switchTimer -= Time.unscaledDeltaTime / 2f;
					}
					holdBIndicatorInstance.SetFillAmount(1f - switchTimer / SwitchTime);
				}
			}
			else
			{
				switchTimer = 0f;
				cursorControlHints.SetButtonVisible(CursorControlHints.Button.Switch, visible: false);
				holdBIndicatorInstance.Hide();
			}
		}
		if (Piece != null && Piece.MarkedForDestruction)
		{
			SetPiece(null);
		}
		if (Piece != null)
		{
			Piece.Tint();
		}
	}

	private void switchToPlay()
	{
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
		{
			Disable();
			GameEventManager.SendEvent(new FreePlayPlayerSwitchEvent(networkNumber, GameControl.GamePhase.PLAY));
			CallCmdSwitchFreeMode();
		}
	}

	public override void ClampToBoundary()
	{
		ClampToBoundary(boundary);
	}

	public override void ClampToBoundary(Bounds boundary)
	{
		if (boundary.extents.sqrMagnitude > 0f)
		{
			Vector3 localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);
			localPosition.x = Mathf.Min(boundary.max.x + holdingPieceOffset, Mathf.Max(boundary.min.x - holdingPieceOffset, base.transform.localPosition.x));
			localPosition.y = Mathf.Min(boundary.max.y + holdingPieceOffset, Mathf.Max(boundary.min.y - holdingPieceOffset, base.transform.localPosition.y));
			base.transform.localPosition = localPosition;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (disabled || waitingForFixedUpdate)
		{
			return;
		}
		if (Piece != null && !waitingForFixedUpdate && !tryingToCancel && !placementPhysicsLock && !WaitingForPlaceMessageResponse)
		{
			Vector2 vector = Piece.GetTransformedPlacementOffset();
			gridPosition.Set(Mathf.Round(base.transform.position.x + heldPositionOffset.x) + vector.x, Mathf.Round(base.transform.position.y + heldPositionOffset.y) + vector.y, 0f);
			if (Piece.ConstrainX)
			{
				if (Mathf.Abs(Piece.transform.position.y - gridPosition.y) > 0.1f)
				{
					AkSoundEngine.PostEvent("UI_InGame_Move_Object", base.gameObject);
				}
				Piece.transform.position = new Vector2(Piece.OriginalPosition.x, gridPosition.y);
			}
			else if (Piece.ConstrainY)
			{
				if (Mathf.Abs(Piece.transform.position.x - gridPosition.x) > 0.1f)
				{
					AkSoundEngine.PostEvent("UI_InGame_Move_Object", base.gameObject);
				}
				Piece.transform.position = new Vector2(gridPosition.x, Piece.OriginalPosition.y);
			}
			else
			{
				if ((Piece.transform.position - gridPosition).sqrMagnitude > 0.1f)
				{
					AkSoundEngine.PostEvent("UI_InGame_Move_Object", base.gameObject);
				}
				Piece.transform.position = new Vector2(gridPosition.x, gridPosition.y);
			}
		}
		if (!(Piece == null))
		{
			return;
		}
		if (hoveredPiece != null)
		{
			hoveredPiece.HoveredCursors.Remove(this);
			hoveredPiece.Tint();
			hoveredPiece = null;
		}
		bool queriesHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = true;
		Modifiers instance = Modifiers.GetInstance();
		Vector3 vector2 = selectionOffset;
		if (instance.CameraFlippedOnX)
		{
			vector2.x = 0f - selectionOffset.x;
		}
		numHits = Physics2D.BoxCastNonAlloc(base.transform.position + vector2, selectionBoxSize, 0f, Vector2.zero, raycastResultCache, 0f, selectionLayerMask);
		Physics2D.queriesHitTriggers = queriesHitTriggers;
		potentialSelections.Clear();
		for (int i = 0; i != numHits; i++)
		{
			RaycastHit2D raycastHit2D = raycastResultCache[i];
			Placeable placeable = potentialHoeverPiece(raycastHit2D.collider);
			if (placeable != null)
			{
				potentialSelections.Add(placeable);
			}
		}
		bool flag = true;
		foreach (Placeable potentialSelection in potentialSelections)
		{
			if (potentialSelection.IsSubElement)
			{
				checkHoveredPieceAdd(potentialSelection);
				flag = false;
				break;
			}
		}
		if (flag && potentialSelections.Count > 0)
		{
			checkHoveredPieceAdd(potentialSelections[0]);
		}
	}

	protected override void OnAccept()
	{
		OnAcceptDown();
	}

	private void OnAcceptDown()
	{
		if (WaitingForPlaceMessageResponse)
		{
			return;
		}
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.PARTY)
		{
			VersusControl versusControl = LobbyManager.instance.CurrentGameController as VersusControl;
			if (versusControl != null && versusControl.PartyBoxStillActive)
			{
				return;
			}
		}
		if (Piece != null)
		{
			if (Piece.CanPlace())
			{
				MsgPiecePlaced msgPiecePlaced = new MsgPiecePlaced();
				msgPiecePlaced.PlayerNumber = networkNumber;
				msgPiecePlaced.PiecePosition = Piece.transform.position;
				msgPiecePlaced.PieceScale = Piece.transform.localScale;
				msgPiecePlaced.PieceRotation = Piece.transform.rotation;
				msgPiecePlaced.PieceID = Piece.ID;
				msgPiecePlaced.PieceWasMoved = Piece.Placed;
				NetworkManager.singleton.client.Send(NetMsgTypes.PiecePlaced, msgPiecePlaced);
				WaitingForPlaceMessageResponse = true;
			}
			else
			{
				AkSoundEngine.PostEvent("UI_Inventory_CannotPlace", base.gameObject);
			}
			acceptDownHoveredPiece = null;
			acceptDownPositionOffset = Vector3.zero;
		}
		else if (hoveredPiece != null && !hoveredPiece.PlacementLock && hoveredPiece.InteractableInCurrentMode)
		{
			acceptDownHoveredPiece = hoveredPiece;
			acceptDownPositionOffset = hoveredPiece.transform.position - base.transform.position - hoveredPiece.GetTransformedPlacementOffset();
		}
		else
		{
			acceptDownHoveredPiece = null;
			acceptDownPositionOffset = Vector3.zero;
		}
	}

	private void OnAcceptUp()
	{
		if (!WaitingForPlaceMessageResponse && !(Piece != null) && hoveredPiece != null && !hoveredPiece.PlacementLock && acceptDownHoveredPiece == hoveredPiece && hoveredPiece.InteractableInCurrentMode)
		{
			acceptDownPositionOffset = hoveredPiece.transform.position - base.transform.position - hoveredPiece.GetTransformedPlacementOffset();
			MsgPiecePickedUp msgPiecePickedUp = new MsgPiecePickedUp();
			msgPiecePickedUp.PlayerNumber = networkNumber;
			msgPiecePickedUp.PieceID = hoveredPiece.ID;
			NetworkManager.singleton.client.Send(NetMsgTypes.PiecePickedUp, msgPiecePickedUp);
			WaitingForPlaceMessageResponse = true;
		}
	}

	private void OnSprintDown()
	{
		if (!WaitingForPlaceMessageResponse)
		{
			if (GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY && Piece == null && hoveredPiece != null && hoveredPiece.duplicable && hoveredPiece.InteractableInCurrentMode)
			{
				sprintDownHoveredPiece = hoveredPiece;
				sprintDownPositionOffset = hoveredPiece.transform.position - base.transform.position - hoveredPiece.GetTransformedPlacementOffset();
			}
			else
			{
				sprintDownHoveredPiece = null;
				sprintDownPositionOffset = Vector3.zero;
			}
		}
	}

	private void OnSprintUp()
	{
		if (WaitingForPlaceMessageResponse)
		{
			return;
		}
		if (GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY && Piece == null && hoveredPiece != null && hoveredPiece.duplicable && hoveredPiece.InteractableInCurrentMode && sprintDownHoveredPiece == hoveredPiece)
		{
			sprintDownPositionOffset = hoveredPiece.transform.position - base.transform.position - hoveredPiece.GetTransformedPlacementOffset();
			PickableBlock pickableBlock = null;
			if (hoveredPiece.PickableBlock != null)
			{
				pickableBlock = hoveredPiece.PickableBlock;
			}
			else
			{
				MultipiecePart multipiecePart = hoveredPiece as MultipiecePart;
				if (multipiecePart != null && multipiecePart.MainBlock != null)
				{
					GameEventManager.SendEvent(new PickBlockEvent(networkNumber, multipiecePart.MainBlock.PickableBlock, hoveredPiece));
				}
			}
			if (pickableBlock != null)
			{
				GameEventManager.SendEvent(new PickBlockEvent(networkNumber, pickableBlock, hoveredPiece));
				NetworkheldPositionOffset = sprintDownPositionOffset;
				CallCmdSetHeldPositionOffset(heldPositionOffset);
				if (hoveredPiece != null)
				{
					hoveredPiece.HoveredCursors.Remove(this);
					hoveredPiece.Tint();
					hoveredPiece = null;
				}
			}
		}
		else
		{
			sprintDownHoveredPiece = null;
			sprintDownPositionOffset = Vector3.zero;
		}
	}

	protected override void OnBack()
	{
		if (WaitingForPlaceMessageResponse)
		{
			return;
		}
		base.OnBack();
		if (Piece != null && GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY)
		{
			if (Piece.Placed)
			{
				Piece.ResetTransform(networkNumber);
				StartCoroutine(waitForPieceReset());
			}
			else
			{
				ClearCurrentPiece();
			}
		}
	}

	private IEnumerator waitForPieceReset()
	{
		tryingToCancel = true;
		if (placementPhysicsLock)
		{
			Debug.Log("PiecePlacementCursor.waitForPieceReset: Waiting for previous operation to complete");
			while (placementPhysicsLock)
			{
				yield return null;
			}
		}
		placementPhysicsLock = true;
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		bool flag = false;
		try
		{
			if (Piece.CanPlace() || Piece.IgnoreBounds)
			{
				MsgPiecePlaced msgPiecePlaced = new MsgPiecePlaced();
				msgPiecePlaced.PlayerNumber = networkNumber;
				msgPiecePlaced.PiecePosition = Piece.transform.position;
				msgPiecePlaced.PieceScale = Piece.transform.localScale;
				msgPiecePlaced.PieceRotation = Piece.transform.rotation;
				msgPiecePlaced.PieceID = Piece.ID;
				msgPiecePlaced.PieceWasMoved = true;
				msgPiecePlaced.ResetPosition = true;
				NetworkManager.singleton.client.Send(NetMsgTypes.PiecePlaced, msgPiecePlaced);
				WaitingForPlaceMessageResponse = true;
			}
			else
			{
				AkSoundEngine.PostEvent("UI_Inventory_CannotPlace", base.gameObject);
				flag = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while waiting for piece reset\n" + ex.Message + "\n" + ex.StackTrace);
		}
		placementPhysicsLock = false;
		if (flag)
		{
			yield return new WaitForSeconds(0.5f);
		}
		tryingToCancel = false;
	}

	protected override void OnRotateLeft()
	{
		base.OnRotateLeft();
		if (!waitingForFixedUpdate && !tryingToCancel && !placementPhysicsLock && !WaitingForPlaceMessageResponse)
		{
			PerformLeftRotation(sprint);
		}
	}

	protected override void OnRotateRight()
	{
		base.OnRotateRight();
		if (!waitingForFixedUpdate && !tryingToCancel && !placementPhysicsLock && !WaitingForPlaceMessageResponse)
		{
			PerformRightRotation(sprint);
		}
	}

	private void PerformLeftRotation(bool sprintPressed)
	{
		if (!(Piece != null))
		{
			return;
		}
		bool flag = true;
		switch ((sprintPressed && Piece.OrientationAlt != Placeable.OrientMode.NONE) ? Piece.OrientationAlt : Piece.Orientation)
		{
		case Placeable.OrientMode.ROTATE:
			Piece.transform.Rotate(0f, 0f, 90f);
			break;
		case Placeable.OrientMode.FLIPX:
			Piece.transform.localScale = new Vector3(0f - Piece.transform.localScale.x, 1f, 1f);
			break;
		case Placeable.OrientMode.FLIPY:
			Piece.transform.localScale = new Vector3(1f, 0f - Piece.transform.localScale.y, 1f);
			break;
		case Placeable.OrientMode.FLIPXANDY:
			if (Piece.transform.localScale.x > 0f && Piece.transform.localScale.y > 0f)
			{
				Piece.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
			else if (Piece.transform.localScale.x < 0f && Piece.transform.localScale.y > 0f)
			{
				Piece.transform.localScale = new Vector3(-1f, -1f, 1f);
			}
			else if (Piece.transform.localScale.x < 0f && Piece.transform.localScale.y < 0f)
			{
				Piece.transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else if (Piece.transform.localScale.x > 0f && Piece.transform.localScale.y < 0f)
			{
				Piece.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			AkSoundEngine.PostEvent("UI_Inventory_Flip_Item", base.gameObject);
			if (base.hasAuthority)
			{
				GameState.GetInstance().controlTips.ReceiveInput(networkNumber, ControlTipData.KnowledgeType.ROTATE);
			}
		}
		Piece.Flip(sprintPressed);
		if (base.hasAuthority)
		{
			CallCmdRotatePiece(clockwise: false, sprintPressed);
		}
	}

	private void PerformRightRotation(bool sprintPressed)
	{
		if (!(Piece != null))
		{
			return;
		}
		bool flag = true;
		switch ((sprintPressed && Piece.OrientationAlt != Placeable.OrientMode.NONE) ? Piece.OrientationAlt : Piece.Orientation)
		{
		case Placeable.OrientMode.ROTATE:
			Piece.transform.Rotate(0f, 0f, -90f);
			break;
		case Placeable.OrientMode.FLIPX:
			Piece.transform.localScale = new Vector3(0f - Piece.transform.localScale.x, 1f, 1f);
			break;
		case Placeable.OrientMode.FLIPY:
			Piece.transform.localScale = new Vector3(1f, 0f - Piece.transform.localScale.y, 1f);
			break;
		case Placeable.OrientMode.FLIPXANDY:
			if (Piece.transform.localScale.x > 0f && Piece.transform.localScale.y > 0f)
			{
				Piece.transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else if (Piece.transform.localScale.x < 0f && Piece.transform.localScale.y > 0f)
			{
				Piece.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			else if (Piece.transform.localScale.x < 0f && Piece.transform.localScale.y < 0f)
			{
				Piece.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
			else if (Piece.transform.localScale.x > 0f && Piece.transform.localScale.y < 0f)
			{
				Piece.transform.localScale = new Vector3(-1f, -1f, 1f);
			}
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			AkSoundEngine.PostEvent("UI_Inventory_Flip_Item", base.gameObject);
			if (base.hasAuthority)
			{
				GameState.GetInstance().controlTips.ReceiveInput(networkNumber, ControlTipData.KnowledgeType.ROTATE);
			}
		}
		Piece.Flip(sprintPressed);
		if (base.hasAuthority)
		{
			CallCmdRotatePiece(clockwise: true, sprintPressed);
		}
	}

	protected override void OnInventory()
	{
		base.OnInventory();
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		if (gameMode == GameState.GameMode.PARTY || gameMode == GameState.GameMode.CHALLENGE || WaitingForPlaceMessageResponse)
		{
			return;
		}
		if (Piece != null)
		{
			bool placed = Piece.Placed;
			if (!Piece.inDestructible)
			{
				ClearCurrentPiece();
			}
			if (placed)
			{
				return;
			}
		}
		Freeze();
		waitingForFixedUpdate = false;
		base.NetworkWaitingForInventory = true;
		CallCmdSetWaitingForInventory(waiting: true);
		Disable(sound: true, LobbyManager.instance.IsInOnlineGame);
		GameEventManager.SendEvent(new PlayerInventoryEvent(entered: true, networkNumber));
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (paused || disabled)
		{
			return;
		}
		UIUpdate();
		if (firstFrame)
		{
			firstFrame = false;
			return;
		}
		if (Piece != null && !Piece.CanPlace())
		{
			cursorSpriteRenderer.sprite = bad;
		}
		else
		{
			cursorSpriteRenderer.sprite = ok;
		}
		ResetInput();
	}

	public override void Disable(bool sound = true, bool showNotebookSprite = false)
	{
		base.Disable(sound, showNotebookSprite);
		if (Piece != null)
		{
			Piece.Disable();
		}
		cursorControlHints.HideAll();
		if (hoveredPiece != null)
		{
			hoveredPiece.HoveredCursors.Remove(this);
			hoveredPiece.Tint();
			hoveredPiece = null;
		}
		placementsLeftText.enabled = false;
		switchTimer = 0f;
		SelectionCollider.enabled = false;
	}

	public override void Enable()
	{
		base.Enable();
		if (Piece != null)
		{
			Piece.EnablePlacement();
		}
		if (UseScreenPosition && ScreenPositionController != null)
		{
			ScreenPositionController.SetPreciseCursor(precise: false);
		}
		base.NetworkWaitingForInventory = false;
		if (base.hasAuthority)
		{
			CallCmdSetWaitingForInventory(waiting: false);
		}
		placementsLeftText.enabled = true;
		SelectionCollider.enabled = SelectionColliderLastState;
	}

	public override void Pause()
	{
		base.Pause();
		if (disabled)
		{
			return;
		}
		if (Piece != null && pieceSpriteRenderers != null)
		{
			SpriteRenderer[] array = pieceSpriteRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
		cursorControlHints.HideAll();
		placementsLeftText.enabled = false;
		SelectionColliderLastState = SelectionCollider.enabled;
	}

	public override void Unpause()
	{
		base.Unpause();
		if (disabled || paused || scoreboard)
		{
			return;
		}
		if (Piece != null && pieceSpriteRenderers != null)
		{
			SpriteRenderer[] array = pieceSpriteRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}
		placementsLeftText.enabled = true;
		SelectionCollider.enabled = SelectionColliderLastState;
	}

	public virtual void UIUpdate()
	{
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		if (frozen || placementPhysicsLock || !AssociatedGamePlayer.IsLocalPlayer)
		{
			return;
		}
		ButtonsNotToHide.Clear();
		if (Piece != null)
		{
			if (gameMode != GameState.GameMode.PARTY)
			{
				if ((gameMode == GameState.GameMode.FREEPLAY || gameMode == GameState.GameMode.CREATIVE) && Piece.Placed)
				{
					if (!Piece.inDestructible && Piece.InteractableInCurrentMode)
					{
						ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Inventory, visible: true, "Inventory/Stash"));
					}
				}
				else
				{
					ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Inventory, visible: true, "Inventory/InventoryTitle"));
				}
				ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Cancel, visible: true, "Inventory/Cancel"));
				if (Piece != null && Piece.Orientation != Placeable.OrientMode.NONE)
				{
					switch (Piece.Orientation)
					{
					case Placeable.OrientMode.FLIPX:
					case Placeable.OrientMode.FLIPY:
						ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Rotate, visible: true, "Inventory/Flip"));
						break;
					case Placeable.OrientMode.ROTATE:
						ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Rotate, visible: true, "Inventory/Rotate"));
						break;
					}
				}
				if (Piece != null && Piece.OrientationAlt != Placeable.OrientMode.NONE && Piece.OrientationAlt != Piece.Orientation)
				{
					switch (Piece.OrientationAlt)
					{
					case Placeable.OrientMode.FLIPX:
					case Placeable.OrientMode.FLIPY:
						ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Copy, visible: true, "Inventory/Flip", sprint));
						if (sprint)
						{
							ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Rotate, visible: true, "Inventory/Flip"));
						}
						break;
					case Placeable.OrientMode.ROTATE:
						ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Copy, visible: true, "Inventory/Rotate", sprint));
						if (sprint)
						{
							ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Rotate, visible: true, "Inventory/Rotate"));
						}
						break;
					}
				}
			}
		}
		else
		{
			ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Inventory, visible: true, "Inventory/InventoryTitle"));
			ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Switch, gameMode == GameState.GameMode.FREEPLAY, "InGameText/Switch"));
			ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.PickUp, hoveredPiece != null && hoveredPiece.InteractableInCurrentMode, "Inventory/Pick Up"));
			ButtonsNotToHide.Add(cursorControlHints.SetButtonVisibleReturn(CursorControlHints.Button.Copy, hoveredPiece != null && hoveredPiece.duplicable && hoveredPiece.InteractableInCurrentMode, "Inventory/Copy"));
		}
		cursorControlHints.HideAll(ButtonsNotToHide);
	}

	public override void Hide()
	{
		Placed = true;
		if (cursorSpriteRenderer != null)
		{
			cursorSpriteRenderer.enabled = false;
		}
		cursorControlHints.HideAll();
		if (cursorArtMatcher != null)
		{
			cursorArtMatcher.Disable();
		}
		if (hoveredPiece != null)
		{
			hoveredPiece.HoveredCursors.Remove(this);
			hoveredPiece.Tint();
			hoveredPiece = null;
		}
		Freeze();
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.POOF, base.transform.position, 0.7f);
	}

	private void PlacePieceDeferred(MsgPiecePlaced placeMsg, Placeable piece, bool pieceWasPickedUp)
	{
		StartCoroutine(WaitForPlacement(placeMsg, piece, pieceWasPickedUp));
	}

	private IEnumerator WaitForPlacement(MsgPiecePlaced placeMsg, Placeable piece, bool pieceWasPickedUp)
	{
		if (placementPhysicsLock)
		{
			Debug.Log("PiecePlacementCursor.WaitForPlacement: Waiting for previous operation to complete");
			while (placementPhysicsLock)
			{
				yield return null;
			}
		}
		placementPhysicsLock = true;
		try
		{
			Debug.Log("Waiting to place " + piece.UsefulName);
			if (piece.ID != placeMsg.PieceID)
			{
				Debug.LogWarning("Warning: piece.ID = " + piece.ID + " but placeMsg.PieceID = " + placeMsg.PieceID);
			}
			GameState.GetInstance().IncrementPieceCount(piece.Name);
			piece.transform.position = placeMsg.PiecePosition;
			piece.transform.localScale = placeMsg.PieceScale;
			piece.transform.rotation = placeMsg.PieceRotation;
			if (!placeMsg.PieceWasMoved)
			{
				piece.ID = placeMsg.PieceID;
				piece.UpdateChildIDs();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while initializing piece placement operation\n" + ex.Message + "\n" + ex.StackTrace);
			placementPhysicsLock = false;
			yield break;
		}
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		try
		{
			piece.Place(networkNumber, sendEvent: true, !base.hasAuthority);
			if (base.hasAuthority && piece.IsNetworked && piece.NetSurrogate == null)
			{
				CallCmdSpawnNetSurrogate(piece.ID);
			}
			if (piece.SFXEventName != "")
			{
				AkSoundEngine.PostEvent("UI_Inventory_Drop_" + piece.SFXEventName, base.gameObject);
			}
			Rigidbody2D component = piece.GetComponent<Rigidbody2D>();
			if (component != null)
			{
				component.isKinematic = true;
			}
			piece.gameObject.layer = 9;
			if (base.hasAuthority)
			{
				SetPiece(null);
				if (KeepPiece && !pieceWasPickedUp)
				{
					GameEventManager.SendEvent(new PickBlockEvent(networkNumber, piece.PickableBlock, piece));
				}
			}
			piece.Tint();
		}
		catch (Exception ex2)
		{
			Debug.LogError("Exception while waiting for piece placement operation\n" + ex2.Message + "\n" + ex2.StackTrace);
		}
		placementPhysicsLock = false;
	}

	public void OnTriggerStay2D(Collider2D c)
	{
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		OnBack();
	}

	public override void ReceiveEvent(InputEvent e)
	{
		base.ReceiveEvent(e);
		if (e.Key == InputEvent.InputKey.Back && e.Valueb)
		{
			backJustPressed = e.Changed;
		}
		if (paused || disabled)
		{
			return;
		}
		if (e.Key == InputEvent.InputKey.Sprint && e.Changed)
		{
			if (e.Valueb)
			{
				OnSprintDown();
			}
			else
			{
				OnSprintUp();
			}
		}
		if (e.Key == InputEvent.InputKey.Accept && e.Changed && !e.Valueb)
		{
			OnAcceptUp();
		}
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		Type type = e.GetType();
		if (type == typeof(PickBlockEvent))
		{
			PickBlockEvent pickBlockEvent = (PickBlockEvent)e;
			if (pickBlockEvent.PlayerNumber == networkNumber)
			{
				if (pickBlockEvent.PickablePiece != null)
				{
					Placeable placeable = UnityEngine.Object.Instantiate(pickBlockEvent.PickablePiece.placeablePrefab);
					placeable.GenerateIDOnPick(placeable.ID, networkNumber);
					if (pickBlockEvent.ReuseTransformPlaceable != null)
					{
						if (pickBlockEvent.ReuseTransformPlaceable.canSetCustomColor)
						{
							placeable.SetColor(pickBlockEvent.ReuseTransformPlaceable.CustomColor);
						}
						if (pickBlockEvent.ReuseTransformPlaceable.damageLevel > 0)
						{
							placeable.SetInitialDamageLevel(pickBlockEvent.ReuseTransformPlaceable.damageLevel, allowDamageReset: true);
						}
					}
					else
					{
						if (pickBlockEvent.PickablePiece.canHaveCustomColorSet)
						{
							placeable.SetColor(pickBlockEvent.PickablePiece.CustomColor);
						}
						if (pickBlockEvent.PickablePiece.DamageLevel > 0)
						{
							placeable.SetInitialDamageLevel(pickBlockEvent.PickablePiece.DamageLevel, allowDamageReset: true);
						}
					}
					if (GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY)
					{
						MsgBookPiecePicked msgBookPiecePicked = new MsgBookPiecePicked
						{
							pieceNumber = LobbyManager.instance.CurrentGameController.MetaList.GetIndexForPlaceable(placeable.Name),
							NetworkPlayerNumber = networkNumber,
							PieceID = placeable.ID,
							canSetCustomColor = placeable.canSetCustomColor,
							customColor = placeable.CustomColor,
							damageLevel = placeable.damageLevel
						};
						if (pickBlockEvent.ReuseTransformPlaceable != null)
						{
							msgBookPiecePicked.SetTransform = true;
							msgBookPiecePicked.PiecePosition = pickBlockEvent.ReuseTransformPlaceable.transform.position;
							msgBookPiecePicked.PieceRotation = pickBlockEvent.ReuseTransformPlaceable.transform.rotation;
							msgBookPiecePicked.PieceScale = pickBlockEvent.ReuseTransformPlaceable.transform.localScale;
							msgBookPiecePicked.PieceRotationDirection = pickBlockEvent.ReuseTransformPlaceable.RotationDirection;
						}
						placeable.ApplyTransformFromMessage(msgBookPiecePicked);
						NetworkManager.singleton.client.Send(NetMsgTypes.BookPiecePicked, msgBookPiecePicked);
					}
					else if (base.hasAuthority)
					{
						MsgSetPartyPieceID msg = new MsgSetPartyPieceID
						{
							NetworkPlayerNumber = networkNumber,
							PieceID = placeable.ID
						};
						NetworkManager.singleton.client.Send(NetMsgTypes.SetPartyPieceID, msg);
					}
					SetPiece(placeable, destroyPrevious: true);
				}
				if (GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY && GameSettings.GetInstance().GameMode != GameState.GameMode.CHALLENGE)
				{
					Enable();
				}
				else
				{
					Disable();
					Freeze();
				}
			}
		}
		if (type == typeof(StartPhaseEvent) && (e as StartPhaseEvent).Phase == GameControl.GamePhase.PLACE)
		{
			Placed = false;
		}
		if (type == typeof(SoftPauseEvent))
		{
			SoftPauseEvent softPauseEvent = e as SoftPauseEvent;
			if (softPauseEvent.SoftPaused && softPauseEvent.PlayerNumber == networkNumber)
			{
				base.Networkpaused = true;
				Pause();
				if (cursorSpriteRenderer != null)
				{
					cursorSpriteRenderer.sprite = notebook;
				}
			}
			else if (!softPauseEvent.SoftPaused && softPauseEvent.PlayerNumber == networkNumber)
			{
				base.Networkpaused = false;
				Unpause();
			}
		}
		if (type == typeof(PlayerInventoryEvent))
		{
			PlayerInventoryEvent playerInventoryEvent = e as PlayerInventoryEvent;
			GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
			if (gameMode != GameState.GameMode.PARTY && gameMode != GameState.GameMode.CHALLENGE)
			{
				if (playerInventoryEvent.Entered)
				{
					if (playerInventoryEvent.PlayerNumber == networkNumber && base.hasAuthority && currentGamePhase == GameControl.GamePhase.PLACE && !disabled)
					{
						Disable(sound: true, LobbyManager.instance.IsInOnlineGame);
					}
				}
				else if (playerInventoryEvent.PlayerNumber == networkNumber && base.hasAuthority)
				{
					if (gameMode == GameState.GameMode.FREEPLAY)
					{
						if (LocalPlayer.FreeplayPhase == GameControl.GamePhase.PLACE)
						{
							Enable();
						}
					}
					else if (currentGamePhase == GameControl.GamePhase.PLACE && !Placed && disabled)
					{
						Enable();
					}
				}
			}
		}
		if (!(type == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.BookPiecePicked)
		{
			MsgBookPiecePicked msgBookPiecePicked2 = networkMessageReceivedEvent.ReadMessage as MsgBookPiecePicked;
			if (msgBookPiecePicked2.NetworkPlayerNumber == networkNumber)
			{
				if (!base.hasAuthority)
				{
					Placeable placeable2 = UnityEngine.Object.Instantiate(LobbyManager.instance.CurrentGameController.MetaList.GetPlaceableByIndex(msgBookPiecePicked2.pieceNumber));
					placeable2.ID = msgBookPiecePicked2.PieceID;
					placeable2.UpdateChildIDs();
					placeable2.ApplyTransformFromMessage(msgBookPiecePicked2);
					if (msgBookPiecePicked2.canSetCustomColor)
					{
						placeable2.SetColor(msgBookPiecePicked2.customColor);
					}
					if (msgBookPiecePicked2.damageLevel > 0)
					{
						placeable2.SetInitialDamageLevel(msgBookPiecePicked2.damageLevel, allowDamageReset: true);
					}
					SetPiece(placeable2, destroyPrevious: true);
				}
				if (!WaitingForInventory)
				{
					Enable();
				}
			}
		}
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetPartyPieceID)
		{
			MsgSetPartyPieceID msgSetPartyPieceID = networkMessageReceivedEvent.ReadMessage as MsgSetPartyPieceID;
			if (msgSetPartyPieceID.NetworkPlayerNumber == networkNumber && !base.hasAuthority)
			{
				if (Piece != null)
				{
					Piece.ID = msgSetPartyPieceID.PieceID;
					Piece.UpdateChildIDs();
				}
				else
				{
					Debug.LogError("ERROR while handling MsgSetPartyPieceID in PiecePlacementCursor: Piece is null (Piece ID: " + msgSetPartyPieceID.PieceID + " PlayerNumber: " + msgSetPartyPieceID.NetworkPlayerNumber + ")");
				}
			}
		}
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PiecePlaced)
		{
			MsgPiecePlaced msgPiecePlaced = networkMessageReceivedEvent.ReadMessage as MsgPiecePlaced;
			if (msgPiecePlaced.PlayerNumber == networkNumber)
			{
				WaitingForPlaceMessageResponse = false;
				Placeable placeable3 = null;
				if (msgPiecePlaced.PieceID != 0)
				{
					foreach (Placeable allPlaceable in Placeable.AllPlaceables)
					{
						if (allPlaceable != null && allPlaceable.ID == msgPiecePlaced.PieceID)
						{
							placeable3 = allPlaceable;
							break;
						}
					}
					if (placeable3 == null)
					{
						Debug.LogError("MsgPiecePlaced: Failed to find piece with ID " + msgPiecePlaced.PieceID);
					}
				}
				if (msgPiecePlaced.PieceID <= 0)
				{
					GameEventManager.SendEvent(new PlacementSkippedEvent(networkNumber));
				}
				else if (placeable3 != null)
				{
					PlacePieceDeferred(msgPiecePlaced, placeable3, placeable3.Placed);
				}
				if (Piece != null && Piece == placeable3)
				{
					SetPiece(null);
				}
			}
		}
		if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.PiecePickedUp)
		{
			return;
		}
		MsgPiecePickedUp msgPiecePickedUp = networkMessageReceivedEvent.ReadMessage as MsgPiecePickedUp;
		if (msgPiecePickedUp.PlayerNumber != networkNumber)
		{
			return;
		}
		WaitingForPlaceMessageResponse = false;
		Placeable placeable4 = null;
		if (msgPiecePickedUp.PieceID != 0)
		{
			bool flag = false;
			foreach (Placeable allPlaceable2 in Placeable.AllPlaceables)
			{
				if (allPlaceable2 != null && allPlaceable2.ID == msgPiecePickedUp.PieceID)
				{
					placeable4 = allPlaceable2;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogError("MsgPiecePickedUp: Failed to find piece with ID " + msgPiecePickedUp.PieceID);
			}
		}
		if (placeable4 != null)
		{
			PickupPiece(placeable4);
		}
	}

	[Command]
	private void CmdRotatePiece(bool clockwise, bool sprintPressed)
	{
		CallRpcRotatePiece(clockwise, sprintPressed);
	}

	[ClientRpc]
	private void RpcRotatePiece(bool clockwise, bool sprintPressed)
	{
		if (!base.hasAuthority)
		{
			if (clockwise)
			{
				PerformRightRotation(sprintPressed);
			}
			else
			{
				PerformLeftRotation(sprintPressed);
			}
		}
	}

	[Command]
	private void CmdSwitchFreeMode()
	{
		CallRpcSwitchFreeMode();
	}

	[ClientRpc]
	private void RpcSwitchFreeMode()
	{
		if (!base.hasAuthority)
		{
			GameEventManager.SendEvent(new FreePlayPlayerSwitchEvent(networkNumber, GameControl.GamePhase.PLAY));
		}
	}

	[Command]
	private void CmdSpawnNetSurrogate(int spawnForBlockID)
	{
		if (LobbyManager.instance.CurrentGameController != null)
		{
			LobbyManager.instance.CurrentGameController.SpawnNetSurrogate(spawnForBlockID);
		}
		else
		{
			Debug.LogError("Could not spawn net surrogate - no game controller");
		}
	}

	[Command]
	private void CmdClearPiece()
	{
		CallRpcClearPiece();
	}

	[ClientRpc]
	private void RpcClearPiece()
	{
		if (Piece != null)
		{
			if (Piece.Placed)
			{
				GameEventManager.SendEvent(new DestroyPieceEvent(Piece, networkNumber));
			}
			Piece.Disable();
			Piece.DestroySelf(destroyChildren: true, useSmoke: false, sendNetworkSignal: false);
			SetPiece(null);
		}
	}

	private void PickupPiece(Placeable p)
	{
		Debug.Log("Picking up piece " + p.UsefulName);
		p.DetachChildrenForPickup(detachFromParent: true);
		SetPiece(p, destroyPrevious: false, pickup: false);
		Piece.EnablePlacement();
		Piece.PickUp();
		LobbyManager.instance.CurrentGameController.CheckAttachmentRequiredColliders();
		LobbyManager.instance.CurrentGameController.DestroyMarkedPiecesNow();
	}

	private void ClearCurrentPiece()
	{
		if (base.hasAuthority)
		{
			CallCmdClearPiece();
			AkSoundEngine.PostEvent("UI_Lobby_Cursor_Disappear_Poof", base.gameObject);
		}
	}

	[ClientRpc]
	public void RpcSetPlacementsLeftText(int n)
	{
		if (n == 0)
		{
			placementsLeftText.text = "";
		}
		else
		{
			placementsLeftText.text = n.ToString();
		}
	}

	[Command]
	private void CmdSetHeldPositionOffset(Vector3 v)
	{
		NetworkheldPositionOffset = v;
	}

	protected override void InitControllerButtons(Controller usedController)
	{
		base.InitControllerButtons(usedController);
		holdBIndicatorInstance.SetLocalController(usedController);
	}

	static PiecePlacementCursor()
	{
		raycastResultCache = new RaycastHit2D[128];
		kCmdCmdRotatePiece = 1847991782;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PiecePlacementCursor), kCmdCmdRotatePiece, InvokeCmdCmdRotatePiece);
		kCmdCmdSwitchFreeMode = -2058712816;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PiecePlacementCursor), kCmdCmdSwitchFreeMode, InvokeCmdCmdSwitchFreeMode);
		kCmdCmdSpawnNetSurrogate = 212179497;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PiecePlacementCursor), kCmdCmdSpawnNetSurrogate, InvokeCmdCmdSpawnNetSurrogate);
		kCmdCmdClearPiece = -469971282;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PiecePlacementCursor), kCmdCmdClearPiece, InvokeCmdCmdClearPiece);
		kCmdCmdSetHeldPositionOffset = 1295714694;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PiecePlacementCursor), kCmdCmdSetHeldPositionOffset, InvokeCmdCmdSetHeldPositionOffset);
		kRpcRpcRotatePiece = 49567824;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PiecePlacementCursor), kRpcRpcRotatePiece, InvokeRpcRpcRotatePiece);
		kRpcRpcSwitchFreeMode = 810172006;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PiecePlacementCursor), kRpcRpcSwitchFreeMode, InvokeRpcRpcSwitchFreeMode);
		kRpcRpcClearPiece = 1688772356;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PiecePlacementCursor), kRpcRpcClearPiece, InvokeRpcRpcClearPiece);
		kRpcRpcSetPlacementsLeftText = -2021497887;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PiecePlacementCursor), kRpcRpcSetPlacementsLeftText, InvokeRpcRpcSetPlacementsLeftText);
		NetworkCRC.RegisterBehaviour("PiecePlacementCursor", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdRotatePiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRotatePiece called on client.");
		}
		else
		{
			((PiecePlacementCursor)obj).CmdRotatePiece(reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSwitchFreeMode(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwitchFreeMode called on client.");
		}
		else
		{
			((PiecePlacementCursor)obj).CmdSwitchFreeMode();
		}
	}

	protected static void InvokeCmdCmdSpawnNetSurrogate(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnNetSurrogate called on client.");
		}
		else
		{
			((PiecePlacementCursor)obj).CmdSpawnNetSurrogate((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdClearPiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdClearPiece called on client.");
		}
		else
		{
			((PiecePlacementCursor)obj).CmdClearPiece();
		}
	}

	protected static void InvokeCmdCmdSetHeldPositionOffset(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetHeldPositionOffset called on client.");
		}
		else
		{
			((PiecePlacementCursor)obj).CmdSetHeldPositionOffset(reader.ReadVector3());
		}
	}

	public void CallCmdRotatePiece(bool clockwise, bool sprintPressed)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRotatePiece called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRotatePiece(clockwise, sprintPressed);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRotatePiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(clockwise);
		networkWriter.Write(sprintPressed);
		SendCommandInternal(networkWriter, 0, "CmdRotatePiece");
	}

	public void CallCmdSwitchFreeMode()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSwitchFreeMode called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSwitchFreeMode();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSwitchFreeMode);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSwitchFreeMode");
	}

	public void CallCmdSpawnNetSurrogate(int spawnForBlockID)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnNetSurrogate called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnNetSurrogate(spawnForBlockID);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnNetSurrogate);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)spawnForBlockID);
		SendCommandInternal(networkWriter, 0, "CmdSpawnNetSurrogate");
	}

	public void CallCmdClearPiece()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdClearPiece called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdClearPiece();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdClearPiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdClearPiece");
	}

	public void CallCmdSetHeldPositionOffset(Vector3 v)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetHeldPositionOffset called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetHeldPositionOffset(v);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetHeldPositionOffset);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(v);
		SendCommandInternal(networkWriter, 0, "CmdSetHeldPositionOffset");
	}

	protected static void InvokeRpcRpcRotatePiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRotatePiece called on server.");
		}
		else
		{
			((PiecePlacementCursor)obj).RpcRotatePiece(reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcSwitchFreeMode(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSwitchFreeMode called on server.");
		}
		else
		{
			((PiecePlacementCursor)obj).RpcSwitchFreeMode();
		}
	}

	protected static void InvokeRpcRpcClearPiece(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearPiece called on server.");
		}
		else
		{
			((PiecePlacementCursor)obj).RpcClearPiece();
		}
	}

	protected static void InvokeRpcRpcSetPlacementsLeftText(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPlacementsLeftText called on server.");
		}
		else
		{
			((PiecePlacementCursor)obj).RpcSetPlacementsLeftText((int)reader.ReadPackedUInt32());
		}
	}

	public void CallRpcRotatePiece(bool clockwise, bool sprintPressed)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRotatePiece called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRotatePiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(clockwise);
		networkWriter.Write(sprintPressed);
		SendRPCInternal(networkWriter, 0, "RpcRotatePiece");
	}

	public void CallRpcSwitchFreeMode()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSwitchFreeMode called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSwitchFreeMode);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcSwitchFreeMode");
	}

	public void CallRpcClearPiece()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcClearPiece called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcClearPiece);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcClearPiece");
	}

	public void CallRpcSetPlacementsLeftText(int n)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetPlacementsLeftText called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetPlacementsLeftText);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)n);
		SendRPCInternal(networkWriter, 0, "RpcSetPlacementsLeftText");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(heldPositionOffset);
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
			writer.Write(heldPositionOffset);
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
			heldPositionOffset = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 0x800) != 0)
		{
			heldPositionOffset = reader.ReadVector3();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
