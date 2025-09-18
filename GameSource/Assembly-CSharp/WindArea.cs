using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class WindArea : MonoBehaviour, IGameEventListener
{
	public enum OperationMode
	{
		Wind,
		Blackhole
	}

	public OperationMode operationMode;

	private bool pauseWind;

	public Vector2 blowDirection;

	public float projectileBlowPower;

	public bool gravityProjectileUseAlternateBlowPower;

	public float projectileBlowPowerAgainstGravity;

	public float characterBlowPower;

	public AnimationCurve blackHoleDistanceAttenuation;

	public Transform blackholeCenter;

	private HashSet<Character> affectedCharacters = new HashSet<Character>();

	private HashSet<Projectile> affectedProjectiles = new HashSet<Projectile>();

	private HashSet<Projectile> destroyedProjectiles = new HashSet<Projectile>();

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
		Character componentInParent = c.GetComponentInParent<Character>();
		if (componentInParent != null && !componentInParent.Waiting)
		{
			affectedCharacters.Add(componentInParent);
			return;
		}
		Projectile componentInParent2 = c.GetComponentInParent<Projectile>();
		if (componentInParent2 != null && !componentInParent2.collided)
		{
			affectedProjectiles.Add(componentInParent2);
		}
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		Character componentInParent = c.GetComponentInParent<Character>();
		if (componentInParent != null && !componentInParent.Waiting)
		{
			affectedCharacters.Remove(componentInParent);
			return;
		}
		Projectile componentInParent2 = c.GetComponentInParent<Projectile>();
		if (componentInParent2 != null)
		{
			affectedProjectiles.Remove(componentInParent2);
		}
	}

	private void FixedUpdate()
	{
		if (pauseWind)
		{
			return;
		}
		foreach (Projectile affectedProjectile in affectedProjectiles)
		{
			if (affectedProjectile != null && affectedProjectile.gameObject.activeInHierarchy)
			{
				Rigidbody2D component = affectedProjectile.GetComponent<Rigidbody2D>();
				Vector2 velocity = component.velocity;
				Vector2 vector = blowDirection;
				float num = ((!affectedProjectile.UseGravity) ? projectileBlowPower : projectileBlowPowerAgainstGravity);
				if (operationMode == OperationMode.Blackhole)
				{
					Vector3 vector2 = blackholeCenter.position - affectedProjectile.transform.position;
					float magnitude = vector2.magnitude;
					if (magnitude > 0.01f)
					{
						vector = vector2 / magnitude;
						num *= blackHoleDistanceAttenuation.Evaluate(magnitude);
					}
				}
				Vector2 vector3 = vector * num * Time.deltaTime;
				Vector3 vector4 = affectedProjectile.transform.InverseTransformVector(vector3);
				vector4.x *= affectedProjectile.sideAerodynamismFactor;
				vector4.y *= affectedProjectile.headOnAerodynamismFactor;
				vector3 = affectedProjectile.transform.TransformVector(vector4);
				velocity.x += vector3.x;
				velocity.y += vector3.y;
				if (!affectedProjectile.UseGravity && (vector3.x != 0f || vector3.y != 0f))
				{
					affectedProjectile.SoftRotateToTarget(affectedProjectile.GetZRotForVelocity(vector3), projectileBlowPower * affectedProjectile.sideAerodynamismFactor);
					velocity = velocity.magnitude * (Quaternion.AngleAxis(affectedProjectile.transform.eulerAngles.z, Vector3.forward) * Vector3.up);
				}
				component.velocity = velocity;
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
		foreach (Character affectedCharacter in affectedCharacters)
		{
			if (affectedCharacter == null)
			{
				continue;
			}
			Vector2 direction = blowDirection;
			float num2 = characterBlowPower;
			if (operationMode == OperationMode.Blackhole)
			{
				Vector3 vector5 = blackholeCenter.position - affectedCharacter.transform.position;
				float magnitude2 = vector5.magnitude;
				if (magnitude2 > 0.01f)
				{
					direction = vector5 / magnitude2;
					num2 *= blackHoleDistanceAttenuation.Evaluate(magnitude2);
				}
			}
			affectedCharacter.ApplyPhysicsModifier(new PhysicsModifier(PhysicsModifier.ModType.Wind, num2, direction, base.gameObject));
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is PauseEvent pauseEvent)
		{
			pauseWind = pauseEvent.Paused;
		}
	}
}
