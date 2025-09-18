using UnityEngine;

public class InheritMotion : MonoBehaviour
{
	protected PhysicsModifier[] pms;

	protected Vector2 lastPosition;

	protected Vector2 lastLastPosition;

	public bool UseNewCalculationMethod;

	private void Start()
	{
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector3.zero, base.gameObject);
	}

	private void FixedUpdate()
	{
		if (UseNewCalculationMethod)
		{
			lastLastPosition = lastPosition;
		}
		lastPosition = base.transform.position;
	}

	public PhysicsModifier[] GetPhysicsModifiers()
	{
		Vector2 vector = ((!UseNewCalculationMethod) ? (((Vector2)base.transform.position - lastPosition) / Time.fixedDeltaTime) : (((Vector2)base.transform.position - lastLastPosition) / Time.fixedDeltaTime));
		if (vector.sqrMagnitude > 0f)
		{
			pms[0].Direction = vector.normalized;
			pms[0].Magnitude = vector.magnitude;
		}
		else
		{
			pms[0].Direction = Vector2.zero;
			pms[0].Magnitude = 0f;
		}
		return pms;
	}
}
