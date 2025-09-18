using UnityEngine;

public class WaveMove : ActiveBlock
{
	public float MoveSpeed = 10f;

	public float StartTime = 3f;

	public float EndXPos = 90f;

	public Surfboard Surfboard;

	private float startTimer;

	private Vector3 initialPosition;

	private Vector3 velocity;

	private Vector3 lastPosition;

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
	}

	protected override void Act(float deltaTime)
	{
		if (paused || scoreboard)
		{
			return;
		}
		startTimer += deltaTime;
		if (startTimer >= StartTime)
		{
			velocity = (base.transform.position - lastPosition) / deltaTime;
			lastPosition = base.transform.position;
			base.transform.Translate(MoveSpeed * deltaTime, 0f, 0f);
			if (base.transform.position.x > EndXPos)
			{
				base.transform.position = initialPosition;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Debug.Log("Resetting wave");
		base.transform.position = initialPosition;
	}

	public override void Pause()
	{
		base.Pause();
	}

	public override void Unpause()
	{
		base.Unpause();
	}

	public void AttachSurfboard()
	{
		Surfboard.AttachToWave(base.transform);
	}

	public override void SpriteAssignmentRule(SpriteRenderer sr)
	{
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

	private void OnTriggerEnter2D(Collider2D c)
	{
		if (c.gameObject.GetComponent<Surfboard>() != null)
		{
			AttachSurfboard();
		}
	}
}
