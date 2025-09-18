using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SteamLockoutStrings", order = 1)]
public class SteamLockoutStrings : ScriptableObject
{
	public string nameOfBranchToLockOut;
}
