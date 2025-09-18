namespace GameEvent;

public class PrepareBlankLevelForScreenShot : GameEvent
{
	public bool Hidden;

	public PrepareBlankLevelForScreenShot(bool hidden)
	{
		Hidden = hidden;
	}
}
