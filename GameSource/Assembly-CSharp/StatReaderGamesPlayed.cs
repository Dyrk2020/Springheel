using UnityEngine.EventSystems;

public class StatReaderGamesPlayed : StatReader, IPointerClickHandler, IEventSystemHandler
{
	public enum StatType
	{
		GamesPlayed,
		OnlineGamesPlayed,
		PartyModeGamesPlayed,
		CreativeModeGamesPlayed,
		SandboxModeGamesPlayed,
		GamesSinceLastLevelUnlocked,
		GamesSinceLastCharacterLevelUnlocked
	}

	public StatType StatToRead;

	protected override string getValue()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		return StatToRead switch
		{
			StatType.GamesPlayed => saveFileDataForMainUser.GetStat<StatCount>("GamesPlayed").count.ToString(), 
			StatType.OnlineGamesPlayed => saveFileDataForMainUser.GetStat<StatCount>("OnlineGamesPlayed").count.ToString(), 
			StatType.PartyModeGamesPlayed => saveFileDataForMainUser.GetStat<StatCount>("PartyModeGamesPlayed").count.ToString(), 
			StatType.CreativeModeGamesPlayed => saveFileDataForMainUser.GetStat<StatCount>("CreativeModeGamesPlayed").count.ToString(), 
			StatType.SandboxModeGamesPlayed => saveFileDataForMainUser.GetStat<StatCount>("SandboxModeGamesPlayed").count.ToString(), 
			StatType.GamesSinceLastLevelUnlocked => saveFileDataForMainUser.GetStat<StatCount>("GamesSinceLastLevelUnlocked").count.ToString(), 
			StatType.GamesSinceLastCharacterLevelUnlocked => saveFileDataForMainUser.GetStat<StatCount>("GamesSinceLastCharacterLevelUnlocked").count.ToString(), 
			_ => "0", 
		};
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		switch (StatToRead)
		{
		case StatType.GamesPlayed:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("GamesPlayed");
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("GamesPlayed");
			}
			break;
		case StatType.OnlineGamesPlayed:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("OnlineGamesPlayed");
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("OnlineGamesPlayed");
			}
			break;
		case StatType.PartyModeGamesPlayed:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("PartyModeGamesPlayed");
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("PartyModeGamesPlayed");
			}
			break;
		case StatType.CreativeModeGamesPlayed:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("CreativeModeGamesPlayed");
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("CreativeModeGamesPlayed");
			}
			break;
		case StatType.SandboxModeGamesPlayed:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("SandboxModeGamesPlayed");
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("SandboxModeGamesPlayed");
			}
			break;
		case StatType.GamesSinceLastLevelUnlocked:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("GamesSinceLastLevelUnlocked");
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("GamesSinceLastLevelUnlocked");
			}
			break;
		case StatType.GamesSinceLastCharacterLevelUnlocked:
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				saveFileDataForMainUser.IncrementStat("GamesSinceLastCharacterLevelUnlocked");
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				saveFileDataForMainUser.DecrementStat("GamesSinceLastCharacterLevelUnlocked");
			}
			break;
		}
		TextField.text = getValue();
	}
}
