public class StatReaderDeaths : StatReader
{
	public enum StatTypes
	{
		TotalDeaths,
		DeathsByTrap,
		DeathsBySuicide,
		DeathsByFalling,
		DeathsByHazard,
		DeathsBySpikeBall,
		DeathsByBarbedWire,
		DeathsByArrow,
		DeathsByTennisBall,
		DeathsBySpinningSaw,
		DeathsByLinearSaw,
		DeathsByPropeller,
		DeathsByFlippingBlock,
		DeathsByBlackHole,
		DeathsByHockeyPuck,
		DeathsByPunchingPlant,
		DeathsByPressureTriggerSpikes,
		DeathsByWreckingBall
	}

	public StatTypes StatToRead;

	protected override string getValue()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		return StatToRead switch
		{
			StatTypes.TotalDeaths => saveFileDataForMainUser.GetStat<StatCount>("TotalDeaths").count.ToString(), 
			StatTypes.DeathsByTrap => saveFileDataForMainUser.GetStat<StatCount>("DeathsByTrap").count.ToString(), 
			StatTypes.DeathsBySuicide => saveFileDataForMainUser.GetStat<StatCount>("DeathsBySuicide").count.ToString(), 
			StatTypes.DeathsByFalling => saveFileDataForMainUser.GetStat<StatCount>("DeathsByFalling").count.ToString(), 
			StatTypes.DeathsByHazard => saveFileDataForMainUser.GetStat<StatCount>("DeathsByHazard").count.ToString(), 
			StatTypes.DeathsBySpikeBall => saveFileDataForMainUser.GetStat<StatCount>("DeathsBySpikeBall").count.ToString(), 
			StatTypes.DeathsByBarbedWire => saveFileDataForMainUser.GetStat<StatCount>("DeathsByBarbedWire").count.ToString(), 
			StatTypes.DeathsByArrow => saveFileDataForMainUser.GetStat<StatCount>("DeathsByArrow").count.ToString(), 
			StatTypes.DeathsByTennisBall => saveFileDataForMainUser.GetStat<StatCount>("DeathsByTennisBall").count.ToString(), 
			StatTypes.DeathsBySpinningSaw => saveFileDataForMainUser.GetStat<StatCount>("DeathsBySpinningSaw").count.ToString(), 
			StatTypes.DeathsByLinearSaw => saveFileDataForMainUser.GetStat<StatCount>("DeathsByLinearSaw").count.ToString(), 
			StatTypes.DeathsByPropeller => saveFileDataForMainUser.GetStat<StatCount>("DeathsByPropeller").count.ToString(), 
			StatTypes.DeathsByFlippingBlock => saveFileDataForMainUser.GetStat<StatCount>("DeathsByFlippingBlock").count.ToString(), 
			StatTypes.DeathsByBlackHole => saveFileDataForMainUser.GetStat<StatCount>("DeathsByBlackHole").count.ToString(), 
			StatTypes.DeathsByHockeyPuck => saveFileDataForMainUser.GetStat<StatCount>("DeathsByHockeyPuck").count.ToString(), 
			StatTypes.DeathsByPunchingPlant => saveFileDataForMainUser.GetStat<StatCount>("DeathsByPunchingPlant").count.ToString(), 
			StatTypes.DeathsByPressureTriggerSpikes => saveFileDataForMainUser.GetStat<StatCount>("DeathsByPressureTriggerSpikes").count.ToString(), 
			StatTypes.DeathsByWreckingBall => saveFileDataForMainUser.GetStat<StatCount>("DeathsByWreckingBall").count.ToString(), 
			_ => "0", 
		};
	}
}
