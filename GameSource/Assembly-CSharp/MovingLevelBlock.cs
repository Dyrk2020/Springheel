using UnityEngine;

public class MovingLevelBlock : MultipiecePart
{
	protected Vector2 lastPosition;

	protected Vector2 velocity;

	protected Vector2 lastVelocity;

	protected Vector2 newVelocity;

	public string CustomAttachedSpriteLayer;

	protected override void Awake()
	{
		base.Awake();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard)
		{
			velocity = ((Vector2)base.transform.position - lastPosition) / deltaTime;
			lastPosition = base.transform.position;
		}
	}

	public override void Reset()
	{
		base.Reset();
		velocity = Vector3.zero;
		lastPosition = base.transform.position;
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
		if (CustomAttachedSpriteLayer == "")
		{
			base.SpriteAssignmentRule(sr);
		}
		else
		{
			sr.sortingLayerName = CustomAttachedSpriteLayer;
		}
	}
}
