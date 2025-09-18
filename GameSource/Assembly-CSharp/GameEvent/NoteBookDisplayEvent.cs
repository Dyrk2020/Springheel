namespace GameEvent;

public class NoteBookDisplayEvent : GameEvent
{
	public readonly bool Opened;

	public NoteBookDisplayEvent(bool opened)
	{
		Opened = opened;
	}
}
