using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class AllowedOnSteamBranch : MonoBehaviour
{
	public List<SteamLockoutStrings> lockedOutFromSteamBranches = new List<SteamLockoutStrings>();

	private void Awake()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		string pchName;
		bool currentBetaName = SteamApps.GetCurrentBetaName(out pchName, 256);
		if (currentBetaName)
		{
			Debug.Log(base.gameObject.name + ":Player is on the beta branch: " + pchName);
		}
		else
		{
			Debug.Log(base.gameObject.name + "Player is on the default (public) branch.");
		}
		if (!currentBetaName)
		{
			return;
		}
		bool flag = false;
		foreach (SteamLockoutStrings lockedOutFromSteamBranch in lockedOutFromSteamBranches)
		{
			if (string.Compare(pchName, lockedOutFromSteamBranch.nameOfBranchToLockOut) == 0)
			{
				flag = true;
			}
		}
		if (flag)
		{
			Debug.Log(base.gameObject.name + " is being deactivate by steam beta branch" + pchName);
			base.gameObject.SetActive(value: false);
		}
	}
}
