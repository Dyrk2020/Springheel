namespace GameEvent;

public class GameEndEvent : GameEvent
{
	public readonly GameState.GameMode GameMode;

	public readonly GameState.LevelName LevelName;

	public readonly bool Online;

	public readonly bool GameCompleted;

	public readonly int RoundsPlayed;

	public GameEndEvent(GameState.GameMode gameMode, GameState.LevelName levelName, bool online, bool gameCompleted, int roundsPlayed)
	{
		GameMode = gameMode;
		LevelName = levelName;
		Online = online;
		GameCompleted = gameCompleted;
		RoundsPlayed = roundsPlayed;
	}
}
