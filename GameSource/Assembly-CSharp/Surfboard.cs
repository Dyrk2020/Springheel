using UnityEngine;

public class Surfboard : ActiveBlock
{
	public Animator SurfboardAnimator;

	private Vector3 initialPosition;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	private bool moving;

	public float MoveSpeed = 10f;

	protected override void Start()
	{
		base.Start();
		initialPosition = base.transform.position;
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard)
		{
			if (moving)
			{
				base.transform.Translate(MoveSpeed * deltaTime, 0f, 0f);
			}
			velocity = ((Vector2)SurfboardAnimator.transform.position - lastPosition) / deltaTime;
			lastPosition = SurfboardAnimator.transform.position;
		}
	}

	public override void Reset()
	{
		base.Reset();
		Debug.Log("Resetting surfboard");
		base.transform.position = initialPosition;
		SurfboardAnimator.SetBool("Surfing", value: false);
		moving = false;
	}

	public void AttachToWave(Transform wave)
	{
		SurfboardAnimator.applyRootMotion = false;
		SurfboardAnimator.SetBool("Surfing", value: true);
		Debug.Log("Surfing");
	}

	public void StartMoving()
	{
		Debug.Log("Starting to move");
		SurfboardAnimator.applyRootMotion = true;
		moving = true;
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

	public override void SpriteAssignmentRule(SpriteRenderer sr)
	{
		sr.sortingLayerName = "Foreground Background";
	}
}
