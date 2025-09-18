namespace GameEvent;

public class FreePlayCharacterRespawnEvent : GameEvent
{
	public Character character;

	public FreePlayCharacterRespawnEvent(Character character)
	{
		this.character = character;
	}
}
