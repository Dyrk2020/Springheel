public abstract class RichPresenceManager
{
	private static RichPresenceManager instance;

	public static RichPresenceManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new SteamRichPresenceManager();
			}
			return instance;
		}
	}

	public abstract void SetDefaultPresenceString(Player player = null);

	public abstract void SetLastPresenceForPlayer(Player player);

	public abstract void SetLobbyPresenceString(GameState.GameMode gameMode, bool online);

	public abstract void SetGamePresenceString(GameState.LevelName levelName, string levelCode, GameState.GameMode gameMode, bool online);
}
