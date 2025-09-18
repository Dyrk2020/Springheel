namespace GameEvent;

public class ControllerConnectionEvent : GameEvent
{
	public readonly bool Connected;

	public readonly Player Player;

	public ControllerConnectionEvent(bool connected, Player player)
	{
		Connected = connected;
		Player = player;
	}
}
