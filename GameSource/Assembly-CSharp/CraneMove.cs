using UnityEngine;

public class CraneMove : ActiveBlock
{
	protected Vector3 initialPosition;

	protected float timeMoving;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	protected Vector2 newVelocity;

	public Animator CraneAnimator;

	public string StartSoundFXString;

	public string StopSoundFXString;

	protected override void Start()
	{
		base.Start();
		initialPosition = base.transform.position;
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	protected override void Activate()
	{
		base.Activate();
		timeMoving = 0f;
		CraneAnimator.SetBool("Moving", value: true);
		AkSoundEngine.PostEvent(StartSoundFXString, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard)
		{
			velocity = ((Vector2)CraneAnimator.transform.position - lastPosition) / deltaTime;
			lastPosition = CraneAnimator.transform.position;
		}
	}

	public override void Reset()
	{
		base.Reset();
		velocity = Vector3.zero;
		CraneAnimator.SetBool("Moving", value: false);
		AkSoundEngine.PostEvent(StopSoundFXString, base.gameObject);
	}

	public override void Pause()
	{
		base.Pause();
		CraneAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		CraneAnimator.speed = 1f;
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		if (velocity.sqrMagnitude > 0f)
		{
			pms[0].Direction = velocity.normalized;
			pms[0].Magnitude = velocity.magnitude;
		}
		else
		{
			pms[0].Direction = Vector2.zero;
			pms[0].Magnitude = 0f;
		}
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		if (velocity.sqrMagnitude > 0f)
		{
			pms[0].Direction = velocity.normalized;
			pms[0].Magnitude = velocity.magnitude;
		}
		else
		{
			pms[0].Direction = Vector2.zero;
			pms[0].Magnitude = 0f;
		}
		return pms;
	}
}
