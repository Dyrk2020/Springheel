public class StatReaderPoints : StatReader
{
	public enum StatType
	{
		CoinsCollected,
		CoinsLost,
		CoinsStolen,
		ComebackPointsEarned,
		SoloPointsEarned,
		TrapPointsEarned,
		PostmortemVictories
	}

	public StatType StatToTrack;

	protected override string getValue()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		return StatToTrack switch
		{
			StatType.CoinsCollected => saveFileDataForMainUser.GetStat<StatCount>("CoinsCollected").count.ToString(), 
			StatType.CoinsLost => saveFileDataForMainUser.GetStat<StatCount>("CoinsLost").count.ToString(), 
			StatType.CoinsStolen => saveFileDataForMainUser.GetStat<StatCount>("CoinsStolen").count.ToString(), 
			StatType.ComebackPointsEarned => saveFileDataForMainUser.GetStat<StatCount>("ComebackPointsEarned").count.ToString(), 
			StatType.SoloPointsEarned => saveFileDataForMainUser.GetStat<StatCount>("SoloPointsEarned").count.ToString(), 
			StatType.TrapPointsEarned => saveFileDataForMainUser.GetStat<StatCount>("TrapPointsEarned").count.ToString(), 
			StatType.PostmortemVictories => saveFileDataForMainUser.GetStat<StatCount>("PostmortemVictories").count.ToString(), 
			_ => "0", 
		};
	}
}
