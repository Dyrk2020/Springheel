namespace GameEvent;

public class TwitchItemVoteEvent : GameEvent
{
	public readonly int pickableID;

	public TwitchItemVoteEvent(int pickableID)
	{
		this.pickableID = pickableID;
	}
}
