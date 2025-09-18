using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class BlowerPusher : MonoBehaviour, IGameEventListener
{
	public float strength = 1f;

	public float maxAirSpeed = 10f;

	public float maxReverseAirPushBack;

	public float speedInCommonFactor;

	public AnimationCurve distanceFactor;

	public AnimationCurve offcenterFactor;

	public Transform BlowerDirectionTarget;

	public Transform BlowerBase;

	public Transform BlowerPerpendicular;

	public bool projectilePusher;

	private HashSet<Projectile> affectedProjectiles = new HashSet<Projectile>();

	private HashSet<Character> affectedCharacters = new HashSet<Character>();

	private HashSet<Character> affectedCharactersLastFrame = new HashSet<Character>();

	private List<Projectile> destroyedProjectiles = new List<Projectile>(64);

	public LayerMask layerMask;

	private bool pauseAttraction;

	private void Start()
	{
		GameEventManager.ChangeListener<PauseEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<PauseEvent>(this, adding: false);
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		OnTriggerStay2D(c);
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		if (projectilePusher)
		{
			Projectile componentInParent = c.GetComponentInParent<Projectile>();
			if (componentInParent != null && !componentInParent.collided)
			{
				affectedProjectiles.Add(componentInParent);
			}
		}
		else
		{
			Character componentInParent2 = c.GetComponentInParent<Character>();
			if (componentInParent2 != null && !componentInParent2.Waiting)
			{
				affectedCharacters.Add(componentInParent2);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		if (projectilePusher)
		{
			Projectile componentInParent = c.GetComponentInParent<Projectile>();
			if (componentInParent != null)
			{
				affectedProjectiles.Remove(componentInParent);
			}
		}
	}

	private void FixedUpdate()
	{
		if (pauseAttraction)
		{
			return;
		}
		Vector2 vector = BlowerDirectionTarget.position - BlowerBase.position;
		Vector2 rhs = BlowerPerpendicular.position - BlowerBase.position;
		Vector2 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		float sqrMagnitude = vector.sqrMagnitude;
		float sqrMagnitude2 = rhs.sqrMagnitude;
		if (projectilePusher)
		{
			foreach (Projectile affectedProjectile in affectedProjectiles)
			{
				if (affectedProjectile != null && affectedProjectile.gameObject.activeInHierarchy)
				{
					RaycastHit2D raycastHit2D = Physics2D.BoxCast((Vector2)affectedProjectile.transform.position - normalized, new Vector2(0.25f, 0.25f), base.transform.parent.eulerAngles.z, -vector, 30f, layerMask);
					if (raycastHit2D.collider != null && !(raycastHit2D.collider.gameObject.GetComponentInParent<UpBlower>() == null))
					{
						Rigidbody2D component = affectedProjectile.GetComponent<Rigidbody2D>();
						Vector2 velocity = component.velocity;
						Vector3 vector2 = affectedProjectile.transform.position - BlowerBase.position;
						Vector2 vector3 = normalized;
						float time = Mathf.Clamp01(Vector2.Dot(vector2, vector3) / magnitude);
						float num = distanceFactor.Evaluate(time) * strength;
						Vector2 vector4 = vector3 * num * Time.deltaTime;
						Vector3 vector5 = affectedProjectile.transform.InverseTransformVector(vector4);
						vector5.x *= affectedProjectile.sideAerodynamismFactor;
						vector5.y *= affectedProjectile.headOnAerodynamismFactor;
						vector4 = affectedProjectile.transform.TransformVector(vector5);
						velocity += vector4;
						component.velocity = velocity;
						if (vector4.x != 0f || vector4.y != 0f)
						{
							affectedProjectile.SoftRotateToTarget(affectedProjectile.GetZRotForVelocity(vector4), 5f * affectedProjectile.sideAerodynamismFactor);
						}
					}
				}
				else
				{
					destroyedProjectiles.Add(affectedProjectile);
				}
			}
			foreach (Projectile destroyedProjectile in destroyedProjectiles)
			{
				affectedProjectiles.Remove(destroyedProjectile);
			}
			destroyedProjectiles.Clear();
			return;
		}
		foreach (Character affectedCharacter in affectedCharacters)
		{
			if (affectedCharacter == null)
			{
				continue;
			}
			int num2 = Physics2D.BoxCastNonAlloc((Vector2)affectedCharacter.transform.position - normalized, new Vector2(0.25f, 0.25f), base.transform.parent.eulerAngles.z, -vector, Placeable.raycastResultCache, 30f, layerMask);
			bool flag = false;
			for (int i = 0; i < num2; i++)
			{
				RaycastHit2D raycastHit2D2 = Placeable.raycastResultCache[i];
				if (raycastHit2D2.collider != null)
				{
					Transform parent = raycastHit2D2.collider.transform.parent;
					if (parent != null && parent.GetComponentInChildren<BlowerPusher>() == this && parent == base.transform.parent)
					{
						break;
					}
					CollisionTag component2 = raycastHit2D2.collider.GetComponent<CollisionTag>();
					if (component2 != null && component2.ContainsAnyTag(TagComparer.Tag.Solid))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				Vector2 lhs = affectedCharacter.transform.position - BlowerBase.position;
				float num3 = Vector2.Dot(lhs, vector) / sqrMagnitude;
				float f = Vector2.Dot(lhs, rhs) / sqrMagnitude2;
				float num4 = Vector2.Dot(affectedCharacter.GetComponent<Rigidbody2D>().velocity, vector) / magnitude;
				float num5 = 1f - num4 * speedInCommonFactor;
				if (num3 > 0f)
				{
					float num6 = distanceFactor.Evaluate(num3) * strength * num5 * offcenterFactor.Evaluate(Mathf.Abs(f));
					affectedCharacter.ApplyPhysicsModifier(new PhysicsModifier(PhysicsModifier.ModType.Blackhole, num6, normalized, base.gameObject));
					Debug.DrawLine(BlowerBase.transform.position, BlowerBase.transform.position + (Vector3)(normalized * num6 * 5f), Color.red);
				}
			}
		}
		foreach (Character item in affectedCharactersLastFrame)
		{
			if (item != null && !affectedCharacters.Contains(item))
			{
				AkSoundEngine.PostEvent("SFX_Pieces_UpBlower_BlowingCharacterExit", item.gameObject);
			}
		}
		foreach (Character affectedCharacter2 in affectedCharacters)
		{
			if (affectedCharacter2 != null && !affectedCharactersLastFrame.Contains(affectedCharacter2))
			{
				AkSoundEngine.PostEvent("SFX_Pieces_UpBlower_BlowingCharacter", affectedCharacter2.gameObject);
			}
		}
		affectedCharactersLastFrame.Clear();
		foreach (Character affectedCharacter3 in affectedCharacters)
		{
			affectedCharactersLastFrame.Add(affectedCharacter3);
		}
		affectedCharacters.Clear();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is PauseEvent pauseEvent)
		{
			pauseAttraction = pauseEvent.Paused;
		}
	}
}
