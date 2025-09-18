namespace GameEvent;

public class DrivingPlayerRemovedEvent : GameEvent
{
	public readonly string abortReason;

	public DrivingPlayerRemovedEvent(string abortReason = null)
	{
		this.abortReason = abortReason;
	}
}
