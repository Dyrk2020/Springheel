namespace GameEvent;

public class EndPhaseEvent : GameEvent
{
	public readonly GameControl.GamePhase Phase;

	public EndPhaseEvent(GameControl.GamePhase phase)
	{
		Phase = phase;
	}
}
