using UnityEngine;

public class VolcanoRockMove : ActiveBlock
{
	public float bladeSpeed;

	protected Vector3 initialPosition;

	protected float timeMoving;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	protected Vector2 newVelocity;

	public Animator IcebergAnimator;

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
		IcebergAnimator.SetBool("Moving", value: true);
		AkSoundEngine.PostEvent("SFX_Level_Iceberg_Boat_Start", base.gameObject);
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
		AkSoundEngine.PostEvent("SFX_Level_Iceberg_Boat_Stop", base.gameObject);
	}

	public override void Pause()
	{
		base.Pause();
		IcebergAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		IcebergAnimator.speed = 1f;
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
