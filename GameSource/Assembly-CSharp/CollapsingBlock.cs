using UnityEngine;

public class CollapsingBlock : ActiveBlock
{
	public float Delay;

	public Animator TrapDoorAnimator;

	public bool Collided;

	public float collideTime;

	public bool real;

	private bool soundPlayed;

	public float VyTriggerOffset;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetSurrogate != null && NetSurrogate.TriggerVal)
		{
			Collided = true;
		}
		if (Collided && real)
		{
			collideTime += Time.fixedDeltaTime;
			if (collideTime >= Delay)
			{
				TrapDoorAnimator.SetBool("Open", value: true);
				SwitchColliderTo(ColliderModeEnum.NoColliders);
				if (!soundPlayed)
				{
					AkSoundEngine.PostEvent("SFX_Pieces_Trap_Door", base.gameObject);
					soundPlayed = true;
				}
			}
		}
		else
		{
			soundPlayed = false;
			if (!real)
			{
				Reset();
			}
		}
	}

	public override void Reset()
	{
		TrapDoorAnimator.SetBool("Open", value: false);
		if (real)
		{
			SwitchColliderTo(ColliderModeEnum.RunPhase);
		}
		Collided = false;
		collideTime = 0f;
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

	private void OnCollisionEnter2D(Collision2D c)
	{
		if (!base.Active)
		{
			return;
		}
		Character component = c.gameObject.GetComponent<Character>();
		if (component != null && component.Feet.transform.position.y > base.transform.position.y + VyTriggerOffset)
		{
			Collided = true;
			if (NetSurrogate != null)
			{
				NetSurrogate.TriggerVal = true;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (!base.Active)
		{
			return;
		}
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null && componentInParent.Feet.transform.position.y > base.transform.position.y + VyTriggerOffset)
		{
			Collided = true;
			if (NetSurrogate != null)
			{
				NetSurrogate.TriggerVal = true;
			}
		}
	}

	public override void Pause()
	{
		base.Pause();
		TrapDoorAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		TrapDoorAnimator.speed = 1f;
	}
}
