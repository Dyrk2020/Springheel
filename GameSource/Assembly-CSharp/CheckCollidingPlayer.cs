using UnityEngine;

public class CheckCollidingPlayer : MonoBehaviour
{
	public bool Colliding;

	public bool Hazard;

	public string HazardType;

	public bool DeathPit;

	public int HazardPlacedByPlayer;

	public Vector3 HazardPoint;

	public string CheckTag = "";

	public string IgnoreTag = "";

	[BitMask(typeof(TagComparer.Tag))]
	public TagComparer.Tag checkTagMask;

	[BitMask(typeof(TagComparer.Tag))]
	public TagComparer.Tag ignoreTagMask;

	public bool CollidingLastFrame;

	public bool HazardLastFrame;

	public Character AssociatedCharacter;

	protected Placeable lastCollidingPiece;

	protected Placeable lastCollidingPhysicsPiece;

	protected GameObject lastCollidingObject;

	public TagComparer tagComparer = new TagComparer();

	public Collider2D[] mainColliders;

	public Collider2D claw;

	public bool Enabled { get; protected set; }

	public void Enable()
	{
		Collider2D[] array = mainColliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		Enabled = true;
	}

	public void EnableClaw()
	{
		if (Enabled && !claw.enabled)
		{
			claw.enabled = true;
		}
	}

	public void DisableClaw()
	{
		if (claw.enabled)
		{
			claw.enabled = false;
		}
	}

	public void Disable()
	{
		Collider2D[] array = mainColliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		claw.enabled = false;
		Enabled = false;
	}

	private void Awake()
	{
		CollisionTag component = GetComponent<CollisionTag>();
		tagComparer.Initialize((int)component.bitMask, (int)checkTagMask, (int)ignoreTagMask);
	}

	private void Start()
	{
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		for (int i = 0; i != componentsInChildren.Length - 1; i++)
		{
			Collider2D collider = componentsInChildren[i];
			for (int j = i + 1; j != componentsInChildren.Length; j++)
			{
				Physics2D.IgnoreCollision(collider, componentsInChildren[j], ignore: true);
			}
		}
	}

	private void FixedUpdate()
	{
		CollidingLastFrame = Colliding;
		HazardLastFrame = Hazard;
		Colliding = false;
		Hazard = false;
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		DealWithCollision(c);
	}

	private void OnCollisionEnter2D(Collision2D c)
	{
		DealWithCollision(c);
	}

	private void OnCollisionStay2D(Collision2D c)
	{
		DealWithCollision(c);
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		DealWithCollision(c);
	}

	private void DealWithCollision(Collision2D c)
	{
		DealWithCollision(c.collider);
	}

	private bool IsDeathPit(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Deathpit);
		}
		return false;
	}

	private bool HasNoPhysicModifier(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.NoPhysicModifier);
		}
		return false;
	}

	private void DealWithCollision(Collider2D c)
	{
		if (AssociatedCharacter != null && !AssociatedCharacter.Active)
		{
			return;
		}
		bool tagMatch = false;
		CollisionTag.AllTags.TryGetValue(c, out var value);
		if (value != null && TagComparer.DoTagMatch(tagComparer, (int)value.bitMask, out tagMatch))
		{
			return;
		}
		DeathPit = false;
		GameObject gameObject = c.gameObject;
		if (tagMatch)
		{
			Colliding = true;
			Hazard = true;
			Placeable componentInParent = gameObject.GetComponentInParent<Placeable>();
			if (componentInParent != null)
			{
				HazardType = componentInParent.KillType;
				HazardPlacedByPlayer = componentInParent.placedByPlayerNumber;
			}
			else
			{
				Projectile component = gameObject.GetComponent<Projectile>();
				if (component != null)
				{
					HazardType = component.LaunchedFrom.KillType;
					HazardPlacedByPlayer = component.placedByPlayerNumber;
				}
				else
				{
					BeeSwarm componentInParent2 = gameObject.GetComponentInParent<BeeSwarm>();
					if (componentInParent2 != null)
					{
						HazardType = componentInParent2.KillType;
						HazardPlacedByPlayer = componentInParent2.placedByPlayerNumber;
					}
					else
					{
						HazardType = "World";
						HazardPlacedByPlayer = 0;
					}
				}
			}
			HazardPoint = gameObject.transform.position;
		}
		else if (IsDeathPit(gameObject, value))
		{
			DeathPit = true;
			DrownComponent component2 = gameObject.GetComponent<DrownComponent>();
			if (component2 != null)
			{
				if (component2.isLava)
				{
					HazardType = "Drowning_In_Lava";
				}
				else
				{
					HazardType = "Drowning";
				}
			}
			else
			{
				HazardType = "Falling";
			}
			Hazard = true;
			HazardPoint = base.transform.position;
			HazardPlacedByPlayer = 0;
		}
		PassPhysicsModifier(gameObject, value);
		PassImplus(gameObject, value);
	}

	public void PassPhysicsModifier(GameObject go, CollisionTag collisionTag)
	{
		if (collisionTag == null)
		{
			collisionTag = go.GetComponent<CollisionTag>();
		}
		if (HasNoPhysicModifier(go, collisionTag))
		{
			return;
		}
		Placeable componentInParent = go.GetComponentInParent<Placeable>();
		if (componentInParent != null && AssociatedCharacter != null)
		{
			PhysicsModifier[] physicsModifiers = componentInParent.GetPhysicsModifiers();
			foreach (PhysicsModifier pm in physicsModifiers)
			{
				AssociatedCharacter.ApplyPhysicsModifier(pm);
			}
		}
		InheritMotion componentInParent2 = go.GetComponentInParent<InheritMotion>();
		if (componentInParent2 != null && AssociatedCharacter != null)
		{
			PhysicsModifier[] physicsModifiers = componentInParent2.GetPhysicsModifiers();
			foreach (PhysicsModifier pm2 in physicsModifiers)
			{
				AssociatedCharacter.ApplyPhysicsModifier(pm2);
			}
		}
	}

	public void PassImplus(GameObject go, CollisionTag collisionTag)
	{
		if (collisionTag == null)
		{
			collisionTag = go.GetComponent<CollisionTag>();
		}
		if (HasImpulseTag(go, collisionTag))
		{
			ColliderHittingCharacter component = go.GetComponent<ColliderHittingCharacter>();
			if (component != null && AssociatedCharacter != null)
			{
				component.DeathImpulse(AssociatedCharacter);
			}
		}
	}

	private bool HasImpulseTag(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Impulse);
		}
		return false;
	}
}
