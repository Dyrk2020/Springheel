using System;
using System.Collections;
using UnityEngine;

public class StatsNextUnlock : StatReader
{
	public float UpdateInterval = 1f;

	protected override void Start()
	{
		base.Start();
		StartCoroutine(updateUnlock(UpdateInterval));
	}

	private IEnumerator updateUnlock(float time)
	{
		while (true)
		{
			yield return new WaitForSeconds(time);
			TextField.text = getValue();
		}
	}

	protected override string getValue()
	{
		GameState.LevelName[] array = (GameState.LevelName[])Enum.GetValues(typeof(GameState.LevelName));
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		StatCountArray stat = saveFileDataForMainUser.GetStat<StatCountArray>("LevelsPlayed");
		StatBoolArray stat2 = saveFileDataForMainUser.GetStat<StatBoolArray>("LevelsUnlocked");
		StatBoolArray stat3 = saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked");
		StatCountArray stat4 = saveFileDataForMainUser.GetStat<StatCountArray>("OutfitsUnlocked");
		StatCount stat5 = saveFileDataForMainUser.GetStat<StatCount>("GamesSinceLastLevelUnlocked");
		StatCount stat6 = saveFileDataForMainUser.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked");
		StatCount stat7 = saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed");
		if (stat.values[0] > 0 && stat.values[1] > 0 && !stat2.values[2])
		{
			return "OLDMANSION";
		}
		if (stat5.count > 2)
		{
			for (int i = 1; i != array.Length - 1; i++)
			{
				if (i != 10)
				{
					if (array[i + 1] >= GameState.LevelName.RANDOM)
					{
						break;
					}
					int num = i + 1;
					if (i == 9)
					{
						num++;
					}
					if (stat.values[i] > 0 && !stat2.values[num] && stat2.values[i])
					{
						return array[num].ToString();
					}
				}
			}
		}
		if (stat6.count >= 3)
		{
			if (stat2.values[2] && !stat3.values[6])
			{
				return "Squirrel";
			}
			if (stat2.values[4] && !stat3.values[5])
			{
				return "Chameleon";
			}
			if (stat2.values[6] && !stat3.values[7])
			{
				return "Robot Bunny";
			}
			if (stat2.values[7] && !stat3.values[8])
			{
				return "Elephant";
			}
			if (stat2.values[11] && !stat3.values[9])
			{
				return "Monkey";
			}
			if (stat2.values[13] && !stat3.values[10])
			{
				return "Snake";
			}
			if (stat2.values[14] && !stat3.values[11])
			{
				return "Hippo";
			}
			if (stat2.values[19] && !stat3.values[12])
			{
				return "Turtle";
			}
			if (stat2.values[22] && !stat3.values[14])
			{
				return "Fox";
			}
			if (stat2.values[23] && !stat3.values[15])
			{
				return "Platypus";
			}
		}
		if (stat7.count >= 3)
		{
			for (int j = 1; j != stat4.values.Length; j++)
			{
				if (stat4.values[j] < 3)
				{
					return "Possible outfit?";
				}
			}
		}
		return "None";
	}
}
