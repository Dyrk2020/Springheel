namespace GameEvent;

public class FreePlayPlayerSwitchEvent : GameEvent
{
	public readonly int NetworkNumber;

	public readonly GameControl.GamePhase Phase;

	public FreePlayPlayerSwitchEvent(int networkNumber, GameControl.GamePhase phase)
	{
		NetworkNumber = networkNumber;
		Phase = phase;
	}
}
