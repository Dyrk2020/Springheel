using System;

public class Ice : MultipiecePart
{
	public float Slickness;

	protected override void Awake()
	{
		base.Awake();
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.Friction, 0f - Slickness, base.gameObject);
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		pms[0].Magnitude = 0f - Slickness;
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		PhysicsModifier[] physicsModifiers = base.GetPhysicsModifiers();
		PhysicsModifier[] array = new PhysicsModifier[physicsModifiers.Length + 1];
		Array.Copy(physicsModifiers, array, physicsModifiers.Length);
		pms[0].Magnitude = 0f - Slickness;
		array[physicsModifiers.Length] = pms[0];
		return array;
	}
}
