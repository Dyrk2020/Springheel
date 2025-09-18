namespace GameEvent;

public class PlayerInventoryEvent : GameEvent
{
	public readonly bool Entered;

	public readonly int PlayerNumber;

	public readonly bool LeavingPauseMenu;

	public PlayerInventoryEvent(bool entered, int playerNumber, bool leavingPauseMenu = false)
	{
		Entered = entered;
		PlayerNumber = playerNumber;
		LeavingPauseMenu = leavingPauseMenu;
	}
}
