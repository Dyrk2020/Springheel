namespace GameEvent;

public class ScoreboardEvent : GameEvent
{
	public readonly bool Showing;

	public readonly bool AfterTally;

	public ScoreboardEvent(bool show, bool afterTally = false)
	{
		Showing = show;
		AfterTally = afterTally;
	}
}
