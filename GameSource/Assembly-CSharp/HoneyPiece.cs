using System;
using System.Collections.Generic;
using UnityEngine;

public class HoneyPiece : MultipiecePart
{
	public float Stickiness;

	public float JumpStickiness;

	private GameObject dummySource;

	public HoneySticker HoneyStickerPrefab;

	public Transform stickyStart;

	public Transform stickyEnd;

	public CheckColliding honeyAttachmentDontIgnorePickedUp;

	public bool NotSticky;

	public Placeable lastReverseAttachment;

	private bool CanStickItToPlayers
	{
		get
		{
			if (NotSticky || (lastReverseAttachment != null && !lastReverseAttachment.DontBlockGlueSticky && lastReverseAttachment.Group != null && lastReverseAttachment.Group == Group))
			{
				return false;
			}
			return true;
		}
	}

	public bool HasReverseAttachment
	{
		get
		{
			if (lastReverseAttachment != null && lastReverseAttachment.Group != null)
			{
				return lastReverseAttachment.Group == Group;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		pms = new PhysicsModifier[2];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.JumpForce, JumpStickiness, base.gameObject);
		pms[1] = new PhysicsModifier(PhysicsModifier.ModType.Friction, Stickiness, dummySource);
	}

	protected override void Start()
	{
		base.Start();
		if (dummySource == null)
		{
			dummySource = new GameObject("Dummy Friction Object");
			dummySource.transform.parent = base.transform;
			pms[1].Source = dummySource;
		}
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		if (CanStickItToPlayers)
		{
			pms[0].Magnitude = JumpStickiness;
			pms[1].Magnitude = Stickiness;
			return pms;
		}
		return new PhysicsModifier[0];
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		if (CanStickItToPlayers)
		{
			PhysicsModifier[] physicsModifiers = base.GetPhysicsModifiers();
			PhysicsModifier[] array = new PhysicsModifier[physicsModifiers.Length + 2];
			Array.Copy(physicsModifiers, array, physicsModifiers.Length);
			pms[0].Magnitude = JumpStickiness;
			pms[1].Magnitude = Stickiness;
			array[physicsModifiers.Length] = pms[0];
			array[physicsModifiers.Length + 1] = pms[1];
			return array;
		}
		return base.GetPhysicsModifiers();
	}

	public void triggerStickness(Transform stickyTarget)
	{
		int num = UnityEngine.Random.Range(3, 7);
		for (int i = 0; i < num; i++)
		{
			float t = UnityEngine.Random.Range(0f, 1f) / (float)num + (float)i * 1f / (float)num;
			HoneySticker honeySticker = UnityEngine.Object.Instantiate(HoneyStickerPrefab, Vector3.Lerp(stickyStart.position, stickyEnd.position, t), base.transform.rotation);
			honeySticker.transform.parent = base.transform;
			honeySticker.triggerHoneyStick(stickyTarget);
		}
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		if ((bool)honeyAttachmentDontIgnorePickedUp)
		{
			honeyAttachmentDontIgnorePickedUp.ignorePickedUp = false;
		}
	}

	public override void Enable()
	{
		base.Enable();
		Physics2D.queriesHitTriggers = true;
		Physics2D.queriesStartInColliders = true;
		if (ParentPiece != null && ParentPiece is ActiveBlock)
		{
			_ = (ParentPiece as ActiveBlock).MovingPlatform;
		}
		foreach (CheckColliding item in PlacementCollidersNew)
		{
			if (!item.ReverseAttach)
			{
				continue;
			}
			BoxCollider2D component = item.GetComponent<BoxCollider2D>();
			if (component == null)
			{
				continue;
			}
			bool flag = false;
			if (NotSticky)
			{
				flag = true;
			}
			else
			{
				RaycastHit2D raycastHit2D = Physics2D.BoxCast((Vector2)base.transform.position + component.offset, component.size, 0f, Vector2.zero, 0f, Placeable.attachmentColliderMask);
				if (raycastHit2D.collider != null)
				{
					Placeable componentInParent = raycastHit2D.collider.gameObject.GetComponentInParent<Placeable>();
					if (!(componentInParent != null) || componentInParent.DontBlockGlueSticky)
					{
						continue;
					}
					if (componentInParent.Group == null)
					{
						if (componentInParent.IsMobileBlock || (Group != null && Group.TopParent != null && Group.TopParent.IsMobileBlock))
						{
							continue;
						}
					}
					else if (componentInParent.Group != Group && ((componentInParent.Group.TopParent != null && componentInParent.Group.TopParent.IsMobileBlock) || Group == null || !(Group.TopParent != null) || !Group.TopParent.IsMobileBlock))
					{
						continue;
					}
					flag = true;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (CheckColliding item2 in ActiveCollidersNew)
			{
				if (!item2.ReverseAttach)
				{
					item2.Disable();
				}
			}
		}
	}

	public override bool CanPlace()
	{
		if (base.CanPlace())
		{
			if (isSetPiece)
			{
				int num = 0;
				HashSet<int> hashSet = new HashSet<int>();
				Placeable placeable = null;
				for (int i = 0; i < PlacementCollidersNew.Count; i++)
				{
					CheckColliding checkColliding = PlacementCollidersNew[i];
					if (!checkColliding.Required || checkColliding.ReverseAttach || !(checkColliding.CollidingObject != null))
					{
						continue;
					}
					Placeable component = checkColliding.CollidingObject.GetComponent<Placeable>();
					if (component == null)
					{
						return false;
					}
					if (!component.attachableWithGlue)
					{
						return false;
					}
					if (component == placeable || !component.isSetPiece)
					{
						return false;
					}
					if (component.IsMobileBlock)
					{
						if (component.Group != null)
						{
							hashSet.Add(component.Group.id);
						}
						else
						{
							hashSet.Add(num--);
						}
					}
					else if (component.Group != null && component.Group.TopParent != null && component.Group.TopParent.IsMobileBlock)
					{
						hashSet.Add(component.Group.id);
					}
					placeable = component;
				}
				if (hashSet.Count >= 2)
				{
					return false;
				}
			}
			else
			{
				GameObject gameObject = null;
				GameObject gameObject2 = null;
				for (int j = 0; j < PlacementCollidersNew.Count; j++)
				{
					CheckColliding checkColliding2 = PlacementCollidersNew[j];
					if (checkColliding2.ReverseAttach && checkColliding2.CollidingObject != null)
					{
						CountsAsObject component2 = checkColliding2.CollidingObject.GetComponent<CountsAsObject>();
						gameObject2 = ((!(component2 != null) || !(component2.CountAs != null)) ? checkColliding2.CollidingObject : component2.CountAs);
					}
					if (!checkColliding2.Required || checkColliding2.ReverseAttach || !(checkColliding2.CollidingObject != null))
					{
						continue;
					}
					Placeable component3 = checkColliding2.CollidingObject.GetComponent<Placeable>();
					if (component3 != null)
					{
						if (!component3.attachableWithGlue)
						{
							return false;
						}
					}
					else
					{
						CollisionTag component4 = checkColliding2.CollidingObject.GetComponent<CollisionTag>();
						if (!(component4 != null))
						{
							return false;
						}
						if (!component4.ContainsAnyTag(TagComparer.Tag.Solid))
						{
							return false;
						}
					}
					CountsAsObject component5 = checkColliding2.CollidingObject.GetComponent<CountsAsObject>();
					gameObject = ((!(component5 != null) || !(component5.CountAs != null)) ? checkColliding2.CollidingObject : component5.CountAs);
				}
				if (gameObject != null && gameObject2 == gameObject)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public override bool RequiredColliderConditionsSatisfied()
	{
		foreach (CheckColliding item in PlacementCollidersNew)
		{
			if (item == null || !item.Required || item.ReverseAttach)
			{
				continue;
			}
			BoxCollider2D component = item.GetComponent<BoxCollider2D>();
			Physics2D.queriesStartInColliders = true;
			Physics2D.queriesHitTriggers = true;
			int num = Physics2D.BoxCastNonAlloc(component.transform.TransformPoint(component.offset), angle: component.transform.rotation.eulerAngles.z, size: component.size, direction: Vector2.zero, results: Placeable.raycastResultCache, distance: 0f, layerMask: Placeable.attachmentColliderMask);
			if (num > 0)
			{
				bool flag = true;
				for (int i = 0; i < num; i++)
				{
					RaycastHit2D raycastHit2D = Placeable.raycastResultCache[i];
					if (!(raycastHit2D.collider != null))
					{
						continue;
					}
					CheckColliding component2 = raycastHit2D.collider.GetComponent<CheckColliding>();
					if (component2 != null)
					{
						if (!CheckColliding.ShouldIgnoreCollision(item, component2))
						{
							flag = false;
							break;
						}
						continue;
					}
					CollisionTag component3 = raycastHit2D.collider.GetComponent<CollisionTag>();
					if (component3 != null && !component3.ContainsAnyTag(TagComparer.Tag.NoAttach) && !CheckColliding.ShouldIgnoreCollision(item, (int)component3.bitMask))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	public override void PickUp()
	{
		base.PickUp();
		if (isSetPiece && ParentPiece != null)
		{
			DestroySelf(destroyChildren: true);
		}
	}
}
