using UnityEngine;

public class PhysicsModifier
{
	public enum ModType
	{
		BaseMotion,
		Rotation,
		Gravity,
		GroundFriction,
		WallFriction,
		Friction,
		JumpForce,
		AirInertia,
		AirInertiaLeft,
		AirInertiaRight,
		Blackhole,
		Treadmill,
		Wind
	}

	public ModType ModifierType;

	public float Magnitude;

	public Vector2 Direction;

	public float Elapsed;

	public bool Switch;

	public int Mode;

	public GameObject Source;

	public PhysicsModifier(ModType modType, float magnitude, GameObject source)
	{
		ModifierType = modType;
		Magnitude = magnitude;
		Direction = Vector2.zero;
		Source = source;
	}

	public PhysicsModifier(ModType modType, float magnitude, Vector2 direction, GameObject source)
	{
		ModifierType = modType;
		Magnitude = magnitude;
		Direction = direction;
		Source = source;
	}

	public PhysicsModifier(PhysicsModifier pm)
	{
		ModifierType = pm.ModifierType;
		Magnitude = pm.Magnitude;
		Direction = pm.Direction;
		Source = pm.Source;
		Mode = pm.Mode;
		Switch = pm.Switch;
	}
}
