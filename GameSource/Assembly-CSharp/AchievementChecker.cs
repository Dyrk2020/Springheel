using System.Collections.Generic;

public abstract class AchievementChecker
{
	public class AchievementThreshold
	{
		public string statName;

		public int threshold;

		public AchievementThreshold(string statName, int threshold)
		{
			this.statName = statName;
			this.threshold = threshold;
		}
	}

	private const int TotalUnlockableOutfits = 44;

	public static Dictionary<string, AchievementThreshold> achievementThresholds = new Dictionary<string, AchievementThreshold>
	{
		{
			"Craftsperson",
			new AchievementThreshold("PiecesGlued", 1)
		},
		{
			"Engineer",
			new AchievementThreshold("LargeContraptionsMade", 1)
		},
		{
			"Clutch_Performer",
			new AchievementThreshold("TotalSuddenDeaths", 20)
		},
		{
			"Wilhelm_Audition",
			new AchievementThreshold("DeathsByFalling", 100)
		},
		{
			"Animal_Cruelty",
			new AchievementThreshold("DeathsByTrap", 100)
		},
		{
			"Archer",
			new AchievementThreshold("DeathsByArrow", 100)
		},
		{
			"Goon",
			new AchievementThreshold("DeathsByHockeyPuck", 100)
		},
		{
			"Not_So_Sharp",
			new AchievementThreshold("DeathsByBarbedWire", 10)
		},
		{
			"Spaghetti_Award",
			new AchievementThreshold("DeathsByBlackHole", 50)
		},
		{
			"Droppin_Bills",
			new AchievementThreshold("CoinsLost", 10)
		},
		{
			"Gettin_the_Hang_of_It",
			new AchievementThreshold("GamesPlayed", 10)
		},
		{
			"Seasoned_Vet",
			new AchievementThreshold("GamesPlayed", 30)
		},
		{
			"Ultimate_Expert",
			new AchievementThreshold("GamesPlayed", 100)
		},
		{
			"Neat_and_Nimble",
			new AchievementThreshold("WallJumps", 1000)
		},
		{
			"Techie",
			new AchievementThreshold("OnlineGamesPlayed", 10)
		},
		{
			"Showoff",
			new AchievementThreshold("OnlineGamesPlayed", 50)
		},
		{
			"Necromancer_Dancer",
			new AchievementThreshold("PostmortemVictories", 10)
		},
		{
			"Greedy_McGreedster",
			new AchievementThreshold("CoinsCollected", 50)
		},
		{
			"Comeback_Kid",
			new AchievementThreshold("ComebackPointsEarned", 50)
		},
		{
			"Solo_Master",
			new AchievementThreshold("SoloPointsEarned", 100)
		},
		{
			"Space_Time_Cadet",
			new AchievementThreshold("TimesTeleported", 50)
		},
		{
			"Trappist",
			new AchievementThreshold("TrapsPlaced", 200)
		},
		{
			"Threat_to_Public_Security",
			new AchievementThreshold("TrapsPlaced", 1000)
		}
	};

	protected static AchievementChecker instance;

	public static AchievementChecker Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new SteamAchievementChecker();
			}
			return instance;
		}
	}

	public static bool PlatformWantsFreshStats => false;

	public void CheckStatThreshold(SaveFileData saveFileData, string achievementName)
	{
		AchievementThreshold achievementThreshold = achievementThresholds[achievementName];
		if (saveFileData.CheckStatCountGoalReached(achievementThreshold.statName, achievementThreshold.threshold))
		{
			SetAchievement(saveFileData, achievementName);
		}
	}

	public abstract void SetAchievement(SaveFileData saveFileData, string achievement);

	public virtual void CheckAllAchievements(SaveFileData saveFileData)
	{
		GamesPlayed_AchievementCheck(saveFileData);
		OnlineGamesPlayed_AchievementCheck(saveFileData);
		Trap_AchievementCheck(saveFileData);
		Character_Unlocked_AchievementCheck(saveFileData);
		Levels_Unlocked_AchievementCheck(saveFileData);
		Outfits_Unlocked_AchievementCheck(saveFileData);
		Death_AchievementChecks(saveFileData);
		Neat_and_Nimble_AchievementChecks(saveFileData);
		Building_AchievementChecks(saveFileData);
		Droppin_Bills_AchievementCheck(saveFileData);
		Point_AchievementChecks(saveFileData);
		Space_Time_Cadet_AchievementCheck(saveFileData);
		Clutch_Performer_AchievementCheck(saveFileData);
	}

	public virtual void Building_AchievementChecks(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Craftsperson");
		CheckStatThreshold(saveFileData, "Engineer");
	}

	public virtual void Character_Unlocked_AchievementCheck(SaveFileData saveFileData)
	{
		if (!saveFileData.IsCheater)
		{
			if (saveFileData.CheckStatBoolArray("CharactersUnlocked", 5))
			{
				SetAchievement(saveFileData, "A_New_Friend_Appears");
			}
			if (saveFileData.CheckStatBoolArray("CharactersUnlocked", 13))
			{
				SetAchievement(saveFileData, "Building_A_Community");
			}
		}
	}

	public virtual void Clutch_Performer_AchievementCheck(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Clutch_Performer");
	}

	public virtual void Death_AchievementChecks(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Wilhelm_Audition");
		CheckStatThreshold(saveFileData, "Animal_Cruelty");
		CheckStatThreshold(saveFileData, "Archer");
		CheckStatThreshold(saveFileData, "Goon");
		CheckStatThreshold(saveFileData, "Not_So_Sharp");
		CheckStatThreshold(saveFileData, "Spaghetti_Award");
	}

	public virtual void Droppin_Bills_AchievementCheck(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Droppin_Bills");
	}

	public virtual void GamesPlayed_AchievementCheck(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Gettin_the_Hang_of_It");
		CheckStatThreshold(saveFileData, "Seasoned_Vet");
		CheckStatThreshold(saveFileData, "Ultimate_Expert");
	}

	public virtual void Levels_Unlocked_AchievementCheck(SaveFileData saveFileData)
	{
		if (!saveFileData.IsCheater)
		{
			if (saveFileData.CheckStatBoolArray("LevelsUnlocked", 2))
			{
				SetAchievement(saveFileData, "Young_Explorer");
			}
			if (saveFileData.CheckStatBoolArray("LevelsUnlocked", 19))
			{
				SetAchievement(saveFileData, "Magellan");
			}
		}
	}

	public virtual void Neat_and_Nimble_AchievementChecks(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Neat_and_Nimble");
	}

	public virtual void OnlineGamesPlayed_AchievementCheck(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Techie");
		CheckStatThreshold(saveFileData, "Showoff");
	}

	public virtual void Outfits_Unlocked_AchievementCheck(SaveFileData saveFileData)
	{
		if (saveFileData.IsCheater)
		{
			return;
		}
		StatCountArray stat = saveFileData.GetStat<StatCountArray>("OutfitsUnlocked");
		if (PlatformWantsFreshStats && !stat.dirty)
		{
			return;
		}
		int num = 0;
		int[] values = stat.values;
		foreach (int num2 in values)
		{
			if ((num2 & 1) != 0)
			{
				num++;
			}
			if ((num2 & 2) != 0)
			{
				num++;
			}
			if ((num2 & 4) != 0)
			{
				num++;
			}
			if ((num2 & 8) != 0)
			{
				num++;
			}
		}
		if (num >= 1)
		{
			SetAchievement(saveFileData, "Gettin_Fancy");
		}
		if (num >= 44)
		{
			SetAchievement(saveFileData, "Full_Wardrobe");
		}
	}

	public virtual void Point_AchievementChecks(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Necromancer_Dancer");
		CheckStatThreshold(saveFileData, "Greedy_McGreedster");
		CheckStatThreshold(saveFileData, "Comeback_Kid");
		CheckStatThreshold(saveFileData, "Solo_Master");
	}

	public virtual void Space_Time_Cadet_AchievementCheck(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Space_Time_Cadet");
	}

	public virtual void Trap_AchievementCheck(SaveFileData saveFileData)
	{
		CheckStatThreshold(saveFileData, "Trappist");
		CheckStatThreshold(saveFileData, "Threat_to_Public_Security");
	}

	public void Takin_On_the_World_AchievementUnlock(SaveFileData saveFileData)
	{
		SetAchievement(saveFileData, "Takin_On_the_World");
	}

	public void Back_to_the_Basics_AchievementUnlock(SaveFileData saveFileData)
	{
		SetAchievement(saveFileData, "Back_to_the_Basics");
	}
}
