namespace GameEvent;

public class StartPhaseEvent : GameEvent
{
	public readonly GameControl.GamePhase Phase;

	public StartPhaseEvent(GameControl.GamePhase phase)
	{
		Phase = phase;
	}
}
