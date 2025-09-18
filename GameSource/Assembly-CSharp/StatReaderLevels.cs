using UnityEngine;
using UnityEngine.EventSystems;

public class StatReaderLevels : StatReader, IPointerClickHandler, IEventSystemHandler
{
	public enum StatType
	{
		Games,
		Rounds,
		Time,
		Unlocked
	}

	public StatType StatToTrack;

	public GameState.LevelName Level;

	protected override string getValue()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		switch (StatToTrack)
		{
		case StatType.Games:
			return saveFileDataForMainUser.GetStat<StatCountArray>("LevelsPlayed").values[(int)Level].ToString();
		case StatType.Rounds:
			return saveFileDataForMainUser.GetStat<StatCountArray>("TotalLevelRounds").values[(int)Level].ToString();
		case StatType.Time:
		{
			float num = saveFileDataForMainUser.GetStat<StatFloatArray>("TotalLevelTime").values[(int)Level];
			int num2 = Mathf.FloorToInt(num / 60f);
			int num3 = Mathf.FloorToInt(num - (float)(num2 * 60));
			return num2 + ":" + ((num3 < 10) ? "0" : "") + num3;
		}
		case StatType.Unlocked:
			if (!saveFileDataForMainUser.GetStat<StatBoolArray>("LevelsUnlocked").values[(int)Level])
			{
				return "N";
			}
			return "Y";
		default:
			return "0";
		}
	}

	public void ONClick()
	{
		Debug.Log("CLICK");
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		switch (StatToTrack)
		{
		case StatType.Games:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("LevelsPlayed", (int)Level);
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("LevelsPlayed", (int)Level);
			}
			break;
		case StatType.Unlocked:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				StatBoolArray stat = saveFileDataForMainUser.GetStat<StatBoolArray>("LevelsUnlocked");
				stat.Set((int)Level, !stat.values[(int)Level]);
			}
			break;
		}
		TextField.text = getValue();
	}
}
