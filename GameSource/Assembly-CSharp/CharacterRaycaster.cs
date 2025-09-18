using UnityEngine;

public class CharacterRaycaster : MonoBehaviour
{
	public Character character;

	public Transform raycastStartAnchor;

	public Transform raycastEndAnchor;

	public Vector3 raycastDirection;

	public float raycastLength = 0.25f;

	public int numRays = 3;

	public LayerMask layerMask;

	public bool raycastEnabled = true;

	public bool NoPhysicsModifiersIfOnGround;

	public float lastCollisionDistance;

	public float approachDistance;

	public float margin = 0.05f;

	public float realCollisionDistance = 0.1f;

	public float baseHorizontalMotionOfObject;

	public float baseVerticalMotionOfObject;

	public bool Colliding;

	public bool CollidingWall;

	public bool CollidingHazard;

	public bool CollidingSpecialButton;

	public int NumberRayHits;

	public static int optionACheckMask = 128;

	public static int optionAIgnoreMask = 2304;

	public static int optionBMask = 10368;

	public static int optionCMask = 256;

	public static int optionDMask = 67108864;

	private RaycastHit2D[] hit = new RaycastHit2D[4];

	public float ScaledMargin => margin * Modifiers.GetInstance().CharacterRelativeScale;

	private bool OnRaycastHitCollider(Collider2D c, CollisionTag collisionTag)
	{
		if (Modifiers.GetInstance().PlayerPlayerCollisions && (c == character.playerPlayerColliderCrouch || c == character.playerPlayerCollider || c == character.playerPlayerColliderDead))
		{
			return false;
		}
		if (collisionTag != null)
		{
			if (collisionTag.ContainsAnyTag(optionDMask))
			{
				CollidingSpecialButton = true;
			}
			if (collisionTag.ContainsAnyTag(optionACheckMask) && !collisionTag.ContainsAnyTag(optionAIgnoreMask))
			{
				Colliding = true;
				CollidingWall = true;
				NumberRayHits++;
				return true;
			}
			if (collisionTag.ContainsAnyTag(optionBMask) && !collisionTag.ContainsAnyTag(optionCMask))
			{
				Colliding = true;
				CollidingWall = false;
				NumberRayHits++;
				return true;
			}
			if (collisionTag.ContainsAnyTag(optionCMask))
			{
				CollidingHazard = true;
				return true;
			}
		}
		return false;
	}

	private bool OnRaycastHitSolidSurface(Collider2D c, CollisionTag collisionTag)
	{
		if (Modifiers.GetInstance().PlayerPlayerCollisions && (c == character.playerPlayerColliderCrouch || c == character.playerPlayerCollider || c == character.playerPlayerColliderDead))
		{
			return false;
		}
		if (collisionTag != null)
		{
			if (collisionTag.ContainsAnyTag(optionDMask))
			{
				return false;
			}
			if (collisionTag.ContainsAnyTag(optionACheckMask) && !collisionTag.ContainsAnyTag(optionAIgnoreMask))
			{
				return true;
			}
			if (collisionTag.ContainsAnyTag(optionBMask))
			{
				return true;
			}
		}
		return false;
	}

	public void RaycastUpdate()
	{
		if (!base.enabled)
		{
			return;
		}
		lastCollisionDistance = 1000f;
		approachDistance = 1000f;
		NumberRayHits = 0;
		baseHorizontalMotionOfObject = 0f;
		baseVerticalMotionOfObject = 0f;
		Colliding = false;
		CollidingWall = false;
		CollidingHazard = false;
		CollidingSpecialButton = false;
		if (!raycastEnabled)
		{
			return;
		}
		Modifiers instance = Modifiers.GetInstance();
		Vector3 vector = default(Vector3);
		Physics2D.queriesHitTriggers = false;
		float characterRelativeScale = instance.CharacterRelativeScale;
		for (int i = 0; i < numRays; i++)
		{
			vector = ((i != 0) ? Vector3.Lerp(raycastStartAnchor.position, raycastEndAnchor.position, (float)i / (float)(numRays - 1)) : raycastStartAnchor.position);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, raycastDirection, raycastLength * characterRelativeScale, layerMask.value);
			if (!(raycastHit2D.collider != null) || !(raycastHit2D.distance < lastCollisionDistance))
			{
				continue;
			}
			CollisionTag.AllTags.TryGetValue(raycastHit2D.collider, out var value);
			bool num = OnRaycastHitSolidSurface(raycastHit2D.collider, value);
			float num2 = 0f;
			PhysicsModifier[] array = null;
			PhysicsModifier[] array2;
			if (num)
			{
				if (raycastHit2D.distance < approachDistance)
				{
					approachDistance = raycastHit2D.distance;
				}
				Placeable placeable = null;
				placeable = ((!raycastHit2D.collider.TryGetComponent<CheckColliding>(out var component) || !(component.attachedTo != null)) ? raycastHit2D.collider.GetComponentInParent<Placeable>() : component.attachedTo);
				if (placeable != null)
				{
					array = placeable.GetPhysicsModifiers();
					array2 = array;
					foreach (PhysicsModifier physicsModifier in array2)
					{
						if (physicsModifier.ModifierType == PhysicsModifier.ModType.BaseMotion)
						{
							Vector2 vector2 = physicsModifier.Magnitude * physicsModifier.Direction;
							baseHorizontalMotionOfObject = vector2.x;
							baseVerticalMotionOfObject = vector2.y;
						}
					}
				}
			}
			num2 = ((raycastDirection.y != 0f) ? Mathf.Clamp(baseVerticalMotionOfObject * raycastDirection.y * Time.fixedDeltaTime, 0f, 1f) : Mathf.Clamp(baseHorizontalMotionOfObject * raycastDirection.x * Time.fixedDeltaTime, 0f, 1f));
			if (raycastHit2D.distance <= realCollisionDistance + num2 && OnRaycastHitCollider(raycastHit2D.collider, value) && raycastHit2D.distance < lastCollisionDistance)
			{
				if (value != null && value.ContainsAnyTag(optionDMask))
				{
					lastCollisionDistance = ScaledMargin;
				}
				else
				{
					lastCollisionDistance = raycastHit2D.distance;
				}
			}
			if (!Colliding || array == null)
			{
				continue;
			}
			array2 = array;
			foreach (PhysicsModifier pm in array2)
			{
				if (!NoPhysicsModifiersIfOnGround || !character.OnGround)
				{
					character.ApplyPhysicsModifier(pm);
				}
			}
		}
	}
}
