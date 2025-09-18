using System;
using UnityEngine;

public class MovingPlatform : ActiveBlock
{
	public float moveSpeed;

	public float moveAngle;

	public float maxDistance;

	public Transform Fan;

	public Vector3 moveDir;

	private float amountMoved;

	public Vector3 startPos;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	protected Animator animator;

	protected Rigidbody2D rb;

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInChildren<Animator>();
		startPos = base.transform.position;
		rb = GetComponent<Rigidbody2D>();
	}

	protected override void Awake()
	{
		base.Awake();
		startPos = base.transform.position;
		float num = base.transform.eulerAngles.z + moveAngle;
		moveDir = new Vector2(Mathf.Cos(num * MathF.PI / 180f), Mathf.Sin(num * MathF.PI / 180f));
		moveDir.Normalize();
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
	}

	public override void Enable()
	{
		base.Enable();
		Fan.GetComponent<Collider2D>().enabled = true;
		Fan.GetComponent<Renderer>().enabled = true;
	}

	public override void Disable()
	{
		base.Disable();
		Fan.GetComponent<Collider2D>().enabled = false;
		Fan.GetComponent<Renderer>().enabled = false;
	}

	public override void Place(int playerNumber, bool sendEvent, bool force = false)
	{
		base.Place(playerNumber, sendEvent, force);
		startPos = base.transform.position;
		float num = base.transform.eulerAngles.z + moveAngle;
		moveDir = new Vector2(Mathf.Cos(num * MathF.PI / 180f), Mathf.Sin(num * MathF.PI / 180f));
	}

	public override void Reset()
	{
		base.Reset();
		base.transform.position = startPos;
		velocity = Vector3.zero;
		float num = base.transform.eulerAngles.z + moveAngle;
		moveDir = new Vector2(Mathf.Cos(num * MathF.PI / 180f), Mathf.Sin(num * MathF.PI / 180f));
		moveDir.Normalize();
		amountMoved = 0f;
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
		animator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		animator.speed = 1f;
	}
}
