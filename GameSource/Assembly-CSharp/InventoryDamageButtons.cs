using System.Collections.Generic;
using UnityEngine;

public class InventoryDamageButtons : MonoBehaviour
{
	public List<CrumbleBlockDamageLevelSetter> damageLevelSetters;

	public void OnSetDamageLevel(int damage)
	{
		foreach (CrumbleBlockDamageLevelSetter damageLevelSetter in damageLevelSetters)
		{
			damageLevelSetter.forcedDamageLevel = damage;
			damageLevelSetter.pickableBlock.DamageLevel = damage;
		}
	}
}
