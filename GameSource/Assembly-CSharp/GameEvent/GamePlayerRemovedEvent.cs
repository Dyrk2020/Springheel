namespace GameEvent;

public class GamePlayerRemovedEvent : GameEvent
{
	public readonly int PlayerNetworkNumber;

	public GamePlayerRemovedEvent(int playerNumber)
	{
		PlayerNetworkNumber = playerNumber;
	}
}
