using System;
using System.Collections;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class Cursor : NetworkBehaviour, InputReceiver, IGameEventListener
{
	public float CursorSpeed;

	public float CursorSpeedMultiplier;

	public float AccurateSlowCursor;

	[SyncVar]
	public int networkNumber;

	[SyncVar]
	public int localNumber;

	public Player LocalPlayer;

	public AnimationCurve AccelerationCurve;

	public float AccelerationTime;

	public BoxCollider2D BoundingBox;

	public Vector2 PieceOffset;

	public LobbyPlayer AssociatedLobbyPlayer;

	public GamePlayer AssociatedGamePlayer;

	public Camera UseCamera;

	public MultiControllerButton[] multiControllerButtons;

	public float extraZoomSpeed;

	public float extraZoom;

	[NonSerialized]
	public float extraZoomCurrent = 1f;

	public Placeable hoveredPiece;

	public IPickable hoveredPick;

	public IPickable lastHoveredPick;

	[SyncVar]
	public bool FindPlayerOnSpawn;

	[SyncVar]
	public bool FindControllerOnSpawn;

	public NameTag nameTag;

	[SyncVar]
	public bool WaitingForInventory;

	protected Vector3 gridPosition;

	protected SpriteRenderer cursorSpriteRenderer;

	[SyncVar]
	protected bool disabled;

	[SyncVar]
	protected bool paused;

	[SyncVar]
	protected bool scoreboard;

	[SyncVar]
	protected bool frozen;

	protected bool firstFrame;

	protected Bounds boundary;

	protected Collider2D boundingCollider;

	protected float cursorAccelMult;

	protected bool accelerating;

	[SyncVar]
	protected Color cursorColor = Color.white;

	[SyncVar]
	protected Character.Animals cursorAnimal;

	protected Sprite bad;

	protected Sprite ok;

	protected Sprite notebook;

	protected GameControl.GamePhase currentGamePhase;

	protected CursorArtMatcher cursorArtMatcher;

	protected bool waitingForFixedUpdate;

	protected bool FixedUpdateHappened;

	protected bool waitForEnableToBeDone;

	protected bool StillShowingOnClient;

	protected float afkTimer;

	protected bool accept;

	protected bool start;

	protected bool back;

	protected bool sprint;

	protected bool buttonSprint;

	protected bool triggerSprint;

	protected bool inventory;

	protected bool rotateLeft;

	protected bool rotateRight;

	protected bool acceptDown;

	protected bool startDown;

	protected bool backDown;

	protected bool sprintDown;

	protected bool inventoryDown;

	protected bool rotateLeftDown;

	protected bool rotateRightDown;

	protected bool escDown;

	protected bool acceptUp;

	protected bool startUp;

	protected bool backUp;

	protected bool sprintUp;

	protected bool inventoryUp;

	protected bool rotateLeftUp;

	protected bool rotateRightUp;

	protected float up;

	protected float down;

	protected float left;

	protected float right;

	protected float zoomTrigger;

	protected bool zoomToggle;

	protected bool followingMouse;

	public bool UseScreenPosition;

	public Controller ScreenPositionController;

	private Vector3 mouseReferencePoint;

	private float cameraChangeTimeout;

	private bool requestRotateLeft;

	private bool requestRotateRight;

	public bool lastRotateWasMouseWheel;

	private const int framesBetweenRotations = 3;

	private int lastRotation = 3;

	[NonSerialized]
	public bool IgnoreNextBackDown;

	private static int kCmdCmdSetSprites;

	private static int kRpcRpcSetSprites;

	private static int kCmdCmdSetLocalPlayerID;

	private static int kRpcRpcFindLocalPlayer;

	private static int kCmdCmdGetLocalController;

	private static int kRpcRpcGetLocalController;

	private static int kCmdCmdSetWaitingForInventory;

	private static int kCmdCmdEnable;

	private static int kRpcRpcEnable;

	private static int kCmdCmdDisable;

	private static int kRpcRpcDisable;

	private static int kCmdCmdFreeze;

	private static int kRpcRpcFreeze;

	private static int kCmdCmdUnfreeze;

	private static int kRpcRpcUnfreeze;

	private static int kCmdCmdPause;

	private static int kRpcRpcPause;

	private static int kCmdCmdUnpause;

	private static int kRpcRpcUnpause;

	private static int kCmdCmdHide;

	private static int kRpcRpcHide;

	public bool Paused => paused;

	public bool Frozen => frozen;

	public bool Enabled => !disabled;

	public bool Held => accept;

	public bool Sprinting => sprint;

	public bool isHovering
	{
		get
		{
			if (!hoverState())
			{
				return hoverStatePick();
			}
			return true;
		}
	}

	public Color CursorColor
	{
		get
		{
			return cursorColor;
		}
		set
		{
			SetColor(value);
		}
	}

	public Character.Animals CursorAnimal
	{
		get
		{
			return cursorAnimal;
		}
		set
		{
			SetSprites(value);
		}
	}

	public float TimeSpentAFK => afkTimer;

	public bool IsControllerAFK
	{
		get
		{
			if (AssociatedLobbyPlayer != null && AssociatedLobbyPlayer.LocalPlayer != null && AssociatedLobbyPlayer.LocalPlayer.UseController != null)
			{
				return AssociatedLobbyPlayer.LocalPlayer.UseController.TimeSinceLastInput > 1f;
			}
			return false;
		}
	}

	protected bool ScrollingMouseWheel
	{
		get
		{
			if (((LocalPlayer != null) ? LocalPlayer : PlayerManager.GetInstance().GetPlayer(localNumber)).UseController is KeyboardInput)
			{
				return Input.mouseScrollDelta.y != 0f;
			}
			return false;
		}
	}

	public int NetworknetworkNumber
	{
		get
		{
			return networkNumber;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref networkNumber, 1u);
		}
	}

	public int NetworklocalNumber
	{
		get
		{
			return localNumber;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref localNumber, 2u);
		}
	}

	public bool NetworkFindPlayerOnSpawn
	{
		get
		{
			return FindPlayerOnSpawn;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref FindPlayerOnSpawn, 4u);
		}
	}

	public bool NetworkFindControllerOnSpawn
	{
		get
		{
			return FindControllerOnSpawn;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref FindControllerOnSpawn, 8u);
		}
	}

	public bool NetworkWaitingForInventory
	{
		get
		{
			return WaitingForInventory;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref WaitingForInventory, 16u);
		}
	}

	public bool Networkdisabled
	{
		get
		{
			return disabled;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref disabled, 32u);
		}
	}

	public bool Networkpaused
	{
		get
		{
			return paused;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref paused, 64u);
		}
	}

	public bool Networkscoreboard
	{
		get
		{
			return scoreboard;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref scoreboard, 128u);
		}
	}

	public bool Networkfrozen
	{
		get
		{
			return frozen;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref frozen, 256u);
		}
	}

	public Color NetworkcursorColor
	{
		get
		{
			return cursorColor;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref cursorColor, 512u);
		}
	}

	public Character.Animals NetworkcursorAnimal
	{
		get
		{
			return cursorAnimal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref cursorAnimal, 1024u);
		}
	}

	public virtual bool hoverState()
	{
		return hoveredPiece != null;
	}

	public virtual bool hoverStatePick()
	{
		return hoveredPick != null;
	}

	public virtual void Awake()
	{
		cursorSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		nameTag = GetComponentInChildren<NameTag>();
	}

	public virtual void Start()
	{
		ChangeListener(adding: true);
		CursorColor = cursorColor;
		CursorAnimal = cursorAnimal;
		if (base.hasAuthority)
		{
			if (FindPlayerOnSpawn)
			{
				Debug.Log(base.name + " Finding player " + localNumber);
				CallCmdSetLocalPlayerID(localNumber);
			}
			if (FindControllerOnSpawn)
			{
				Debug.Log(base.name + " Finding controller " + localNumber);
				CallCmdGetLocalController(localNumber);
			}
			GameEventManager.SendEvent(new NetworkCursorSpawnedEvent(this));
		}
		if (UseCamera != null)
		{
			mouseReferencePoint = new Vector3(0f, 0f, 0f - UseCamera.transform.position.z);
		}
		if (disabled)
		{
			Disable(sound: false);
		}
		else if (frozen)
		{
			Freeze();
		}
		else if (paused)
		{
			Pause();
		}
	}

	public virtual void OnDestroy()
	{
		ChangeListener(adding: false);
		GameEventManager.RemoveListenerFromAll(this);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<GameStartEvent>(this, adding);
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<SpecialUIEvent>(this, adding);
		GameEventManager.ChangeListener<SoftPauseEvent>(this, adding);
	}

	protected virtual void Update()
	{
		if (cameraChangeTimeout > 0f)
		{
			cameraChangeTimeout -= Time.unscaledDeltaTime;
		}
		if (nameTag != null)
		{
			if (AssociatedLobbyPlayer != null)
			{
				if (nameTag.Currentname == "")
				{
					nameTag.setNameBoxText(AssociatedLobbyPlayer.playerName, this);
				}
			}
			else if (AssociatedGamePlayer != null)
			{
				if (nameTag.Currentname == "")
				{
					nameTag.setNameBoxText(AssociatedGamePlayer.playerName, this);
				}
			}
			else if (nameTag.Currentname != "")
			{
				nameTag.setNameBoxText("", this);
				nameTag.currentAlpha = 0f;
			}
			if (AssociatedLobbyPlayer != null)
			{
				nameTag.UpdateIcons(AssociatedLobbyPlayer);
			}
			else
			{
				LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(networkNumber);
				if (lobbyPlayer != null)
				{
					nameTag.UpdateIcons(lobbyPlayer);
				}
			}
			if ((disabled || Frozen || !LobbyManager.instance.IsInOnlineGame) && !StillShowingOnClient)
			{
				nameTag.currentAlpha = 0f;
			}
			else if (nameTag.Currentname != "")
			{
				if (GameSettings.GetInstance().OnlinePlayerNames == OnlinePlayerNames.AlwaysOff)
				{
					nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 0f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime);
				}
				else
				{
					nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 1f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime);
				}
			}
			else
			{
				nameTag.currentAlpha = Mathf.MoveTowards(nameTag.currentAlpha, 0f, GameSettings.GetInstance().SteamNameHideSpeed * Time.unscaledDeltaTime);
			}
		}
		if (waitingForFixedUpdate && FixedUpdateHappened)
		{
			waitingForFixedUpdate = false;
			FixedUpdateHappened = false;
			OnAccept();
		}
		if (LobbyManager.instance != null && LobbyManager.instance.IsInOnlineGame && IsControllerAFK)
		{
			afkTimer += Time.unscaledDeltaTime;
		}
		else
		{
			afkTimer = 0f;
		}
		if (paused || disabled || frozen || waitingForFixedUpdate)
		{
			return;
		}
		followingMouse = UseCamera != null && UseScreenPosition && ScreenPositionController != null;
		if (followingMouse)
		{
			Vector3 position = ScreenPositionController.GetVector();
			position.z = 0f - UseCamera.transform.position.z;
			Vector3 translation = UseCamera.ScreenToWorldPoint(position) - UseCamera.ScreenToWorldPoint(mouseReferencePoint);
			base.transform.Translate(translation);
		}
		Vector2 vector = new Vector2(right - left, up - down);
		if (UseCamera != null && UseCamera.GetComponent<CameraFlipper>() != null)
		{
			switch (Modifiers.GetInstance().CameraFlipping)
			{
			case Modifiers.CameraFlipModes.FlipX:
				vector.x = 0f - vector.x;
				break;
			case Modifiers.CameraFlipModes.FlipY:
				vector.y = 0f - vector.y;
				break;
			case Modifiers.CameraFlipModes.FlipXY:
				vector = -vector;
				break;
			}
		}
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		if (!accelerating && vector.sqrMagnitude > 0f)
		{
			StartCoroutine(startCursorAcceleration());
		}
		else if (vector.sqrMagnitude == 0f)
		{
			accelerating = false;
		}
		if (accelerating)
		{
			vector *= cursorAccelMult;
		}
		vector.x *= CursorSpeed;
		vector.y *= CursorSpeed;
		if (sprint)
		{
			vector.x *= CursorSpeedMultiplier;
			vector.y *= CursorSpeedMultiplier;
		}
		if (triggerSprint && buttonSprint)
		{
			vector.x *= CursorSpeedMultiplier;
			vector.y *= CursorSpeedMultiplier;
		}
		base.transform.Translate(vector.x * Time.unscaledDeltaTime, vector.y * Time.unscaledDeltaTime, 0f);
		if (!followingMouse && vector.sqrMagnitude > 0f)
		{
			ClampToBoundary();
		}
		else if (UseCamera != null)
		{
			ZoomCamera component = UseCamera.GetComponent<ZoomCamera>();
			if (component != null)
			{
				ClampToBoundary(component.GetBounds());
			}
		}
		if (zoomToggle)
		{
			extraZoomCurrent = Mathf.MoveTowards(extraZoomCurrent, 2f, extraZoomSpeed * Time.fixedDeltaTime);
		}
		else
		{
			extraZoomCurrent = Mathf.MoveTowards(extraZoomCurrent, 1f, extraZoomSpeed * Time.fixedDeltaTime);
		}
	}

	protected virtual void FixedUpdate()
	{
		if (paused || disabled || frozen)
		{
			return;
		}
		if (waitingForFixedUpdate)
		{
			FixedUpdateHappened = true;
			return;
		}
		lastRotation++;
		if (lastRotation >= 3)
		{
			if (requestRotateLeft)
			{
				requestRotateLeft = false;
				lastRotation = 0;
				OnRotateLeft();
			}
			if (requestRotateRight)
			{
				lastRotation = 0;
				requestRotateRight = false;
				OnRotateRight();
			}
			lastRotateWasMouseWheel = false;
		}
	}

	protected virtual void OnAccept()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnBack()
	{
	}

	protected virtual void OnRotateLeft()
	{
	}

	protected virtual void OnRotateRight()
	{
	}

	protected virtual void OnInventory()
	{
	}

	protected virtual void LateUpdate()
	{
		if (waitForEnableToBeDone)
		{
			ResetInput();
			waitForEnableToBeDone = false;
		}
		else
		{
			if (paused || disabled || frozen || waitingForFixedUpdate)
			{
				return;
			}
			if (followingMouse)
			{
				Vector3 position = ScreenPositionController.GetVector(absolute: true);
				position.z = 0f - UseCamera.transform.position.z;
				Vector3 position2 = UseCamera.ScreenToWorldPoint(position);
				position2.z = 0f;
				if (BoundingBox != null)
				{
					Vector3 vector = new Vector3(BoundingBox.offset.x * base.transform.lossyScale.x, BoundingBox.offset.y * base.transform.lossyScale.y, 0f);
					position2 -= vector;
				}
				base.transform.position = position2;
				ZoomCamera component = UseCamera.GetComponent<ZoomCamera>();
				if (component != null)
				{
					ClampToBoundary(component.GetBounds());
				}
			}
			Player localPlayer = LocalPlayer;
			bool flag = localPlayer != null && localPlayer.UseController != null && localPlayer.UseController.IsKeyboard && Controller.InputFieldIsActive;
			if (paused || disabled || frozen || flag)
			{
				return;
			}
			if (firstFrame)
			{
				firstFrame = false;
				return;
			}
			if (acceptDown)
			{
				waitingForFixedUpdate = true;
			}
			if (escDown)
			{
				OnStart();
			}
			if (startDown)
			{
				OnStart();
			}
			if (backDown)
			{
				OnBack();
			}
			if (inventoryDown)
			{
				OnInventory();
			}
			bool cameraFlippedOnX = Modifiers.GetInstance().CameraFlippedOnX;
			if (rotateLeftDown)
			{
				if (cameraFlippedOnX)
				{
					requestRotateRight = true;
				}
				else
				{
					requestRotateLeft = true;
				}
				if (ScrollingMouseWheel)
				{
					lastRotateWasMouseWheel = true;
				}
			}
			if (rotateRightDown)
			{
				if (cameraFlippedOnX)
				{
					requestRotateLeft = true;
				}
				else
				{
					requestRotateRight = true;
				}
				if (ScrollingMouseWheel)
				{
					lastRotateWasMouseWheel = true;
				}
			}
			ResetInput();
		}
	}

	public void SetBounds(Collider2D boundingCollider)
	{
		this.boundingCollider = boundingCollider;
		boundary = boundingCollider.bounds;
	}

	public void SetCursorColliderBounds(Collider2D boundingCollider)
	{
		this.boundingCollider = boundingCollider;
	}

	public void SetBounds(Bounds boundary)
	{
		this.boundary = boundary;
	}

	public virtual void SetSprites(Character.Animals animal)
	{
		NetworkcursorAnimal = animal;
		SetSprites(CharacterSpriteManager.GetInstance().GetCharacterSprites(animal));
	}

	[Command]
	private void CmdSetSprites(Character.Animals animal)
	{
		CallRpcSetSprites(animal);
	}

	[ClientRpc]
	private void RpcSetSprites(Character.Animals animal)
	{
		if (!base.hasAuthority)
		{
			SetSprites(animal);
		}
	}

	protected virtual void SetSprites(CharacterSpriteLibrary spriteLib)
	{
		if (spriteLib != null)
		{
			ok = spriteLib.OKCursor;
			bad = spriteLib.BadCursor;
			notebook = spriteLib.NotebookCursor;
			cursorSpriteRenderer.sprite = ok;
			UpdateLayerOrder(cursorAnimal);
		}
	}

	protected virtual void UpdateLayerOrder(Character.Animals animal)
	{
		bool flag = false;
		if (AssociatedLobbyPlayer != null)
		{
			if (!AssociatedLobbyPlayer.Initialized)
			{
				AssociatedLobbyPlayer.RunAfterInitialized(delegate
				{
					UpdateLayerOrder(animal);
				});
				flag = true;
			}
		}
		else if (AssociatedGamePlayer != null && !AssociatedGamePlayer.Initialized)
		{
			AssociatedGamePlayer.RunAfterInitialized(delegate
			{
				UpdateLayerOrder(animal);
			});
			flag = true;
		}
		int num = 30000 + (int)animal * 100;
		if (!flag && ((AssociatedLobbyPlayer != null && AssociatedLobbyPlayer.IsLocalPlayer) || (AssociatedGamePlayer != null && AssociatedGamePlayer.IsLocalPlayer)))
		{
			num += 1000;
		}
		cursorSpriteRenderer.sortingOrder = num;
		if (nameTag != null)
		{
			nameTag.MatchLayerOrder(cursorSpriteRenderer);
		}
		if (base.hasAuthority)
		{
			CallCmdSetSprites(animal);
		}
	}

	protected void ResetInput()
	{
		rotateLeft = false;
		rotateRight = false;
		acceptDown = false;
		startDown = false;
		backDown = false;
		sprintDown = false;
		inventoryDown = false;
		rotateLeftDown = false;
		rotateRightDown = false;
		escDown = false;
		acceptUp = false;
		startUp = false;
		backUp = false;
		sprintUp = false;
		inventoryUp = false;
		rotateLeftUp = false;
		rotateRightUp = false;
	}

	public virtual void Hide()
	{
		if (base.hasAuthority)
		{
			CallCmdHide();
		}
	}

	public virtual void Disable(bool sound = true, bool showNotebookSprite = false)
	{
		if (cursorSpriteRenderer != null)
		{
			if (showNotebookSprite && !base.hasAuthority)
			{
				cursorSpriteRenderer.sprite = notebook;
				StillShowingOnClient = true;
			}
			else
			{
				cursorSpriteRenderer.enabled = false;
				if (cursorArtMatcher != null)
				{
					cursorArtMatcher.Disable();
				}
			}
		}
		Networkdisabled = true;
		firstFrame = true;
		if (sound)
		{
			AkSoundEngine.PostEvent("UI_Lobby_Cursor_Disappear_Poof", base.gameObject);
		}
		if (base.hasAuthority)
		{
			CallCmdDisable(sound, showNotebookSprite);
		}
	}

	public virtual void Enable()
	{
		if (cursorSpriteRenderer != null)
		{
			cursorSpriteRenderer.enabled = true;
		}
		Networkdisabled = false;
		Networkfrozen = false;
		sprint = false;
		triggerSprint = false;
		buttonSprint = false;
		ResetInput();
		left = 0f;
		right = 0f;
		up = 0f;
		down = 0f;
		StillShowingOnClient = false;
		afkTimer = 0f;
		if (UseScreenPosition && ScreenPositionController != null)
		{
			Vector3 position = ScreenPositionController.GetVector(absolute: true);
			position.z = 0f - UseCamera.transform.position.z;
			Vector3 position2 = UseCamera.ScreenToWorldPoint(position);
			position2.x -= 0.28f;
			position2.y -= 0.78f;
			base.transform.position = position2;
			ScreenPositionController.SetPreciseCursor(precise: true);
		}
		cursorArtMatcher = GetComponent<CursorArtMatcher>();
		if (cursorArtMatcher != null)
		{
			cursorArtMatcher.Enable();
		}
		if (base.hasAuthority)
		{
			CallCmdEnable();
		}
		waitForEnableToBeDone = true;
	}

	public virtual void Freeze()
	{
		Networkfrozen = true;
		ResetInput();
		left = 0f;
		right = 0f;
		up = 0f;
		down = 0f;
		if (base.hasAuthority)
		{
			CallCmdFreeze();
		}
	}

	public virtual void Unfreeze()
	{
		Networkfrozen = false;
		firstFrame = true;
		if (base.hasAuthority)
		{
			CallCmdUnfreeze();
		}
	}

	public virtual void Pause()
	{
		if (cursorSpriteRenderer != null)
		{
			cursorSpriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
		}
		if (cursorArtMatcher != null)
		{
			cursorArtMatcher.SetArtAlpha(0.5f);
		}
		if (!disabled)
		{
			firstFrame = true;
			if (base.hasAuthority)
			{
				CallCmdPause();
			}
		}
	}

	public virtual void Unpause()
	{
		if (cursorSpriteRenderer != null)
		{
			Color color = cursorSpriteRenderer.color;
			cursorSpriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
		}
		if (cursorArtMatcher != null)
		{
			cursorArtMatcher.SetArtAlpha(1f);
		}
		if (!disabled && !paused && !scoreboard)
		{
			ResetInput();
			if (base.hasAuthority)
			{
				CallCmdUnpause();
			}
		}
	}

	public virtual void SetColor(Color c)
	{
		NetworkcursorColor = c;
		cursorSpriteRenderer.color = c;
	}

	public virtual void ClampToBoundary()
	{
		ClampToBoundary(boundary);
	}

	public virtual void ClampToBoundary(Bounds boundary)
	{
		if (boundary.extents.sqrMagnitude > 0f)
		{
			Vector3 localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);
			localPosition.x = Mathf.Min(boundary.max.x, Mathf.Max(boundary.min.x, base.transform.localPosition.x));
			localPosition.y = Mathf.Min(boundary.max.y, Mathf.Max(boundary.min.y, base.transform.localPosition.y));
			base.transform.localPosition = localPosition;
		}
	}

	public virtual void ReceiveEvent(InputEvent e)
	{
		switch (e.Key)
		{
		case InputEvent.InputKey.Up:
			up = e.Valuef;
			break;
		case InputEvent.InputKey.Down:
			down = e.Valuef;
			break;
		case InputEvent.InputKey.Left:
			left = e.Valuef;
			break;
		case InputEvent.InputKey.Right:
			right = e.Valuef;
			break;
		case InputEvent.InputKey.Esc:
			if (e.Changed)
			{
				escDown = e.Valueb;
			}
			break;
		case InputEvent.InputKey.Accept:
			accept = e.Valueb;
			if (e.Changed)
			{
				if (accept)
				{
					acceptDown = true;
				}
				else
				{
					acceptUp = true;
				}
			}
			break;
		case InputEvent.InputKey.Start:
			start = e.Valueb;
			if (e.Changed)
			{
				if (start)
				{
					startDown = true;
				}
				else
				{
					startUp = true;
				}
			}
			break;
		case InputEvent.InputKey.Sprint:
			sprint = e.Valueb;
			buttonSprint = e.Valueb;
			break;
		case InputEvent.InputKey.RightTrigger:
			sprint = e.Valueb;
			triggerSprint = e.Valueb;
			break;
		case InputEvent.InputKey.LeftTrigger:
			if (Enabled && e.Changed && e.Valueb)
			{
				zoomToggle = !zoomToggle;
			}
			break;
		case InputEvent.InputKey.Zoom:
			if (Enabled && e.Changed && e.Valueb)
			{
				zoomToggle = !zoomToggle;
			}
			break;
		case InputEvent.InputKey.Back:
			back = e.Valueb;
			if (!e.Changed)
			{
				break;
			}
			if (back)
			{
				if (IgnoreNextBackDown)
				{
					IgnoreNextBackDown = false;
				}
				else
				{
					backDown = true;
				}
			}
			else
			{
				backUp = true;
			}
			break;
		case InputEvent.InputKey.Inventory:
			inventory = e.Valueb;
			if (e.Changed)
			{
				if (inventory)
				{
					inventoryDown = true;
				}
				else
				{
					inventoryUp = true;
				}
			}
			break;
		case InputEvent.InputKey.RotateLeft:
			rotateLeft = e.Valueb;
			if (e.Changed)
			{
				if (rotateLeft)
				{
					rotateLeftDown = true;
					cameraChangeTimeout = 0.1f;
					checkCameraToggle();
				}
				else
				{
					rotateLeftUp = true;
				}
			}
			break;
		case InputEvent.InputKey.RotateRight:
			rotateRight = e.Valueb;
			if (e.Changed)
			{
				if (rotateRight)
				{
					rotateRightDown = true;
					cameraChangeTimeout = 0.1f;
					checkCameraToggle();
				}
				else
				{
					rotateRightUp = true;
				}
			}
			break;
		case InputEvent.InputKey.ChangeMode:
			UseScreenPosition = e.Valueb;
			if (e.Valueb)
			{
				ScreenPositionController = e.Sender;
				if (UseCamera != null)
				{
					mouseReferencePoint = new Vector3(0f, 0f, 0f - UseCamera.transform.position.z);
				}
			}
			break;
		}
	}

	private void checkCameraToggle()
	{
		if (Enabled && !Frozen && UseCamera == ZoomCamera.CurrentZoomCamera && LobbyManager.instance != null && !LobbyManager.instance.AllLocal && ((rotateLeftDown && rotateRightDown) || (rotateLeft && rotateRight && cameraChangeTimeout > 0f)) && ZoomCamera.CurrentZoomCamera != null)
		{
			ZoomCamera.CurrentZoomCamera.GetComponent<ZoomCamera>().ToggleLocalOnly();
		}
	}

	private bool HasNoPhysicModifier(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag((TagComparer.Tag)25165824);
		}
		return false;
	}

	public Placeable potentialHoeverPiece(Collider2D c)
	{
		if (disabled || frozen)
		{
			return null;
		}
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (HasNoPhysicModifier(c.gameObject, component))
		{
			return null;
		}
		Placeable placeable = c.GetComponent<Placeable>();
		if (placeable == null)
		{
			placeable = c.GetComponentInParent<Placeable>();
		}
		if (placeable != null && !placeable.PickedUp && !placeable.Permanent && !placeable.Protected)
		{
			return placeable;
		}
		return null;
	}

	public void checkHoveredPieceAdd(Placeable p)
	{
		if (!(p != null) || p.PickedUp || p.Permanent || p.Protected)
		{
			return;
		}
		if (hoveredPiece != null && p != hoveredPiece)
		{
			if (p.ParentPiece != hoveredPiece)
			{
				return;
			}
			hoveredPiece.HoveredCursors.Remove(this);
			hoveredPiece.Tint();
		}
		if (p is MultipiecePart && p.transform.parent != null && !p.GetComponent<MultipiecePart>().MainBlock.Separable)
		{
			hoveredPiece = p.transform.parent.GetComponentInParent<Placeable>();
		}
		else
		{
			hoveredPiece = p;
		}
		if (hoveredPiece != null)
		{
			if (!hoveredPiece.HoveredCursors.Contains(this))
			{
				hoveredPiece.HoveredCursors.Add(this);
			}
			hoveredPiece.Tint();
		}
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			currentGamePhase = startPhaseEvent.Phase;
		}
		if (type == typeof(ScoreboardEvent))
		{
			if ((e as ScoreboardEvent).Showing)
			{
				Networkscoreboard = true;
				Pause();
			}
			else
			{
				Networkscoreboard = false;
				Unpause();
			}
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				Networkpaused = true;
				Pause();
			}
			else
			{
				Networkpaused = false;
				Unpause();
			}
		}
	}

	[Command]
	public void CmdSetLocalPlayerID(int localNumber)
	{
		NetworklocalNumber = localNumber;
		CallRpcFindLocalPlayer(localNumber);
	}

	[ClientRpc]
	public void RpcFindLocalPlayer(int localNumber)
	{
		if (base.hasAuthority && localNumber != 0)
		{
			LocalPlayer = PlayerManager.GetInstance().GetPlayer(localNumber);
			if (LocalPlayer != null)
			{
				LocalPlayer.PlayerCursor = this;
			}
		}
	}

	[Command]
	public void CmdGetLocalController(int localNumber)
	{
		CallRpcGetLocalController(localNumber);
	}

	[ClientRpc]
	public void RpcGetLocalController(int localNumber)
	{
		if (base.hasAuthority && (localNumber != 0 || LocalPlayer != null))
		{
			Player player = ((LocalPlayer != null) ? LocalPlayer : PlayerManager.GetInstance().GetPlayer(localNumber));
			if (player != null)
			{
				player.UseController.AddReceiver(this);
				SetLocalController(player.UseController);
			}
		}
	}

	[Command]
	protected void CmdSetWaitingForInventory(bool waiting)
	{
		NetworkWaitingForInventory = waiting;
	}

	[Command]
	public void CmdEnable()
	{
		CallRpcEnable();
	}

	[ClientRpc]
	private void RpcEnable()
	{
		if (!base.hasAuthority)
		{
			Enable();
		}
	}

	[Command]
	public void CmdDisable(bool sound, bool showNotebookSprite)
	{
		CallRpcDisable(sound, showNotebookSprite);
	}

	[ClientRpc]
	private void RpcDisable(bool sound, bool showNotebookSprite)
	{
		if (!base.hasAuthority)
		{
			Disable(sound, showNotebookSprite);
		}
	}

	[Command]
	public void CmdFreeze()
	{
		CallRpcFreeze();
	}

	[ClientRpc]
	private void RpcFreeze()
	{
		if (!base.hasAuthority)
		{
			Freeze();
		}
	}

	[Command]
	public void CmdUnfreeze()
	{
		CallRpcUnfreeze();
	}

	[ClientRpc]
	private void RpcUnfreeze()
	{
		if (!base.hasAuthority)
		{
			Unfreeze();
		}
	}

	[Command]
	public void CmdPause()
	{
		CallRpcPause();
	}

	[ClientRpc]
	private void RpcPause()
	{
		if (!base.hasAuthority)
		{
			Pause();
		}
	}

	[Command]
	public void CmdUnpause()
	{
		CallRpcUnpause();
	}

	[ClientRpc]
	private void RpcUnpause()
	{
		if (!base.hasAuthority)
		{
			Unpause();
		}
	}

	[Command]
	public void CmdHide()
	{
		CallRpcHide();
	}

	[ClientRpc]
	private void RpcHide()
	{
		if (!base.hasAuthority)
		{
			Hide();
		}
	}

	protected virtual void InitControllerButtons(Controller usedController)
	{
		MultiControllerButton[] array = multiControllerButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ForceController(usedController);
		}
	}

	public void SetLocalController(Controller usedController)
	{
		InitControllerButtons(usedController);
	}

	private IEnumerator startCursorAcceleration()
	{
		if (!accelerating)
		{
			accelerating = true;
			cursorAccelMult = 0f;
			float accelTime = 0f;
			while (accelerating && accelTime < AccelerationTime)
			{
				accelTime += Time.unscaledDeltaTime;
				cursorAccelMult = Mathf.Lerp(0f, 1f, accelTime / AccelerationTime);
				yield return null;
			}
			if (accelerating)
			{
				cursorAccelMult = 1f;
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetSprites(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSprites called on client.");
		}
		else
		{
			((Cursor)obj).CmdSetSprites((Character.Animals)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSetLocalPlayerID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLocalPlayerID called on client.");
		}
		else
		{
			((Cursor)obj).CmdSetLocalPlayerID((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdGetLocalController(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetLocalController called on client.");
		}
		else
		{
			((Cursor)obj).CmdGetLocalController((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetWaitingForInventory(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetWaitingForInventory called on client.");
		}
		else
		{
			((Cursor)obj).CmdSetWaitingForInventory(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdEnable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEnable called on client.");
		}
		else
		{
			((Cursor)obj).CmdEnable();
		}
	}

	protected static void InvokeCmdCmdDisable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDisable called on client.");
		}
		else
		{
			((Cursor)obj).CmdDisable(reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdFreeze(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFreeze called on client.");
		}
		else
		{
			((Cursor)obj).CmdFreeze();
		}
	}

	protected static void InvokeCmdCmdUnfreeze(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnfreeze called on client.");
		}
		else
		{
			((Cursor)obj).CmdUnfreeze();
		}
	}

	protected static void InvokeCmdCmdPause(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPause called on client.");
		}
		else
		{
			((Cursor)obj).CmdPause();
		}
	}

	protected static void InvokeCmdCmdUnpause(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnpause called on client.");
		}
		else
		{
			((Cursor)obj).CmdUnpause();
		}
	}

	protected static void InvokeCmdCmdHide(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHide called on client.");
		}
		else
		{
			((Cursor)obj).CmdHide();
		}
	}

	public void CallCmdSetSprites(Character.Animals animal)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetSprites called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetSprites(animal);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetSprites);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)animal);
		SendCommandInternal(networkWriter, 0, "CmdSetSprites");
	}

	public void CallCmdSetLocalPlayerID(int localNumber)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetLocalPlayerID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetLocalPlayerID(localNumber);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetLocalPlayerID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendCommandInternal(networkWriter, 0, "CmdSetLocalPlayerID");
	}

	public void CallCmdGetLocalController(int localNumber)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdGetLocalController called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdGetLocalController(localNumber);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdGetLocalController);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendCommandInternal(networkWriter, 0, "CmdGetLocalController");
	}

	public void CallCmdSetWaitingForInventory(bool waiting)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetWaitingForInventory called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetWaitingForInventory(waiting);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetWaitingForInventory);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(waiting);
		SendCommandInternal(networkWriter, 0, "CmdSetWaitingForInventory");
	}

	public void CallCmdEnable()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdEnable called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdEnable();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdEnable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdEnable");
	}

	public void CallCmdDisable(bool sound, bool showNotebookSprite)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDisable called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDisable(sound, showNotebookSprite);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDisable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(sound);
		networkWriter.Write(showNotebookSprite);
		SendCommandInternal(networkWriter, 0, "CmdDisable");
	}

	public void CallCmdFreeze()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdFreeze called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdFreeze();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdFreeze);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdFreeze");
	}

	public void CallCmdUnfreeze()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUnfreeze called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUnfreeze();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUnfreeze);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdUnfreeze");
	}

	public void CallCmdPause()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPause called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPause();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPause);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdPause");
	}

	public void CallCmdUnpause()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUnpause called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUnpause();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUnpause);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdUnpause");
	}

	public void CallCmdHide()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdHide called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdHide();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdHide);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdHide");
	}

	protected static void InvokeRpcRpcSetSprites(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSprites called on server.");
		}
		else
		{
			((Cursor)obj).RpcSetSprites((Character.Animals)reader.ReadInt32());
		}
	}

	protected static void InvokeRpcRpcFindLocalPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFindLocalPlayer called on server.");
		}
		else
		{
			((Cursor)obj).RpcFindLocalPlayer((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcGetLocalController(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGetLocalController called on server.");
		}
		else
		{
			((Cursor)obj).RpcGetLocalController((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcEnable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnable called on server.");
		}
		else
		{
			((Cursor)obj).RpcEnable();
		}
	}

	protected static void InvokeRpcRpcDisable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDisable called on server.");
		}
		else
		{
			((Cursor)obj).RpcDisable(reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcFreeze(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFreeze called on server.");
		}
		else
		{
			((Cursor)obj).RpcFreeze();
		}
	}

	protected static void InvokeRpcRpcUnfreeze(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUnfreeze called on server.");
		}
		else
		{
			((Cursor)obj).RpcUnfreeze();
		}
	}

	protected static void InvokeRpcRpcPause(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPause called on server.");
		}
		else
		{
			((Cursor)obj).RpcPause();
		}
	}

	protected static void InvokeRpcRpcUnpause(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUnpause called on server.");
		}
		else
		{
			((Cursor)obj).RpcUnpause();
		}
	}

	protected static void InvokeRpcRpcHide(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHide called on server.");
		}
		else
		{
			((Cursor)obj).RpcHide();
		}
	}

	public void CallRpcSetSprites(Character.Animals animal)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetSprites called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetSprites);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)animal);
		SendRPCInternal(networkWriter, 0, "RpcSetSprites");
	}

	public void CallRpcFindLocalPlayer(int localNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcFindLocalPlayer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcFindLocalPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendRPCInternal(networkWriter, 0, "RpcFindLocalPlayer");
	}

	public void CallRpcGetLocalController(int localNumber)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcGetLocalController called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcGetLocalController);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)localNumber);
		SendRPCInternal(networkWriter, 0, "RpcGetLocalController");
	}

	public void CallRpcEnable()
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
		SendRPCInternal(networkWriter, 0, "RpcEnable");
	}

	public void CallRpcDisable(bool sound, bool showNotebookSprite)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDisable called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDisable);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(sound);
		networkWriter.Write(showNotebookSprite);
		SendRPCInternal(networkWriter, 0, "RpcDisable");
	}

	public void CallRpcFreeze()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcFreeze called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcFreeze);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcFreeze");
	}

	public void CallRpcUnfreeze()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUnfreeze called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUnfreeze);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcUnfreeze");
	}

	public void CallRpcPause()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPause called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPause);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcPause");
	}

	public void CallRpcUnpause()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUnpause called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUnpause);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcUnpause");
	}

	public void CallRpcHide()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcHide called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcHide);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcHide");
	}

	static Cursor()
	{
		kCmdCmdSetSprites = -162728222;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdSetSprites, InvokeCmdCmdSetSprites);
		kCmdCmdSetLocalPlayerID = 653982171;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdSetLocalPlayerID, InvokeCmdCmdSetLocalPlayerID);
		kCmdCmdGetLocalController = -966440825;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdGetLocalController, InvokeCmdCmdGetLocalController);
		kCmdCmdSetWaitingForInventory = 1095647252;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdSetWaitingForInventory, InvokeCmdCmdSetWaitingForInventory);
		kCmdCmdEnable = -984786983;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdEnable, InvokeCmdCmdEnable);
		kCmdCmdDisable = -1477690542;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdDisable, InvokeCmdCmdDisable);
		kCmdCmdFreeze = -952341267;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdFreeze, InvokeCmdCmdFreeze);
		kCmdCmdUnfreeze = 785393478;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdUnfreeze, InvokeCmdCmdUnfreeze);
		kCmdCmdPause = 1363497184;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdPause, InvokeCmdCmdPause);
		kCmdCmdUnpause = 865363815;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdUnpause, InvokeCmdCmdUnpause);
		kCmdCmdHide = 182299928;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Cursor), kCmdCmdHide, InvokeCmdCmdHide);
		kRpcRpcSetSprites = 1996015416;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcSetSprites, InvokeRpcRpcSetSprites);
		kRpcRpcFindLocalPlayer = 996344455;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcFindLocalPlayer, InvokeRpcRpcFindLocalPlayer);
		kRpcRpcGetLocalController = -717331747;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcGetLocalController, InvokeRpcRpcGetLocalController);
		kRpcRpcEnable = -202718417;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcEnable, InvokeRpcRpcEnable);
		kRpcRpcDisable = 1291598524;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcDisable, InvokeRpcRpcDisable);
		kRpcRpcFreeze = -170272701;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcFreeze, InvokeRpcRpcFreeze);
		kRpcRpcUnfreeze = 734008604;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcUnfreeze, InvokeRpcRpcUnfreeze);
		kRpcRpcPause = 1111630538;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcPause, InvokeRpcRpcPause);
		kRpcRpcUnpause = -660314415;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcUnpause, InvokeRpcRpcUnpause);
		kRpcRpcHide = 728364526;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Cursor), kRpcRpcHide, InvokeRpcRpcHide);
		NetworkCRC.RegisterBehaviour("Cursor", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)networkNumber);
			writer.WritePackedUInt32((uint)localNumber);
			writer.Write(FindPlayerOnSpawn);
			writer.Write(FindControllerOnSpawn);
			writer.Write(WaitingForInventory);
			writer.Write(disabled);
			writer.Write(paused);
			writer.Write(scoreboard);
			writer.Write(frozen);
			writer.Write(cursorColor);
			writer.Write((int)cursorAnimal);
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
			writer.WritePackedUInt32((uint)networkNumber);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)localNumber);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(FindPlayerOnSpawn);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(FindControllerOnSpawn);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(WaitingForInventory);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(disabled);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(paused);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(scoreboard);
		}
		if ((base.syncVarDirtyBits & 0x100) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(frozen);
		}
		if ((base.syncVarDirtyBits & 0x200) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(cursorColor);
		}
		if ((base.syncVarDirtyBits & 0x400) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)cursorAnimal);
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
			networkNumber = (int)reader.ReadPackedUInt32();
			localNumber = (int)reader.ReadPackedUInt32();
			FindPlayerOnSpawn = reader.ReadBoolean();
			FindControllerOnSpawn = reader.ReadBoolean();
			WaitingForInventory = reader.ReadBoolean();
			disabled = reader.ReadBoolean();
			paused = reader.ReadBoolean();
			scoreboard = reader.ReadBoolean();
			frozen = reader.ReadBoolean();
			cursorColor = reader.ReadColor();
			cursorAnimal = (Character.Animals)reader.ReadInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			networkNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			localNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			FindPlayerOnSpawn = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			FindControllerOnSpawn = reader.ReadBoolean();
		}
		if ((num & 0x10) != 0)
		{
			WaitingForInventory = reader.ReadBoolean();
		}
		if ((num & 0x20) != 0)
		{
			disabled = reader.ReadBoolean();
		}
		if ((num & 0x40) != 0)
		{
			paused = reader.ReadBoolean();
		}
		if ((num & 0x80) != 0)
		{
			scoreboard = reader.ReadBoolean();
		}
		if ((num & 0x100) != 0)
		{
			frozen = reader.ReadBoolean();
		}
		if ((num & 0x200) != 0)
		{
			cursorColor = reader.ReadColor();
		}
		if ((num & 0x400) != 0)
		{
			cursorAnimal = (Character.Animals)reader.ReadInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
