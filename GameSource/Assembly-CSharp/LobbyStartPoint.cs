using System.Collections.Generic;
using UnityEngine;

public class LobbyStartPoint : MonoBehaviour
{
	public Character.Animals AssociatedCharacter;

	public List<SteamLockoutStrings> SteamLockOutStrings = new List<SteamLockoutStrings>();

	private void Start()
	{
		if (!StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatBoolArray>("CharactersUnlocked").values[(int)AssociatedCharacter])
		{
			Character componentInChildren = base.gameObject.GetComponentInChildren<Character>();
			if (componentInChildren != null && !componentInChildren.Picked)
			{
				componentInChildren.Disable(moveAway: false);
			}
		}
	}
}
