using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class Projectile : MonoBehaviour, IGameEventListener
{
	public delegate void ProjectileGravityModifier(Projectile currentProjectile, ref float currentGravityScale);

	public static List<Projectile> AllProjectiles = new List<Projectile>();

	public Vector2 Direction;

	public Sprite[] Images;

	public float Speed;

	public Bounds killBoundary;

	public float Lifespan;

	public bool UseGravity;

	public float GravityScale = 1f;

	public bool RotateToAngle;

	public float angleFrictionFactor;

	public float autoRotateDelay;

	public bool collided;

	public string HitSoundEvent;

	public string HitSoundEventPlayer;

	public bool attachOnHit;

	protected Vector3 pauseVel = Vector3.zero;

	protected Vector3 initialPosition;

	protected Quaternion initialRotation;

	protected Vector3 initialScale;

	public GameObject collidedWithObject;

	public GameObject debris;

	public GameObject hitPlayerDebris;

	public ProjectileLauncher LaunchedFrom;

	public SpriteRenderer spriteRenderer;

	public float penetrationDepth;

	public float pushCharacterStrength;

	public float randomHitTargetRange;

	public float timeOfPenetrationEase;

	private float timeAlive;

	private bool scoreboard;

	public int placedByPlayerNumber;

	protected Animator animator;

	[HideInInspector]
	public Teleporter lastExitedTeleporter;

	public float maxLinearSpeed;

	public float headOnAerodynamismFactor = 1f;

	public float sideAerodynamismFactor = 1f;

	public bool movingPlatform;

	public bool DestroyedByAttractorRepulsor;

	public int projectileNumber = -1;

	protected bool BroadCastDestructions = true;

	public bool BoardCastRandomHits;

	public GameObject collisionIndicator;

	protected bool crashing;

	public Transform raycastOrigin;

	public bool destroyedByOtherProjectiles;

	public ObjPool srcPool;

	public static TagComparer.Tag solidPlayerMask = (TagComparer.Tag)160;

	public static TagComparer.Tag playerBodyMask = (TagComparer.Tag)65568;

	public static int attractorRepulsorLayer = 19;

	private ScreenWrapping screenWrapping;

	private bool hasWrapped;

	private Vector3 launchPosition;

	private float destroyRadiusAfterWrap = 2f;

	public static ProjectileGravityModifier ModifyProjectileGravity;

	public bool Paused { get; protected set; }

	private void Awake()
	{
		if (!spriteRenderer)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		if (Images.Length != 0)
		{
			spriteRenderer.sprite = Images[UnityEngine.Random.Range(0, Images.Length)];
		}
		initialPosition = base.transform.position;
		initialRotation = base.transform.rotation;
		initialScale = base.transform.localScale;
		animator = GetComponentInChildren<Animator>();
		screenWrapping = UnityEngine.Object.FindObjectOfType<ScreenWrapping>();
		ChangeListener(adding: true);
	}

	private void OnEnable()
	{
		AllProjectiles.Add(this);
	}

	private void OnDisable()
	{
		AllProjectiles.Remove(this);
	}

	public void Launch()
	{
		Modifiers instance = Modifiers.GetInstance();
		GetComponent<Rigidbody2D>().velocity = base.transform.up * Speed * instance.ProjectileSpeed;
		killBoundary.center = base.transform.position;
		pauseVel = base.transform.up * Speed * instance.ProjectileSpeed;
		launchPosition = base.transform.position;
		hasWrapped = false;
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<LevelResetEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	private void FixedUpdate()
	{
		if (Paused || scoreboard)
		{
			return;
		}
		timeAlive += Time.fixedDeltaTime;
		Modifiers instance = Modifiers.GetInstance();
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		bool flag = false;
		if (screenWrapping != null)
		{
			if (hasWrapped && Vector3.Distance(base.transform.position, launchPosition) < destroyRadiusAfterWrap)
			{
				collided = true;
			}
		}
		else
		{
			flag = !killBoundary.Contains(base.transform.position);
		}
		if (collided || flag || timeAlive > Lifespan / instance.ProjectileSpeed)
		{
			if (BroadCastDestructions && BoardCastRandomHits)
			{
				SignalDestructions();
			}
			bool flag2 = false;
			if (collidedWithObject != null)
			{
				CollisionTag component2 = collidedWithObject.GetComponent<CollisionTag>();
				if (component2 != null)
				{
					flag2 = component2.ContainsAllTags(solidPlayerMask);
				}
			}
			Character character = null;
			if (flag2)
			{
				character = collidedWithObject.transform.GetComponentInParent<Character>();
			}
			if (!movingPlatform && (bool)character && character.hasAuthority && (!character.Dead || !character.Dying) && (!character.isZombie || !character.Dying))
			{
				character.KillCharacter(LaunchedFrom.KillType, deathFreezeOn: true, LaunchedFrom.placedByPlayerNumber);
				SignalDestructions();
			}
			if (!movingPlatform && attachOnHit && flag2 && (bool)hitPlayerDebris && (character.Dead || character.Dying) && !character.isGhost)
			{
				GameObject gameObject;
				Transform transform;
				if ((bool)character)
				{
					gameObject = UnityEngine.Object.Instantiate(hitPlayerDebris, base.transform.position, base.transform.rotation);
					gameObject.transform.localScale = base.transform.localScale;
					transform = character.DeadCollider.transform;
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate(hitPlayerDebris, base.transform.position, base.transform.rotation);
					transform = collidedWithObject.transform;
				}
				gameObject.transform.parent = transform.transform;
				Vector3 vector = new Vector3(UnityEngine.Random.Range(0f - randomHitTargetRange, randomHitTargetRange), UnityEngine.Random.Range(0f - randomHitTargetRange, randomHitTargetRange), 0f);
				Vector3 toDirection = transform.transform.position + vector - gameObject.transform.position;
				toDirection.Normalize();
				Quaternion quaternion = Quaternion.FromToRotation(base.transform.up, toDirection);
				Vector3 vector2 = base.transform.up * (penetrationDepth + UnityEngine.Random.Range(0f, randomHitTargetRange));
				Vector3 endLocalDiff = transform.transform.InverseTransformVector(vector2);
				if (transform.parent.transform.localScale.x < 0f)
				{
					quaternion = Quaternion.Inverse(quaternion);
				}
				ArrowHitDebris component3 = gameObject.GetComponent<ArrowHitDebris>();
				if (component3 != null)
				{
					component3.startSettleHitDebirs(gameObject.transform.localPosition, endLocalDiff, gameObject.transform.localRotation, quaternion, timeOfPenetrationEase);
				}
			}
			else if ((bool)debris)
			{
				UnityEngine.Object.Instantiate(debris, base.transform.position, base.transform.rotation).transform.localScale = base.transform.localScale;
			}
			if ((bool)character && pushCharacterStrength > 0f)
			{
				Vector2 rhs = component.velocity - character.GetComponent<Rigidbody2D>().velocity;
				float num = Mathf.Clamp01(Vector2.Dot(component.velocity, rhs));
				character.AddImpulse(component.velocity * num * pushCharacterStrength, 0.1f);
			}
			srcPool.AddObjToPool(base.gameObject);
		}
		if (UseGravity)
		{
			float currentGravityScale = GravityScale * instance.ProjectileSpeed * instance.GravityScale;
			if (ModifyProjectileGravity != null)
			{
				ModifyProjectileGravity(this, ref currentGravityScale);
			}
			component.gravityScale = currentGravityScale;
		}
		if (RotateToAngle)
		{
			Vector2 velocity = component.velocity;
			if (velocity != Vector2.zero)
			{
				float zRotForVelocity = GetZRotForVelocity(velocity);
				if (autoRotateDelay != 0f)
				{
					SoftRotateToTarget(zRotForVelocity, 1f / autoRotateDelay);
				}
				float num2 = Mathf.Clamp(Vector3.Dot(base.transform.up, velocity.normalized), 0f, 1f) * angleFrictionFactor;
				if (num2 > 0f)
				{
					Vector2 vector3 = velocity.normalized * num2 * Time.deltaTime;
					if (Vector3.Dot(velocity, vector3) >= 0f)
					{
						velocity -= vector3;
					}
				}
			}
		}
		float magnitude = component.velocity.magnitude;
		float num3 = maxLinearSpeed * instance.ProjectileSpeed;
		if (magnitude > num3)
		{
			component.velocity = component.velocity / magnitude * num3;
		}
	}

	public float GetZRotForVelocity(Vector3 vel)
	{
		return 57.29578f * Mathf.Atan2(vel.y, vel.x) - 90f;
	}

	public void Reset()
	{
		base.transform.position = initialPosition;
		base.transform.rotation = initialRotation;
		base.transform.localScale = initialScale;
		crashing = false;
		BroadCastDestructions = false;
		collidedWithObject = null;
		collided = false;
		timeAlive = 0f;
		projectileNumber = -1;
		lastExitedTeleporter = null;
		hasWrapped = false;
	}

	public void SoftRotateToTarget(float targetRot, float inverseTime)
	{
		float z = base.transform.rotation.eulerAngles.z;
		float z2 = targetRot;
		if (inverseTime != 0f)
		{
			float num;
			for (num = targetRot - z; num >= 180f; num -= 360f)
			{
			}
			for (; num < -180f; num += 360f)
			{
			}
			if (Mathf.Abs(num) > 1f)
			{
				z2 = z + num * (Time.deltaTime * inverseTime);
			}
		}
		base.transform.rotation = Quaternion.Euler(0f, 0f, z2);
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (GameState.GetInstance().Paused)
		{
			return;
		}
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (destroyedByOtherProjectiles && component != null && component.ContainsAnyTag(TagComparer.Tag.Hazard))
		{
			Projectile componentInParent = c.GetComponentInParent<Projectile>();
			if (componentInParent != null && componentInParent.LaunchedFrom != LaunchedFrom)
			{
				collidedWithObject = c.gameObject;
				collided = true;
				return;
			}
		}
		bool flag = false;
		if (component != null)
		{
			flag = component.ContainsAnyTag((TagComparer.Tag)33554560);
			component.ContainsAllTags(playerBodyMask);
		}
		if (flag && (LaunchedFrom == null || c.gameObject != LaunchedFrom.gameObject))
		{
			bool flag2 = true;
			bool flag3 = false;
			if (component != null)
			{
				flag3 = component.ContainsAllTags(solidPlayerMask);
			}
			if (flag3)
			{
				CheckCollidingPlayer component2 = c.GetComponent<CheckCollidingPlayer>();
				if (component2 != null)
				{
					if (component2.AssociatedCharacter.InBlackHole)
					{
						flag2 = false;
					}
					else if (DestroyedByAttractorRepulsor)
					{
						if (component2.AssociatedCharacter.hasAuthority)
						{
							SignalDestructions();
							if (HitSoundEventPlayer != "")
							{
								AkSoundEngine.PostEvent(HitSoundEventPlayer, base.gameObject);
							}
						}
						else
						{
							flag2 = false;
						}
					}
				}
			}
			else if (HitSoundEvent != "")
			{
				AkSoundEngine.PostEvent(HitSoundEvent, base.gameObject);
			}
			Projectile componentInParent2 = c.gameObject.GetComponentInParent<Projectile>();
			if (componentInParent2 != null && componentInParent2.LaunchedFrom == LaunchedFrom)
			{
				flag2 = false;
			}
			if (flag2)
			{
				collidedWithObject = c.gameObject;
				collided = true;
			}
		}
		if (!DestroyedByAttractorRepulsor || c.gameObject.layer != attractorRepulsorLayer)
		{
			return;
		}
		bool flag4 = true;
		BlowerPusher component3 = c.gameObject.GetComponent<BlowerPusher>();
		if (component3 != null)
		{
			Vector2 vector = component3.BlowerDirectionTarget.position - component3.BlowerBase.position;
			if (raycastOrigin != null)
			{
				RaycastHit2D raycastHit2D = Physics2D.BoxCast((Vector2)raycastOrigin.position - vector.normalized, new Vector2(0.25f, 0.25f), component3.transform.eulerAngles.z, -vector, 30f, component3.layerMask);
				if (raycastHit2D.collider != null && raycastHit2D.collider.gameObject.GetComponentInParent<UpBlower>() == null)
				{
					flag4 = false;
				}
			}
		}
		if (flag4)
		{
			StartUnstableCrash();
		}
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		if (GameState.GetInstance().Paused || !DestroyedByAttractorRepulsor || c.gameObject.layer != attractorRepulsorLayer)
		{
			return;
		}
		bool flag = false;
		BlowerPusher component = c.gameObject.GetComponent<BlowerPusher>();
		if (component != null)
		{
			Vector3 vector = component.BlowerDirectionTarget.position - component.BlowerBase.position;
			if (raycastOrigin != null)
			{
				RaycastHit2D raycastHit2D = Physics2D.BoxCast(raycastOrigin.position - vector.normalized, new Vector2(0.25f, 0.25f), component.transform.eulerAngles.z, -vector, 30f, component.layerMask);
				if (raycastHit2D.collider != null && raycastHit2D.collider.gameObject.GetComponentInParent<UpBlower>() != null)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			StartUnstableCrash();
		}
	}

	private void OnDestroy()
	{
		if (base.enabled)
		{
			GetComponent<Collider2D>().enabled = false;
			spriteRenderer.enabled = false;
			ChangeListener(adding: false);
		}
	}

	public void Pause()
	{
		pauseVel = GetComponent<Rigidbody2D>().velocity;
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		if (UseGravity)
		{
			GetComponent<Rigidbody2D>().gravityScale = 0f;
		}
		if (animator != null)
		{
			animator.speed = 0f;
		}
	}

	public void Unpause()
	{
		if (!scoreboard && !Paused)
		{
			GetComponent<Rigidbody2D>().velocity = pauseVel;
			if (UseGravity)
			{
				Modifiers instance = Modifiers.GetInstance();
				GetComponent<Rigidbody2D>().gravityScale = GravityScale * instance.ProjectileSpeed * instance.GravityScale;
			}
			if (animator != null)
			{
				animator.speed = 1f;
			}
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if ((type == typeof(StartPhaseEvent) || type == typeof(LevelResetEvent)) && base.gameObject.activeSelf)
		{
			srcPool.AddObjToPool(base.gameObject);
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				Paused = true;
				Pause();
			}
			else
			{
				Paused = false;
				Unpause();
			}
		}
		if (type == typeof(ScoreboardEvent))
		{
			if ((e as ScoreboardEvent).Showing)
			{
				scoreboard = true;
			}
			else
			{
				scoreboard = false;
			}
		}
		if (!(type == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.ProjectileDestroyed)
		{
			return;
		}
		MsgProjectileDestroyed msgProjectileDestroyed = networkMessageReceivedEvent.ReadMessage as MsgProjectileDestroyed;
		if (!(LaunchedFrom != null) || msgProjectileDestroyed.LauncherID != LaunchedFrom.ID || msgProjectileDestroyed.ProjectileNumber != projectileNumber)
		{
			return;
		}
		if (animator != null && DestroyedByAttractorRepulsor)
		{
			animator.SetTrigger("Unstable");
			if (collisionIndicator != null && !collided)
			{
				UnityEngine.Object.Instantiate(collisionIndicator, base.transform.position, base.transform.rotation);
			}
		}
		else
		{
			collided = true;
		}
	}

	public void SignalDestructions()
	{
		MsgProjectileDestroyed msgProjectileDestroyed = new MsgProjectileDestroyed();
		msgProjectileDestroyed.LauncherID = LaunchedFrom.ID;
		msgProjectileDestroyed.ProjectileNumber = projectileNumber;
		NetworkManager.singleton.client.Send(NetMsgTypes.ProjectileDestroyed, msgProjectileDestroyed);
	}

	public void Crash()
	{
		collided = true;
		BroadCastDestructions = false;
	}

	public void StartUnstableCrash()
	{
		if (!crashing)
		{
			crashing = true;
			if (animator != null)
			{
				animator.SetTrigger("Unstable");
			}
			SignalDestructions();
		}
	}

	public void ProjectileTrigger(Collider2D collider)
	{
		if (destroyedByOtherProjectiles)
		{
			Projectile componentInParent = collider.GetComponentInParent<Projectile>();
			if (componentInParent == null || componentInParent.LaunchedFrom != LaunchedFrom)
			{
				collided = true;
				collidedWithObject = collider.gameObject;
			}
		}
	}

	public void ModifyLifespan(float percent)
	{
		timeAlive += Lifespan * percent;
	}

	public void NotifyWrapped()
	{
		hasWrapped = true;
	}
}
