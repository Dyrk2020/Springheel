namespace GameEvent;

public class PartyBoxEvent : GameEvent
{
	public readonly bool Opened;

	public PartyBoxEvent(bool opened)
	{
		Opened = opened;
	}
}
