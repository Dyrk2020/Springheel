using UnityEngine;

public class TreadmillPart : MultipiecePart
{
	protected Vector2 lastPosition;

	protected Vector2 lastPosition2;

	protected Vector2 velocity;

	private float angularVel;

	private float lastAngle;

	private float lastAngle2;

	public bool track;

	protected override void Awake()
	{
		base.Awake();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.Treadmill, 0f, Vector2.zero, base.gameObject);
		pms[0].Mode = 0;
		pms[0].Switch = false;
	}

	protected override void Start()
	{
		base.Start();
		lastPosition = base.transform.position;
		velocity = Vector2.zero;
		lastAngle = base.transform.localRotation.eulerAngles.z;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		lastPosition2 = lastPosition;
		lastPosition = base.transform.position;
		lastAngle2 = lastAngle;
		lastAngle = base.transform.localEulerAngles.z;
	}

	protected void CalculatePhysicsValues()
	{
		velocity = (lastPosition - lastPosition2) / Time.fixedDeltaTime;
		float num = lastAngle - lastAngle2;
		if (num < -180f)
		{
			num += 360f;
		}
		else if (num > 180f)
		{
			num -= 360f;
		}
		angularVel = num / Time.fixedDeltaTime;
		if (base.transform.parent != null && base.transform.parent.localScale.x < 0f)
		{
			angularVel *= -1f;
		}
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		CalculatePhysicsValues();
		if (Mathf.Abs(angularVel) > 0.001f)
		{
			pms[0].Direction = base.transform.position;
			pms[0].Magnitude = angularVel;
			pms[0].Switch = pms[0].Mode == 0;
			pms[0].Mode = 1;
		}
		else if (velocity.sqrMagnitude > 0f)
		{
			pms[0].Direction = velocity.normalized;
			pms[0].Magnitude = velocity.magnitude;
			pms[0].Switch = pms[0].Mode == 1;
			pms[0].Mode = 0;
		}
		else
		{
			pms[0].Direction = Vector2.zero;
			pms[0].Magnitude = 0f;
			pms[0].Switch = pms[0].Mode == 1;
			pms[0].Mode = 0;
		}
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		CalculatePhysicsValues();
		if (Mathf.Abs(angularVel) > 0.001f)
		{
			pms[0].Direction = base.transform.position;
			pms[0].Magnitude = angularVel;
			pms[0].Switch = pms[0].Mode == 0;
			pms[0].Mode = 1;
		}
		else if (velocity.sqrMagnitude > 0f)
		{
			pms[0].Direction = velocity.normalized;
			pms[0].Magnitude = velocity.magnitude;
			pms[0].Switch = pms[0].Mode == 1;
			pms[0].Mode = 0;
		}
		else
		{
			pms[0].Direction = Vector2.zero;
			pms[0].Magnitude = 0f;
			pms[0].Switch = pms[0].Mode == 1;
			pms[0].Mode = 0;
		}
		return pms;
	}
}
