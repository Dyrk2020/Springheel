using UnityEngine;

public class IcebergMove : ActiveBlock
{
	public float bladeSpeed;

	public Animator BladeAnimator;

	protected Vector3 initialPosition;

	protected float timeMoving;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	protected Vector2 newVelocity;

	public Animator IcebergAnimator;

	public string StartSoundFXString;

	public string StopSoundFXString;

	protected override void Start()
	{
		base.Start();
		initialPosition = base.transform.position;
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	protected void Update()
	{
		if (BladeAnimator != null)
		{
			BladeAnimator.SetFloat("Speed", bladeSpeed);
		}
	}

	protected override void Activate()
	{
		base.Activate();
		timeMoving = 0f;
		IcebergAnimator.SetBool("Moving", value: true);
		AkSoundEngine.PostEvent(StartSoundFXString, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard)
		{
			velocity = ((Vector2)IcebergAnimator.transform.position - lastPosition) / deltaTime;
			lastPosition = IcebergAnimator.transform.position;
		}
	}

	public override void Reset()
	{
		base.Reset();
		velocity = Vector3.zero;
		IcebergAnimator.SetBool("Moving", value: false);
		AkSoundEngine.PostEvent(StopSoundFXString, base.gameObject);
	}

	public override void Pause()
	{
		base.Pause();
		IcebergAnimator.speed = 0f;
		if (BladeAnimator != null)
		{
			BladeAnimator.speed = 0f;
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		IcebergAnimator.speed = 1f;
		if (BladeAnimator != null)
		{
			BladeAnimator.speed = 1f;
		}
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
