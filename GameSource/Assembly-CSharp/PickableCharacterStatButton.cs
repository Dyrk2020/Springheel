using System.Globalization;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class PickableCharacterStatButton : PickableButton
{
	public enum CharacterStatButtonJobs
	{
		AnimalStat,
		Jumps,
		DistanceRun,
		TotalWins,
		TotalSuccess,
		TotalDeaths,
		PostMortemWins,
		SoloPoints,
		Comebackpoints
	}

	public CharacterStatButtonJobs job;

	public Character.Animals AnimalName;

	public Text WinsText;

	public Text GoalsText;

	public Text DeathsText;

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
		StatBoolArray stat = saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked");
		StatCountArray stat2 = saveFileDataForMainUser.GetStat<StatCountArray>("CharacterWins");
		StatCountArray stat3 = saveFileDataForMainUser.GetStat<StatCountArray>("CharacterSuccess");
		StatCountArray stat4 = saveFileDataForMainUser.GetStat<StatCountArray>("CharacterDeaths");
		switch (job)
		{
		case CharacterStatButtonJobs.AnimalStat:
			Show(stat.values[(int)AnimalName]);
			WinsText.text = stat2.values[(int)AnimalName].ToString();
			GoalsText.text = stat3.values[(int)AnimalName].ToString();
			DeathsText.text = stat4.values[(int)AnimalName].ToString();
			break;
		case CharacterStatButtonJobs.Jumps:
			buttonText.text = (saveFileDataForMainUser.GetStat<StatCount>("Jumps").count + saveFileDataForMainUser.GetStat<StatCount>("WallJumps").count).ToString();
			break;
		case CharacterStatButtonJobs.DistanceRun:
			buttonText.text = saveFileDataForMainUser.GetStat<StatFloat>("DistanceRun").value.ToString("F", CultureInfo.CreateSpecificCulture(LocalizationManager.CurrentLanguageCode));
			break;
		case CharacterStatButtonJobs.TotalWins:
		{
			int num3 = 0;
			int[] values = stat2.values;
			foreach (int num4 in values)
			{
				num3 += num4;
			}
			buttonText.text = num3.ToString();
			break;
		}
		case CharacterStatButtonJobs.TotalSuccess:
		{
			int num = 0;
			int[] values = stat3.values;
			foreach (int num2 in values)
			{
				num += num2;
			}
			buttonText.text = num.ToString();
			break;
		}
		case CharacterStatButtonJobs.TotalDeaths:
			buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("TotalDeaths").count.ToString();
			break;
		case CharacterStatButtonJobs.PostMortemWins:
			buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("PostmortemVictories").count.ToString();
			break;
		case CharacterStatButtonJobs.SoloPoints:
			buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("SoloPointsEarned").count.ToString();
			break;
		case CharacterStatButtonJobs.Comebackpoints:
			buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("ComebackPointsEarned").count.ToString();
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
		if ((bool)WinsText)
		{
			WinsText.enabled = show;
		}
		if ((bool)GoalsText)
		{
			GoalsText.enabled = show;
		}
		if ((bool)DeathsText)
		{
			DeathsText.enabled = show;
		}
	}
}
