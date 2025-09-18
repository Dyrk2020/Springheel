using UnityEngine;

public class ProjectileLauncher : ActiveBlock
{
	public Projectile Ammo;

	public float firstShotDelay;

	public float initialDelay;

	public float interval;

	public float Angle;

	public Vector3 LaunchPoint;

	public float lastShot;

	public string SoundEventName;

	private Quaternion shootAngle;

	public Animator ShootAnimator;

	public bool shooting;

	protected Vector3 adjustedLaunchPoint;

	protected int projectileCounter;

	protected ObjPool pool;

	protected override void Start()
	{
		base.Start();
		UpdateInitialDelay();
		lastShot = 0f - initialDelay - firstShotDelay;
		pool = base.gameObject.AddComponent<ObjPool>();
		Modifiers instance = Modifiers.GetInstance();
		float num = interval / instance.RateOfFire;
		float num2 = Ammo.Lifespan / instance.ProjectileSpeed;
		pool.Initilize(Ammo.gameObject, (int)(num2 / num) + 5);
	}

	private void UpdateInitialDelay()
	{
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
		{
			initialDelay = 0.5f;
		}
		else
		{
			initialDelay = 0.5f - LobbyManager.instance.GetAveragePingToServer();
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdateInitialDelay();
		if (paused || scoreboard)
		{
			return;
		}
		float num = interval / Modifiers.GetInstance().RateOfFire;
		if (base.Active)
		{
			lastShot += Time.deltaTime;
		}
		if (ShootAnimator != null)
		{
			if (!(lastShot >= num) || shooting)
			{
				return;
			}
			ShootAnimator.SetTrigger("Shoot");
			shooting = true;
			if (num == 0f)
			{
				lastShot = 0f;
				return;
			}
			while (lastShot >= num)
			{
				lastShot -= num;
			}
		}
		else
		{
			if (!(lastShot >= num))
			{
				return;
			}
			if (num == 0f)
			{
				lastShot = 0f;
			}
			else
			{
				while (lastShot >= num)
				{
					lastShot -= num;
				}
			}
			ShootProjectile();
		}
	}

	protected override void Act(float deltaTime)
	{
		if (ShootAnimator != null)
		{
			ShootAnimator.speed = Modifiers.GetInstance().RateOfFire;
		}
		if (base.transform.lossyScale.x < 0f)
		{
			shootAngle = base.transform.rotation * Quaternion.Euler(0f, 0f, 360f - Angle);
		}
		else if (base.transform.lossyScale.y < 0f)
		{
			shootAngle = base.transform.rotation * Quaternion.Euler(0f, 0f, 180f - Angle);
		}
		else
		{
			shootAngle = base.transform.rotation * Quaternion.Euler(0f, 0f, Angle);
		}
	}

	public override void Enable()
	{
		base.Enable();
		lastShot = 0f - initialDelay - firstShotDelay;
	}

	public override void Disable()
	{
		base.Disable();
		shooting = false;
	}

	protected override void Activate()
	{
		base.Activate();
	}

	public override void Reset()
	{
		base.Reset();
		lastShot = 0f - initialDelay - firstShotDelay;
		projectileCounter = 0;
		if (shooting)
		{
			shooting = false;
			if (ShootAnimator != null)
			{
				ShootAnimator.SetTrigger("Reset");
				ShootAnimator.ResetTrigger("Shoot");
			}
		}
	}

	public void ShootProjectile()
	{
		if (disabled || !isActive)
		{
			return;
		}
		GameObject objFromPool = pool.GetObjFromPool();
		if (objFromPool != null)
		{
			Projectile component = objFromPool.GetComponent<Projectile>();
			component.Reset();
			objFromPool.transform.position = base.transform.position + base.transform.rotation * adjustedLaunchPoint;
			objFromPool.transform.rotation = shootAngle;
			objFromPool.SetActive(value: true);
			component.srcPool = pool;
			projectileCounter++;
			component.projectileNumber = projectileCounter;
			component.LaunchedFrom = this;
			component.placedByPlayerNumber = placedByPlayerNumber;
			component.Launch();
			if (base.transform.lossyScale.x < 0f)
			{
				component.gameObject.transform.localScale = new Vector3(0f - component.gameObject.transform.localScale.x, 1f, 1f);
			}
			foreach (CheckColliding item in ActiveCollidersNew)
			{
				Collider2D[] componentsInChildren = item.GetComponentsInChildren<Collider2D>();
				foreach (Collider2D collider in componentsInChildren)
				{
					Collider2D[] componentsInChildren2 = component.gameObject.GetComponentsInChildren<Collider2D>();
					foreach (Collider2D collider2 in componentsInChildren2)
					{
						Physics2D.IgnoreCollision(collider, collider2, ignore: true);
					}
				}
			}
			if (!SoundEventName.NullOrEmpty())
			{
				AkSoundEngine.PostEvent(SoundEventName, base.gameObject);
			}
		}
		else
		{
			Debug.LogError("Couldn't fire projectile...");
		}
		shooting = false;
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		adjustedLaunchPoint = LaunchPoint;
		if (base.transform.localScale.x < 0f)
		{
			adjustedLaunchPoint.x *= -1f;
		}
	}

	public override void Pause()
	{
		base.Pause();
		if (ShootAnimator != null)
		{
			ShootAnimator.speed = 0f;
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (ShootAnimator != null)
		{
			ShootAnimator.speed = 1f;
		}
	}
}
