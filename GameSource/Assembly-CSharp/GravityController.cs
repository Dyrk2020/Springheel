using System;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
	public static List<HigherGravityZone> AllActiveZones = new List<HigherGravityZone>();

	public const float DEFAULT_MULTIPLIER = 1f;

	[Range(0.1f, 2f)]
	[SerializeField]
	[Header("Higher values increase gravity strength.")]
	private float normalGravityMultiplier = 1f;

	[SerializeField]
	[Range(0.1f, 2f)]
	private float lowGravityMultiplier = 1f;

	[Range(0.1f, 2f)]
	[SerializeField]
	private float highGravityMultiplier = 1f;

	public static GravityController Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		Projectile.ModifyProjectileGravity = (Projectile.ProjectileGravityModifier)Delegate.Combine(Projectile.ModifyProjectileGravity, new Projectile.ProjectileGravityModifier(CalculateProjectileGravity));
	}

	private void OnDisable()
	{
		Projectile.ModifyProjectileGravity = (Projectile.ProjectileGravityModifier)Delegate.Remove(Projectile.ModifyProjectileGravity, new Projectile.ProjectileGravityModifier(CalculateProjectileGravity));
	}

	private void CalculateProjectileGravity(Projectile targetProjectile, ref float currentGravityScale)
	{
		Modifiers.GravityType gravityTypeForProjectile = GetGravityTypeForProjectile(targetProjectile);
		currentGravityScale *= GetAppliedGravityModifier(gravityTypeForProjectile);
	}

	private float GetAppliedGravityModifier(Modifiers.GravityType appliedGravityType)
	{
		Modifiers instance = Modifiers.GetInstance();
		float num = instance.GravityValues[0];
		return instance.GravityValues[(int)appliedGravityType] / num * GetLevelMultiplier(appliedGravityType);
	}

	public static Modifiers.GravityType GetGravityTypeForProjectile(Projectile targetProjectile)
	{
		if (AllActiveZones.Count == 0)
		{
			return Modifiers.GravityType.NORMAL;
		}
		foreach (HigherGravityZone allActiveZone in AllActiveZones)
		{
			if (allActiveZone.projectilesInZone.Contains(targetProjectile))
			{
				return Modifiers.GravityType.HIGH;
			}
		}
		return Modifiers.GravityType.LOW;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static float GetLevelMultiplierFor(Modifiers.GravityType gravityType)
	{
		if (Instance != null)
		{
			return Instance.GetLevelMultiplier(gravityType);
		}
		return 1f;
	}

	private float GetLevelMultiplier(Modifiers.GravityType gravityType)
	{
		return gravityType switch
		{
			Modifiers.GravityType.LOW => lowGravityMultiplier, 
			Modifiers.GravityType.HIGH => highGravityMultiplier, 
			_ => normalGravityMultiplier, 
		};
	}
}
