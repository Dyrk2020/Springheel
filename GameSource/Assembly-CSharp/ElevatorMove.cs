using UnityEngine;

public class ElevatorMove : ActiveBlock
{
	protected Vector3 initialPosition;

	protected float timeMoving;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	protected Vector2 newVelocity;

	public Animator ElevatorAnimator;

	public GameObject OpeningWall;

	private bool animating;

	protected override void Start()
	{
		base.Start();
		initialPosition = base.transform.position;
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
		if (GameState.GetInstance().UsingHotSeat)
		{
			ElevatorAnimator.SetBool("StopInstant", value: true);
		}
	}

	protected override void Activate()
	{
		base.Activate();
		timeMoving = 0f;
		animating = true;
		ElevatorAnimator.SetBool("Moving", value: true);
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard)
		{
			velocity = ((Vector2)ElevatorAnimator.transform.position - lastPosition) / deltaTime;
			lastPosition = ElevatorAnimator.transform.position;
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (GameState.GetInstance().UsingHotSeat)
		{
			InstantReset();
		}
		StopElevator();
	}

	private void StopElevator()
	{
		if (animating)
		{
			velocity = Vector3.zero;
			animating = false;
			ElevatorAnimator.SetBool("Moving", value: false);
		}
	}

	public override void Pause()
	{
		base.Pause();
		ElevatorAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		ElevatorAnimator.speed = 1f;
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

	protected override void ToSuddenDeath()
	{
		InstantReset();
		StopElevator();
		base.ToSuddenDeath();
	}

	private void InstantReset()
	{
		if (animating)
		{
			velocity = Vector3.zero;
			ElevatorAnimator.Play("ElevatorStop");
			ElevatorAnimator.SetBool("Moving", value: true);
			base.transform.position = initialPosition;
		}
	}

	public override void ToPlayMode()
	{
		if (GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
		{
			base.ToPlayMode();
		}
	}
}
