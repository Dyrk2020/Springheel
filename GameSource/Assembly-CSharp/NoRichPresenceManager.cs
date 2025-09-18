public class NoRichPresenceManager : RichPresenceManager
{
	public override void SetDefaultPresenceString(Player player = null)
	{
	}

	public override void SetGamePresenceString(GameState.LevelName levelName, string levelCode, GameState.GameMode gameMode, bool online)
	{
	}

	public override void SetLastPresenceForPlayer(Player player)
	{
	}

	public override void SetLobbyPresenceString(GameState.GameMode gameMode, bool online)
	{
	}
}
