using Steamworks;
using UnityEngine;

public class SteamAchievementChecker : AchievementChecker
{
	private bool storeAchievements;

	public override void SetAchievement(SaveFileData saveFileData, string achievement)
	{
		if (SteamManager.Initialized)
		{
			bool pbAchieved = false;
			if (SteamUserStats.GetAchievement(achievement, out pbAchieved) && !pbAchieved)
			{
				SteamUserStats.SetAchievement(achievement);
				Debug.Log("Unlocked Steam Achievement: " + achievement);
			}
			if (storeAchievements)
			{
				SteamUserStats.StoreStats();
			}
		}
	}

	public override void CheckAllAchievements(SaveFileData saveFileData)
	{
		storeAchievements = false;
		base.CheckAllAchievements(saveFileData);
		storeAchievements = true;
		if (SteamManager.Initialized)
		{
			SteamUserStats.StoreStats();
		}
	}
}
