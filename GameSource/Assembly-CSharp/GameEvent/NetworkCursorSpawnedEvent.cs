namespace GameEvent;

public class NetworkCursorSpawnedEvent : GameEvent
{
	public readonly Cursor SpawnedCursor;

	public NetworkCursorSpawnedEvent(Cursor spawnedCursor)
	{
		SpawnedCursor = spawnedCursor;
	}
}
