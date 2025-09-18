using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class AttractorRepulsor : MonoBehaviour, IGameEventListener
{
	public float radius = 10f;

	public float innerRadius = 1f;

	public float pushAccel = -10f;

	public AnimationCurve distanceFactor;

	public float centerRotateDelay = 0.2f;

	private HashSet<Projectile> affectedProjectiles = new HashSet<Projectile>();

	private List<Projectile> destroyedProjectiles = new List<Projectile>(64);

	private bool pauseAttraction;

	private void Awake()
	{
		Update();
	}

	private void Start()
	{
		GameEventManager.ChangeListener<PauseEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<PauseEvent>(this, adding: false);
	}

	private void Update()
	{
		GetComponent<CircleCollider2D>().radius = radius;
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		OnTriggerStay2D(c);
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		Projectile componentInParent = c.GetComponentInParent<Projectile>();
		if (componentInParent != null && !componentInParent.collided)
		{
			affectedProjectiles.Add(componentInParent);
		}
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		Projectile componentInParent = c.GetComponentInParent<Projectile>();
		if (componentInParent != null)
		{
			affectedProjectiles.Remove(componentInParent);
		}
	}

	private void FixedUpdate()
	{
		if (!pauseAttraction)
		{
			foreach (Projectile affectedProjectile in affectedProjectiles)
			{
				if (affectedProjectile != null && affectedProjectile.gameObject.activeInHierarchy)
				{
					Vector3 vector = affectedProjectile.transform.position - base.transform.position;
					float magnitude = vector.magnitude;
					if (magnitude < innerRadius)
					{
						affectedProjectile.collided = true;
						destroyedProjectiles.Add(affectedProjectile);
						continue;
					}
					Rigidbody2D component = affectedProjectile.GetComponent<Rigidbody2D>();
					Vector3 vector2 = component.velocity.ToVector3();
					float num = Mathf.Clamp(magnitude, 0f, radius) / radius;
					float num2 = distanceFactor.Evaluate(1f - num);
					vector2 += vector / magnitude * pushAccel * num2 * Time.deltaTime;
					component.velocity = vector2;
					affectedProjectile.SoftRotateToTarget(affectedProjectile.GetZRotForVelocity(-vector), 1f / centerRotateDelay);
				}
				else
				{
					destroyedProjectiles.Add(affectedProjectile);
				}
			}
		}
		foreach (Projectile destroyedProjectile in destroyedProjectiles)
		{
			affectedProjectiles.Remove(destroyedProjectile);
		}
		destroyedProjectiles.Clear();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is PauseEvent pauseEvent)
		{
			pauseAttraction = pauseEvent.Paused;
		}
	}
}
