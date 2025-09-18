public class StatReaderCharacter : StatReader
{
	public enum StatType
	{
		Wins,
		Goals,
		Deaths,
		Unlocked,
		Outfits
	}

	public StatType StatToTrack;

	public Character.Animals Animal;

	protected override string getValue()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		switch (StatToTrack)
		{
		case StatType.Wins:
			return saveFileDataForMainUser.GetStat<StatCountArray>("CharacterWins").values[(int)Animal].ToString();
		case StatType.Goals:
			return saveFileDataForMainUser.GetStat<StatCountArray>("CharacterSuccess").values[(int)Animal].ToString();
		case StatType.Deaths:
			return saveFileDataForMainUser.GetStat<StatCountArray>("CharacterDeaths").values[(int)Animal].ToString();
		case StatType.Unlocked:
			if (!saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked").values[(int)Animal])
			{
				return "N";
			}
			return "Y";
		case StatType.Outfits:
		{
			int val = saveFileDataForMainUser.GetStat<StatCountArray>("OutfitsUnlocked").values[(int)Animal];
			int num = CountOnes(val);
			int numOutfitsForAnimal = UnlockInfoLibrary.Instance.GetNumOutfitsForAnimal(Animal);
			if (num > numOutfitsForAnimal)
			{
				num = numOutfitsForAnimal;
			}
			return num + "/" + numOutfitsForAnimal;
		}
		default:
			return "0";
		}
	}

	private int CountOnes(int val)
	{
		int num = 0;
		while (val > 0)
		{
			num += val % 2;
			val >>= 1;
		}
		return num;
	}

	public void OnClick()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		switch (StatToTrack)
		{
		case StatType.Unlocked:
		{
			StatBoolArray stat = saveFileDataForMainUser.GetStat<StatBoolArray>("CharactersUnlocked");
			stat.Set((int)Animal, !stat.values[(int)Animal]);
			break;
		}
		case StatType.Outfits:
			SFROutfitSelectLogic.Instance.Initialize(Animal, this);
			break;
		}
		TextField.text = getValue();
	}
}
