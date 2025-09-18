namespace GameEvent;

public class SetCustomBackgroundEvent : GameEvent
{
	public readonly BackgroundType NewBackground;

	public SetCustomBackgroundEvent(BackgroundType newBackground)
	{
		NewBackground = newBackground;
	}
}
