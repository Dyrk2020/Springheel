namespace GameEvent;

public class SoftPauseEvent : GameEvent
{
	public readonly bool SoftPaused;

	public readonly int PlayerNumber;

	public readonly bool HostPausing;

	public SoftPauseEvent(bool softpause, int playerNumber, bool hostPausing)
	{
		SoftPaused = softpause;
		PlayerNumber = playerNumber;
		HostPausing = hostPausing;
	}
}
