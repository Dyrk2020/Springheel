namespace GameEvent;

public class DestroyPieceEvent : GameEvent
{
	public readonly Placeable Piece;

	public readonly int PlayerNetworkNumber;

	public DestroyPieceEvent(Placeable piece, int playerNetworkNumber)
	{
		Piece = piece;
		PlayerNetworkNumber = playerNetworkNumber;
	}
}
