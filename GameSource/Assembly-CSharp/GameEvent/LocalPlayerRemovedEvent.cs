namespace GameEvent;

public class LocalPlayerRemovedEvent : GameEvent
{
	public readonly Player RemovedPlayer;

	public LocalPlayerRemovedEvent(Player removedPlayer)
	{
		RemovedPlayer = removedPlayer;
	}
}
