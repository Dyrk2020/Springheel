namespace GameEvent;

public class PlayerInGameRuleEvent : GameEvent
{
	public readonly bool Entered;

	public readonly int PlayerNumber;

	public readonly int TargetPageNumber;

	public readonly bool SoundEffect;

	public PlayerInGameRuleEvent(bool entered, int playerNumber, bool BookSoundEffect = true)
	{
		Entered = entered;
		PlayerNumber = playerNumber;
		TargetPageNumber = 0;
		SoundEffect = BookSoundEffect;
	}

	public PlayerInGameRuleEvent(bool entered, int playerNumber, int pageNumber, bool BookSoundEffect = true)
	{
		Entered = entered;
		PlayerNumber = playerNumber;
		TargetPageNumber = pageNumber;
		SoundEffect = BookSoundEffect;
	}
}
