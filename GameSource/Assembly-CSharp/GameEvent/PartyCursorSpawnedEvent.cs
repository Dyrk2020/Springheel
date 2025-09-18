namespace GameEvent;

public class PartyCursorSpawnedEvent : GameEvent
{
	public readonly PartyPickCursor SpawnedCursor;

	public PartyCursorSpawnedEvent(PartyPickCursor spawnedCursor)
	{
		SpawnedCursor = spawnedCursor;
	}
}
