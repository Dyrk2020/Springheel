using System;

public class Honey : Placeable
{
	public float Stickiness;

	public float JumpStickiness;

	protected override void Awake()
	{
		base.Awake();
		pms = new PhysicsModifier[2];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.JumpForce, JumpStickiness, base.gameObject);
		pms[1] = new PhysicsModifier(PhysicsModifier.ModType.Friction, Stickiness, base.gameObject);
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		pms[0].Magnitude = JumpStickiness;
		pms[1].Magnitude = Stickiness;
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		PhysicsModifier[] physicsModifiers = base.GetPhysicsModifiers();
		PhysicsModifier[] array = new PhysicsModifier[physicsModifiers.Length + 2];
		Array.Copy(physicsModifiers, array, physicsModifiers.Length);
		pms[0].Magnitude = JumpStickiness;
		pms[1].Magnitude = Stickiness;
		array[physicsModifiers.Length] = pms[0];
		array[physicsModifiers.Length + 1] = pms[1];
		return array;
	}
}
