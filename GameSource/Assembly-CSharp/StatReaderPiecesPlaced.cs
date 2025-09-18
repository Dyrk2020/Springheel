public class StatReaderPiecesPlaced : StatReader
{
	public enum StatType
	{
		PiecesPlaced,
		PiecesDestroyed,
		TrapsPlaced,
		TrapsDestroyed,
		PlatformsPlaced,
		PlatformsDestroyed,
		MovingPlatformsPlaced,
		MovingPlatformsDestroyed,
		AttachmentsPlaced,
		AttachmentsDestroyed,
		BombsPlaced,
		SpecialPlaced,
		SpecialDestroyed,
		ItemsPlaced,
		ItemsDestroyed,
		PiecesGlued,
		LargeContraptionsMade
	}

	public StatType StatToRead;

	protected override string getValue()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		return StatToRead switch
		{
			StatType.PiecesPlaced => saveFileDataForMainUser.GetStat<StatCount>("PiecesPlaced").count.ToString(), 
			StatType.PiecesDestroyed => saveFileDataForMainUser.GetStat<StatCount>("PiecesDestroyed").count.ToString(), 
			StatType.TrapsPlaced => saveFileDataForMainUser.GetStat<StatCount>("TrapsPlaced").count.ToString(), 
			StatType.TrapsDestroyed => saveFileDataForMainUser.GetStat<StatCount>("TrapsDestroyed").count.ToString(), 
			StatType.PlatformsPlaced => saveFileDataForMainUser.GetStat<StatCount>("PlatformsPlaced").count.ToString(), 
			StatType.PlatformsDestroyed => saveFileDataForMainUser.GetStat<StatCount>("PlatformsDestroyed").count.ToString(), 
			StatType.MovingPlatformsPlaced => saveFileDataForMainUser.GetStat<StatCount>("MovingPlatformsPlaced").count.ToString(), 
			StatType.MovingPlatformsDestroyed => saveFileDataForMainUser.GetStat<StatCount>("MovingPlatformsDestroyed").count.ToString(), 
			StatType.AttachmentsPlaced => saveFileDataForMainUser.GetStat<StatCount>("AttachmentsPlaced").count.ToString(), 
			StatType.AttachmentsDestroyed => saveFileDataForMainUser.GetStat<StatCount>("AttachmentsDestroyed").count.ToString(), 
			StatType.BombsPlaced => saveFileDataForMainUser.GetStat<StatCount>("BombsPlaced").count.ToString(), 
			StatType.SpecialPlaced => saveFileDataForMainUser.GetStat<StatCount>("SpecialPlaced").count.ToString(), 
			StatType.SpecialDestroyed => saveFileDataForMainUser.GetStat<StatCount>("SpecialDestroyed").count.ToString(), 
			StatType.ItemsPlaced => saveFileDataForMainUser.GetStat<StatCount>("ItemsPlaced").count.ToString(), 
			StatType.ItemsDestroyed => saveFileDataForMainUser.GetStat<StatCount>("ItemsDestroyed").count.ToString(), 
			StatType.PiecesGlued => saveFileDataForMainUser.GetStat<StatCount>("PiecesGlued").count.ToString(), 
			StatType.LargeContraptionsMade => saveFileDataForMainUser.GetStat<StatCount>("LargeContraptionsMade").count.ToString(), 
			_ => "0", 
		};
	}
}
