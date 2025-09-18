namespace GameEvent;

public class PlayerSucceedEvent : GameEvent
{
	public readonly Character Character;

	public PlayerSucceedEvent(Character winner)
	{
		Character = winner;
	}
}
