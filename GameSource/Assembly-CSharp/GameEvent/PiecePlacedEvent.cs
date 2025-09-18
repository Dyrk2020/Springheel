namespace GameEvent;

public class PiecePlacedEvent : GameEvent
{
	public readonly int PlayerNumber;

	public readonly Placeable PlacedBlock;

	public PiecePlacedEvent(int playerNumber, Placeable placedBlock)
	{
		PlayerNumber = playerNumber;
		PlacedBlock = placedBlock;
	}
}
