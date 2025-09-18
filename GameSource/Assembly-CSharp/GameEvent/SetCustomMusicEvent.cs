namespace GameEvent;

public class SetCustomMusicEvent : GameEvent
{
	public readonly GameState.LevelName NewLevelMusic;

	public SetCustomMusicEvent(GameState.LevelName newLevelMusic)
	{
		NewLevelMusic = newLevelMusic;
	}
}
