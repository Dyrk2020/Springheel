using UnityEngine;

public class BitSummitBuildCharacterExcluder : MonoBehaviour
{
	private void Update()
	{
		if (!GameSettings.GetInstance().BitSummitBuild)
		{
			return;
		}
		LobbyStartPoint component = GetComponent<LobbyStartPoint>();
		if (component != null)
		{
			StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatBoolArray>("CharactersUnlocked").Set((int)component.AssociatedCharacter, value: false);
			Character componentInChildren = GetComponentInChildren<Character>();
			if (componentInChildren != null && componentInChildren.Enabled)
			{
				componentInChildren.Disable();
			}
		}
	}
}
