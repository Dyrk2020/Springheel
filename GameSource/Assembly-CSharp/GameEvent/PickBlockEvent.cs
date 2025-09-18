namespace GameEvent;

public class PickBlockEvent : GameEvent
{
	public readonly int PlayerNumber;

	public readonly PickableBlock PickablePiece;

	public Placeable ReuseTransformPlaceable;

	public PickBlockEvent(int playerNumber, PickableBlock pickablePiece, Placeable reuseTransformPlaceable = null)
	{
		PlayerNumber = playerNumber;
		PickablePiece = pickablePiece;
		ReuseTransformPlaceable = reuseTransformPlaceable;
	}
}
