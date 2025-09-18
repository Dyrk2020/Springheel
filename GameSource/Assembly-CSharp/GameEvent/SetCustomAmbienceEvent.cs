namespace GameEvent;

public class SetCustomAmbienceEvent : GameEvent
{
	public readonly GameState.LevelName NewLevelAmbience;

	public SetCustomAmbienceEvent(GameState.LevelName newLevelMusic)
	{
		NewLevelAmbience = newLevelMusic;
	}
}
