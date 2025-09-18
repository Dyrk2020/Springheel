namespace GameEvent;

public class ModifiersChangedEvent : GameEvent
{
	public TabletRule rule;

	public ModifiersChangedEvent(TabletRule rule)
	{
		this.rule = rule;
	}
}
