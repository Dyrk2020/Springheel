using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Events;

public class CheckColliding : ColliderModeControl, IGameEventListener
{
	public bool Colliding;

	public bool Required;

	public bool AntiRequired;

	public bool ReverseAttach;

	[BitMask(typeof(TagComparer.Tag))]
	public TagComparer.Tag checkTagMask;

	[BitMask(typeof(TagComparer.Tag))]
	public TagComparer.Tag ignoreTagMask;

	public bool CollidingLastFrame;

	public GameObject CollidingObject;

	public bool TrackAllCollidingCharacters;

	public HashSet<Character> CollidingCharacters = new HashSet<Character>();

	public bool ignorePickedUp = true;

	public bool dontTurnOffOnPlay;

	public Collider2D CheckBounds;

	public bool InBounds;

	protected Placeable lastCollidingPiece;

	protected GameObject lastCollidingObject;

	public bool IgnoreCollisionPieces;

	protected bool On = true;

	public bool Enabled;

	public Placeable attachedTo;

	private TagComparer tagComparer = new TagComparer();

	private bool FlippingPlacementLayer;

	public UnityAction onCollidingCharactersUpdated;

	protected bool CollidingWithAttachment { get; set; }

	protected override void Awake()
	{
		base.Awake();
		if (base.gameObject.layer == LayerMask.NameToLayer("Placement"))
		{
			FlippingPlacementLayer = true;
		}
		CollisionTag component = GetComponent<CollisionTag>();
		if (!component.ContainsAnyTag(TagComparer.Tag.Start))
		{
			ignoreTagMask |= TagComparer.Tag.StartProtection;
		}
		tagComparer.Initialize((int)component.bitMask, (int)checkTagMask, (int)ignoreTagMask);
	}

	public void reInitializeTags()
	{
		CollisionTag component = GetComponent<CollisionTag>();
		tagComparer.Initialize((int)component.bitMask, (int)checkTagMask, (int)ignoreTagMask);
	}

	private void Start()
	{
		ChangeListener(addRemove: true);
		attachedTo = GetComponentInParent<Placeable>();
	}

	public virtual void OnDestroy()
	{
		ChangeListener(addRemove: false);
	}

	public void ChangeListener(bool addRemove)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, addRemove);
		GameEventManager.ChangeListener<TurnOffCheckColliders>(this, addRemove);
	}

	private void FixedUpdate()
	{
		if (On)
		{
			CollidingLastFrame = Colliding;
			Colliding = false;
			CollidingObject = null;
			if (TrackAllCollidingCharacters)
			{
				CollidingCharacters.Clear();
			}
			InBounds = CheckBounds == null;
		}
	}

	public override void Enable()
	{
		base.Enable();
		Enabled = true;
		CollidingObject = null;
		if (TrackAllCollidingCharacters)
		{
			CollidingCharacters.Clear();
		}
	}

	public override void Disable()
	{
		base.Disable();
		Enabled = false;
		CollidingObject = null;
		if (TrackAllCollidingCharacters)
		{
			CollidingCharacters.Clear();
		}
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		if (On)
		{
			handleCollision(c.gameObject);
			if (c == CheckBounds)
			{
				InBounds = true;
			}
		}
	}

	private void OnCollisionStay2D(Collision2D c)
	{
		if (On)
		{
			handleCollision(c.gameObject);
			if (c.collider == CheckBounds)
			{
				InBounds = true;
			}
		}
	}

	public bool CheckCollidingObject(GameObject go)
	{
		bool tagMatch = false;
		CheckColliding component = go.GetComponent<CheckColliding>();
		if (component != null)
		{
			if (TagComparer.DoTagMatch(tagComparer, component.tagComparer, out tagMatch))
			{
				return false;
			}
		}
		else
		{
			CollisionTag component2 = go.GetComponent<CollisionTag>();
			if (component2 != null && TagComparer.DoTagMatch(tagComparer, (int)component2.bitMask, out tagMatch))
			{
				return false;
			}
		}
		if (checkTagMask == TagComparer.Tag.None || tagMatch)
		{
			Placeable placeable;
			if (lastCollidingPiece != null && go == lastCollidingPiece.gameObject)
			{
				placeable = lastCollidingPiece;
			}
			else if (lastCollidingObject != null && go == lastCollidingObject)
			{
				placeable = null;
			}
			else
			{
				placeable = go.GetComponentInChildren<Placeable>();
				if (placeable == null)
				{
					placeable = go.GetComponentInParent<Placeable>();
				}
			}
			if (placeable != null)
			{
				lastCollidingPiece = placeable;
				if (placeable.PickedUp && ignorePickedUp)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void handleCollision(GameObject go)
	{
		if ((attachedTo != null && go.transform.IsChildOf(attachedTo.transform)) || base.transform.IsChildOf(go.transform) || go.transform.IsChildOf(base.transform))
		{
			return;
		}
		bool tagMatch = false;
		CheckColliding component = go.GetComponent<CheckColliding>();
		if (component != null)
		{
			if (TagComparer.DoTagMatch(tagComparer, component.tagComparer, out tagMatch))
			{
				return;
			}
		}
		else
		{
			CollisionTag component2 = go.GetComponent<CollisionTag>();
			if ((component2 != null && TagComparer.DoTagMatch(tagComparer, (int)component2.bitMask, out tagMatch)) || TagComparer.DoTagMatch(tagComparer, go.tag, out tagMatch))
			{
				return;
			}
		}
		if (IgnoreCollisionPieces && go.GetComponent<CollisionPiece>() != null)
		{
			return;
		}
		if (checkTagMask == TagComparer.Tag.None || tagMatch)
		{
			Placeable placeable;
			if (lastCollidingPiece != null && go == lastCollidingPiece.gameObject)
			{
				placeable = lastCollidingPiece;
			}
			else if (lastCollidingObject != null && go == lastCollidingObject)
			{
				placeable = null;
			}
			else
			{
				placeable = go.GetComponentInChildren<Placeable>();
				if (placeable == null)
				{
					placeable = go.GetComponentInParent<Placeable>();
				}
			}
			if (placeable != null)
			{
				lastCollidingPiece = placeable;
				if ((attachedTo != null && ((placeable.Group != null && placeable.Group == attachedTo.Group) || checkIsParent(attachedTo, placeable) || checkIsParent(placeable, attachedTo)) && (!Required || !placeable.PickedUp)) || (placeable.PickedUp && ignorePickedUp))
				{
					return;
				}
			}
			CollidingObject = ((placeable == null) ? go : placeable.gameObject);
			Colliding = true;
		}
		if (!(CollidingObject != null))
		{
			return;
		}
		lastCollidingObject = CollidingObject;
		if (TrackAllCollidingCharacters)
		{
			Character componentInParent = CollidingObject.GetComponentInParent<Character>();
			if ((bool)componentInParent && CollidingCharacters.Add(componentInParent) && onCollidingCharactersUpdated != null)
			{
				onCollidingCharactersUpdated();
			}
		}
	}

	private bool checkIsParent(Placeable parent, Placeable child)
	{
		if (child.ParentPiece != null)
		{
			if (child.ParentPiece == parent)
			{
				return true;
			}
			return checkIsParent(parent, child.ParentPiece);
		}
		return false;
	}

	public void SetIgnoreCollision(CheckColliding cc, bool ignore)
	{
		Collider2D[] components = GetComponents<Collider2D>();
		foreach (Collider2D collider in components)
		{
			Collider2D[] components2 = cc.GetComponents<Collider2D>();
			foreach (Collider2D collider2 in components2)
			{
				Physics2D.IgnoreCollision(collider, collider2, ignore);
			}
		}
	}

	public override void SwitchToMode(ColliderModeEnum newPhase, bool forceUpdate = false)
	{
		if (!forceUpdate && currentPhase == newPhase)
		{
			return;
		}
		switch (newPhase)
		{
		case ColliderModeEnum.PlacementPhase:
			On = PlacementPhase;
			if (FlippingPlacementLayer)
			{
				base.gameObject.layer = LayerMask.NameToLayer("Placement");
			}
			break;
		case ColliderModeEnum.PlacedPhase:
			On = false;
			if (FlippingPlacementLayer)
			{
				base.gameObject.layer = LayerMask.NameToLayer("Placed");
			}
			break;
		case ColliderModeEnum.RunPhase:
			On = dontTurnOffOnPlay;
			break;
		case ColliderModeEnum.NoColliders:
			Switch(OnOff: false);
			On = false;
			break;
		}
		base.SwitchToMode(newPhase, forceUpdate);
		currentPhase = newPhase;
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (!dontTurnOffOnPlay)
		{
			if (type == typeof(StartPhaseEvent) && (e as StartPhaseEvent).Phase == GameControl.GamePhase.PLACE)
			{
				On = false;
			}
			if (type == typeof(TurnOffCheckColliders))
			{
				On = false;
			}
		}
	}

	public void OnDisable()
	{
		Colliding = false;
		CollidingObject = null;
	}

	public static bool ShouldIgnoreCollision(CheckColliding a, CheckColliding b)
	{
		return TagComparer.ShouldIgnoreCollision(a.tagComparer, b.tagComparer);
	}

	public static bool ShouldIgnoreCollision(CheckColliding a, int bMask)
	{
		return TagComparer.ShouldIgnoreCollision(a.tagComparer, bMask);
	}

	public static bool ShouldIgnoreCollision(CheckColliding a, string bTag)
	{
		return TagComparer.ShouldIgnoreCollision(a.tagComparer, bTag);
	}
}
