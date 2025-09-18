using Steamworks;

public class SteamRichPresenceManager : RichPresenceManager
{
	public override void SetDefaultPresenceString(Player player = null)
	{
		if (!SteamManager.Destroyed && SteamManager.Initialized)
		{
			SteamFriends.SetRichPresence("steam_display", "#default");
		}
		DiscordListener.SetDefaultPresenceString(player);
	}

	public override void SetGamePresenceString(GameState.LevelName levelName, string levelCode, GameState.GameMode gameMode, bool online)
	{
		if (!SteamManager.Destroyed && SteamManager.Initialized)
		{
			SteamFriends.SetRichPresence("level", levelName.ToString());
			SteamFriends.SetRichPresence("code", levelCode);
			SteamFriends.SetRichPresence("mode", GameSettings.GetInstance().GameMode.ToString());
			if (online)
			{
				if (levelCode.NullOrEmpty())
				{
					SteamFriends.SetRichPresence("steam_display", "#playingOnlineGame");
				}
				else
				{
					SteamFriends.SetRichPresence("steam_display", "#playingOnlineCode");
				}
			}
			else if (levelCode.NullOrEmpty())
			{
				SteamFriends.SetRichPresence("steam_display", "#playingLocalGame");
			}
			else
			{
				SteamFriends.SetRichPresence("steam_display", "#playingLocalCode");
			}
		}
		DiscordListener.SetGamePresenceString(levelName, levelCode, gameMode, online);
	}

	public override void SetLastPresenceForPlayer(Player player)
	{
	}

	public override void SetLobbyPresenceString(GameState.GameMode gameMode, bool online)
	{
		if (!SteamManager.Destroyed && SteamManager.Initialized)
		{
			SteamFriends.SetRichPresence("mode", gameMode.ToString());
			if (online)
			{
				SteamFriends.SetRichPresence("steam_display", "#inOnlineLobby");
			}
			else
			{
				SteamFriends.SetRichPresence("steam_display", "#inLocalLobby");
			}
		}
		DiscordListener.SetLobbyPresenceString(gameMode, online);
	}
}
