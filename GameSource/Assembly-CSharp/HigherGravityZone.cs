using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HigherGravityZone : MonoBehaviour
{
	[SerializeField]
	private Modifiers.GravityType insideGravity = Modifiers.GravityType.HIGH;

	[SerializeField]
	private Modifiers.GravityType outsideGravity = Modifiers.GravityType.LOW;

	[SerializeField]
	private Collider2D zoneCollider;

	public List<SpriteRenderer> Arrows = new List<SpriteRenderer>();

	public List<Character> chrsInZone = new List<Character>();

	public List<Projectile> projectilesInZone = new List<Projectile>();

	private void Awake()
	{
		foreach (SpriteRenderer arrow in Arrows)
		{
			arrow.enabled = false;
		}
	}

	private void OnEnable()
	{
		GravityController.AllActiveZones.Add(this);
	}

	private void OnDisable()
	{
		GravityController.AllActiveZones.Remove(this);
	}

	private void FixedUpdate()
	{
		ManageProjectilesInZone();
	}

	private void ManageProjectilesInZone()
	{
		CleanOrphanedProjectiles();
		UpdateActiveProjectiles();
	}

	private void CleanOrphanedProjectiles()
	{
		for (int num = projectilesInZone.Count - 1; num >= 0; num--)
		{
			Projectile projectile = projectilesInZone[num];
			if (projectile == null || !Projectile.AllProjectiles.Contains(projectile))
			{
				projectilesInZone.RemoveAt(num);
			}
		}
	}

	private void UpdateActiveProjectiles()
	{
		foreach (Projectile allProjectile in Projectile.AllProjectiles)
		{
			if (allProjectile.UseGravity)
			{
				bool flag = zoneCollider.OverlapPoint(allProjectile.transform.position);
				bool flag2 = projectilesInZone.Contains(allProjectile);
				if (flag && !flag2)
				{
					projectilesInZone.Add(allProjectile);
				}
				else if (!flag && flag2)
				{
					projectilesInZone.Remove(allProjectile);
				}
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Character character = checkForCharacter(collision);
		if (!(character == null))
		{
			if (!chrsInZone.Contains(character))
			{
				SetGravityOnCharacter(character, insideGravity);
				chrsInZone.Add(character);
			}
			ChangeArrowsVisibility();
		}
	}

	private void ChangeArrowsVisibility()
	{
		bool flag = chrsInZone.Count != 0;
		foreach (SpriteRenderer arrow in Arrows)
		{
			arrow.enabled = flag;
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		Character character = checkForCharacter(collider);
		if (!(character == null))
		{
			if (chrsInZone.Contains(character))
			{
				SetGravityOnCharacter(character, outsideGravity);
				chrsInZone.Remove(character);
			}
			ChangeArrowsVisibility();
		}
	}

	private Character checkForCharacter(Collider2D collider)
	{
		if (!CollisionTag.AllTags.TryGetValue(collider, out var value) || !value.ContainsAnyTag(TagComparer.Tag.Player))
		{
			return null;
		}
		if (collider.name != "LowerBodyTrigger" && collider.name != "DeadCollider")
		{
			return null;
		}
		Rigidbody2D attachedRigidbody = collider.attachedRigidbody;
		if (attachedRigidbody == null)
		{
			return null;
		}
		return attachedRigidbody.GetComponent<Character>();
	}

	private void SetGravityOnCharacter(Character character, Modifiers.GravityType gravityMode)
	{
		switch (gravityMode)
		{
		case Modifiers.GravityType.LOW:
			AkSoundEngine.PostEvent("SFX_Level_SpaceStation_To_Lo_Gravity", base.gameObject);
			break;
		case Modifiers.GravityType.HIGH:
			AkSoundEngine.PostEvent("SFX_Level_SpaceStation_To_Hi_Gravity", base.gameObject);
			break;
		}
		character.ForceGravity(gravityMode);
		character.SetForcedGravityMultiplier(GravityController.GetLevelMultiplierFor(gravityMode));
	}
}
