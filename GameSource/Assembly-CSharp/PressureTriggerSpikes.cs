using UnityEngine;

public class PressureTriggerSpikes : ActiveBlock
{
	public Animator Animator;

	public CheckColliding Trigger;

	public bool triggered;

	protected float LockOutTimer;

	public float lockOutTime = 1f;

	protected bool deathTriggerOnce;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!isActive)
		{
			return;
		}
		if (LockOutTimer > 0f)
		{
			LockOutTimer -= Time.fixedDeltaTime;
			return;
		}
		if (NetSurrogate != null && NetSurrogate.TriggerVal && !triggered)
		{
			Animator.SetBool("Triggered", value: true);
			triggered = true;
		}
		if (!Trigger.Colliding || triggered || triggered)
		{
			return;
		}
		foreach (Character collidingCharacter in Trigger.CollidingCharacters)
		{
			if ((collidingCharacter.isGhost || (!collidingCharacter.Dying && !collidingCharacter.Dead) || !deathTriggerOnce) && !collidingCharacter.Frozen)
			{
				Animator.SetBool("Triggered", value: true);
				triggered = true;
				if (NetSurrogate != null)
				{
					NetSurrogate.TriggerVal = true;
				}
				if ((collidingCharacter.Dying || collidingCharacter.Dead) && !collidingCharacter.isGhost)
				{
					deathTriggerOnce = true;
				}
				break;
			}
		}
	}

	protected override void Activate()
	{
		base.Activate();
		LockOutTimer = 0f;
	}

	public override void Reset()
	{
		triggered = false;
		LockOutTimer = lockOutTime;
		Animator.SetBool("Triggered", value: false);
		deathTriggerOnce = false;
	}

	public override void Enable()
	{
		base.Enable();
		Reset();
	}

	public void SpikesOver()
	{
		triggered = false;
		Animator.SetBool("Triggered", value: false);
	}

	public override void Pause()
	{
		base.Pause();
		Animator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		Animator.speed = 1f;
	}
}
