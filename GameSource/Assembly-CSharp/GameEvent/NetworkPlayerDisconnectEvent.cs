namespace GameEvent;

public class NetworkPlayerDisconnectEvent : GameEvent
{
	public readonly int NetworkNumber;

	public readonly bool Kicked;

	public NetworkPlayerDisconnectEvent(int networkNumber, bool kicked)
	{
		NetworkNumber = networkNumber;
		Kicked = kicked;
	}
}
