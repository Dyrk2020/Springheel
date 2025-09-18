namespace GameEvent;

public class PauseEvent : GameEvent
{
	public readonly bool Paused;

	public readonly int PlayerNumber;

	public PauseEvent(bool pause, int playerNumber)
	{
		Paused = pause;
		PlayerNumber = playerNumber;
	}
}
