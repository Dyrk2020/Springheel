namespace GameEvent;

public class PlacementSkippedEvent : GameEvent
{
	public readonly int PlayerNumber;

	public PlacementSkippedEvent(int playerNumber)
	{
		PlayerNumber = playerNumber;
	}
}
