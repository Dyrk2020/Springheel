using UnityEngine;

public class StatReaderPlayerActions : StatReader
{
	public enum StatTypes
	{
		Jumps,
		WallJumps,
		TimesTeleported,
		SpringBounces,
		DistanceRun,
		DistanceSlid
	}

	public StatTypes StatToTrack;

	protected override string getValue()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		return StatToTrack switch
		{
			StatTypes.Jumps => saveFileDataForMainUser.GetStat<StatCount>("Jumps").count.ToString(), 
			StatTypes.WallJumps => saveFileDataForMainUser.GetStat<StatCount>("WallJumps").count.ToString(), 
			StatTypes.TimesTeleported => saveFileDataForMainUser.GetStat<StatCount>("TimesTeleported").count.ToString(), 
			StatTypes.SpringBounces => saveFileDataForMainUser.GetStat<StatCount>("SpringBounces").count.ToString(), 
			StatTypes.DistanceRun => Mathf.RoundToInt(saveFileDataForMainUser.GetStat<StatFloat>("DistanceRun").value).ToString(), 
			StatTypes.DistanceSlid => Mathf.RoundToInt(saveFileDataForMainUser.GetStat<StatFloat>("DistanceSlid").value).ToString(), 
			_ => "0", 
		};
	}
}
