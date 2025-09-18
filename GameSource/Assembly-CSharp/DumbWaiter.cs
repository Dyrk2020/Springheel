using System.Collections;
using GameEvent;
using UnityEngine;

public class DumbWaiter : ActiveBlock
{
	public Animator Animator;

	public Animator MotorAnimator;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	public bool MovementActivated;

	public float netMovementDelay = 0.5f;

	protected override void Start()
	{
		base.Start();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	protected void Update()
	{
		if (NetSurrogate != null && NetSurrogate.TriggerVal && !MovementActivated)
		{
			StartMovement();
		}
	}

	public void Go()
	{
		if (NetSurrogate != null)
		{
			NetSurrogate.TriggerVal = true;
			float num = netMovementDelay - LobbyManager.instance.GetAveragePingToServer();
			if (num < 0f)
			{
				num = 0f;
			}
			NetSurrogate.FloatVal = num;
		}
	}

	public void StartMovement()
	{
		if (base.Active && !MovementActivated)
		{
			MovementActivated = true;
			StartCoroutine(DelayStartAnimator(NetSurrogate.FloatVal - LobbyManager.instance.GetAveragePingToServer()));
		}
	}

	private IEnumerator DelayStartAnimator(float time)
	{
		do
		{
			yield return null;
			time -= Time.deltaTime;
		}
		while (time > 0f);
		if (base.Active && MovementActivated)
		{
			Animator.SetBool("Moving", value: true);
			MotorAnimator.SetBool("Moving", value: true);
		}
	}

	public void MovementDone()
	{
		MovementActivated = false;
		Animator.SetBool("Moving", value: false);
		MotorAnimator.SetBool("Moving", value: false);
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
		MotorAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		Animator.speed = 1f;
		MotorAnimator.speed = 1f;
	}

	public override void Reset()
	{
		base.Reset();
		Animator.SetBool("Moving", value: false);
		MotorAnimator.SetBool("Moving", value: false);
		MovementActivated = false;
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
			Animator.SetBool("Moving", value: false);
			MotorAnimator.SetBool("Moving", value: false);
		}
	}
}
