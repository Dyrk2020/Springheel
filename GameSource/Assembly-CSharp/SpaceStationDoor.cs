using GameEvent;
using UnityEngine;

public class SpaceStationDoor : ActiveBlock
{
	public Animator Animator;

	public SpaceStationDoor followOtherDoor;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	public bool MovementActivated;

	protected bool lastNetsurrogateBool;

	protected override void Start()
	{
		base.Start();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
		if (followOtherDoor != null)
		{
			NetSurrogate = followOtherDoor.NetSurrogate;
		}
	}

	protected void Update()
	{
		if (NetSurrogate != null && !MovementActivated && lastNetsurrogateBool != NetSurrogate.BoolVal)
		{
			lastNetsurrogateBool = NetSurrogate.BoolVal;
			Animator.SetBool("Bool", NetSurrogate.BoolVal);
		}
	}

	public void Toggle()
	{
		if (NetSurrogate != null && !MovementActivated)
		{
			NetSurrogate.BoolVal = !NetSurrogate.BoolVal;
		}
	}

	public void MovementDone()
	{
		MovementActivated = false;
	}

	public void MovementStarted()
	{
		MovementActivated = true;
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard)
		{
			velocity = ((Vector2)Animator.transform.position - lastPosition) / deltaTime;
			lastPosition = Animator.transform.position;
		}
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

	public override void Reset()
	{
		base.Reset();
		NetSurrogate.BoolVal = false;
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

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(LevelResetEvent))
		{
			Reset();
		}
	}
}
