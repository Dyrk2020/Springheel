using UnityEngine;

public class ColliderHittingCharacter : MonoBehaviour
{
	public Transform CenterOfMass;

	protected Vector3 previousCenterOfMass;

	protected Vector3 velocity;

	public bool normalizeVector;

	public bool ignoreCom;

	public float minimumVelocitySquared = 40f;

	public float Strength;

	public float extraUp;

	private void FixedUpdate()
	{
		velocity = (CenterOfMass.position - previousCenterOfMass) / Time.fixedDeltaTime;
		previousCenterOfMass = CenterOfMass.position;
	}

	private void OnCollisionEnter2D(Collision2D c)
	{
		Character component = c.gameObject.GetComponent<Character>();
		if (!(component == null) && !component.isGhost)
		{
			AddImpulseToCharacter(component, velocity);
		}
	}

	public void DeathImpulse(Character chr)
	{
		AddImpulseToCharacter(chr, velocity);
	}

	protected void AddImpulseToCharacter(Character chr, Vector2 inputVelocity)
	{
		Vector2 vector = chr.transform.position - CenterOfMass.position;
		float num = Vector2.Dot(velocity.normalized, vector.normalized);
		if (velocity.sqrMagnitude < minimumVelocitySquared)
		{
			return;
		}
		if (ignoreCom)
		{
			if (normalizeVector)
			{
				chr.AddImpulse(inputVelocity.normalized * Strength + Vector2.up * extraUp, 0.5f);
			}
			else
			{
				chr.AddImpulse(inputVelocity * Strength + Vector2.up * extraUp, 0.5f);
			}
		}
		else if (num > 0f && chr.AddImpulse((Vector2)velocity.normalized * Strength * num + Vector2.up * extraUp, 0.3f))
		{
			Debug.DrawRay(CenterOfMass.position, (Vector2)velocity.normalized * Strength * num + Vector2.up * extraUp, Color.blue);
			Debug.DrawRay(CenterOfMass.position, vector, Color.green);
			Debug.DrawRay(CenterOfMass.position, velocity, Color.red);
		}
	}
}
