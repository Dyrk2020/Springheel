namespace GameEvent;

public class LocalPlayerAddedEvent : GameEvent
{
	public readonly Player NewPlayer;

	public LocalPlayerAddedEvent(Player newPlayer)
	{
		NewPlayer = newPlayer;
	}
}
