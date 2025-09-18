namespace GameEvent;

public class SpecialUIEvent : GameEvent
{
	public enum SpecialUI
	{
		NOITEMSELECTED,
		SCOREBOARDDELAY,
		REFRESHSEARCH
	}

	public readonly SpecialUI SpecialUIType;

	public SpecialUIEvent(SpecialUI specialUIType)
	{
		SpecialUIType = specialUIType;
	}
}
