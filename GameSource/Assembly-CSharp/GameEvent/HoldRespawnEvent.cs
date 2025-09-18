namespace GameEvent;

public class HoldRespawnEvent : GameEvent
{
	public readonly bool Hold;

	public HoldRespawnEvent(bool hold)
	{
		Hold = hold;
	}
}
