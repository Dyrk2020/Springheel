using System;
using System.Globalization;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class PickableLevelStatButton : PickableButton
{
	public enum LevelStatButtonJobs
	{
		LevelStat,
		TotalStat,
		PartyPercentage,
		CreativePercentage
	}

	public LevelStatButtonJobs job;

	public GameState.LevelName levelName;

	public Text GamesPlayText;

	public Text TimePlayedText;

	public Text NumRoundPlayedText;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (!Visible || !initialized)
		{
			return;
		}
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		StatBoolArray stat = saveFileDataForMainUser.GetStat<StatBoolArray>("LevelsUnlocked");
		StatCountArray stat2 = saveFileDataForMainUser.GetStat<StatCountArray>("LevelsPlayed");
		StatFloatArray stat3 = saveFileDataForMainUser.GetStat<StatFloatArray>("TotalLevelTime");
		StatCountArray stat4 = saveFileDataForMainUser.GetStat<StatCountArray>("TotalLevelRounds");
		switch (job)
		{
		case LevelStatButtonJobs.LevelStat:
		{
			Show(stat.values[(int)levelName]);
			GamesPlayText.text = stat2.values[(int)levelName].ToString();
			TimeSpan timeSpan2 = TimeSpan.FromSeconds(stat3.values[(int)levelName]);
			TimePlayedText.text = $"{timeSpan2.Hours:D2}:{timeSpan2.Minutes:D2}";
			NumRoundPlayedText.text = stat4.values[(int)levelName].ToString();
			break;
		}
		case LevelStatButtonJobs.TotalStat:
		{
			GamesPlayText.text = saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed").count.ToString();
			TimeSpan timeSpan = TimeSpan.FromSeconds(saveFileDataForMainUser.GetStat<StatFloat>("TotalMatchTime").value);
			TimePlayedText.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}";
			NumRoundPlayedText.text = saveFileDataForMainUser.GetStat<StatCount>("TotalRounds").count.ToString();
			break;
		}
		case LevelStatButtonJobs.PartyPercentage:
			if (saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed").count > 0)
			{
				buttonText.text = ((float)saveFileDataForMainUser.GetStat<StatCount>("PartyModeGamesPlayed").count / (float)saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed").count).ToString("P1", CultureInfo.CreateSpecificCulture(LocalizationManager.CurrentLanguageCode));
			}
			else
			{
				buttonText.text = "0%";
			}
			break;
		case LevelStatButtonJobs.CreativePercentage:
			if (saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed").count > 0)
			{
				buttonText.text = ((float)saveFileDataForMainUser.GetStat<StatCount>("CreativeModeGamesPlayed").count / (float)saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed").count).ToString("P1", CultureInfo.CreateSpecificCulture(LocalizationManager.CurrentLanguageCode));
			}
			else
			{
				buttonText.text = "0%";
			}
			break;
		}
	}

	public override void Enable(bool onOff = true)
	{
		base.Enable(onOff);
	}

	protected void Show(bool show)
	{
		buttonText.enabled = show;
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = show;
		}
		if ((bool)GamesPlayText)
		{
			GamesPlayText.enabled = show;
		}
		if ((bool)TimePlayedText)
		{
			TimePlayedText.enabled = show;
		}
		if ((bool)NumRoundPlayedText)
		{
			NumRoundPlayedText.enabled = show;
		}
	}
}
