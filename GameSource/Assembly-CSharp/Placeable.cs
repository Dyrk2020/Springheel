using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class Placeable : MonoBehaviour, IGameEventListener
{
	public enum OrientMode
	{
		ROTATE,
		FLIPX,
		FLIPY,
		NONE,
		FLIPXANDY
	}

	public enum PieceCategory
	{
		PLATFORM,
		TRAP,
		MOVINGPLATFORM,
		ATTACHMENT,
		BOMB,
		SPECIAL,
		ITEM
	}

	public enum RotationDirections
	{
		None,
		Clockwise,
		CounterClockwise
	}

	public enum Tag
	{
		None = 0,
		StaticBlock = 1,
		DynamicBlock = 2,
		Hazard = 4,
		ProjectileLauncher = 8,
		Bomb = 0x10,
		Coin = 0x20,
		Glue = 0x40,
		Attachment = 0x80,
		SetPiece = 0x100,
		MovingPlatform = 0x200
	}

	public enum Rarity
	{
		NeverPick = 0,
		Rare = 200,
		Normal = 1000,
		Common = 2000
	}

	private static int idCount = 0;

	[Header("----Placeable----")]
	public int ID;

	public string Name;

	public string KillType;

	public string SFXEventName;

	public string SFXDestruction;

	public string TwitchShortName;

	public PieceCategory Category;

	[BitMask(typeof(Tag))]
	public Tag placeableTag;

	public PickableBlock PickableBlock;

	public Placeable InventoryInstance;

	public Vector2 PlacementOffset = new Vector2(0f, 0f);

	public bool IgnoreOffsetInRotation;

	public bool InvertFlipOffsetHorizonal;

	public string DefaultSortingLayer = "Default";

	public bool DebugShowGroupLinks;

	public int BlockProbabilityUIOrder = 1000;

	[Header("Mass and Difficulty")]
	public Rarity BaseRarity = Rarity.Normal;

	public float UseGluedVariantModifier;

	public float Area = 1f;

	public float Challenge;

	public float Mass;

	public float massInfluenceRatio = 1f;

	public float MaximumMassSpeedRatio = 10f;

	public bool RadiusCalculatedMass;

	public Transform centerOfMass;

	public bool Permanent;

	public bool inDestructible;

	public bool dontChangeArtLayers;

	public bool dontFollowParentLayer;

	public bool alwaysMovingSpriteLayer;

	public bool useDefaultSortingLayerEvenWhenMoving;

	public bool AttachedPieceMovingLayer;

	public bool LimitToArtSprites;

	public Placeable ParentPiece;

	public bool AttachesToParent;

	public Placeable[] FilterOverride = new Placeable[0];

	public bool IgnorePlacementRules;

	public bool IgnoreBounds;

	public Sprite[] createExplosionAnimation;

	protected List<CheckColliding> PlacementCollidersNew = new List<CheckColliding>();

	protected List<CheckColliding> ActiveCollidersNew = new List<CheckColliding>();

	[Header("All Colliders")]
	public List<ColliderModeControl> AllColliderControls;

	public SpriteRenderer[] PlacementGuides;

	public SpriteRenderer[] ArtSprites;

	public SortOrder spriteSortOrder;

	public AttachmentGroup Group;

	public Vector2 OriginalPosition;

	public Quaternion OriginalRotation;

	protected Vector3 originalScale = Vector3.one;

	public GameObject explosionSmokeDebris;

	public bool scaleSmokeDebris = true;

	protected bool smokePoofused;

	public OrientMode Orientation;

	public OrientMode OrientationAlt;

	protected bool markedForDestruction;

	public bool Protected;

	public List<Cursor> HoveredCursors = new List<Cursor>();

	protected bool pickedUp;

	[HideInInspector]
	public Vector3 relativeAttachPosition = new Vector3(0f, 0f, 0f);

	public bool KeepRelativeAttachPosition = true;

	public List<Placeable> ChildPieces = new List<Placeable>();

	protected bool placed;

	protected bool disabled;

	protected PhysicsModifier[] pms;

	public bool PlacementLock;

	protected int bombTints;

	protected Color bombTintColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	public int placedByPlayerNumber;

	public bool noneDefaultColors;

	public Color customSubtractHighlight;

	protected Color[] initialColors;

	public bool IsNetworked;

	public NetworkSurrogate NetSurrogate;

	public bool IsSaveable = true;

	public bool IsSubElement;

	public string originalSubElementName;

	public int placementCost = 1;

	public bool duplicable = true;

	public bool interactableInFreePlayOnly;

	public bool isSetPiece;

	public bool attachableWithGlue = true;

	public bool canSetCustomColor;

	public Color CustomColor;

	public bool damageable;

	public int damageLevel;

	public bool alwaysSaveDamage;

	public int DestroyedDamageLevel = 3;

	public int initialDamage;

	private bool CanPlaceCheckThisFixedUpdate;

	private bool CanPlaceCache;

	public bool ConstrainX;

	public bool ConstrainY;

	public HashSet<Placeable> AttachedBy = new HashSet<Placeable>();

	public bool DontBlockGlueSticky;

	public static List<Placeable> AllPlaceables = new List<Placeable>();

	public static TagComparer.Tag canPlaceAttachmentMask = (TagComparer.Tag)163840;

	public static RaycastHit2D[] raycastResultCache = new RaycastHit2D[128];

	public static LayerMask attachmentColliderMask;

	protected CheckColliding[] ReverseAttachments { get; set; }

	public Vector3 OriginalScale
	{
		get
		{
			return originalScale;
		}
		set
		{
			originalScale = value;
		}
	}

	public bool Placing { get; protected set; }

	public bool InInventory { get; protected set; }

	public bool MarkedForDestruction
	{
		get
		{
			return markedForDestruction;
		}
		protected set
		{
			markedForDestruction = value;
		}
	}

	public bool PickedUp
	{
		get
		{
			return pickedUp;
		}
		set
		{
			setPickedUp(value);
		}
	}

	public bool Disabled => disabled;

	public bool Placed => placed;

	public bool Enabled => !disabled;

	public bool HasReverseAttachments { get; protected set; }

	public virtual RotationDirections RotationDirection
	{
		get
		{
			return RotationDirections.None;
		}
		set
		{
		}
	}

	public bool InteractableInCurrentMode
	{
		get
		{
			if (interactableInFreePlayOnly && GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsMobileBlock
	{
		get
		{
			PhysicsModifier[] physicsModifier = GetPhysicsModifier();
			for (int i = 0; i != physicsModifier.Length; i++)
			{
				PhysicsModifier physicsModifier2 = physicsModifier[i];
				if (physicsModifier2 != null && (physicsModifier2.ModifierType == PhysicsModifier.ModType.BaseMotion || physicsModifier2.ModifierType == PhysicsModifier.ModType.Rotation || physicsModifier2.ModifierType == PhysicsModifier.ModType.Treadmill))
				{
					return true;
				}
			}
			return false;
		}
	}

	public string UsefulName => "";

	protected virtual void Awake()
	{
		AllPlaceables.Add(this);
		relativeAttachPosition = base.transform.localPosition;
		if (ID == 0)
		{
			idCount += 2;
			ID = idCount;
		}
		else if (ID > idCount)
		{
			idCount = ID;
		}
		spriteSortOrder = new SortOrder(base.gameObject, LimitToArtSprites);
		foreach (ColliderModeControl allColliderControl in AllColliderControls)
		{
			CheckColliding component = allColliderControl.GetComponent<CheckColliding>();
			if (component != null)
			{
				if (component.PlacementPhase)
				{
					PlacementCollidersNew.Add(component);
				}
				if (component.RunPhase)
				{
					ActiveCollidersNew.Add(component);
				}
			}
		}
		int num = 0;
		foreach (CheckColliding item in PlacementCollidersNew)
		{
			if (item.ReverseAttach)
			{
				HasReverseAttachments = true;
				num++;
			}
		}
		ReverseAttachments = new CheckColliding[num];
		num = 0;
		foreach (CheckColliding item2 in PlacementCollidersNew)
		{
			if (item2.ReverseAttach)
			{
				ReverseAttachments[num] = item2;
				num++;
			}
		}
		if (noneDefaultColors)
		{
			initialColors = new Color[ArtSprites.Length];
			for (int i = 0; i < ArtSprites.Length; i++)
			{
				initialColors[i] = ArtSprites[i].color;
			}
		}
		ChangeListener(adding: true);
	}

	protected virtual void Start()
	{
	}

	public virtual void OnDestroy()
	{
		ChangeListener(adding: false);
		AllPlaceables.Remove(this);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<LevelResetEvent>(this, adding);
	}

	protected virtual void FixedUpdate()
	{
		if (CanPlaceCheckThisFixedUpdate)
		{
			CanPlaceCheckThisFixedUpdate = false;
		}
		if (!disabled && KeepRelativeAttachPosition && (bool)ParentPiece && !ParentPiece.MarkedForDestruction && (bool)base.transform.parent && Mathf.Abs((relativeAttachPosition - base.transform.localPosition).sqrMagnitude) > 0.001f)
		{
			Debug.Log(base.name.ToString() + " is getting it's relativeAttachPositionReset during Fixed Update.  Why?");
			base.transform.localPosition = relativeAttachPosition;
		}
	}

	public void SwitchColliderTo(ColliderModeEnum mode)
	{
		foreach (ColliderModeControl allColliderControl in AllColliderControls)
		{
			allColliderControl.SwitchToMode(mode);
		}
	}

	public void AddBombTint(Color c)
	{
		bombTints++;
		bombTintColor = c;
	}

	public void RemoveBombTint()
	{
		bombTints--;
		if (bombTints < 0)
		{
			bombTints = 0;
		}
	}

	public virtual void Tint()
	{
		if (GameSettings.GetInstance() == null)
		{
			return;
		}
		if (bombTints > 0)
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = bombTintColor;
			}
		}
		else if (pickedUp || (ParentPiece != null && ParentPiece.PickedUp))
		{
			if (CanPlace())
			{
				if (noneDefaultColors)
				{
					for (int j = 0; j < ArtSprites.Length; j++)
					{
						ArtSprites[j].color = initialColors[j];
					}
					return;
				}
				SpriteRenderer[] artSprites = ArtSprites;
				for (int i = 0; i < artSprites.Length; i++)
				{
					artSprites[i].color = GameSettings.GetInstance().neutralColor;
				}
			}
			else
			{
				SpriteRenderer[] artSprites = ArtSprites;
				for (int i = 0; i < artSprites.Length; i++)
				{
					artSprites[i].color = GameSettings.GetInstance().negativeColor;
				}
			}
		}
		else if (noneDefaultColors)
		{
			if (HoveredCursors.Count == 1)
			{
				for (int k = 0; k < ArtSprites.Length; k++)
				{
					ArtSprites[k].color = GameSettings.GetInstance().highlightColor - customSubtractHighlight;
				}
			}
			else if (HoveredCursors.Count == 2)
			{
				for (int l = 0; l < ArtSprites.Length; l++)
				{
					ArtSprites[l].color = GameSettings.GetInstance().highlightColor2 - customSubtractHighlight;
				}
			}
			else if (HoveredCursors.Count == 3)
			{
				for (int m = 0; m < ArtSprites.Length; m++)
				{
					ArtSprites[m].color = GameSettings.GetInstance().highlightColor3 - customSubtractHighlight;
				}
			}
			else if (HoveredCursors.Count > 3)
			{
				for (int n = 0; n < ArtSprites.Length; n++)
				{
					ArtSprites[n].color = GameSettings.GetInstance().highlightColor4 - customSubtractHighlight;
				}
			}
			else
			{
				for (int num = 0; num < ArtSprites.Length; num++)
				{
					ArtSprites[num].color = initialColors[num];
				}
			}
		}
		else if (HoveredCursors.Count == 1)
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = GameSettings.GetInstance().highlightColor;
			}
		}
		else if (HoveredCursors.Count == 2)
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = GameSettings.GetInstance().highlightColor2;
			}
		}
		else if (HoveredCursors.Count == 3)
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = GameSettings.GetInstance().highlightColor3;
			}
		}
		else if (HoveredCursors.Count > 3)
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = GameSettings.GetInstance().highlightColor4;
			}
		}
		else
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = GameSettings.GetInstance().neutralColor;
			}
		}
	}

	public virtual void Disable()
	{
		disabled = true;
		placed = false;
		Placing = false;
		HoveredCursors.Clear();
		InInventory = false;
		SwitchColliderTo(ColliderModeEnum.NoColliders);
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Vector3 localScale = base.transform.localScale;
		localScale.x /= Mathf.Abs(localScale.x);
		localScale.y /= Mathf.Abs(localScale.y);
		localScale.z /= Mathf.Abs(localScale.z);
		base.transform.localScale = localScale;
	}

	public virtual void Enable()
	{
		disabled = false;
		Placing = false;
		HoveredCursors.Clear();
		InInventory = false;
		SwitchColliderTo(ColliderModeEnum.RunPhase);
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer in artSprites)
		{
			if (!(spriteRenderer == null))
			{
				if (!dontChangeArtLayers)
				{
					SpriteAssignmentRule(spriteRenderer);
				}
				spriteRenderer.enabled = true;
			}
		}
		artSprites = PlacementGuides;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = false;
		}
		Vector3 localScale = base.transform.localScale;
		localScale.x /= Mathf.Abs(localScale.x);
		localScale.y /= Mathf.Abs(localScale.y);
		localScale.z /= Mathf.Abs(localScale.z);
		base.transform.localScale = localScale;
		int num = 0;
		foreach (Placeable childPiece in ChildPieces)
		{
			num++;
			if (childPiece != null)
			{
				if (!childPiece.pickedUp)
				{
					childPiece.Enable();
				}
				if (!childPiece.dontFollowParentLayer)
				{
					childPiece.spriteSortOrder.setSortOrder(spriteSortOrder.currentBaseOrder + num);
				}
			}
		}
	}

	public virtual void SpriteAssignmentRule(SpriteRenderer sr)
	{
		if (sr == null)
		{
			return;
		}
		if (alwaysMovingSpriteLayer)
		{
			sr.sortingLayerName = "MovingBlocks";
			return;
		}
		sr.sortingLayerName = DefaultSortingLayer;
		if (!(ParentPiece != null))
		{
			return;
		}
		if (useDefaultSortingLayerEvenWhenMoving)
		{
			sr.sortingLayerName = DefaultSortingLayer;
			return;
		}
		PhysicsModifier[] physicsModifiers = ParentPiece.GetPhysicsModifiers();
		for (int i = 0; i != physicsModifiers.Length; i++)
		{
			PhysicsModifier physicsModifier = physicsModifiers[i];
			if (physicsModifier != null && (physicsModifier.ModifierType == PhysicsModifier.ModType.BaseMotion || physicsModifier.ModifierType == PhysicsModifier.ModType.Rotation || physicsModifier.ModifierType == PhysicsModifier.ModType.Treadmill))
			{
				sr.sortingLayerName = "MovingBlocks";
				break;
			}
		}
		if (ParentPiece.AttachedPieceMovingLayer)
		{
			sr.sortingLayerName = "MovingBlocks";
		}
	}

	public virtual void EnablePlacement(bool showGuides = true)
	{
		disabled = false;
		Placing = showGuides;
		InInventory = false;
		SwitchColliderTo(ColliderModeEnum.PlacementPhase);
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer in artSprites)
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.enabled = true;
			}
		}
		artSprites = PlacementGuides;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = showGuides;
		}
		Vector3 localScale = base.transform.localScale;
		localScale.x /= Mathf.Abs(localScale.x);
		localScale.y /= Mathf.Abs(localScale.y);
		localScale.z /= Mathf.Abs(localScale.z);
		base.transform.localScale = localScale;
		foreach (Placeable childPiece in ChildPieces)
		{
			if (childPiece != null)
			{
				childPiece.EnablePlacement(showGuides: false);
			}
		}
	}

	public virtual void EnablePlaced()
	{
		SwitchColliderTo(ColliderModeEnum.PlacedPhase);
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer in artSprites)
		{
			if (!(spriteRenderer == null))
			{
				if (!dontChangeArtLayers)
				{
					SpriteAssignmentRule(spriteRenderer);
				}
				spriteRenderer.enabled = true;
			}
		}
		foreach (Placeable childPiece in ChildPieces)
		{
			if (childPiece != null)
			{
				childPiece.EnablePlaced();
			}
		}
		artSprites = PlacementGuides;
		for (int i = 0; i < artSprites.Length; i++)
		{
			artSprites[i].enabled = false;
		}
	}

	public void UpdateSortOrder()
	{
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer in artSprites)
		{
			if (!(spriteRenderer == null) && !dontChangeArtLayers)
			{
				SpriteAssignmentRule(spriteRenderer);
			}
		}
		int num = 0;
		foreach (Placeable childPiece in ChildPieces)
		{
			num++;
			if (childPiece != null && !childPiece.dontFollowParentLayer)
			{
				childPiece.spriteSortOrder.setSortOrder(spriteSortOrder.currentBaseOrder + num);
			}
		}
	}

	public virtual void PickUp()
	{
		if (!PickedUp)
		{
			PickedUp = true;
		}
		SwitchColliderTo(ColliderModeEnum.PlacementPhase);
		if (ParentPiece != null)
		{
			foreach (CheckColliding item in PlacementCollidersNew)
			{
				if (item.Required)
				{
					item.ignorePickedUp = false;
				}
			}
		}
		else
		{
			foreach (CheckColliding item2 in PlacementCollidersNew)
			{
				if (item2.Required)
				{
					item2.ignorePickedUp = true;
				}
			}
		}
		Placeable[] array = ChildPieces.ToArray();
		foreach (Placeable placeable in array)
		{
			if (placeable != null)
			{
				placeable.PickUp();
			}
		}
	}

	public virtual void Place(int playerNumber)
	{
		Place(playerNumber, sendEvent: true);
	}

	public virtual void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		placedByPlayerNumber = playerNumber;
		PickedUp = false;
		placed = true;
		float z = base.transform.eulerAngles.z;
		z = Mathf.Round(z / 90f) * 90f;
		base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		OriginalPosition = base.transform.position;
		OriginalRotation = base.transform.rotation;
		OriginalScale = base.transform.localScale;
		SpriteRenderer[] placementGuides = PlacementGuides;
		for (int i = 0; i < placementGuides.Length; i++)
		{
			placementGuides[i].enabled = false;
		}
		Debug.Log("Placing " + UsefulName);
		bool flag = false;
		foreach (CheckColliding item in PlacementCollidersNew)
		{
			if (!item.Required || item.ReverseAttach)
			{
				continue;
			}
			GameObject collidingObject = item.CollidingObject;
			if (ParentPiece != null)
			{
				collidingObject = ParentPiece.gameObject;
			}
			if (!(collidingObject != null))
			{
				continue;
			}
			Placeable component = collidingObject.GetComponent<Placeable>();
			if (!(component != null) || component.pickedUp || (isSetPiece && !component.isSetPiece))
			{
				continue;
			}
			if (component.Group == null)
			{
				if (Group == null)
				{
					new AttachmentGroup(component, this);
				}
				else
				{
					Group.AddLink(this, component);
				}
			}
			else if (Group == null)
			{
				component.Group.AddLink(component, this);
			}
			else
			{
				component.Group.AddLink(component, this);
				AttachmentGroup.MergeGroups(Group, component.Group);
			}
			if (AttachesToParent && Group != null && Group == component.Group)
			{
				component.AttachedBy.Add(this);
			}
			if (base.transform.parent != component.transform && !flag)
			{
				component.AttachPiece(this);
				flag = true;
			}
		}
		SwitchColliderTo(ColliderModeEnum.PlacedPhase);
		if (sendEvent)
		{
			GameEventManager.SendEvent(new PiecePlacedEvent(playerNumber, this));
		}
		if (!dontChangeArtLayers && !dontFollowParentLayer)
		{
			spriteSortOrder.setSortOrder(GameState.GetInstance().CurrentSortOrderNumber);
		}
		HashSet<Placeable> hashSet = null;
		Placeable[] array = ChildPieces.ToArray();
		foreach (Placeable placeable in array)
		{
			if (!(placeable != null))
			{
				continue;
			}
			placeable.Place(playerNumber, sendEvent: false, force: true);
			if (placeable.AttachesToParent)
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<Placeable>();
				}
				hashSet.Add(placeable);
			}
		}
		if (hashSet == null || hashSet.Count <= 0)
		{
			return;
		}
		foreach (Placeable item2 in hashSet)
		{
			if (Group == null)
			{
				new AttachmentGroup(this, item2);
			}
			else if (item2.Group != Group)
			{
				Group.AddLink(this, item2);
			}
		}
	}

	public virtual void ResetTransform(int playerNumber)
	{
		base.transform.position = OriginalPosition;
		base.transform.rotation = OriginalRotation;
		base.transform.localScale = OriginalScale;
	}

	public virtual bool CanPlace()
	{
		if (!CanPlaceCheckThisFixedUpdate)
		{
			CanPlaceCache = CanPlaceCheck();
			CanPlaceCheckThisFixedUpdate = true;
		}
		return CanPlaceCache;
	}

	public virtual bool CanPlaceCheck()
	{
		if (IgnorePlacementRules)
		{
			return true;
		}
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		bool flag = false;
		bool flag2 = false;
		foreach (CheckColliding item in PlacementCollidersNew)
		{
			if (item == null)
			{
				Debug.LogWarning("Collider for piece is NULL. It might be placeable when it should not be!");
				return true;
			}
			if (item.Enabled)
			{
				flag2 = true;
			}
			if (item.InBounds || IgnoreBounds)
			{
				flag = true;
			}
			bool flag3 = false;
			if (item.CollidingObject != null)
			{
				flag3 = item.CollidingObject.transform.IsChildOf(base.transform) || base.transform.IsChildOf(item.CollidingObject.transform);
			}
			CollisionTag collisionTag = null;
			if (item.CollidingObject != null)
			{
				collisionTag = item.CollidingObject.GetComponent<CollisionTag>();
				if (collisionTag == null)
				{
					Debug.LogError("Error, this object does not have a collision tag:" + item.CollidingObject.name);
				}
			}
			if (item.Colliding)
			{
				if (!item.Required && !item.AntiRequired)
				{
					if (flag3)
					{
						return false;
					}
					if (!(collisionTag != null) || !collisionTag.ContainsAnyTag(canPlaceAttachmentMask))
					{
						return false;
					}
				}
			}
			else if (item.Required && !item.ReverseAttach)
			{
				return false;
			}
			if (item.AntiRequired && item.CollidingObject != null)
			{
				list2.Add(item.CollidingObject);
			}
			else if (item.Required && item.CollidingObject != null)
			{
				list.Add(item.CollidingObject);
			}
		}
		foreach (GameObject item2 in list)
		{
			if (list2.Contains(item2))
			{
				return false;
			}
			if (isSetPiece)
			{
				Placeable component = item2.GetComponent<Placeable>();
				if (component != null && !component.isSetPiece)
				{
					return false;
				}
			}
		}
		foreach (Placeable childPiece in ChildPieces)
		{
			if (childPiece != null && !childPiece.CanPlace())
			{
				return false;
			}
		}
		if (PlacementCollidersNew.Count > 0 && flag2 && !flag)
		{
			return false;
		}
		return true;
	}

	public Placeable GetTopPiece()
	{
		if (ParentPiece == null)
		{
			return this;
		}
		return ParentPiece.GetTopPiece();
	}

	public virtual void Flip(bool sprintDown)
	{
	}

	public virtual float GetTotalMass()
	{
		float num = Mass;
		foreach (Placeable childPiece in ChildPieces)
		{
			if (childPiece != null)
			{
				num += childPiece.GetTotalMass();
			}
		}
		return num;
	}

	public virtual PhysicsModifier[] GetPhysicsModifiers()
	{
		if (ParentPiece != null)
		{
			PhysicsModifier[] physicsModifiers = ParentPiece.GetPhysicsModifiers();
			int num = 0;
			for (int i = 0; i != physicsModifiers.Length; i++)
			{
				if (physicsModifiers[i].ModifierType == PhysicsModifier.ModType.BaseMotion || physicsModifiers[i].ModifierType == PhysicsModifier.ModType.Rotation || physicsModifiers[i].ModifierType == PhysicsModifier.ModType.Treadmill)
				{
					num++;
				}
			}
			if (num == 0)
			{
				return new PhysicsModifier[0];
			}
			PhysicsModifier[] array = new PhysicsModifier[num];
			num = 0;
			for (int j = 0; j != physicsModifiers.Length; j++)
			{
				if (physicsModifiers[j].ModifierType == PhysicsModifier.ModType.BaseMotion || physicsModifiers[j].ModifierType == PhysicsModifier.ModType.Rotation || physicsModifiers[j].ModifierType == PhysicsModifier.ModType.Treadmill)
				{
					array[num] = physicsModifiers[j];
					num++;
				}
			}
			return array;
		}
		return new PhysicsModifier[0];
	}

	public virtual PhysicsModifier[] GetPhysicsModifier()
	{
		return new PhysicsModifier[0];
	}

	public Placeable[] GetPiecesAtAttachPoints()
	{
		if (!HasReverseAttachments)
		{
			return new Placeable[0];
		}
		List<Placeable> list = new List<Placeable>();
		CheckColliding[] reverseAttachments = ReverseAttachments;
		foreach (CheckColliding checkColliding in reverseAttachments)
		{
			if (checkColliding.CollidingObject != null)
			{
				Placeable component = checkColliding.CollidingObject.GetComponent<Placeable>();
				if (component != null && component.Placed)
				{
					list.Add(checkColliding.CollidingObject.GetComponent<Placeable>());
				}
				continue;
			}
			BoxCollider2D component2 = checkColliding.GetComponent<BoxCollider2D>();
			Physics2D.queriesStartInColliders = true;
			Physics2D.queriesHitTriggers = true;
			int num = Physics2D.BoxCastNonAlloc(component2.transform.TransformPoint(component2.offset), angle: component2.transform.rotation.eulerAngles.z, size: component2.size, direction: Vector2.zero, results: raycastResultCache, distance: 0f, layerMask: attachmentColliderMask);
			for (int j = 0; j != num; j++)
			{
				RaycastHit2D raycastHit2D = raycastResultCache[j];
				if (checkColliding.CheckCollidingObject(raycastHit2D.collider.gameObject))
				{
					Placeable componentInParent = raycastHit2D.transform.GetComponentInParent<Placeable>();
					if (componentInParent != null && componentInParent != this && checkColliding.CheckCollidingObject(componentInParent.gameObject))
					{
						list.Add(componentInParent);
					}
				}
			}
		}
		return list.ToArray();
	}

	public void AttachPiece(Placeable p)
	{
		if (this == p)
		{
			return;
		}
		if (p.ParentPiece != null && p.Group != p.ParentPiece.Group)
		{
			p.ParentPiece.DetachPiece(p, removeFromGroup: true);
		}
		if (!ChildPieces.Contains(p))
		{
			ChildPieces.Add(p);
		}
		if (!p.dontChangeArtLayers)
		{
			p.spriteSortOrder.setSortOrder(spriteSortOrder.currentBaseOrder);
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer sr in componentsInChildren)
			{
				if (!dontChangeArtLayers)
				{
					SpriteAssignmentRule(sr);
				}
			}
		}
		p.ParentPiece = this;
		p.transform.parent = base.transform;
		IgnoreCollisionsUpward(p, ignore: true);
		p.relativeAttachPosition = p.transform.localPosition;
		p.OriginalPosition = p.transform.position;
	}

	public void DetachPiece(Placeable p, bool removeFromGroup)
	{
		if (p.Group != null && removeFromGroup)
		{
			p.Group.RemovePlaceable(p);
			p.Group = null;
		}
		if (ChildPieces.Contains(p))
		{
			ChildPieces.Remove(p);
		}
		if (p.ParentPiece == this)
		{
			p.ParentPiece = null;
			p.transform.parent = null;
			p.relativeAttachPosition = new Vector3(0f, 0f, 0f);
		}
		IgnoreCollisionsUpward(p, ignore: false);
		if (Group != null && Group.links.Count == 0)
		{
			Group = null;
		}
	}

	public virtual void DetachAllChildren(bool keepAttachments = false)
	{
		Placeable[] array = ChildPieces.ToArray();
		foreach (Placeable placeable in array)
		{
			if (placeable != null && (!keepAttachments || !placeable.AttachesToParent))
			{
				DetachPiece(placeable, removeFromGroup: true);
			}
		}
	}

	public void IgnoreCollisionsUpward(Placeable p, bool ignore)
	{
		foreach (CheckColliding item in ActiveCollidersNew)
		{
			foreach (CheckColliding item2 in p.ActiveCollidersNew)
			{
				item.SetIgnoreCollision(item2, ignore);
			}
		}
		if (ParentPiece != null)
		{
			ParentPiece.IgnoreCollisionsUpward(p, ignore);
		}
	}

	public void DestroySelfIn(bool destroyChildren = false, bool useSmoke = true, float time = 0f)
	{
		StartCoroutine(CoroutineDestroySelf(destroyChildren, useSmoke, time));
	}

	private IEnumerator CoroutineDestroySelf(bool destroyChildren = false, bool useSmoke = true, float time = 0f)
	{
		yield return new WaitForSeconds(time);
		DestroySelf(destroyChildren, useSmoke);
	}

	public void CreateSmokePoof()
	{
		if (smokePoofused)
		{
			return;
		}
		smokePoofused = true;
		Vector3 vector = base.transform.position;
		Vector3 vector2 = base.transform.position;
		if (scaleSmokeDebris)
		{
			foreach (ColliderModeControl allColliderControl in AllColliderControls)
			{
				Collider2D component = allColliderControl.GetComponent<Collider2D>();
				if (component != null)
				{
					Bounds bounds = component.bounds;
					Vector3 min = bounds.min;
					Vector3 max = bounds.max;
					vector = Vector3.Min(vector, min);
					vector2 = Vector3.Max(vector2, max);
				}
			}
			float num = (vector2 - vector).magnitude / 2f;
			if (num > 0.1f && explosionSmokeDebris != null)
			{
				UnityEngine.Object.Instantiate(explosionSmokeDebris, base.transform.position, base.transform.rotation).transform.localScale = num * base.transform.localScale;
			}
		}
		else if (explosionSmokeDebris != null)
		{
			UnityEngine.Object.Instantiate(explosionSmokeDebris, base.transform.position, base.transform.rotation).transform.localScale = base.transform.localScale;
		}
	}

	public virtual void DestroySelf(bool destroyChildren = false, bool useSmoke = true, bool sendNetworkSignal = true)
	{
		if (!destroyChildren)
		{
			DetachAllChildren(keepAttachments: true);
			foreach (Placeable childPiece in ChildPieces)
			{
				if (childPiece != null && childPiece.AttachesToParent)
				{
					childPiece.CreateSmokePoof();
					childPiece.Disable();
					childPiece.MarkedForDestruction = true;
				}
			}
		}
		if (ParentPiece != null)
		{
			ParentPiece.DetachPiece(this, removeFromGroup: true);
		}
		if (MarkedForDestruction || this == null)
		{
			return;
		}
		if (useSmoke)
		{
			CreateSmokePoof();
		}
		if (!Placed)
		{
			MarkedForDestruction = true;
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			Disable();
			MarkedForDestruction = true;
		}
		if (!SFXDestruction.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(SFXDestruction, base.gameObject);
		}
		if (sendNetworkSignal)
		{
			MsgPieceDestroyed msgPieceDestroyed = new MsgPieceDestroyed();
			msgPieceDestroyed.BlockID = ID;
			msgPieceDestroyed.SceneLoadNumber = LobbyManagerManager.Instance.SceneLoadCounter;
			msgPieceDestroyed.MachineNetworkNumber = LobbyManager.instance.ALocalNetworkNumber();
			if (LobbyManager.instance.client != null)
			{
				LobbyManager.instance.client.Send(NetMsgTypes.PieceDestroyed, msgPieceDestroyed);
			}
		}
	}

	protected virtual void setPickedUp(bool value)
	{
		pickedUp = value;
	}

	public virtual void ToPlayMode()
	{
		SwitchColliderTo(ColliderModeEnum.RunPhase);
		StartCoroutine(EnableNextFrame());
	}

	private IEnumerator EnableNextFrame()
	{
		yield return null;
		Enable();
	}

	protected virtual void ToPlaceMode(bool enableSelection)
	{
		if (!MarkedForDestruction)
		{
			EnablePlaced();
		}
	}

	protected virtual void ToSuddenDeath()
	{
	}

	protected virtual void EndPlay()
	{
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			if (placed)
			{
				if (startPhaseEvent.Phase == GameControl.GamePhase.PLAY)
				{
					ToPlayMode();
				}
				if (startPhaseEvent.Phase == GameControl.GamePhase.PLACE)
				{
					ToPlaceMode(GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY);
				}
				if (startPhaseEvent.Phase == GameControl.GamePhase.SUDDENDEATH)
				{
					ToSuddenDeath();
				}
			}
		}
		if (type == typeof(EndPhaseEvent))
		{
			EndPhaseEvent endPhaseEvent = e as EndPhaseEvent;
			if (placed && endPhaseEvent.Phase == GameControl.GamePhase.PLAY)
			{
				EndPlay();
			}
		}
	}

	public void GenerateIDOnPick(int sequenceID, int playerNetworkNumber)
	{
		int num = (ID = 10 * (sequenceID + playerNetworkNumber * 100000));
		int num2 = 0;
		Placeable[] componentsInChildren = GetComponentsInChildren<Placeable>();
		foreach (Placeable placeable in componentsInChildren)
		{
			if (placeable != this)
			{
				placeable.ID = num + ++num2;
			}
		}
	}

	public void UpdateChildIDs()
	{
		int num = 10 * (ID / 10);
		int num2 = 0;
		Placeable[] componentsInChildren = GetComponentsInChildren<Placeable>();
		foreach (Placeable placeable in componentsInChildren)
		{
			if (placeable != this)
			{
				placeable.ID = num + ++num2;
			}
		}
	}

	public int GetOriginalSequenceID()
	{
		if (ID < 1000000)
		{
			return ID;
		}
		int num = ID / 10;
		int num2 = num - num % 100000;
		return num - num2;
	}

	public static void SetInitialSequenceID(int id)
	{
		idCount = id;
		Debug.Log("Placeable: Initial sequence ID set to " + id);
	}

	public void ApplyTransformFromMessage(MsgBookPiecePicked pieceMsg)
	{
		if (pieceMsg.SetTransform)
		{
			base.transform.position = pieceMsg.PiecePosition;
			base.transform.localRotation = pieceMsg.PieceRotation;
			base.transform.localScale = pieceMsg.PieceScale;
			RotationDirection = pieceMsg.PieceRotationDirection;
		}
	}

	public virtual void SetColor(Color newColor)
	{
		if (canSetCustomColor)
		{
			CustomColor = newColor;
			for (int i = 0; i < initialColors.Length; i++)
			{
				initialColors[i] = newColor;
			}
		}
		Tint();
	}

	public IEnumerable<Placeable> GetLinkedSetPieces()
	{
		if (Group != null && isSetPiece)
		{
			yield return this;
		}
	}

	public virtual bool RequiredColliderConditionsSatisfied()
	{
		return true;
	}

	public float calculateMassRatio()
	{
		if (Mass == 0f)
		{
			return 1f;
		}
		float num = 0f;
		if (RadiusCalculatedMass)
		{
			Vector3 position = base.transform.position;
			float radiusModifier = GameSettings.GetInstance().RadiusModifier;
			foreach (Placeable childPiece in ChildPieces)
			{
				if (!(childPiece == null))
				{
					Vector3 vector = ((childPiece.GetComponent<Coin>() != null) ? ((Vector3)childPiece.OriginalPosition) : ((!(childPiece.centerOfMass != null)) ? childPiece.transform.position : childPiece.centerOfMass.position));
					float sqrMagnitude = (vector - position).sqrMagnitude;
					num += childPiece.Mass * ((sqrMagnitude - 1f) * radiusModifier + 1f);
				}
			}
		}
		else
		{
			num = Mathf.Max(0f, GetTotalMass() - Mass);
		}
		num *= massInfluenceRatio;
		float maximumMassSpeedRatio = MaximumMassSpeedRatio;
		return Mathf.Clamp((num + Mass) / Mass, 1f, maximumMassSpeedRatio);
	}

	public virtual IEnumerable<Placeable> GetChildrenAndParts()
	{
		Placeable[] array = ChildPieces.ToArray();
		foreach (Placeable placeable in array)
		{
			if (placeable != null)
			{
				yield return placeable;
			}
		}
	}

	public void DetachChildrenForPickup(bool detachFromParent)
	{
		List<Placeable> list = new List<Placeable>();
		foreach (Placeable childrenAndPart in GetChildrenAndParts())
		{
			if (childrenAndPart.AttachesToParent)
			{
				Vector3 vector = childrenAndPart.relativeAttachPosition;
				if (Group != null && childrenAndPart.Group == Group)
				{
					Group.RemovePlaceable(childrenAndPart);
				}
				childrenAndPart.Group = null;
				childrenAndPart.relativeAttachPosition = vector;
				list.Add(childrenAndPart);
			}
			else
			{
				MultipiecePart multipiecePart = childrenAndPart as MultipiecePart;
				if (multipiecePart != null && multipiecePart.MainBlock == this && multipiecePart.Group != null)
				{
					multipiecePart.DetachChildrenForPickup(detachFromParent: false);
				}
			}
		}
		PickedUp = true;
		if (Group != null)
		{
			Group.RemovePlaceable(this);
			Group = null;
		}
		if (detachFromParent && ParentPiece != null)
		{
			ParentPiece.DetachPiece(this, removeFromGroup: true);
		}
		foreach (Placeable item in list)
		{
			if (item.ChildPieces.Count > 0)
			{
				item.DetachAllChildren();
			}
			if (!ChildPieces.Contains(item))
			{
				ChildPieces.Add(item);
			}
			item.ParentPiece = this;
			item.transform.SetParent(base.transform, worldPositionStays: true);
		}
	}

	public virtual void SetInitialDamageLevel(int damage, bool allowDamageReset)
	{
		damageLevel = damage;
	}

	public virtual void AddBombDamage(int damageAmount)
	{
		damageLevel += damageAmount;
	}

	public Vector3 GetTransformedPlacementOffset()
	{
		Vector3 vector = PlacementOffset;
		if (base.transform.localScale.x < 0f && !InvertFlipOffsetHorizonal)
		{
			vector.x = 0f - vector.x;
		}
		if (base.transform.localScale.y < 0f)
		{
			vector.y = 0f - vector.y;
		}
		if (IgnoreOffsetInRotation)
		{
			return vector;
		}
		return base.transform.rotation * vector;
	}
}
