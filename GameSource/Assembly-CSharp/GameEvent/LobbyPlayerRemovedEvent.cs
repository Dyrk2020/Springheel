namespace GameEvent;

public class LobbyPlayerRemovedEvent : GameEvent
{
	public readonly int PlayerNumber;

	public LobbyPlayerRemovedEvent(int playerNumber)
	{
		PlayerNumber = playerNumber;
	}
}
