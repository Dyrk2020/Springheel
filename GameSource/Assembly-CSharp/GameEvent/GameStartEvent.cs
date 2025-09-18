namespace GameEvent;

public class GameStartEvent : GameEvent
{
	public readonly GameState.GameMode GameMode;

	public readonly GameState.LevelName Level;

	public readonly string LevelCode;

	public GameStartEvent(GameState.GameMode mode, GameState.LevelName level, string levelCode)
	{
		GameMode = mode;
		Level = level;
		LevelCode = levelCode;
	}
}
