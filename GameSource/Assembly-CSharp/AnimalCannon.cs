using System.Collections.Generic;
using UnityEngine;

public class AnimalCannon : ActiveBlock
{
	private class ChrCannonInfo
	{
		public float time;

		public bool added;

		public bool anticipation;

		public bool launched;
	}

	private class CooldownInfo
	{
		public float time;
	}

	public Animator cannonAnimator;

	public Transform LaunchTarget;

	public Transform smokeTransform;

	public Transform blurTransform;

	public Color defaultSmokeColor;

	public float TimeToLaunch = 0.5f;

	public float LaunchAnticipation = 0.2f;

	private Dictionary<Character, ChrCannonInfo> chrsInCannonData = new Dictionary<Character, ChrCannonInfo>();

	public List<Character> chrsToRemoveFromCannon = new List<Character>();

	public static List<AnimalCannon> AllAnimalCannon = new List<AnimalCannon>(128);

	protected int netSurrogateLaunchInt;

	public Transform boosterCenter;

	private HashSet<Projectile> trackedProjectiles = new HashSet<Projectile>();

	private Dictionary<GameObject, CooldownInfo> coolDowns = new Dictionary<GameObject, CooldownInfo>();

	private HashSet<GameObject> coolDownsToRemove = new HashSet<GameObject>();

	public float boostCooldownTime = 0.3f;

	public float boostRadius = 1f;

	public float boostCharacterStrength = 10f;

	public float boostProjectileStrength = 10f;

	public float boostAngle = 45f;

	protected Color nextSmokeColor;

	private int facingDirection = 1;

	protected override void Awake()
	{
		base.Awake();
		AllAnimalCannon.Add(this);
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		facingDirection = ((LaunchTarget.transform.position.x - base.transform.position.x > 0f) ? 1 : (-1));
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		AllAnimalCannon.Remove(this);
	}

	protected override void Act(float deltaTime)
	{
		base.Act(deltaTime);
		if (paused || scoreboard)
		{
			return;
		}
		ProcessChrsInCannon();
		foreach (GameObject key in coolDowns.Keys)
		{
			CooldownInfo cooldownInfo = coolDowns[key];
			cooldownInfo.time += Time.deltaTime;
			if (cooldownInfo.time >= boostCooldownTime)
			{
				coolDownsToRemove.Add(key);
			}
		}
		foreach (GameObject item in coolDownsToRemove)
		{
			coolDowns.Remove(item);
		}
		coolDownsToRemove.Clear();
		foreach (Projectile trackedProjectile in trackedProjectiles)
		{
			if ((trackedProjectile.transform.position - boosterCenter.position).magnitude <= boostRadius && GetCooldownTime(trackedProjectile.gameObject) <= 0f)
			{
				ShootProjectile(trackedProjectile);
			}
		}
		if (NetSurrogate != null && NetSurrogate.TriggerVal)
		{
			cannonAnimator.SetTrigger("Add");
		}
		if (NetSurrogate != null && NetSurrogate.IntVal != netSurrogateLaunchInt)
		{
			netSurrogateLaunchInt = NetSurrogate.IntVal;
			cannonAnimator.SetTrigger("Launch");
		}
	}

	public void ProcessChrsInCannon()
	{
		if (base.Paused)
		{
			return;
		}
		foreach (Character key in chrsInCannonData.Keys)
		{
			ChrCannonInfo chrCannonInfo = chrsInCannonData[key];
			chrCannonInfo.time += Time.fixedDeltaTime;
			if (!chrCannonInfo.added)
			{
				chrCannonInfo.added = true;
				chrCannonInfo.time = 0f - TimeToLaunch;
				key.InCannon = true;
				key.ForceAllowSuicideFor(TimeToLaunch);
				if (NetSurrogate != null)
				{
					NetSurrogate.TriggerVal = true;
				}
			}
			else if (!chrCannonInfo.launched)
			{
				if (chrCannonInfo.time > 0f - LaunchAnticipation && !chrCannonInfo.anticipation)
				{
					chrCannonInfo.anticipation = true;
					if (NetSurrogate != null)
					{
						NetSurrogate.IntVal++;
					}
					GameSettings.animalColors animalColors = default(GameSettings.animalColors);
					GameSettings.animalColors[] characterColors = GameSettings.GetInstance().characterColors;
					for (int i = 0; i < characterColors.Length; i++)
					{
						GameSettings.animalColors animalColors2 = characterColors[i];
						if (animalColors2.type == key.CharacterSprite)
						{
							animalColors = animalColors2;
						}
					}
					nextSmokeColor = animalColors.mainColor;
				}
				if (chrCannonInfo.time >= 0f)
				{
					chrCannonInfo.time = 0f;
					chrCannonInfo.launched = true;
					if (key.Dying)
					{
						key.AddDeathTimerTimer(TimeToLaunch / 2f);
					}
					key.PositionCharacter(base.transform.position);
					key.transform.position = base.transform.position;
					key.Unfreeze();
					key.Facing = facingDirection;
					key.ResetPhysicsVariablesForCannon();
					ShootCharacter(key);
				}
			}
			else if (!(chrCannonInfo.time < GameSettings.GetInstance().cannonRentryTimeOutPeriod))
			{
				chrsToRemoveFromCannon.Add(key);
			}
		}
		foreach (Character item in chrsToRemoveFromCannon)
		{
			chrsInCannonData.Remove(item);
		}
		chrsToRemoveFromCannon.Clear();
	}

	private bool IsPlayer(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Player);
		}
		return false;
	}

	private bool IsBody(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAnyTag(TagComparer.Tag.Body);
		}
		return false;
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		if (!base.Active)
		{
			return;
		}
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (IsPlayer(c.gameObject, component) && IsBody(c.gameObject, component))
		{
			Character component2 = c.transform.parent.GetComponent<Character>();
			if (component2 == null)
			{
				component2 = c.transform.parent.parent.GetComponent<Character>();
			}
			if (component2 != null && !component2.InCannon && !component2.Frozen && component2.hasAuthority && !component2.Success && !chrsInCannonData.ContainsKey(component2))
			{
				component2.ResetPhysicsVariablesForCannon();
				component2.Freeze(hide: true, freezeAnimator: true, disableColliders: true);
				component2.PositionCharacter(base.transform.position);
				chrsInCannonData.Add(component2, new ChrCannonInfo());
				component2.InCannon = true;
				AkSoundEngine.PostEvent("SFX_Pieces_Cannon_Load_Player", base.gameObject);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		CancelLaunch();
		trackedProjectiles.Clear();
		coolDowns.Clear();
	}

	public override void DestroySelf(bool destroyChildren = false, bool useSmoke = true, bool sendNetworkSignal = true)
	{
		CancelLaunch();
		_ = base.MarkedForDestruction;
		base.DestroySelf(destroyChildren, useSmoke, sendNetworkSignal);
	}

	public override void Pause()
	{
		base.Pause();
		cannonAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		cannonAnimator.speed = 1f;
	}

	public void CancelLaunch()
	{
		foreach (Character key in chrsInCannonData.Keys)
		{
			key.Unfreeze();
			key.InCannon = false;
		}
		chrsInCannonData.Clear();
	}

	public void RemoveCharacterFromCannon(Character chr)
	{
		if (chr.Frozen)
		{
			chr.Unfreeze();
		}
		chrsInCannonData.Remove(chr);
	}

	public static void OnCharacterRespawned(Character chr)
	{
		foreach (AnimalCannon item in AllAnimalCannon)
		{
			if (item != null && item.chrsInCannonData.ContainsKey(chr))
			{
				item.RemoveCharacterFromCannon(chr);
			}
		}
	}

	private void ShootCharacter(Character character)
	{
		character.InCannon = false;
		character.ClearVelocity();
		character.AddImpulse((LaunchTarget.position - base.transform.position) * boostCharacterStrength);
		character.EnableBoostEffect(GameSettings.GetInstance().boostEffectDuration);
		Vector3 vector = ((base.transform.localScale.y >= 0f) ? Vector3.up : Vector3.down);
		int mask = LayerMask.GetMask("Default", "Fixed", "Placed");
		RaycastHit2D raycastHit2D = Physics2D.Raycast(LaunchTarget.position, vector, 1f, mask);
		if (raycastHit2D.collider != null)
		{
			if (!raycastHit2D.collider.gameObject.tag.StartsWith("Solid"))
			{
				character.transform.position = LaunchTarget.position;
			}
		}
		else
		{
			character.transform.position = LaunchTarget.position;
		}
		if (character.isGhost)
		{
			character.AudioEventExact("SFX_Pieces_Cannon_Shoot_Player_Ghost");
		}
		else if (character.isZombie)
		{
			character.AudioEventExact("SFX_Pieces_Cannon_Shoot_Player_Zombie");
		}
		else if (character.agonyStarted)
		{
			character.AudioEventExact("SFX_Pieces_Cannon_Shoot_Player_Agony");
		}
		else
		{
			character.AudioEventExact("SFX_Pieces_Cannon_Shoot_Player_" + character.CharacterSFXNameNoCustom);
		}
	}

	public void SmookPoofSpawn()
	{
		SmokePool.Instance.SpawnSmokeTransform(SmokePool.SmokeType.ANIMALCANNON_SMOKE, smokeTransform);
		SmokePool.Instance.SpawnSmokeTransform(SmokePool.SmokeType.ANIMLACANNON_BLUR, blurTransform, nextSmokeColor);
		nextSmokeColor = defaultSmokeColor;
	}

	public void OnProjectileEnterTrigger(Collider2D collision)
	{
		if (base.Active)
		{
			Projectile componentInParent = collision.gameObject.GetComponentInParent<Projectile>();
			if (componentInParent != null)
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Cannon_Load_Projectile", base.gameObject);
				trackedProjectiles.Add(componentInParent);
			}
		}
	}

	public void OnProjectileExitTrigger(Collider2D collision)
	{
		Projectile componentInParent = collision.gameObject.GetComponentInParent<Projectile>();
		if (componentInParent != null)
		{
			trackedProjectiles.Remove(componentInParent);
		}
	}

	private void ShootProjectile(Projectile projectile)
	{
		Rigidbody2D component = projectile.GetComponent<Rigidbody2D>();
		Vector2 velocity = component.velocity;
		Vector3 upwards = (LaunchTarget.position - base.transform.position) * boostProjectileStrength;
		velocity.x = upwards.x;
		velocity.y = upwards.y;
		projectile.transform.position = boosterCenter.transform.position;
		projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, upwards);
		component.velocity = velocity;
		coolDowns[projectile.gameObject] = new CooldownInfo();
		cannonAnimator.SetTrigger("ShootProjectile");
		AkSoundEngine.PostEvent("SFX_Pieces_Cannon_Shoot_Projectile", base.gameObject);
		projectile.ModifyLifespan(0.05f);
	}

	private float GetCooldownTime(GameObject obj)
	{
		if (coolDowns.TryGetValue(obj, out var value))
		{
			return value.time;
		}
		return -1f;
	}
}
