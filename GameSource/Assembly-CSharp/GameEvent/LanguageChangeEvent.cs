namespace GameEvent;

public class LanguageChangeEvent : GameEvent
{
	public readonly string LanguageString;

	public LanguageChangeEvent(string LanguageString)
	{
		this.LanguageString = LanguageString;
	}
}
