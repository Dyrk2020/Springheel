using System;
using UnityEngine;

public class HorizontalMovingPlatform : ActiveBlock
{
	public float moveSpeed;

	public float moveAngle;

	public float maxDistance;

	public Vector3 moveDir;

	private float amountMoved;

	public Vector3 startPos;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	protected Animator animator;

	public Transform LineHolder;

	protected Vector3 LinePosition;

	protected Rigidbody2D rb;

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInChildren<Animator>();
		startPos = base.transform.position;
		LinePosition = LineHolder.transform.position;
		rb = GetComponent<Rigidbody2D>();
	}

	protected override void Awake()
	{
		base.Awake();
		UpdateMoveDir();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, Modifiers.GetInstance().PlatformMoveSpeed * moveSpeed, moveDir, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		if (amountMoved >= maxDistance)
		{
			moveDir = -1f * moveDir;
			amountMoved = 0f;
		}
		float value = calculateMassRatio();
		value = Mathf.Clamp(value, 1f, MaximumMassSpeedRatio);
		float num = Modifiers.GetInstance().PlatformMoveSpeed * moveSpeed * deltaTime / value;
		if (num + amountMoved > maxDistance)
		{
			num = maxDistance - amountMoved;
		}
		rb.MovePosition(base.transform.position + moveDir * num);
		amountMoved += num;
		velocity = ((Vector2)base.transform.position - lastPosition) / deltaTime;
		lastPosition = base.transform.position;
		LineHolder.transform.position = LinePosition;
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		startPos = base.transform.position;
		UpdateMoveDir();
		LinePosition = LineHolder.transform.position;
	}

	public override void Reset()
	{
		base.Reset();
		base.transform.position = startPos;
		velocity = Vector3.zero;
		UpdateMoveDir();
		amountMoved = 0f;
		LineHolder.transform.position = LinePosition;
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

	public override void Pause()
	{
		base.Pause();
		if (animator != null)
		{
			animator.speed = 0f;
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (animator != null)
		{
			animator.speed = 1f;
		}
	}

	public void UpdateMoveDir()
	{
		float num = moveAngle;
		moveDir = new Vector2(Mathf.Cos(num * MathF.PI / 180f), Mathf.Sin(num * MathF.PI / 180f));
		moveDir = new Vector3(moveDir.x * base.transform.localScale.x, moveDir.y);
		moveDir = base.transform.rotation * moveDir;
		moveDir.Normalize();
	}
}
