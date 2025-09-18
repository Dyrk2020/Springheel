namespace GameEvent;

public class PlatformPlayerRemovedEvent : GameEvent
{
	public readonly Player RemovedPlayer;

	public PlatformPlayerRemovedEvent(Player removedPlayer)
	{
		RemovedPlayer = removedPlayer;
	}
}
