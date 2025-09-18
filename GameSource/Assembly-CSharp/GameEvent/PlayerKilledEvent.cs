namespace GameEvent;

public class PlayerKilledEvent : GameEvent
{
	public readonly string Cause;

	public readonly Player Player;

	public PlayerKilledEvent(Player victim, string cause)
	{
		Cause = cause;
		Player = victim;
	}
}
