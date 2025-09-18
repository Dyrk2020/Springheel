using System.Collections;
using UnityEngine;

public class CrumblingBlock : ActiveBlock
{
	public float Delay;

	public float fallDelay = 0.2f;

	protected float fallTime;

	public float colliderOffDelay = 0.1f;

	public bool Collided;

	public float collideTime;

	public bool real;

	public float XAxis_VelocityTriggerValue = 2f;

	public float YAxis_VelocityTriggerValue = 10f;

	public SpriteRenderer mainArtSprite;

	private bool DamageResultDelay;

	private bool fallAudioPlayed;

	public string BlockRootName;

	public bool falling;

	public float gravityValue = -2f;

	public float gravityFloor = -200f;

	protected float velocityY;

	protected Rigidbody2D rb;

	protected Animator animator;

	public ParticleSystem crumbleParticles;

	public Color particleColor;

	public CrumblingBlockScriptableObject artReference;

	public float crumbleFPS = 18f;

	public bool AllowReset;

	private bool surrogateSynced;

	protected override void Start()
	{
		base.Start();
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		ParticleSystem.EmissionModule emission = crumbleParticles.emission;
		emission.enabled = false;
		ParticleSystem.MainModule main = crumbleParticles.main;
		main.startColor = particleColor;
		if (NetSurrogate != null)
		{
			NetSurrogate.IntVal = initialDamage;
		}
	}

	private void ApplyDamageLevelSprite(int damage)
	{
		switch (damage)
		{
		case 0:
			mainArtSprite.sprite = artReference.hold;
			break;
		case 1:
			mainArtSprite.sprite = artReference.broken1;
			break;
		case 2:
			mainArtSprite.sprite = artReference.broken2;
			break;
		case 3:
			mainArtSprite.sprite = artReference.Crumbling[0];
			break;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetSurrogate != null)
		{
			if (!surrogateSynced)
			{
				NetSurrogate.IntVal = damageLevel;
				surrogateSynced = true;
			}
			if (!base.Active && NetSurrogate.IntVal < 10)
			{
				return;
			}
			if (NetSurrogate.IntVal != damageLevel)
			{
				damageLevel = NetSurrogate.IntVal;
				GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
				if (AllowReset && (gameMode == GameState.GameMode.PARTY || gameMode == GameState.GameMode.CREATIVE))
				{
					initialDamage = damageLevel;
				}
				DamageResultDelay = true;
				collideTime = 0f;
				StartCoroutine(emitParticles());
				ApplyDamageLevelSprite(damageLevel);
				if (damageLevel == 1)
				{
					AkSoundEngine.PostEvent("SFX_Pieces_CrumbleBlock_01", base.gameObject);
					animator.SetTrigger("Shake");
				}
				else if (damageLevel == 2)
				{
					AkSoundEngine.PostEvent("SFX_Pieces_CrumbleBlock_02", base.gameObject);
					animator.SetTrigger("ShakeMed");
				}
				else if (damageLevel >= DestroyedDamageLevel)
				{
					AkSoundEngine.PostEvent("SFX_Pieces_CrumbleBlock_03", base.gameObject);
					falling = true;
					fallTime = 0f;
					animator.SetTrigger("ShakeHard");
					spriteSortOrder.setSortOrder(spriteSortOrder.currentBaseOrder + 10000);
				}
			}
			if (DamageResultDelay)
			{
				collideTime += Time.fixedDeltaTime;
				if (collideTime >= Delay)
				{
					DamageResultDelay = false;
				}
			}
		}
		if (!falling || base.Paused)
		{
			return;
		}
		if (!AllowReset || NetSurrogate.IntVal > 10)
		{
			base.MarkedForDestruction = true;
		}
		fallTime += Time.fixedDeltaTime;
		if (fallTime > fallDelay)
		{
			if (!fallAudioPlayed)
			{
				AkSoundEngine.PostEvent("SFX_Pieces_CrumbleBlock_Break", base.gameObject);
				fallAudioPlayed = true;
			}
			if (base.transform.position.y > gravityFloor)
			{
				velocityY += gravityValue * Time.fixedDeltaTime;
				rb.MovePosition(base.transform.position + new Vector3(0f, velocityY * Time.fixedDeltaTime, 0f));
				int num = Mathf.Clamp(Mathf.FloorToInt((fallTime - fallDelay) / (1f / crumbleFPS)), 0, artReference.Crumbling.Length - 1);
				mainArtSprite.sprite = artReference.Crumbling[num];
			}
		}
		if (fallTime > colliderOffDelay)
		{
			SwitchColliderTo(ColliderModeEnum.NoColliders);
		}
	}

	public override void Reset()
	{
		GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
		if (AllowReset && (gameMode == GameState.GameMode.CHALLENGE || gameMode == GameState.GameMode.FREEPLAY))
		{
			ResetDamage();
		}
	}

	public override void Enable()
	{
		base.Enable();
		real = true;
	}

	public override void Disable()
	{
		base.Disable();
		real = false;
	}

	public void DisableSolidCollider()
	{
		SwitchColliderTo(ColliderModeEnum.NoColliders);
	}

	public void CharacterTrigger(Character chr)
	{
		if (base.Active && !DamageResultDelay && NetSurrogate != null && chr != null)
		{
			if (Mathf.Abs(chr.Velocity.x) > XAxis_VelocityTriggerValue || Mathf.Abs(chr.Velocity.y) > YAxis_VelocityTriggerValue)
			{
				BlockHit();
			}
			else if (Mathf.Abs(chr.previousVelocity.x) > XAxis_VelocityTriggerValue || Mathf.Abs(chr.previousVelocity.y) > YAxis_VelocityTriggerValue)
			{
				BlockHit();
			}
		}
	}

	private IEnumerator emitParticles()
	{
		ParticleSystem.EmissionModule emitter = crumbleParticles.emission;
		emitter.enabled = true;
		yield return new WaitForSeconds(0.5f);
		emitter.enabled = false;
	}

	public void BlockHit()
	{
		if (damageLevel == NetSurrogate.IntVal)
		{
			DamageResultDelay = true;
			NetSurrogate.IntVal++;
		}
	}

	public void ProjectileTrigger()
	{
		if (!DamageResultDelay && NetSurrogate != null)
		{
			BlockHit();
		}
	}

	public override void SetInitialDamageLevel(int damage, bool allowDamageReset)
	{
		Debug.Log("Setting initial damage level: " + damage);
		initialDamage = damage;
		SetDamageLevel(damage);
		AllowReset = allowDamageReset;
		ApplyDamageLevelSprite(damage);
	}

	public override void AddBombDamage(int damageAmount)
	{
		NetSurrogate.IntVal = damageAmount;
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
		{
			initialDamage = damageAmount;
		}
	}

	private void SetDamageLevel(int damage)
	{
		if (NetSurrogate != null)
		{
			NetSurrogate.IntVal = damage;
		}
		damageLevel = damage;
		ApplyDamageLevelSprite(damage);
		DamageResultDelay = false;
		collideTime = Delay;
		if (damage == DestroyedDamageLevel)
		{
			Vector3 position = base.transform.position;
			position.y = gravityFloor;
			base.transform.position = position;
		}
	}

	public void ResetDamage()
	{
		Protected = false;
		falling = false;
		fallAudioPlayed = false;
		fallTime = 0f;
		velocityY = 0f;
		base.transform.position = OriginalPosition;
		if (damageLevel == DestroyedDamageLevel && AllowReset)
		{
			GameControl gameControl = ((LobbyManager.instance != null) ? LobbyManager.instance.CurrentGameController : null);
			if (gameControl != null)
			{
				if (gameControl.Phase == GameControl.GamePhase.PLAY || gameControl.Phase == GameControl.GamePhase.SUDDENDEATH)
				{
					SwitchColliderTo(ColliderModeEnum.RunPhase);
				}
				else
				{
					SwitchColliderTo(ColliderModeEnum.PlacedPhase);
				}
			}
		}
		SetDamageLevel(initialDamage);
	}

	public override void Pause()
	{
		base.Pause();
		animator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		animator.speed = 1f;
	}
}
