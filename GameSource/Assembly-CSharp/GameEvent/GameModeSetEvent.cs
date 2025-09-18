namespace GameEvent;

public class GameModeSetEvent : GameEvent
{
	public readonly GameState.GameMode Mode;

	public GameModeSetEvent(GameState.GameMode mode)
	{
		Mode = mode;
	}
}
